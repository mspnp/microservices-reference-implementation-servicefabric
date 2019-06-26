// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Fabric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fabrikam.Workflow.Service.RequestProcessing;
using Fabrikam.Workflow.Service.Services;

namespace Fabrikam.Workflow.Service
{
    public class ServiceStartup
    {
        private static string ReverseProxyBaseUri;

        public static void ConfigureServices(StatelessServiceContext context, IServiceCollection services)
        {
            var preConfig = new ConfigurationBuilder()
                .AddJsonFile(context, "appsettings.json")
                .AddEnvironmentVariables();
                
            var config = preConfig.Build();

            if (config["AzureKeyVault:KeyVaultUri"] is var keyVaultUri && !string.IsNullOrWhiteSpace(keyVaultUri))
            {
                preConfig.AddAzureKeyVault(keyVaultUri);
                config = preConfig.Build();
            }

            services.AddSingleton(config);

            // Configure AppInsights
            services.AddApplicationInsightsServiceFabricEnricher(context);
            services.AddApplicationInsightsTelemetry(config);

            services
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConfiguration(config.GetSection("Logging"));
                    loggingBuilder.AddApplicationInsights();
                })
                .AddSingleton<StatelessServiceContext>(context)
                .AddTransient<IRequestProcessor, RequestProcessor>()
                .AddSingleton<IDeliveryServiceCaller, DeliveryServiceCaller>()
                .AddSingleton<IDroneSchedulerServiceCaller, DroneSchedulerServiceCaller>()
                .AddSingleton<IPackageServiceCaller, PackageServiceCaller>()
                .AddSingleton<WorkflowService>();

            ReverseProxyBaseUri = config["ReverseProxyUri"];

            services.AddHttpClient<IPackageServiceCaller, PackageServiceCaller>(c =>
            {
                c.BaseAddress = GetProxyAddress(new Uri($"fabric:/{config["DownstreamServices:APP_NAME_PACKAGE"]}/{config["DownstreamServices:SERVICE_NAME_PACKAGE"]}/"));
            })
            .AddResiliencyPolicies(config);

            services.AddHttpClient<IDroneSchedulerServiceCaller, DroneSchedulerServiceCaller>(c =>
            {
                c.BaseAddress = GetProxyAddress(new Uri($"fabric:/{config["DownstreamServices:APP_NAME_DRONE"]}/{config["DownstreamServices:SERVICE_NAME_DRONE"]}/"));
            })
            .AddResiliencyPolicies(config);

            services.AddHttpClient<IDeliveryServiceCaller, DeliveryServiceCaller>(c =>
            {
                c.BaseAddress = GetProxyAddress(new Uri($"fabric:/{config["DownstreamServices:APP_NAME_DELIVERY"]}/{config["DownstreamServices:SERVICE_NAME_DELIVERY"]}/api/deliveries/"));
            })
            .AddResiliencyPolicies(config);
        }

        private static Uri GetProxyAddress(Uri serviceName)
        {
            return new Uri($"{ReverseProxyBaseUri}{serviceName.AbsolutePath}");
        }
    }
}
