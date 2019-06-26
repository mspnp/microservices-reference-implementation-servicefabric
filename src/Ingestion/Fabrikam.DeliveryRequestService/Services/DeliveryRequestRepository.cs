using DeliveryRequestService.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryRequestService.Services
{
    public class DeliveryRequestRepository : IDeliveryRequestRepository
    {
        private readonly IQueueClient _queueClient;
        private readonly ILogger _logger;

        public DeliveryRequestRepository(IQueueClient queueClient, ILogger<DeliveryRequestRepository> logger)
        {
            _queueClient = queueClient;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(InternalDeliveryRequest deliveryRequest)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deliveryRequest)));

            try
            {
                // Send the message to the queue.
                await _queueClient.SendAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError("Error sending delivery request to service bus queue {errorMessage}", e.StackTrace);
                return false;
            }

            return true;
        }
    }
}
