using System.IO.Compression;

namespace CryptoWeb
{

    public static class FileHandler
    {
        public static string CreateZipAndGetResultFileName(ref List<string> FilesPaths, bool type)
        {
            uint id = (uint)new Random().Next(1, 100);
            string ZipPath = (type == true) ? $"{GlobalValue.ENCRYPT_PATH}ResultFileArchive{id}.zip" : $"{GlobalValue.DECRYPT_PATH}ResultFileArchive{id}.zip";

            using (FileStream ZipToOpen = new FileStream(ZipPath, FileMode.Create))
            {
                using (ZipArchive ReturnArchive = new ZipArchive(ZipToOpen, ZipArchiveMode.Create))
                {
                    FilesPaths.ForEach(ResultFile =>
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
