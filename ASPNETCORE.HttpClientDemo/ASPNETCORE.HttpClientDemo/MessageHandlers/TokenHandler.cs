using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace ASPNETCORE.HttpClientDemo.MessageHandlers
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly ILogger<TokenHandler> logger;

        public TokenHandler(ILogger<TokenHandler> logger)
        {
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // for .Net 5, please use 'Options' instead
            // var properties = request.Options;
            var properties = request.Properties;
            properties.Add("State", "Token");
            this.logger.LogInformation($"{nameof(TokenHandler)} start...");
            var token = "token";
            request.SetBearerToken(token);
            var response = base.SendAsync(request, cancellationToken);
            this.logger.LogInformation($"{nameof(TokenHandler)} end...");

            return response;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}