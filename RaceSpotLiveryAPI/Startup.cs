using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaceSpotLiveryAPI.Contexts;
using RaceSpotLiveryAPI.Services;

namespace RaceSpotLiveryAPI
{
    public class Startup
    {
        public const string AppS3BucketKey = "AppS3Bucket";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IIracingService, IracingService>();
            services.AddControllers().AddNewtonsoftJson();

            services.AddAWSService<Amazon.S3.IAmazonS3>();

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Facebook.AppId"];
                facebookOptions.AppSecret = Configuration["Facebook:AppSecret"];
            });
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Google.ClientId"];
                    options.ClientSecret = Configuration["Google.ClientSecret"];
                });

            services.AddDbContext<RaceSpotDBContext>(options => options.UseNpgsql(Configuration["RdsConnectionString"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
