using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eSyaPatientPortal.DL.Entities;
using eSyaPatientPortal.DL.Repository;
using eSyaPatientPortal.IF;
using eSyaPatientPortal.WebAPI.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eSyaPatientPortal.WebAPI
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
            services.AddDbContext<eSyaEnterpriseContext>(options => 
            options.UseSqlServer(Configuration.GetConnectionString("dbContext")));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(typeof(HttpAuthAttribute));
            //}).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services.AddScoped<IBusinessLocationRepository, BusinessLocationRepository>();
            services.AddScoped<IDoctorClinicRepository, DoctorClinicRepository>();
            services.AddScoped<IAppointmentBookingRepository, AppointmentBookingRepository>();
            services.AddScoped<IPatientInfoRepository, PatientInfoRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
