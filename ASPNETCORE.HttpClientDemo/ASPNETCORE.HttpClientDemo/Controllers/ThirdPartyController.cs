using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCORE.HttpClientDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ThirdPartyController : ControllerBase
    {
        [HttpGet("data")]
        public async Task<IHeaderDictionary> Get()
        {
            await Task.Delay(100);
            return this.Request.Headers;
        }
    }
}