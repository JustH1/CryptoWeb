using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Text;

namespace CryptoWeb
{
    public class EncryptingMiddleware
    {
        private readonly RequestDelegate next;
        private CryptoAES cryptoAES = new CryptoAES();
        private static string ENCRYPT_PATH = $"{Directory.GetCurrentDirectory()}\\Uploaded\\Encrypt\\";

        public EncryptingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async void InvokeAsync(HttpContext context) 
        {
            IFormFileCollection? files = null;
            List<string> EncryptedFilesPaths = null;

            files = context.Request.Form.Files;

            if (files != null)
            {
                EncryptedFilesPaths = new List<string>();

                GetEncryptedFilesPaths(files, ref EncryptedFilesPaths);

                cryptoAES.NewIVAndKey(context.Request.Query["pass"], 16);

                CreateFileAndWriteData(files, EncryptedFilesPaths);

                await context.Response.WriteAsync(CreateZipAndGetResultFileName(ref EncryptedFilesPaths));

                await next.Invoke(context);

                EncryptedFilesPaths.ForEach(file =>
                {
                    File.Delete(file);
                });
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Files are missing.");
            }
        }
        private void GetEncryptedFilesPaths(IFormFileCollection? files, ref List<string> resultPaths)
        {
            foreach (var item in files)
            {
                string filePath = $"{ENCRYPT_PATH}{item.FileName.Split('.')[0]}.bin";
                resultPaths.Append<string>(filePath);
            }
        }
        private async void CreateFileAndWriteData(IFormFileCollection? files, List<string> EncryptedFilesPaths)
        {
            for (int i = 0; i < files.Count; i++)
            {
                string dataStr = files[i].FileName.Split('.')[1] + "\0";
                using (StreamReader reader = new StreamReader(files[i].OpenReadStream()))
                {
                    dataStr += await reader.ReadToEndAsync();
                }

                byte[] EncryptedData = cryptoAES.Encrypt(Encoding.UTF8.GetBytes(dataStr));

                using (BinaryWriter bw = new BinaryWriter(File.Open(EncryptedFilesPaths[i], FileMode.Append)))
                {
                    bw.Write(EncryptedData);
                }
            }
        }
        private string CreateZipAndGetResultFileName(ref List<string> EncryptedFilesPaths)
        {
            string ZipPath = $"{ENCRYPT_PATH}ReturnEncryptZip{new Random().Next(1, 200)}.zip";

            using (FileStream ZipToOpen = new FileStream(ZipPath, FileMode.Create))
            {
                using (ZipArchive ReturnArchive = new ZipArchive(ZipToOpen, ZipArchiveMode.Create))
                {
                    EncryptedFilesPaths.ForEach(ResultFile =>
                    {
                        ReturnArchive.CreateEntryFromFile(ResultFile, Path.GetFileName(ResultFile));
                    });
                }
            }
            return Path.GetFileName(ZipPath);
        }
    }
}
