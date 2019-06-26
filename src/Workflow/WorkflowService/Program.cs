// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Fabrikam.Workflow.Service
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("WorkflowServiceType",
                    context => 
                    {
                        var serviceCollection = new ServiceCollection();
                        ServiceStartup.ConfigureServices(context, serviceCollection);
                        var serviceProvider = serviceCollection.BuildServiceProvider();

                        var telemetryConfiguration = serviceProvider.GetRequiredService<TelemetryConfiguration>();
                        var telemetryConfigurationSetup = serviceProvider.GetRequiredService<IConfigureOptions<TelemetryConfiguration>>();
                        telemetryConfigurationSetup.Configure(telemetryConfiguration);

                        return serviceProvider.GetRequiredService<WorkflowService>();
                    }).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(WorkflowService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
