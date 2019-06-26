// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using DeliveryRequestService.Extensions;
using DeliveryRequestService.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Net.Http;

namespace DeliveryRequestService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class DeliveryRequestService : StatelessService
    {
        private readonly ILogger _logger;

        public DeliveryRequestService(StatelessServiceContext context)
            : base(context)
        {
            _logger = new LoggerFactory().CreateLogger<DeliveryRequestService>();
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()

                                    .UseKestrel()
                                    .ConfigureAppConfiguration((builderContext, config)=>
                                    {
                                        config.AddJsonFile(serviceContext, "appsettings.json")
                                            .AddJsonFile(serviceContext, $"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json")
                                            .AddEnvironmentVariables();

                                        var builtConfig = config.Build();

                                        if(builtConfig["AzureKeyVault:KeyVaultUri"] is var keyVaultUri && !string.IsNullOrWhiteSpace(keyVaultUri))
                                        {
                                            config.AddAzureKeyVault(keyVaultUri);
                                        }
                                    })
                                    .ConfigureServices(
                                        (context, services) => services
                                            .AddSingleton(new HttpClient())
                                            .AddSingleton(new FabricClient())
                                            .AddSingleton(serviceContext)
                                            .AddSingleton(context.Configuration)
                                            .AddSingleton<IQueueClient>(c=>
                                                new QueueClient(new ServiceBusConnectionStringBuilder(context.Configuration["ServiceBusQueue:SendConnectionString"]))
                                            )
                                            .AddSingleton<IDeliveryRequestRepository, DeliveryRequestRepository>()
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext)))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .ConfigureLogging((hostingContext, logging)=>
                                    {
                                        logging.AddApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"]);
                                    })
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        internal static Uri GetDeliveryRequestServiceName(ServiceContext context)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/DeliveryRequest");
        }

    }
}
