using System.Text;

namespace CryptoWeb
{
    public class Program
    {
        private static string UPLOADED_FILE_PATH = $"{Directory.GetCurrentDirectory()}/Uploaded";
        private static Crypto.cs.CryptoAES cryptoAES = new Crypto.cs.CryptoAES();

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

        private static void FileUploadDecrypt(IApplicationBuilder builder)
        {
            IFormFileCollection files = null;
            List<string> DecryptedFilesPath = new List<string>();

            builder.Use(async (context, next) =>
            {
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
                            DecryptedBytes.AddRange(await cryptoAES.Decrypt(DecryptedBytes.ToArray()));

                            int index = DecryptedBytes.IndexOf(0);
                            byte[] extantionByte = new byte[index];
                            Array.Copy(DecryptedBytes.ToArray(), extantionByte, index);
                            string extantionFile = Encoding.UTF8.GetString(extantionByte);

                            string extantionCurrentFile = $"{UPLOADED_FILE_PATH}/Encrypt/{file.FileName.Split('.')[0]}.{extantionFile}";
                            DecryptedFilesPath.Add(extantionCurrentFile);
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
            });
            builder.Run(async (context) =>
            {
                context.Response.StatusCode = 200;
                foreach (var FilePath in DecryptedFilesPath)
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
                    foreach (var file in EncryptedFilesPath)
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    await context.Response.WriteAsync("Files are missing.");
                }
            });
            //encrypting content of files and moving this into created buffer 
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
            //sending encrypted files
            builder.Run(async (context) =>
            {
                context.Response.StatusCode = 200;
                foreach (var filePath in EncryptedFilesPath)
                {
                    await context.Response.SendFileAsync(filePath);
                }
            });
        }
    }
}