// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fabrikam.Workflow.Service.Models;
using Fabrikam.Workflow.Service.Utils;

namespace Fabrikam.Workflow.Service.Services
{
    public class DeliveryServiceCaller : IDeliveryServiceCaller
    {
        private readonly HttpClient _httpClient;

        public DeliveryServiceCaller(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DeliverySchedule> ScheduleDeliveryAsync(Delivery deliveryRequest, string droneId)
        {
            try
            {
                var schedule = CreateDeliverySchedule(deliveryRequest, droneId);

                var partitionKey = GetPartitionKey(deliveryRequest.DeliveryId);
                var requestUri = $"{schedule.Id}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

                var response = await _httpClient.PutAsJsonAsync(requestUri, schedule);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return await response.Content.ReadAsAsync<DeliverySchedule>();
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return new DeliverySchedule { Id = deliveryRequest.DeliveryId };
                }

                throw new BackendServiceCallFailedException(response.ReasonPhrase);
            }
            catch (BackendServiceCallFailedException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BackendServiceCallFailedException(e.Message, e);
            }
        }

        private long GetPartitionKey(string deliveryId)
        {
            var byteArray = Encoding.ASCII.GetBytes(deliveryId);
            var hash = MD5.Create().ComputeHash(byteArray);

            return BitConverter.ToInt64(hash, 0);
        }

        private DeliverySchedule CreateDeliverySchedule(Delivery deliveryRequest, string droneId)
        {
            DeliverySchedule scheduleDelivery = new DeliverySchedule
            {
                Id = deliveryRequest.DeliveryId,
                Owner = new UserAccount { AccountId = Guid.NewGuid().ToString(), UserId = deliveryRequest.OwnerId },
                Pickup = LocationRandomizer.GetRandomLocation(),
                Dropoff = LocationRandomizer.GetRandomLocation(),
                Deadline = deliveryRequest.Deadline,
                Expedited = deliveryRequest.Expedited,
                ConfirmationRequired = (ConfirmationType)deliveryRequest.ConfirmationRequired,
                DroneId = droneId,
            };

            return scheduleDelivery;
        }
    }
}
