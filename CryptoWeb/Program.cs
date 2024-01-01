using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace CryptoWeb
{
    public class Program
    {
        static string UPLOADED_FILE_PATH = $"{Directory.GetCurrentDirectory()}/Uploaded";
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", async (context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("root/index.html");
            });

            app.UseWhen(context => context.Request.Path == "/upload" && context.Request.Method == "POST" &&
            context.Request.Query["type"] == "encryption", FileUploadEncrypt);

            app.UseWhen(context => context.Request.Path == "/upload" && context.Request.Method == "POST" &&
            context.Request.Query["type"] == "decryption", FileUploadDecrypt);

            app.Run();
        }


        private static void FileUploadEncrypt(IApplicationBuilder builder)
        {
            IFormFileCollection? files = null;
            List<string> EncryptedFilesPath = null;
            Crypto.cs.CryptoAES cryptoAES = new Crypto.cs.CryptoAES();
            
            //создание файлов для записи
            builder.Use(async (context, next) =>
            {
                files = context.Request.Form.Files;
                if (files != null)
                {
                    EncryptedFilesPath = new List<string>();
                    foreach (var item in files)
                    {
                        string filePath = $"{UPLOADED_FILE_PATH}/Encrypt/{item.FileName.Split('.')[0]}.bin";
                        using (BinaryWriter bw = new BinaryWriter(File.Create(filePath)))
                        {
                            bw.Write(Encoding.UTF8.GetBytes(Path.GetExtension(item.FileName) + '\0'));
                            EncryptedFilesPath.Add(filePath);
                        }
                    }
                    await next.Invoke(context);
                    await Console.Out.WriteLineAsync($"Request on encrypt: {files.Count} files.");
                }
                else
                {
                    await context.Response.WriteAsync("Files are missing.");
                }
            });

            builder.Use(async (context, next) => 
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string dataStr = String.Empty;
                    using (StreamReader reader = new StreamReader(files[i].OpenReadStream()))
                    {
                        dataStr = await reader.ReadToEndAsync();
                    }

                    byte[] EtcryptedData = await cryptoAES.Encrypt(Encoding.UTF8.GetBytes(dataStr));

                    using (BinaryWriter bw = new BinaryWriter(File.Open(EncryptedFilesPath[i], FileMode.Append)))
                    {
                        bw.Write(EtcryptedData);
                    }
                    await next.Invoke(context);
                }
            });
        }





        private static void FileUpload(IApplicationBuilder builder)
        {
            IFormFileCollection? files = null;
            bool type = true; //true - encryption, false - decryption

            builder.Use(async (context, next) =>
            {
                files = context.Request.Form.Files;

                if (files != null)
                {
                    if (context.Request.Query["type"] == "encryption")
                    {
                        type = true;
                    }
                    else if (context.Request.Query["type"] == "decryption")
                    {
                        type = false;
                    }
                    await next(context);
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} : Null files passed.");
                }
            });
            builder.Use(async (context, next) =>
            {
                Crypto.cs.CryptoAES cryptoAES = new Crypto.cs.CryptoAES();
                IFormFileCollection files = context.Request.Form.Files;
                foreach (var file in files)
                {
                    using (StreamReader stream = new StreamReader(file.OpenReadStream()))
                    {
                        string pathResultFile = $"{UPLOADED_FILE_PATH}/{file.FileName}.bin";
                        string dataStr = await stream.ReadToEndAsync();
                        byte[] bytes = Encoding.UTF8.GetBytes(dataStr);
                        File.Create(pathResultFile);

                        if (type)
                        {
                            bytes = await cryptoAES.Encrypt(bytes);
                        }
                        else
                        {
                            bytes = await cryptoAES.Encrypt(bytes);
                        }

                        using (BinaryWriter bw = new BinaryWriter(File.Open(pathResultFile, FileMode.Append)))
                        {
                            new Task(() =>
                            {
                                bw.Write(bytes);
                            }).Wait();
                        }
                    }
                }
            });

        }


    }
}