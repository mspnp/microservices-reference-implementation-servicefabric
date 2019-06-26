// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using DeliveryService.Controllers;
using DeliveryService.Models;
using DeliveryService.Services;
using DroneDelivery.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DeliveryService.Tests
{
    public class DeliveriesControllerFixture
    {
        private static UserAccount userAccount = new UserAccount("userid", "accountid");

        private static Delivery delivery = new Delivery("deliveryid",
                                        userAccount,
                                        new Location(0, 0, 0),
                                        new Location(1, 1, 1),
                                        "deadline",
                                        false,
                                        ConfirmationType.FingerPrint,
                                        "droneid");
        [Fact]
        public async Task Get_Returns404_IfDeliveryIdNotValid()
        {
            // Arrange
            var logger = new Mock<ILogger<DeliveriesController>>();

            var target = new DeliveriesController(new Mock<IDeliveryRepository>().Object,
                                                  logger.Object);

            // Act
            var result = await target.Get("invaliddeliveryid") as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetStatus_Returns404_IfDeliveryIdNotValid()
        {
            // Arrange
            var logger = new Mock<ILogger<DeliveriesController>>();

            var target = new DeliveriesController(new Mock<IDeliveryRepository>().Object,
                                                  logger.Object);

            // Act
            var result = await target.GetStatus("invaliddeliveryid") as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Put_Returns204_IfDeliveryIdExists()
        {
            // Arrange
            var deliveryRepository = new Mock<IDeliveryRepository>();
            deliveryRepository.Setup(r => r.CreateAsync(It.IsAny<Delivery>())).ReturnsAsync(false);
            deliveryRepository.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Delivery>())).Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<DeliveriesController>>();

            var target = new DeliveriesController(deliveryRepository.Object,
                                                  logger.Object);

            // Act
            var result = await target.Put(new Delivery("existingdeliveryid", new UserAccount("user", "account"), new Location(0, 0, 0), new Location(2, 2, 2), "deadline", true, ConfirmationType.FingerPrint, "drone"), "existingdeliveryid");
            var statusCodeResult = (StatusCodeResult)result;

            // Assert
            Assert.NotNull(statusCodeResult);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Put_AddsToRepository()
        {
            // Arrange
            var deliveryRepository = new Mock<IDeliveryRepository>();
            deliveryRepository.Setup(r => r.CreateAsync(It.IsAny<Delivery>())).ReturnsAsync(true);

            var logger = new Mock<ILogger<DeliveriesController>>();

            var target = new DeliveriesController(deliveryRepository.Object,
                                                  logger.Object);
            // Act
            var result = await target.Put(new Delivery("deliveryid", new UserAccount("user", "account"), new Location(0, 0, 0), new Location(2, 2, 2), "deadline", true, ConfirmationType.FingerPrint, "drone"), "deliveryid");
            var createdAtRouteResult = (CreatedAtRouteResult)result;

            // Assert
            Assert.Equal(201, createdAtRouteResult.StatusCode);
            Assert.NotNull(createdAtRouteResult.Value);
            deliveryRepository.VerifyAll();
        }
    }
}
