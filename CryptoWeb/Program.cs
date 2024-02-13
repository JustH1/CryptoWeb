using System;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

            app.UseWhen(context => context.Request.Path == "/Encrypt" && context.Request.Method == "POST", FileUploadEncrypt);

            app.UseWhen(context => context.Request.Path == "/Decrypt" && context.Request.Method == "POST", FileUploadDecrypt);

            app.UseWhen(context => context.Request.Path == "/download" && context.Request.Method == "GET", FileDownload);

            app.Run();
        }

        private static string DECRYPT_PATH = $"{Directory.GetCurrentDirectory()}\\Uploaded\\Decrypt\\";
        private static string ENCRYPT_PATH = $"{Directory.GetCurrentDirectory()}\\Uploaded\\Encrypt\\";
        private static void FileDownload(IApplicationBuilder builder)
        {
            //string url = $"/download?type=encrypt&array={Uri.EscapeDataString(serializedArray)}";

            IEnumerable<string> ResultPaths = new List<string>();

            builder.Use(async (context, next) =>
            {
                string serializedArray = Uri.UnescapeDataString(context.Request.Path.ToString().TrimStart('&').Split('=')[1]);

                string[] deserializedArray = JsonSerializer.Deserialize<string[]>(serializedArray);

                foreach (var item in deserializedArray)
                {
                    Console.WriteLine(item);
                }

                if (context.Request.Query["type"] == "Encrypt")
                {
                    foreach (var FileName in deserializedArray)
                    {
                        ResultPaths.Append(ENCRYPT_PATH + FileName);
                    }
                }
                else
                {
                    foreach (var FileName in deserializedArray)
                    {
                        ResultPaths.Append(DECRYPT_PATH + FileName);
                    }
                }
                await next.Invoke();
            });

            builder.Run(async(context) =>
            {
                string ZipPath = $"{Directory.GetCurrentDirectory()}\\Uploaded\\returnZip{new Random().Next(1,20)}";

                await Console.Out.WriteLineAsync($"Created new return Zip: {ZipPath}");

                using (FileStream ZipToOpen = new FileStream(ZipPath, FileMode.Create))
                {
                    using (ZipArchive ReturnArchive = new ZipArchive(ZipToOpen, ZipArchiveMode.Create))
                    {
                        foreach (var ResultFile in ResultPaths)
                        {
                            ReturnArchive.CreateEntryFromFile(ResultFile, Path.GetFileName(ResultFile));
                        }
                        context.Response.Headers.Add("Content-Disposition", $"attachment; filename={ZipPath}");
                        await context.Response.SendFileAsync(ZipPath);
                        File.Delete(ZipPath);
                    }
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
                cryptoAES.NewIVAndKey(context.Request.Query["pass"].ToString(),16);
                files = context.Request.Form.Files;
                if (files != null)
                {
                    List<byte> DecryptedBytes = new List<byte>();
                    foreach (var file in files)
                    {
                        using (BinaryReader br = new BinaryReader(file.OpenReadStream()))
                        {
                            while (br.BaseStream.Position != br.BaseStream.Length)
                            {
                                DecryptedBytes.Add(br.ReadByte());
                            }
                            DecryptedBytes.AddRange(cryptoAES.Decrypt(DecryptedBytes.ToArray()));

                            int index = DecryptedBytes.IndexOf(0);
                            byte[] extantionByte = new byte[index];
                            Array.Copy(DecryptedBytes.ToArray(), extantionByte, index);
                            string extantionFile = Encoding.UTF8.GetString(extantionByte);

                            string extantionCurrentFile = $"{UPLOADED_FILE_PATH}/Encrypt/{file.FileName.Split('.')[0]}.{extantionFile}";
                            decryptedFilesPath.Add(extantionCurrentFile);
                            using (StreamWriter fs = new StreamWriter(File.Create(extantionCurrentFile)))
                            {
                                string data = Encoding.UTF8.GetString(DecryptedBytes.ToArray());
                                await fs.WriteAsync(data);
                            }
                            DecryptedBytes.Clear();
                        }
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
                    await context.Response.SendFileAsync(FilePath);
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
                        string filePath = $"{UPLOADED_FILE_PATH}\\Encrypt\\{item.FileName.Split('.')[0]}.bin";
                        using (BinaryWriter bw = new BinaryWriter(File.Create(filePath)))
                        {
                            bw.Write(Encoding.UTF8.GetBytes(Path.GetExtension(item.FileName) + '\0'));
                            EncryptedFilesPath.Add(filePath);
                        }
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
                    string dataStr = String.Empty;
                    using (StreamReader reader = new StreamReader(files[i].OpenReadStream()))
                    {
                        dataStr = await reader.ReadToEndAsync();
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
                Console.Write("block_3: ");
                List<string> filesName = new List<string>();
                foreach (var filePath in EncryptedFilesPath)
                {
                    filesName.Add(Path.GetFileName(filePath));
                }
                string serializedArray = JsonSerializer.Serialize(filesName.ToArray());
                string url = $"/download?type=encrypt&array={Uri.EscapeDataString(serializedArray)}";
                context.Response.Redirect(url);
                Console.WriteLine("+;");
            });
        }
    }
}