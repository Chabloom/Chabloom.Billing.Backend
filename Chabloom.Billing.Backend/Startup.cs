// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
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
            services.AddDbContext<BillingDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            const string audience = "Chabloom.Billing.Backend";

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
                    options.Authority = Environment.GetEnvironmentVariable("ACCOUNTS_BACKEND_ADDRESS");
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

            // Load CORS origins
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    var origins = new[]
                    {
                        Environment.GetEnvironmentVariable("ACCOUNTS_FRONTEND_ADDRESS"),
                        Environment.GetEnvironmentVariable("BILLING_FRONTEND_ADDRESS"),
                        Environment.GetEnvironmentVariable("ECOMMERCE_FRONTEND_ADDRESS"),
                        Environment.GetEnvironmentVariable("TRANSACTIONS_FRONTEND_ADDRESS")
                    };
                    builder.WithOrigins(origins);
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

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization("ApiScope"); });
        }
    }
}