// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DeliveryService.Models;
using DeliveryService.Services;
using DroneDelivery.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeliveryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryRepository deliveryRepository;
        private readonly ILogger<DeliveriesController> logger;

        public DeliveriesController(IDeliveryRepository deliveryRepository,
                                    ILogger<DeliveriesController> logger)
        {
            this.deliveryRepository = deliveryRepository;
            this.logger = logger;
        }

        // GET api/deliveries/5
        [Route("/api/[controller]/{id}", Name = "GetDelivery")]
        [HttpGet]
        [ProducesResponseType(typeof(Delivery), 200)]
        public async Task<IActionResult> Get(string id)
        {
            logger.LogInformation("In Get action with id: {Id}", id);

            var delivery = await deliveryRepository.GetAsync(id);

            if (delivery == null)
            {
                logger.LogDebug("Delivery id: {Id} not found", id);
                return NotFound();
            }

            return Ok(delivery);
        }

        // GET api/deliveries/5/status
        [Route("/api/[controller]/{id}/status")]
        [HttpGet]
        [ProducesResponseType(typeof(DeliveryStatus), 200)]
        public async Task<IActionResult> GetStatus(string id)
        {
            logger.LogInformation("In GetStatus action with id: {Id}", id);

            var delivery = await deliveryRepository.GetAsync(id);
            if (delivery == null)
            {
                logger.LogDebug("Delivery id: {Id} not found", id);
                return NotFound();
            }

            var status = new DeliveryStatus(DeliveryStage.HeadedToDropoff, new Location(0, 0, 0), DateTime.Now.AddMinutes(10).ToString(), DateTime.Now.AddHours(1).ToString());
            return Ok(status);
        }

        // PUT api/deliveries/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Delivery), 201)]
        [ProducesResponseType(typeof(void), 204)]
        public async Task<IActionResult> Put([FromBody]Delivery delivery, string id)
        {
            logger.LogInformation("In Put action with delivery {Id}: {Delivery}", id, delivery);

            // Adds new inflight delivery 
            var success = await deliveryRepository.CreateAsync(delivery);

            if (success) return CreatedAtRoute("GetDelivery", new { id = delivery.Id }, delivery);
                
            //This method is mainly used to create deliveries. If the delivery already exists then update
            logger.LogInformation("Updating resource with delivery id: {DeliveryId}", id);

            // Updates inflight delivery 
            await deliveryRepository.UpdateAsync(id, delivery);

            return NoContent();
        }
    }
}
