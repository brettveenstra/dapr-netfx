// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

namespace DaprNetFx.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Provides configuration values from app.config/web.config with environment variable overrides.
    /// </summary>
    internal static class DaprConfigurationProvider
    {
        private const string HttpEndpointKey = "Dapr:HttpEndpoint";
        private const string ApiTokenKey = "Dapr:ApiToken";
        private const string RequiredKey = "Dapr:Required";
        private const string HttpTimeoutSecondsKey = "Dapr:HttpTimeoutSeconds";

        private const string HttpEndpointEnvVar = "DAPR_HTTP_ENDPOINT";
        private const string ApiTokenEnvVar = "DAPR_API_TOKEN";
        private const string RequiredEnvVar = "DAPR_REQUIRED";
        private const string HttpTimeoutSecondsEnvVar = "DAPR_HTTP_TIMEOUT_SECONDS";

        /// <summary>
        /// Loads Dapr client options from configuration sources.
        /// </summary>
        /// <returns>A configured <see cref="DaprClientOptions"/> instance.</returns>
        /// <remarks>
        /// Configuration precedence: Environment variables > app.config > defaults.
        /// </remarks>
        internal static DaprClientOptions LoadOptions()
        {
            var options = new DaprClientOptions();

            // HTTP Endpoint: Environment variable > AppSettings > Default
            options.HttpEndpoint = GetConfigValue(
                HttpEndpointEnvVar,
                HttpEndpointKey,
                options.HttpEndpoint);

            // API Token: Environment variable > AppSettings > null
            options.ApiToken = GetConfigValue(
                ApiTokenEnvVar,
                ApiTokenKey,
                defaultValue: null);

            // Required: Environment variable > AppSettings > true (default)
            var requiredValue = GetConfigValue(
                RequiredEnvVar,
                RequiredKey,
                options.Required.ToString());

            if (bool.TryParse(requiredValue, out var required))
            {
                options.Required = required;
            }

            // HttpTimeout: Environment variable > AppSettings > 30 seconds (default)
            var timeoutValue = GetConfigValue(
                HttpTimeoutSecondsEnvVar,
                HttpTimeoutSecondsKey,
                defaultValue: null);

            if (!string.IsNullOrWhiteSpace(timeoutValue) &&
                int.TryParse(timeoutValue, out var timeoutSeconds) &&
                timeoutSeconds > 0)
            {
                options.HttpTimeout = TimeSpan.FromSeconds(timeoutSeconds);
            }

            return options;
        }

        private static string GetConfigValue(
            string environmentVariable,
            string appSettingsKey,
            string defaultValue)
        {
            // 1. Check environment variable
            var envValue = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }

            // 2. Check app.config/web.config
            var appSettingValue = ConfigurationManager.AppSettings[appSettingsKey];
            if (!string.IsNullOrWhiteSpace(appSettingValue))
            {
                return appSettingValue;
            }

            // 3. Use default
            return defaultValue;
        }
    }
}
