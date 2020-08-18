// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using Chabloom.Payments.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:44303";
                    options.Audience = "Chabloom.Payments";
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "Chabloom.Payments");
                });
            });

            services.AddControllers();

            if (Environment.IsDevelopment())
            {
                // Setup development CORS
                services.AddCors(options =>
                {
                    options.AddPolicy("Development",
                        builder =>
                        {
                            builder.WithOrigins("https://localhost:44334");
                            builder.WithOrigins("http://localhost:3000");
                            builder.WithOrigins("http://localhost:3001");
                            builder.AllowAnyMethod();
                            builder.AllowAnyHeader();
                            builder.AllowCredentials();
                        });
                });

                // Setup generated OpenAPI documentation
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Chabloom Payments",
                        Description = "Chabloom Payments v1 API",
                        Version = "v1"
                    });
                    options.AddSecurityDefinition("openid", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new Uri("https://localhost:44303/.well-known/openid-configuration")
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseCors("Development");
                app.UseSwagger(options => options.RouteTemplate = "/swagger/{documentName}/chabloom-payments-api.json");
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/chabloom-payments-api.json", "Chabloom Payments v1 API");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization("ApiScope"); });
        }
    }
}