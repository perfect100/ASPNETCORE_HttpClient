using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ASPNETCORE.HttpClientDemo.MessageHandlers
{
    public class MyMessageHandler : DelegatingHandler
    {
        private readonly ILogger<MyMessageHandler> logger;
        public MyMessageHandler(ILogger<MyMessageHandler> logger)
        {
            this.logger = logger;
        }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var properties = request.Properties;
            this.logger.LogInformation($"{nameof(MyMessageHandler)} start...");
            var response = base.SendAsync(request, cancellationToken);
            this.logger.LogInformation($"{nameof(MyMessageHandler)} end...");
            return response;
        }
    }
}