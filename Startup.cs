using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GroundHouse
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {            
            //env is an info object that "tells" us everything about environment
            //app is and object that allows us to do certain actions
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();//instance of object for serving default files system
            defaultFilesOptions.DefaultFileNames.Clear();//clears all previous settings
            defaultFilesOptions.DefaultFileNames.Add("foo.html");
            app.UseDefaultFiles(defaultFilesOptions);//allows to use default documents like default.html etc
            //must be registered before useStaticFiles()
            //there is a line that can replace above two middlewares by one (k12, k12 also is about customizing use kit)

            app.UseStaticFiles();//this middleware allows us to use static files and after
                                 //that reverses pipeline(if url was ment to retrieve an existing static file)

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Response");
            });

            #region not project
            //app.UseRouting();//probably some route roules for mvc

            ////for any /laskjdf it will show up "Hello World"
            //app.Use(async (context, next) =>//is some sort of terminal middleware
            //{
            //    logger.LogInformation("MW1: Incoming request");
            //    await next();
            //    logger.LogInformation("MW1: Outgoing response");
            //    //await context.Response.WriteAsync("Hello 1\n");
            //    //await next();//invoking next delegate to continue going through the pipeline
            //});

            //app.Use(async (context, next) =>//is some sort of terminal middleware
            //{
            //    logger.LogInformation("MW2: Incoming request");
            //    await next();
            //    logger.LogInformation("MW2: Outgoing response");               
            //});

            ////If to switch Run to Use in app.Run(); middleware will become not terminal
            ////Use takes two parameters(+ next)

            //app.Run(async (context) =>//is some sort of terminal middleware
            //{
            //    await context.Response.WriteAsync("Request handled and response produced");
            //    logger.LogInformation("Request handled and response produced");
            //});

            //app.UseEndpoints(endpoints =>//endpoint for request processing pipeline
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response
            //        .WriteAsync("Hello world");
            //    });
            //});
            #endregion notpoject
        }
    }
}
