using Microsoft.AspNetCore.Http;

namespace CryptoWeb
{
    public class SendingFileMiddlewere
    {
        private readonly RequestDelegate next;

        public SendingFileMiddlewere(RequestDelegate next)
        {
            this.next = next;
        }
        public async void InvokeAsync(HttpContext context)
        {
            await next.Invoke(context);

            try
            {
                if (context.Items.TryGetValue("ZipFilePath", out var ZipFilePath))
                {
                    string ResultFilePath = ZipFilePath as string ?? "";

                    if (File.Exists(ResultFilePath)) { await context.Response.SendFileAsync(ResultFilePath); }
                    else context.Response.StatusCode = 529;
                }
                else
                {
                    context.Response.StatusCode = 528;
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = 530;      
            }
        }
    }
}
