using dotenv.net;
using dotenv.net.DependencyInjection.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace comino_print_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ReadEnvironmentVariablesFromDotEnv();
            var host = BuildWebHost(args);
            host.Run();
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables())
                .UseStartup<Startup>()
                .Build();

        private static void ReadEnvironmentVariablesFromDotEnv()
        {
            DotEnv.Config(
                new DotEnvOptions
                {
                    ThrowOnError = false,
                    EnvFile = ".env"
                }
            );
        }
    }
}