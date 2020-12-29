using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ASPNETCORE.HttpClientDemo.MessageHandlers;
using ASPNETCORE.HttpClientDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;

namespace ASPNETCORE.HttpClientDemo
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
            // 1.Basic usage
            services.AddHttpClient();
            
            // 2.Named clients
            services.AddHttpClient("github", client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            });
            
            // 3.Typed clients
            services.AddHttpClient<GitHubService>();
            services.AddHttpClient<GitHubService>(client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            });
            
            // 4.Generated clients
            services.AddRefitClient<IHelloClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://api.github.com/");
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
                });
            
            // 5.Outgoing request middleware
            services.AddTransient<TokenHandler>();
            services.AddTransient<MyMessageHandler>();
            services.AddHttpClient("middleware")
                // This handler is on the outside and called first during the 
                // request, last during the response.
                .AddHttpMessageHandler<TokenHandler>()
                // This handler is on the inside, closest to the request being 
                // sent.
                .AddHttpMessageHandler<MyMessageHandler>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    UseCookies = false,
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(0.2));
            
            // 6.Header propagation
            services.AddHeaderPropagation(options =>
            {
                options.Headers.Add("Token");
                options.Headers.Add("MyHeader1");
                options.Headers.Add("MyHeader2");
                options.Headers.Add("MyHeader3", "haha"); // Note order
                options.Headers.Add("MyHeader4", 
                    context => context.HeaderValue + "_end");
                options.Headers.Add("MyHeader5", 
                    context => "_end55555");
            });
            services.AddHttpClient("header-propagation")
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5000");
                }).AddHeaderPropagation();
            
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseHeaderPropagation();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}