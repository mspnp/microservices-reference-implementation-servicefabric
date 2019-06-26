// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Fabric;
using Microsoft.Extensions.Configuration;

namespace Fabrikam.Workflow.Service
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder, ServiceContext serviceContext, string settingsJson)
        {
            var configFolderPath = "PackageRoot/Config";

            var configPackage = serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            if (configPackage != null)
            {
                // Get the Config package directory
                configFolderPath = configPackage.Path;
            }

            // Combine it with appsettings.json file name
            var appSettingsFilePath = System.IO.Path.Combine(configFolderPath, settingsJson);

            // Add to the builder, making sure it will be reloaded every time the file changes, e.g. during Config-only deployment
            builder.AddJsonFile(appSettingsFilePath, optional: false, reloadOnChange: true);

            // Return the builder to allow for call chaining
            return builder;
        }
    }

}
