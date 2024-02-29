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
        private static string UPLOADED_FILE_PATH = $"{Directory.GetCurrentDirectory()}\\Uploaded";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.MapGet("/", async (context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot\\index.html");
            });

            app.Map("/Encrypt", FileUploadEncrypt);

            app.Map("/Decrypt", FileUploadDecrypt);

            app.Map("/Download", FileDownload);

            app.Run();
        }

        private static string DECRYPT_PATH = $"{Directory.GetCurrentDirectory()}\\Uploaded\\Decrypt\\";
        private static string ENCRYPT_PATH = $"{Directory.GetCurrentDirectory()}\\Uploaded\\Encrypt\\";

        private static void FileDownload(IApplicationBuilder builder)
        {
            //Creating archive and send them;
            builder.Run(async (context) =>
            {
                string FileName = context.Request.Query["name"];
                if (context.Request.Query["type"] == "true")
                {
                    await context.Response.SendFileAsync(ENCRYPT_PATH + FileName);
                }
                else
                {
                    await context.Response.SendFileAsync(DECRYPT_PATH + FileName);
                }
            });
        }
        private static void FileUploadDecrypt(IApplicationBuilder builder)
        {
            IFormFileCollection files = null;
            List<string> decryptedFilesPath = new List<string>();
            Crypto.cs.CryptoAES cryptoAES = new Crypto.cs.CryptoAES();

            builder.Use(async (context, next) =>
            {
                Console.Write($"block_2: {context.Request.Query["pass"]}");
                cryptoAES.NewIVAndKey(context.Request.Query["pass"], 16);
                files = context.Request.Form.Files;

                if (files != null)
                {
                    List<byte> DecryptedBytes = new List<byte>();
                    foreach (var file in files)
                    {
                        using (BinaryReader br = new BinaryReader(file.OpenReadStream()))
                        {
                            await Console.Out.WriteLineAsync("1");
                            while (br.BaseStream.Position != br.BaseStream.Length)
                            {
                                DecryptedBytes.Add(br.ReadByte());
                            }
                            await Console.Out.WriteLineAsync("2");
                        }

                        int length = DecryptedBytes.Count;
                        DecryptedBytes.AddRange(cryptoAES.Decrypt(DecryptedBytes.ToArray()));
                        DecryptedBytes.RemoveRange(0,length);

                        foreach (var item in DecryptedBytes)
                        {
                            await Console.Out.WriteLineAsync(item.ToString());
                        }

                        int index = DecryptedBytes.IndexOf(0);

                        Console.WriteLine("kmjokjoij"+index);
                        string ext = Encoding.UTF32.GetString(DecryptedBytes.GetRange(0, index).ToArray());
                        Console.WriteLine(ext);
                        string PathCurrentFile = $"{DECRYPT_PATH}\\{file.FileName.Split('.')[0]}.{ext}";
                        decryptedFilesPath.Add(PathCurrentFile);
                        using (StreamWriter fs = new StreamWriter(File.Create(PathCurrentFile)))
                        {
                            string data = Encoding.UTF8.GetString(DecryptedBytes.GetRange(index, DecryptedBytes.Count-index).ToArray());
                            await fs.WriteAsync(data);
                        }
                        DecryptedBytes.Clear();
                    }
                    await next.Invoke(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Files are missing.");
                }
                cryptoAES = null;
            });
            builder.Run(async (context) =>
            {
                context.Response.StatusCode = 200;
                foreach (var FilePath in decryptedFilesPath)
                {
                    await context.Response.WriteAsync(Path.GetFileName(FilePath));
                }
            });
        }
        private static void FileUploadEncrypt(IApplicationBuilder builder)
        {
            IFormFileCollection? files = null;
            List<string> EncryptedFilesPath = null;

            //creating files for writing and write into them the files extensions
            builder.Use(async (context, next) =>
            {
                Console.WriteLine("Encrypt:");
                Console.Write("block_1: ");
                files = context.Request.Form.Files;
                if (files != null)
                {
                    EncryptedFilesPath = new List<string>();
                    foreach (var item in files)
                    {
                        string filePath = $"{ENCRYPT_PATH}{item.FileName.Split('.')[0]}.bin";
                        EncryptedFilesPath.Add(filePath);
                    }
                    Console.WriteLine("+;");
                    await next.Invoke(context);
                    await Console.Out.WriteLineAsync($"Request on encrypt: {files.Count} files.");
                }
                else
                {
                    await context.Response.WriteAsync("Files are missing.");
                }
            });
            //encrypting content of files and moving this into created buffer 
            builder.Use(async (context, next) =>
            {

                Crypto.cs.CryptoAES cryptoAES = new Crypto.cs.CryptoAES();
                cryptoAES.NewIVAndKey(context.Request.Query["pass"], 16);

                Console.Write($"block_2: {context.Request.Query["pass"]}");
                for (int i = 0; i < files.Count; i++)
                {
                    string dataStr = files[i].FileName.Split('.')[1] + "\0";
                    using (StreamReader reader = new StreamReader(files[i].OpenReadStream()))
                    {
                        dataStr += await reader.ReadToEndAsync();
                    }
                    byte[] EtcryptedData = cryptoAES.Encrypt(Encoding.UTF8.GetBytes(dataStr));
                    using (BinaryWriter bw = new BinaryWriter(File.Open(EncryptedFilesPath[i], FileMode.Append)))
                    {
                        bw.Write(EtcryptedData);
                    }
                }
                Console.WriteLine("+;");
                await next.Invoke(context);
                cryptoAES = null;
            });
            //sending encrypted files
            builder.Run(async (context) =>
            {
                await Console.Out.WriteAsync("FileDownloing: block3");

                string ZipPath = $"{ENCRYPT_PATH}ReturnEncryptZip{new Random().Next(1, 200)}.zip";

                await Console.Out.WriteAsync($"Created new return Zip: {ZipPath}");

                using (FileStream ZipToOpen = new FileStream(ZipPath, FileMode.Create))
                {
                    using (ZipArchive ReturnArchive = new ZipArchive(ZipToOpen, ZipArchiveMode.Create))
                    {
                        foreach (var ResultFile in EncryptedFilesPath)
                        {
                            ReturnArchive.CreateEntryFromFile(ResultFile, Path.GetFileName(ResultFile));
                        }
                    }
                }
                await Console.Out.WriteLineAsync("+;");
                await context.Response.WriteAsync(Path.GetFileName(ZipPath));
            });
        }
    }
}