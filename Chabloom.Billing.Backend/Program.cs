// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    {
                        var vaultAddress = Environment.GetEnvironmentVariable("AZURE_VAULT_ADDRESS");
                        if (string.IsNullOrEmpty(vaultAddress))
                        {
                            return;
                        }

                        var client = new SecretClient(new Uri(vaultAddress), new DefaultAzureCredential());
                        builder.AddAzureKeyVault(client, new KeyVaultSecretManager());
                    }
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}