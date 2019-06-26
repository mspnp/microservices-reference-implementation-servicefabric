// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using DeliveryService.Models;
using System.Threading.Tasks;

namespace DeliveryService.Services
{
    public interface IDeliveryRepository
    {
        Task<Delivery> GetAsync(string id);
        Task<bool> CreateAsync(Delivery delivery);
        Task UpdateAsync(string id, Delivery delivery);
    }
}