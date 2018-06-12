using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Shop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Rabbit rabbit = Rabbit.GetInstance();
            BuildWebHost(args).Run();
            rabbit.StopRabbit();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://127.0.0.1:5001")
                .Build();
    }
}
