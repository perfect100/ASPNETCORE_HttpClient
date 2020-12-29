using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using ASPNETCORE.HttpClientDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Refit;

namespace ASPNETCORE.HttpClientDemo.Controllers
{
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly IHttpClientFactory clientFactory;
        
        public DemoController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }
        
        [HttpGet("basic")]
        public async Task<string> GetBasicUsageHttpClient()
        {
            var client1 = this.clientFactory.CreateClient();
            var client2 = this.clientFactory.CreateClient();

            this.CheckMessageHandlerOfClients(client1, client2);
            
            await Task.Delay(100);
            return "Hi, there! Basic client";
        }
        
        [HttpGet("named")]
        public async Task<string> GetNamedHttpClient()
        {
            var client1 = this.clientFactory.CreateClient("github");
            var client2 = this.clientFactory.CreateClient("github1");
            
            this.CheckMessageHandlerOfClients(client1, client2);
            
            await Task.Delay(100);
            return "Hi, there! Named client";
        }
        
        [HttpGet("typed")]
        public async Task<string> GetTypedHttpClient([FromServices] GitHubService gitHubService)
        {
            // var issueses = gitHubService.GetAspNetDocsIssues();
            await Task.Delay(100);
            return "Hi, there! Typed client";
        }

        [HttpGet("generated")]
        public async Task<object> GetGeneratedClients([FromServices] IHelloClient helloClient)
        {
            try
            {
                return await helloClient.GetMessageAsync();
            }
            catch (ApiException e)
            {
                return e.Content;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        [HttpGet("middleware")]
        public async Task<string> GetMiddleware()
        {
            var client1 = this.clientFactory.CreateClient("middleware");
            var client2 = this.clientFactory.CreateClient("middleware");
            
            this.CheckMessageHandlerOfClients(client1, client2);
            
            return await client1.GetStringAsync("http://www.bing.com");
        }

        [HttpGet("header")]
        public async Task<IActionResult> GetHeaderPropagation()
        {
            var client = this.clientFactory.CreateClient("header-propagation");
            var content = await client.GetStringAsync("/thirdParty/data");
            return Content(content, "application/json");
        }

        [HttpGet("inner")]
        public void GetInnerInfo(
            [FromServices]IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor,
            [FromServices]IEnumerable<IHttpMessageHandlerBuilderFilter> filters,
            [FromServices]HttpMessageHandlerBuilder builder)
        {
            var name = "middleware";
            var options = optionsMonitor.Get(name);
            
            builder.Name = name;
            var _filters = filters.ToArray();
            // This is similar to the initialization pattern in:
            // https://github.com/aspnet/Hosting/blob/e892ed8bbdcd25a0dafc1850033398dc57f65fe1/src/Microsoft.AspNetCore.Hosting/Internal/WebHost.cs#L188
            Action<HttpMessageHandlerBuilder> configure = Configure;
            for (var i = _filters.Length - 1; i >= 0; i--)
            {
                configure = _filters[i].Configure(configure);
            }

            configure(builder);

            var handler = builder.Build();
            
            // Wrap the handler so we can ensure the inner handler outlives the outer handler.
            //var handler = new LifetimeTrackingHttpMessageHandler(builder.Build());

            // Note that we can't start the timer here. That would introduce a very very subtle race condition
            // with very short expiry times. We need to wait until we've actually handed out the handler once
            // to start the timer.
            // 
            // Otherwise it would be possible that we start the timer here, immediately expire it (very short
            // timer) and then dispose it without ever creating a client. That would be bad. It's unlikely
            // this would happen, but we want to be sure.
            
            // return new ActiveHandlerTrackingEntry(name, handler, scope, options.HandlerLifetime);

            void Configure(HttpMessageHandlerBuilder b)
            {
                for (var i = 0; i < options.HttpMessageHandlerBuilderActions.Count; i++)
                {
                    options.HttpMessageHandlerBuilderActions[i](b);
                }
            }
        }

        private void CheckMessageHandlerOfClients(HttpClient client1, HttpClient client2)
        {
            var field = typeof(HttpMessageInvoker).GetRuntimeFields().First(n=> n.Name == "_handler");

            var handler1 = field.GetValue(client1);
            var handler2 = field.GetValue(client2);
            Console.WriteLine($"Client1 equals to Client2 : {ReferenceEquals(client1, client2)}");
            Console.WriteLine($"Handler1 equals to Handler2 : {ReferenceEquals(handler1, handler2)}");
            Console.WriteLine($"Handler1 HashCode : {handler1!.GetHashCode()};");
            Console.WriteLine($"Handler2 HashCode : {handler2!.GetHashCode()};");
        }
    }
}