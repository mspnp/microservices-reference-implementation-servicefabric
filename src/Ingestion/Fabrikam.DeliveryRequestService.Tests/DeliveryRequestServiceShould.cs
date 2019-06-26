// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using DeliveryRequestService.Controllers;
using DeliveryRequestService.Models;
using DeliveryRequestService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DeliveryRequestService
{
    public class DeliveryRequestServiceShould
    {
        [Fact]
        public async Task Return201ForDeliveryRequestCreated()
        {
            // Arrange
            var deliveryId = Guid.NewGuid().ToString();

            var mockRepo = new Mock<IDeliveryRequestRepository>();
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<InternalDeliveryRequest>())).Returns(Task.FromResult(true));

            var mockLogger = new Mock<ILogger<DeliveryRequestsController>>();

            var controller = new DeliveryRequestsController(mockRepo.Object, mockLogger.Object);

            // Act
            var result = await controller.CreateDeliveryRequestAsync(GetDeliveryRequest(deliveryId));
            var statusCodeData = (CreatedAtRouteResult)result;

            // Assert
            Assert.NotNull(statusCodeData);
            Assert.Equal(201, statusCodeData.StatusCode);

        }

        [Fact]
        public async Task BeAbleToSendMessageToQueue()
        {
            // Arrange
            var deliveryId = Guid.NewGuid().ToString();
            var mockQueueClient = new Mock<IQueueClient>();
            mockQueueClient.Setup(x => x.SendAsync(It.IsAny<Message>())).Returns(Task.CompletedTask).Verifiable();

            var mockLogger = new Mock<ILogger<DeliveryRequestRepository>>();
            var repository = new DeliveryRequestRepository(mockQueueClient.Object, mockLogger.Object);

            // Act
            await repository.CreateAsync(GetInternalDeliveryRequest(deliveryId));

            mockQueueClient.VerifyAll();
        }

        private DeliveryRequest GetDeliveryRequest(string deliveryId) => new DeliveryRequest()
        {
            DeliveryId = deliveryId,
            OwnerId = "owner-id",
            PickupLocation = "redmond",
            DropoffLocation = "renton",
            PickupTime = DateTime.Now,
            Deadline = "rightnow",
            Expedited = true,
            ConfirmationRequired = ConfirmationRequired.FingerPrint,
            PackageInfo = null
        };

        private InternalDeliveryRequest GetInternalDeliveryRequest(string deliveryId) => new InternalDeliveryRequest()
        {
            DeliveryId = deliveryId,
            OwnerId = "owner-id",
            PickupLocation = "redmond",
            DropoffLocation = "renton",
            PickupTime = DateTime.Now,
            Deadline = "rightnow",
            Expedited = true,
            ConfirmationRequired = ConfirmationRequired.FingerPrint,
            PackageInfo = null
        };
    }
}
