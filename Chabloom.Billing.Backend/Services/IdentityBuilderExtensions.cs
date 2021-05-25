// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable StringLiteralTypo

namespace Chabloom.Billing.Backend.Services
{
    public static class IdentityBuilderExtensions
    {
        private static readonly string[] Applications =
        {
            "Accounts",
            "Billing",
            "Transactions",
            "Ecommerce"
        };

        public static void SeedIdentityServer(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            if (serviceScope == null)
            {
                return;
            }

            serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            if (!context.Clients.Any())
            {
                // Create initial clients
                var clientPort = 3000;
                var clients = new List<Client>();
                foreach (var application in Applications)
                {
                    var client = new Client
                    {
                        ClientId = $"Chabloom.{application}.Frontend",
                        ClientName = $"Chabloom.{application}.Frontend",
                        AllowedGrantTypes = GrantTypes.Code,
                        AllowedScopes = new List<string>
                        {
                            "openid",
                            "profile",
                            $"Chabloom.{application}.Backend"
                        },
                        RedirectUris = new List<string>
                        {
                            $"http://localhost:{clientPort}/signin-oidc",
                            $"https://localhost:{clientPort}/signin-oidc",
                            $"https://{application.ToLower()}.chabloom.com/signin-oidc",
                            $"https://{application.ToLower()}-uat-1.chabloom.com/signin-oidc",
                            $"https://{application.ToLower()}-dev-1.chabloom.com/signin-oidc"
                        },
                        PostLogoutRedirectUris = new List<string>
                        {
                            $"http://localhost:{clientPort}/signout-oidc",
                            $"https://localhost:{clientPort}/signout-oidc",
                            $"https://{application.ToLower()}.chabloom.com/signout-oidc",
                            $"https://{application.ToLower()}-uat-1.chabloom.com/signout-oidc",
                            $"https://{application.ToLower()}-dev-1.chabloom.com/signout-oidc"
                        },
                        RequireConsent = false,
                        RequireClientSecret = false,
                        RequirePkce = true
                    };
                    // Allow access to accounts backend from other applications
                    if (application == "Billing" || application == "Transactions" || application == "Ecommerce")
                    {
                        client.AllowedScopes.Add("Chabloom.Accounts.Backend");
                    }

                    // Allow access to transactions backend from other applications
                    if (application == "Billing" || application == "Ecommerce")
                    {
                        client.AllowedScopes.Add("Chabloom.Transactions.Backend");
                    }

                    clients.Add(client);
                    ++clientPort;
                }

                // Convert client models to entities
                var clientEntities = clients
                    .Select(client => client.ToEntity())
                    .ToList();
                // Add client entities to database
                context.Clients.AddRange(clientEntities);
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                // Create initial identity resources
                var identityResources = new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()
                };
                // Convert identity resource models to entities
                var identityResourceEntities = identityResources
                    .Select(resource => resource.ToEntity())
                    .ToList();
                // Add identity resource entities to database
                context.IdentityResources.AddRange(identityResourceEntities);
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                // Create initial API scopes
                var apiScopes = Applications
                    .Select(application => new ApiScope($"Chabloom.{application}.Backend"))
                    .ToList();
                // Convert API scope models to entities
                var apiScopeEntities = apiScopes
                    .Select(resource => resource.ToEntity())
                    .ToList();
                // Add API scope entities to database
                context.ApiScopes.AddRange(apiScopeEntities);
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                // Create initial API resources
                var apiResources = Applications
                    .Select(application => new ApiResource($"Chabloom.{application}.Backend")
                    {
                        Scopes = {$"Chabloom.{application}.Backend"}
                    })
                    .ToList();
                // Convert API resource models to entities
                var apiResourceEntities = apiResources
                    .Select(resource => resource.ToEntity())
                    .ToList();
                // Add API resource entities to database
                context.ApiResources.AddRange(apiResourceEntities);
                context.SaveChanges();
            }
        }
    }
}