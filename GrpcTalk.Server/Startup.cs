using Bogus;
using GrpcTalk.Server.Database;
using GrpcTalk.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace GrpcTalk.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            services.AddScoped<IRepository, Repository>();
            services.AddSingleton(GetDefaultData);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TalkServiceImpl>();
            });
        }

        private IList<UserData> GetDefaultData(IServiceProvider serviceProvider)
        {
            var userId = 1;
            return new Faker<UserData>()
                .RuleFor(u => u.Id, f => userId++)
                .RuleFor(u => u.Name, f => f.Name.FirstName())
                .RuleFor(u => u.Emails, f => f.Make(3, () => f.Internet.Email()))
                .Generate(100);
        }
    }
}
