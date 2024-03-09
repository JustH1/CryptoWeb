using System.IO.Compression;

namespace CryptoWeb
{

    public static class FileHandler
    {
        public static string CreateZipAndGetResultFileName(ref List<string> EncryptedFilesPaths)
        {
            string ZipPath = $"{GlobalValue.ENCRYPT_PATH}Crypto{DateTime.Now.ToString()}.zip";

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
            return ZipPath;
        }
        public static void GarbageCollection(ref List<string> filesPath)
        {
            filesPath.ForEach(path =>
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            });
        }
    }
}
