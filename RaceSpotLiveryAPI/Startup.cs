using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Authentication.JwtBearer;

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
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Services;
using RaceSpotLiveryAPI.Authorizers;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RaceSpotLiveryAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //"http://localhost:4200", "http://racespot.media.s3-website.us-east-2.amazonaws.com/", "https://racespot.media"
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("https://racespot.media")
                .AllowAnyHeader()
                .AllowAnyMethod());
            });

            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IIracingService, IracingService>();
            services.AddControllers().AddNewtonsoftJson();

            services.AddAWSService<Amazon.S3.IAmazonS3>();

            services.AddDbContext<RaceSpotDBContext>(options => options.UseNpgsql(Configuration["RdsConnectionString"]));
            services.AddIdentity<ApplicationUser, IdentityRole>()
              .AddEntityFrameworkStores<RaceSpotDBContext>()
              .AddDefaultTokenProviders();

            services.AddAuthentication(x =>
            {
                //x.DefaultScheme = "JwtBearer";
                x.DefaultAuthenticateScheme = "JwtBearer";
                x.DefaultChallengeScheme = "JwtBearer";
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Facebook.AppId"];
                facebookOptions.AppSecret = Configuration["Facebook.AppSecret"];
            })
            .AddGoogle(options =>
            {
                options.ClientId = Configuration["Google.ClientId"];
                options.ClientSecret = Configuration["Google.ClientSecret"];
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication.SigningKey"])),

                    ValidateLifetime = true, //validate the expiration and not before values in the token
                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });

            services.AddTransient<IAuthorizationHandler, AdminAuthorizer>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GlobalAdmin", policy =>
                    policy.Requirements.Add(new AdminRequirement()));
            });
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

            app.UseCors("LocalDev");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
