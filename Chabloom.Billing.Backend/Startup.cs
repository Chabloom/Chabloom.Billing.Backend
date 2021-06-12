// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Tenants;
using Chabloom.Billing.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddIdentity<TenantUser, TenantRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            const string audience = "Chabloom.Billing.Backend";

            var frontendPublicAddress = Environment.GetEnvironmentVariable("BILLING_FRONTEND_ADDRESS");
            var identityServerBuilder = services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ErrorUrl = $"{frontendPublicAddress}/account/error";
                    options.UserInteraction.LoginUrl = $"{frontendPublicAddress}/account/signIn";
                    options.UserInteraction.LogoutUrl = $"{frontendPublicAddress}/account/signOut";
                })
                .AddConfigurationStore(options => options.ConfigureDbContext = x =>
                    x.UseNpgsql(Configuration.GetConnectionString("ConfigurationConnection"),
                        y => y.MigrationsAssembly(audience)))
                .AddOperationalStore(options => options.ConfigureDbContext = x =>
                    x.UseNpgsql(Configuration.GetConnectionString("OperationConnection"),
                        y => y.MigrationsAssembly(audience)))
                .AddAspNetIdentity<TenantUser>();

            const string signingKeyPath = "signing/cert.pfx";
            if (File.Exists(signingKeyPath))
            {
                var signingKeyCert = new X509Certificate2(File.ReadAllBytes(signingKeyPath));
                identityServerBuilder.AddSigningCredential(signingKeyCert);
            }
            else
            {
                identityServerBuilder.AddDeveloperSigningCredential();
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.IsEssential = true;
                })
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