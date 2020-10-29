// Copyright 2020 Chabloom LC. All rights reserved.

using Chabloom.Payments.Data;
using Chabloom.Payments.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chabloom.Payments
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
            var accountsBackendAddress = System.Environment.GetEnvironmentVariable("ACCOUNTS_BACKEND_ADDRESS");
            var paymentsPublicAddress = System.Environment.GetEnvironmentVariable("PAYMENTS_PUBLIC_ADDRESS");
            if (string.IsNullOrEmpty(accountsBackendAddress) ||
                string.IsNullOrEmpty(paymentsPublicAddress))
            {
                accountsBackendAddress = "http://localhost:5001";
                paymentsPublicAddress = "http://localhost:3001";
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = accountsBackendAddress;
                    options.Audience = "Chabloom.Payments";
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "Chabloom.Payments");
                });
                options.AddPolicy("IpcScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "Chabloom.Payments.IPC");
                });
            });

            services.AddScoped<IValidator, Validator>();

            services.AddControllers();

            // Setup CORS
            services.AddCors(options =>
            {
                options.AddPolicy("CORS",
                    builder =>
                    {
                        builder.WithOrigins(paymentsPublicAddress);
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                        builder.AllowCredentials();
                    });
            });

            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseCors("CORS");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization("ApiScope"); });
        }
    }
}