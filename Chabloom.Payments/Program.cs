// Copyright 2020 Chabloom LC. All rights reserved.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Chabloom.Payments
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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}