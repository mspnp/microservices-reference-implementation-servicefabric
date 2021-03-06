﻿using Microsoft.Extensions.Configuration;
using System.Fabric;

namespace DeliveryRequestService.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder, ServiceContext serviceContext, string settingsJson)
        {
            // Get the Config package directory
            var configFolderPath = serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Path;
            
            // Combine it with appsettings.json file name
            var appSettingsFilePath = System.IO.Path.Combine(configFolderPath, settingsJson);
            
            // Add to the builder, making sure it will be reloaded every time the file changes, e.g. during Config-only deployment
            builder.AddJsonFile(appSettingsFilePath, optional: false, reloadOnChange: true);
            
            // Return the builder to allow for call chaining
            return builder;
        }
    }
}
