﻿namespace UnitOfWorkSharp.Host
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Models;

    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

            if (env.IsEnvironment("Development"))
            {
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // use in memory for testing.
            services.AddDbContext<BloggingContext>(opt =>
                    opt.UseMySql(ServerVersion.AutoDetect("Server=localhost;database=uow;uid=root;pwd=root1234;")))
                //.AddDbContext<BloggingContext>(opt => opt.UseInMemoryDatabase("UnitOfWork"))
                .AddUnitOfWork<BloggingContext>()
                .AddCustomRepository<Blog, CustomBlogRepository>();

            services.AddMvc();
        }
    }
}