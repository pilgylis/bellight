using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Bellight.Configurations;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder InitialiseBellightConfigurations(
        this IConfigurationBuilder configurationBuilder,
        bool isDevelopment,
        string? environmentName = null,
        string[]? args = null)
    {
        configurationBuilder.AddJsonFile("appsettings.json", true, true);

        if (!string.IsNullOrEmpty(environmentName))
        {
            configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", true, true);
        }

        if (isDevelopment)
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                configurationBuilder.AddUserSecrets(assembly, true);
            }
        }

        configurationBuilder.AddEnvironmentVariables();

        if (args == null)
        {
            return configurationBuilder;
        }

        configurationBuilder.AddCommandLine(args);
        return configurationBuilder;
    }
}