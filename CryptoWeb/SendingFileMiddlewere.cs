using Microsoft.AspNetCore.Http;
using System.IO;

namespace CryptoWeb
{
    public class SendingFileMiddleware
    {
        private readonly RequestDelegate next;
        ILogger logger;
        public SendingFileMiddleware(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/Download")
            {
                string path = context.Request.Query["type"] == "true" ? GlobalValue.ENCRYPT_PATH : GlobalValue.DECRYPT_PATH;
                path += context.Request.Query["name"];
                if (File.Exists(path))
                {
                    logger.LogInformation($"The file has been sent: {path}");
                    await context.Response.SendFileAsync(path);
                }
                else
                {
                    context.Response.StatusCode = 529;
                }
            } 
            else
            {
                await next.Invoke(context);
            }
        }
    }
}
