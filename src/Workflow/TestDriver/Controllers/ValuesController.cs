// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TestDriver.Models;

namespace TestDriver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly TelemetryClient telemetryClient;
        private readonly string serviceBusConnectionString;
        static IQueueClient queueClient;

        public ValuesController(TelemetryClient telemetryClient, IConfiguration configuration)
        {
            this.telemetryClient = telemetryClient;
            this.serviceBusConnectionString = configuration["ServiceBusQueue:SendConnectionString"];
            queueClient = new QueueClient(new ServiceBusConnectionStringBuilder(serviceBusConnectionString));
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            telemetryClient.TrackTrace("Sending delivery from TestDriver");

            var delivery = new Delivery
            {
                DeliveryId = $"deliveryId-{Guid.NewGuid().ToString()}",
                PackageInfo = new PackageInfo
                {
                    PackageId="package1234",
                    Size = ContainerSize.Large,
                    Weight = 1234
                }
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(delivery)));

            // Send the message to the queue.
            await queueClient.SendAsync(message);

            return new string[] { Activity.Current.RootId };
        }
    }
}
