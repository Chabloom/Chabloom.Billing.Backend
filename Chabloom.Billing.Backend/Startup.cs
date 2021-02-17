// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Collections.Generic;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chabloom.Billing.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            // Get the public address for the current environment
            var frontendPublicAddress = System.Environment.GetEnvironmentVariable("BILLING_FRONTEND_ADDRESS");
            var accountsBackendPublicAddress = System.Environment.GetEnvironmentVariable("ACCOUNTS_BACKEND_ADDRESS");
            if (string.IsNullOrEmpty(frontendPublicAddress) ||
                string.IsNullOrEmpty(accountsBackendPublicAddress))
            {
                if (Environment.EnvironmentName == "MicroK8s")
                {
                    frontendPublicAddress = "http://localhost:3001";
                    accountsBackendPublicAddress = "http://chabloom-accounts-backend";
                }
                else
                {
                    frontendPublicAddress = "http://localhost:3001";
                    accountsBackendPublicAddress = "http://localhost:5000";
                }
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = accountsBackendPublicAddress;
                    options.Audience = "Chabloom.Billing.Backend";
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "Chabloom.Billing.Backend");
                });
            });

            services.AddScoped<IValidator, Validator>();

            // Get CORS origins
            var corsOrigins = new List<string>
            {
                frontendPublicAddress
            };
            // Add development origins if required
            if (Environment.IsDevelopment())
            {
                corsOrigins.Add("http://localhost:3000");
                corsOrigins.Add("http://localhost:3001");
                corsOrigins.Add("http://localhost:3002");
                corsOrigins.Add("http://localhost:3003");
            }

            // Add the CORS policy
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(corsOrigins.ToArray());
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                    builder.AllowCredentials();
                });
            });

            services.AddApplicationInsightsTelemetry();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization("ApiScope"); });
        }
    }
}