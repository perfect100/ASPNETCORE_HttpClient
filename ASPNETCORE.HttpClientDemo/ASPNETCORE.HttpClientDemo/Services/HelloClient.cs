using System.Threading.Tasks;
using Refit;

namespace ASPNETCORE.HttpClientDemo.Services
{
    public interface IHelloClient
    {
        [Get("/helloworld")]
        Task<Reply> GetMessageAsync();
    }

    public class Reply
    {
        public string Message { get; set; }
    }
}