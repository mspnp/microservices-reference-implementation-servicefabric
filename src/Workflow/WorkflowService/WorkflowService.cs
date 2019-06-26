// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Fabrikam.Workflow.Service.Models;
using Fabrikam.Workflow.Service.RequestProcessing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fabrikam.Workflow.Service
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WorkflowService : StatelessService
    {
        private readonly JsonSerializer _serializer;

        private readonly ILogger<WorkflowService> _logger;
        private readonly IRequestProcessor _requestProcessor;
        private readonly IConfigurationRoot _configuration;
        private IQueueClient _receiveClient;

        public WorkflowService(StatelessServiceContext context,
                               IConfigurationRoot configuration, 
                               ILogger<WorkflowService> logger, 
                               IRequestProcessor requestProcessor)
            : base(context)
        {
            _configuration = configuration;
            _logger = logger;
            _requestProcessor = requestProcessor;
            _serializer = new JsonSerializer();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _receiveClient = CreateQueueClient(_configuration);

            _receiveClient.RegisterMessageHandler(
                ProcessMessageAsync,
                new MessageHandlerOptions(ProcessMessageExceptionAsync)
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = Convert.ToInt32(_configuration["ServiceBusQueue:MaxConcurrency"])
                });

            _logger.LogInformation("Registered Message Handler");
        }

        private IQueueClient CreateQueueClient(IConfiguration configuration)
        {
            var queueEndpoint = configuration["ServiceBusQueue:ReceiveConnectionString"];

            return new QueueClient(new ServiceBusConnectionStringBuilder(queueEndpoint))
            {
                PrefetchCount = Convert.ToInt32(configuration["ServiceBusQueue:PrefetchCount"])
            };
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken ct)
        {
            _logger.LogInformation("Processing message {messageId}", message.MessageId);

            if (TryGetDelivery(message, out var delivery))
            {
                try
                {
                    if (await _requestProcessor.ProcessDeliveryRequestAsync(delivery, new ReadOnlyDictionary<string, object>(message.UserProperties)))
                    {
                        await _receiveClient.CompleteAsync(message.SystemProperties.LockToken);
                        return;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error processing message {messageId}", message.MessageId);
                }
            }

            try
            {
                await _receiveClient.DeadLetterAsync(message.SystemProperties.LockToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error moving message {messageId} to dead letter queue", message.MessageId);
            }

            return;
        }

        private Task ProcessMessageExceptionAsync(ExceptionReceivedEventArgs exceptionEvent)
        {
            _logger.LogError(exceptionEvent.Exception, "Exception processing message");

            return Task.CompletedTask;
        }

        private bool TryGetDelivery(Message message, out Delivery delivery)
        {
            try
            {
                using (var payloadStream = new MemoryStream(message.Body, false))
                using (var streamReader = new StreamReader(payloadStream, Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    delivery = _serializer.Deserialize<Delivery>(jsonReader);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot parse payload from message {messageId}", message.MessageId);
            }

            delivery = null;
            return false;
        }
    }
}
