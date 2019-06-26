// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DeliveryService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Newtonsoft.Json;

namespace DeliveryService.Services
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private const string DICTIONARY_NAME = "deliveries";
        private readonly IReliableStateManager stateManager;
        private readonly ILogger<DeliveryRepository> logger;

        public DeliveryRepository(IReliableStateManager stateManager, ILogger<DeliveryRepository> logger)
        {
            this.stateManager = stateManager;
            this.logger = logger;
        }

        public async Task<Delivery> GetAsync(string id)
        {
            var deliveriesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(DICTIONARY_NAME);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                if (!await deliveriesDictionary.ContainsKeyAsync(tx, id))
                {
                    logger.LogInformation("Delivery not found with id: {Id}", id);
                    return null;
                }

                var deliveryJSON = await deliveriesDictionary.GetOrAddAsync(tx, id, string.Empty);
                return JsonConvert.DeserializeObject<Delivery>(deliveryJSON);
            }
        }

        public async Task<bool> CreateAsync(Delivery delivery)
        {
            var deliveriesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(DICTIONARY_NAME);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                if (await deliveriesDictionary.ContainsKeyAsync(tx, delivery.Id))
                {
                    logger.LogInformation("Existing delivery found: {Delivery}", delivery);
                    return false;
                }

                await deliveriesDictionary.AddAsync(tx, delivery.Id, JsonConvert.SerializeObject(delivery));
                await tx.CommitAsync();
                return true;
            }
        }

        public async Task UpdateAsync(string id, Delivery delivery)
        {
            var deliveriesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(DICTIONARY_NAME);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                if (!await deliveriesDictionary.ContainsKeyAsync(tx, delivery.Id))
                {
                    logger.LogInformation("Delivery not found with id: {Id}", delivery.Id);
                }

                await deliveriesDictionary.SetAsync(tx, delivery.Id, JsonConvert.SerializeObject(delivery));
                await tx.CommitAsync();
            }
        }
    }
}
