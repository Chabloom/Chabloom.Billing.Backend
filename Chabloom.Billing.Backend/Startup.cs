// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Auth;
using Chabloom.Billing.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Chabloom.Billing.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            const string audience = "Chabloom.Billing.Backend";

            var frontendPublicAddress = Environment.GetEnvironmentVariable("BILLING_FRONTEND_ADDRESS");
            var identityServerBuilder = services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ErrorUrl = $"{frontendPublicAddress}/Account/Error";
                    options.UserInteraction.LoginUrl = $"{frontendPublicAddress}/Account/SignIn";
                    options.UserInteraction.LogoutUrl = $"{frontendPublicAddress}/Account/SignOut";
                })
                .AddConfigurationStore(options => options.ConfigureDbContext = x =>
                    x.UseNpgsql(Configuration.GetConnectionString("ConfigurationConnection"),
                        y => y.MigrationsAssembly(audience)))
                .AddOperationalStore(options => options.ConfigureDbContext = x =>
                    x.UseNpgsql(Configuration.GetConnectionString("OperationConnection"),
                        y => y.MigrationsAssembly(audience)))
                .AddAspNetIdentity<User>();

            const string signingKeyPath = "signing/cert.pfx";
            if (File.Exists(signingKeyPath))
            {
                Console.WriteLine("Using signing credential from kubernetes storage");
                var signingKeyCert = new X509Certificate2(File.ReadAllBytes(signingKeyPath));
                identityServerBuilder.AddSigningCredential(signingKeyCert);
            }
            else
            {
                Console.WriteLine("Using developer signing credential");
                identityServerBuilder.AddDeveloperSigningCredential();
            }

            var redisConfiguration = Environment.GetEnvironmentVariable("REDIS_CONFIGURATION");
            if (!string.IsNullOrEmpty(redisConfiguration))
            {
                services.AddDataProtection()
                    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConfiguration),
                        $"{audience}-DataProtection");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Environment.GetEnvironmentVariable("BILLING_BACKEND_ADDRESS");
                    options.Audience = audience;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", audience);
                });
            });

            services.AddScoped<IValidator, Validator>();
            services.AddTransient<EmailSender>();
            services.AddTransient<SmsSender>();

            // Load CORS origins
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    var origins = new List<string>
                    {
                        Environment.GetEnvironmentVariable("BILLING_FRONTEND_ADDRESS")
                    };
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        origins.Add("https://localhost:3000");
                        origins.Add("https://localhost:3001");
                        origins.Add("https://localhost:3002");
                        origins.Add("https://localhost:3003");
                    }

                    builder.WithOrigins(origins.ToArray());
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                    builder.AllowCredentials();
                });
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders();

            app.SeedIdentityServer();

            app.UseCors();

            app.UseIdentityServer();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization("ApiScope"); });
        }
    }
}