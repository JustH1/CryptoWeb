using System.Text;
using System.Text;

namespace CryptoWeb
{
    public class DecryptingMiddleware
    {
        private readonly RequestDelegate next;
        ICrypto CryptoAES = new CryptoAES();
        public DecryptingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/Decrypt")
            {
                List<string> DecryptedFilesPath = new List<string>();
                IFormFileCollection? files = null;

                try
                {
                    CryptoAES.NewIVAndKey(context.Request.Query["pass"], 16);
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
                            }

                            int length = DecryptedBytes.Count;

                            DecryptedBytes.AddRange(CryptoAES.Decrypt(DecryptedBytes.ToArray()));
                            DecryptedBytes.RemoveRange(0, length);

                            int index = DecryptedBytes.IndexOf(0);

                            string ext = Encoding.UTF8.GetString(DecryptedBytes.GetRange(0, index).ToArray());

                            string PathCurrentFile = $"{GlobalValue.DECRYPT_PATH}\\{file.FileName.Split('.')[0]}.{ext}";
                            DecryptedFilesPath.Add(PathCurrentFile);

                            using (StreamWriter fs = new StreamWriter(File.Create(PathCurrentFile)))
                            {
                                string data = Encoding.UTF8.GetString(DecryptedBytes.GetRange(index, DecryptedBytes.Count - index).ToArray());
                                await fs.WriteAsync(data);
                            }
                            DecryptedBytes.Clear();
                        }
                        string ZipPath = FileHandler.CreateZipAndGetResultFileName(ref DecryptedFilesPath, false);
                        await context.Response.WriteAsync(Path.GetFileName(ZipPath));
                    }
                    else
                    {
                        context.Response.StatusCode = 460;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 526;
                }
                FileHandler.GarbageCollection(ref DecryptedFilesPath);
            }
        }
    }
}
