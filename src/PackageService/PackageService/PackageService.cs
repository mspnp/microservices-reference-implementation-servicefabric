// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PackageService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents;
using System;
using Microsoft.Azure.Documents.Client;
using PackageService.PackageRoot.Config;

namespace PackageService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class PackageService : StatelessService
    {
        public PackageService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureAppConfiguration((builder, config)=>
                                    {
                                        config.AddJsonFile(serviceContext, "appsettings.json")
                                              .AddJsonFile(serviceContext, $"appsettings.{builder.HostingEnvironment.EnvironmentName}.json")
                                              .AddEnvironmentVariables();

                                        var builtConfig = config.Build();


                                        if(builtConfig["AzureKeyVault:KeyVaultUri"] is var keyVaultUri && !string.IsNullOrEmpty(keyVaultUri))
                                        {
                                            config.AddAzureKeyVault(keyVaultUri);
                                        }

                                        // Get CosmosDB database and collection and store it in DocumentConfig. Those values
                                        // are accessd by the PackageRepository class.
                                        if(builtConfig["CosmosDB:Database"] is var database && !string.IsNullOrEmpty(database))
                                        {
                                            DocumentConfig.DatabaseId = database;
                                        }
                                        if(builtConfig["CosmosDB:Collection"] is var collection && !string.IsNullOrEmpty(collection))
                                        {
                                            DocumentConfig.CollectionId = collection;
                                        }
                                    })
                                    .ConfigureServices((context, services) => services
                                            .AddSingleton(context.Configuration)
                                            .AddSingleton<StatelessServiceContext>(serviceContext)
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext))
                                            .AddSingleton<IPackageRepository, PackageRepository>()
                                            .AddSingleton<IDocumentClient>(new DocumentClient(new Uri(context.Configuration["CosmosDB:Endpoint"]), context.Configuration["CosmosDB:AuthKey"])))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .ConfigureLogging((hostingContext, logging) =>
                                    {
                                        logging.AddApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"]);
                                    })
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
