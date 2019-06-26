// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using PackageService.Models;
using PackageService.Controllers;
using PackageService.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PackageService.Tests
{
    public class PackageControllerFixture
    {
        private static Package package = new Package("TestPackageId", PackageSize.Large, 10.0, "Test Package");

        [Fact]
        public async Task Get_Returns_404_IfPackageNotValid()
        {
            //Arrange
            var target = new PackageController(new Mock<IPackageRepository>().Object, new Mock<ILogger<PackageController>>().Object);

            //Act
            var result = await target.Get("InvalidId") as NotFoundResult;

            //Assert
            Assert.NotNull(result);

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Get_Returns_400_ifIdIsNull()
        {
            //Arrange
            var target = new PackageController(new Mock<IPackageRepository>().Object, new Mock<ILogger<PackageController>>().Object);

            //Act
            var result = await target.Get(null) as BadRequestResult;

            //Assert
            Assert.NotNull(result);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Patch_Returns_404_IfPackageNotFound()
        {
            //Arrange
            var target = new PackageController(new Mock<IPackageRepository>().Object, new Mock<ILogger<PackageController>>().Object);

            //Act
            var result = await target.Patch("TestPackageId", package) as NotFoundResult;

            //Assert

            Assert.NotNull(result);

            Assert.Equal(404, result.StatusCode);

        }

        [Fact]
        public async Task Patch_Returns_400_ifIdIsNull()
        {
            //Arrange
            var target = new PackageController(new Mock<IPackageRepository>().Object, new Mock<ILogger<PackageController>>().Object);

            //Act
            var result = await target.Patch(null, package) as BadRequestResult;

            //Assert
            Assert.NotNull(result);

            Assert.Equal(400, result.StatusCode);
        }


        [Fact]
        public async Task Put_Returns_500_UpsertFailed()
        {
            //Arrange

            var packageRespository = new Mock<IPackageRepository>();

            packageRespository.Setup(r => r.AddPackageAsync(It.IsAny<Package>())).ReturnsAsync(PackageUpsertStatusCode.Invalid);

            var target = new PackageController(packageRespository.Object, new Mock<ILogger<PackageController>>().Object);

            //Act
            var result = await target.Put(package.Id, package);

            var statusCodeResult = (StatusCodeResult)result;

            //Assert
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Put_Returns_201_CreatesNewPackage()
        {
            //Arrange
            var packageRepository = new Mock<IPackageRepository>();

            packageRepository.Setup(r => r.AddPackageAsync(It.IsAny<Package>())).ReturnsAsync(PackageUpsertStatusCode.Created);

            var target = new PackageController(packageRepository.Object,
                new Mock<ILogger<PackageController>>().Object);

            //Act

            var result = await target.Put(package.Id, package);

            CreatedAtRouteResult createdAtRoute = (CreatedAtRouteResult)result;

            //Assert
            Assert.Equal(201, createdAtRoute.StatusCode);
            Assert.NotNull(createdAtRoute.Value);
        }

        [Fact]
        public async Task Put_Returns_400_ifIdIsNull()
        {
            //Arrange
            var target = new PackageController(new Mock<IPackageRepository>().Object, new Mock<ILogger<PackageController>>().Object);

            //Act
            var result = await target.Put(null, package) as BadRequestResult;

            //Assert
            Assert.NotNull(result);

            Assert.Equal(400, result.StatusCode);
        }
    }
}
