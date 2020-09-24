using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroundHouse.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            services.AddDbContextPool<AppDbContext>(options =>
                        options.UseSqlServer(_config.GetConnectionString("HouseDbConnection")));//adding db setting in services
            //services.AddDbContext<AppDbContext>();//differs by using the same instance of AppDbContext each time

            //for customizing password complexity
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;//for 8 characters required
                options.Password.RequiredUniqueChars = 2;//for 2 unique chars
            });//we somehow can do the same with addIdentity<IdentityUser, IdentityRole>(=>)

            //AddMvcCore fails sometimes
            services.AddMvc(options => options.EnableEndpointRouting = false);//adding mvc services to dependency injection container
            services.AddScoped<IHouseRepository, SQLHouseRepository>();//convenient instrument
            //singleton - one time per application lifetime
            //trancient - one time per time it is requested
            //scoped - one per request within the scope(one per each http but same within other requests(like ajax))

            services.AddIdentity<ApplicationUser, IdentityRole>()//for identity core
                    .AddEntityFrameworkStores<AppDbContext>();
            //asp.net core uses built-in IdentityUser class to manage the details of registered users

            //creating policy for claims based authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                     policy =>
                     {
                         policy.RequireClaim("Delete Role", "true");//as much as you want(or just chain another requireClaim)
                     });

                //options.AddPolicy("EditRolePolicy", // this works only if use claim types without values
                //    policy =>
                //    {
                //        policy.RequireClaim("Edit Role");
                //    });

                options.AddPolicy("EditRolePolicy",
                    policy =>
                    {
                        policy.RequireClaim("Edit Role", "true");
                    });

                options.AddPolicy("CreateRolePolicy",
                    policy =>
                    {
                        policy.RequireClaim("Create Role", "true");
                    });
            });

            //customizing /AccessDenied route
            //default is /Account/AccessDenied
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });             
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //env is an info object that "tells" us everything about environment
            //app is and object that allows us to do certain actions
            if (env.IsDevelopment())//as early as possible
            {
                //customizing
                DeveloperExceptionPageOptions developerExceptionPageOptions = new DeveloperExceptionPageOptions
                {
                    SourceCodeLineCount = 7//7 lines will be shown 
                };
                app.UseDeveloperExceptionPage(developerExceptionPageOptions);//middleware for exceptions(alternative
            }
            else if (env.IsStaging() || env.IsProduction() || env.IsEnvironment("XYZ"))//for yellow SOD in Framework)
            {
                app.UseExceptionHandler("/Error");//for global errors handling

                //app.UseStatusCodePages();//just shows status code
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");//400-500 range
                app.UseStatusCodePagesWithReExecute("/Error/{0}");//it somehow puts the status code in placeholder here
                //there is actually difference between ReExecute and Redirects
            }


            //app.UseDefaultFiles();//middleware for default files

            #region custom default file
            //FileServerOptions fileServerOptions = new FileServerOptions();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("foo.html");
            //app.UseFileServer(fileServerOptions);
            #endregion

            #region alternative
            //DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();//instance of object for serving default files system
            //defaultFilesOptions.DefaultFileNames.Clear();//clears all previous settings
            //defaultFilesOptions.DefaultFileNames.Add("foo.html");
            //app.UseDefaultFiles(defaultFilesOptions);//allows to use default documents like default.html etc
            //must be registered before useStaticFiles()
            //there is a line that can replace above two middlewares by one (k12, k12 also is about customizing use kit)
            #endregion
            //order is important
            app.UseStaticFiles();//this middleware allows us to use static files and after
                                 //that reverses pipeline(if url was ment to retrieve an existing static file)
                                 //app.UseMvcWithDefaultRoute();//adding mvc to pipeline

            app.UseAuthentication();//for user authentication
            //it is important to add it before useMvc()

            //app.UseMvc();//adds mvc without any routes
            //app.UseMvcWithDefaultRoute()
            //above and below are identical
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            #region tool for printing whenewer you want
            //app.Run(async (context) =>
            //{                
            //    await context.Response.WriteAsync("Hello world");
            //});
            #endregion

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
            #endregion
        }
    }
}
