using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders.Physical;

namespace Example1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                    .AddRazorRuntimeCompilation();

            services.AddControllersWithViews();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Use(FallbackMiddlewareHandler);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Fallback handler middleware that is fired for any requests that aren't processed.
        /// This ends up being either a 404 result or
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private async Task FallbackMiddlewareHandler(HttpContext context, Func<Task> next)
        {

            var path = context.Request.Path;
            if (!string.IsNullOrEmpty(path) &&  ( path.Value.StartsWith("/services") && (path.Value.Contains("/scripts/") || path.Value.Contains("/contents"))))
            {
                var file = Path.Combine(Environment.CurrentDirectory, "Pages"+ path.Value.Replace("/", @"\"));
                var fi = new FileInfo(file);
                if (fi.Exists)
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 200;
                    }

                    await context.Response.SendFileAsync(new PhysicalFileInfo(fi));
                    await context.Response.CompleteAsync();
                }
            }
            else
            {
                await next();
            }
        }
    }
}
