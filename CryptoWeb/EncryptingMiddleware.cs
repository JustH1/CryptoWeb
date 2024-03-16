using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Text;

namespace CryptoWeb
{
    public class EncryptingMiddleware
    {
        private readonly RequestDelegate next;
        private ICrypto cryptoAES = new CryptoAES();

        public EncryptingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/Encrypt")
            {
                IFormFileCollection? files = null;
                List<string> EncryptedFilesPaths = null;
                try
                {
                    files = context.Request.Form.Files;

                    if (files != null)
                    {
                        EncryptedFilesPaths = new List<string>();
                        GetEncryptedFilesPaths(files, ref EncryptedFilesPaths);

                        cryptoAES.NewIVAndKey(context.Request.Query["pass"], 16);

                        CreateFileAndWriteData(files, EncryptedFilesPaths);
                        string ZipPath = FileHandler.CreateZipAndGetResultFileName(ref EncryptedFilesPaths, true);
                        await context.Response.WriteAsync(Path.GetFileName(ZipPath));
                    }
                    else
                    {
                        context.Response.StatusCode = 460;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 527;
                }
                FileHandler.GarbageCollection(ref EncryptedFilesPaths);
            }
            else
            {
                await next.Invoke(context);
            }
        }
        private void GetEncryptedFilesPaths(IFormFileCollection? files, ref List<string> resultPaths)
        {
            foreach (var item in files)
            {
                string filePath = $"{GlobalValue.ENCRYPT_PATH}{item.FileName.Split('.')[0]}.bin";
                resultPaths.Add(filePath);
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
    }
}
