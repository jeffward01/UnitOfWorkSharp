﻿namespace UnitOfWorkSharp.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder().UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .UseStartup<Startup>()
            .Build();

        host.Run();
    }
}