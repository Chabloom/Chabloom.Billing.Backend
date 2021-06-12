// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Chabloom.Billing.Backend.Data;
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
        public static void SeedIdentityServer(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            if (serviceScope == null)
            {
                return;
            }

            serviceScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>().Database
                .Migrate();
            serviceScope.ServiceProvider
                .GetRequiredService<ConfigurationDbContext>().Database
                .Migrate();
            serviceScope.ServiceProvider
                .GetRequiredService<PersistedGrantDbContext>().Database
                .Migrate();

            const string name = "Chabloom.Billing.Backend";
            const string clientName = "Chabloom.Billing.Frontend";
            const int clientPort = 3001;

            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            if (!context.Clients.Any())
            {
                // Create initial client
                var client = new Client
                {
                    ClientId = clientName,
                    ClientName = clientName,
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        name
                    },
                    RedirectUris = new List<string>
                    {
                        $"http://localhost:{clientPort}/signin-oidc",
                        $"https://localhost:{clientPort}/signin-oidc",
                        "https://billing.chabloom.com/signin-oidc",
                        "https://billing-uat-1.chabloom.com/signin-oidc",
                        "https://billing-dev-1.chabloom.com/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"http://localhost:{clientPort}/signout-oidc",
                        $"https://localhost:{clientPort}/signout-oidc",
                        "https://billing.chabloom.com/signout-oidc",
                        "https://billing-uat-1.chabloom.com/signout-oidc",
                        "https://billing-dev-1.chabloom.com/signout-oidc"
                    },
                    RequireConsent = false,
                    RequireClientSecret = false,
                    RequirePkce = true
                };

                context.Add(client.ToEntity());
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
                context.AddRange(identityResourceEntities);
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                // Create initial API scope
                var apiScope = new ApiScope(name);

                context.Add(apiScope.ToEntity());
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                // Create initial API resource
                var apiResource = new ApiResource(name);

                context.Add(apiResource.ToEntity());
                context.SaveChanges();
            }
        }
    }
}