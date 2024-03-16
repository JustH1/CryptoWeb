using System;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;

namespace CryptoWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<RouteMiddleware>(app.Logger);
            app.UseMiddleware<ErrorHandlingMiddleware>(app.Logger);
            app.UseMiddleware<SendingFileMiddleware>(app.Logger);
            app.UseMiddleware<EncryptingMiddleware>();
            app.UseMiddleware<DecryptingMiddleware>();
            app.Run();
        }

    }
}