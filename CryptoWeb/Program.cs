using System.Reflection.PortableExecutable;

namespace CryptoWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", async(context) => {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("root/index.html"); 
            });
            app.UseWhen(context => context.Request.Path == "/upload" && context.Request.Method == "POST", FileUpload);

            app.Run();
        }
        private static void FileUpload(IApplicationBuilder builder)
        {
            IFormFileCollection? files = null;
            bool type = true; //true - encryption, false - decryption

            builder.Use(async (context, next) =>
            {
                int fileCount = 0;
                files = context.Request.Form.Files;

                if (files != null)
                {
                    if (context.Request.Query["type"] == "encryption")
                    {
                        type = true;
                        await next(context);
                    }
                    else if (context.Request.Query["type"] == "decryption")
                    {
                        type = false;
                        await next(context);
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} : Null files passed");
                }
            });
            builder.Use(async (context, next) =>
            {
                IFormFileCollection files = context.Request.Form.Files;
                
            });

        }

    }
}