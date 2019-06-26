using DeliveryRequestService.Models;
using DeliveryRequestService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DeliveryRequestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryRequestsController : Controller
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly ILogger _logger;

        public DeliveryRequestsController(IDeliveryRequestRepository deliveryRequestRepository, ILogger<DeliveryRequestsController> logger)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _logger = logger;
        }


        // POST api/deliveryRequest
        [Route("/api/[controller]", Name = "CreateDeliveryRequest")]
        [HttpPost]
        [ProducesResponseType(typeof(DeliveryRequest), 201)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDeliveryRequestAsync([FromBody]DeliveryRequest deliveryRequest)
        {
            _logger.LogInformation("In Schedule action with delivery request: {deliveryRequest}", deliveryRequest);

            // Based on the delivery request received, construct an internal delivery request to pass around
            var deliveryId = Guid.NewGuid().ToString();
            var internalDeliveryRequest = new InternalDeliveryRequest(deliveryId, 
                                        deliveryRequest.OwnerId, 
                                        deliveryRequest.PickupLocation, 
                                        deliveryRequest.DropoffLocation, 
                                        deliveryRequest.PickupTime, 
                                        deliveryRequest.Deadline, 
                                        deliveryRequest.Expedited, 
                                        deliveryRequest.ConfirmationRequired, 
                                        deliveryRequest.PackageInfo);

            // Set the delivery id for the payload to return
            deliveryRequest.DeliveryId = deliveryId;
            var completed = await _deliveryRequestRepository.CreateAsync(internalDeliveryRequest);

            return completed ? (IActionResult) CreatedAtRoute("CreateDeliveryRequest", new { id = deliveryRequest.DeliveryId }, deliveryRequest) : BadRequest("Delivery request failed. Check logs for exception details.");

        }
    }
}
