using System.Security.Cryptography;
using System.Text;

namespace CryptoWeb
{
    class CryptoAES : ICrypto
    {
        private AesCng AESObj;

        private class CryptoException : Exception
        {
            public enum ErrorCodeEnum
            {
                OK = 100,
                EncryptionError = 200,
                DecryptionError = 300,
                IncorrectFileFormat = 400
            }
            public ErrorCodeEnum ErrorCode;
            public CryptoException(string message) : base(message) { }
        }

        public CryptoAES()
        {
            AESObj = new AesCng();
        }


        public byte[] GetIV() { return AESObj.IV; }
        public byte[] GetKey() { return AESObj.Key; }

        public void NewIVAndKey(string passwd, int keySize)
        {
            byte[] salt1 = Encoding.UTF8.GetBytes(passwd);
            int myIterations = 1000;
            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(passwd, salt1, myIterations);
            byte[] bytes = k1.GetBytes(keySize);
            AESObj.Key = bytes;
            AESObj.IV = bytes;
        }

        public void Clear() { AESObj.Clear(); }

        /*public string ReadFromFile(string path)
          {
              BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open));
              List<byte> bytes = new List<byte>();

              while (br.BaseStream.Position < br.BaseStream.Length)
              {
                  bytes.Add(br.ReadByte());
              }
              br.Close();
              return Decrypt(AESObj, bytes.ToArray());
          }
          public void WriteToFile(string data, string path)
          {
              BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Append));
              byte[] buff = Encrypt(AESObj, data);
              bw.Write(buff);
              bw.Close();
          }*/


        public byte[] Encrypt(byte[] data) { return Encrypt(AESObj, data); }
        public byte[] Decrypt(byte[] data) { return Decrypt(AESObj, data); }

        private byte[] Encrypt(SymmetricAlgorithm sa, byte[] data)
        {

            try
            {
                ICryptoTransform cryptoTransform = sa.CreateEncryptor();
                byte[] outBlock = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                return outBlock;
            }
            catch (Exception ex)
            {
                throw new CryptoException($"Encryption Error. Base Error:{ex.Message}") { ErrorCode = CryptoException.ErrorCodeEnum.EncryptionError };
            }
        }
        private byte[] Decrypt(SymmetricAlgorithm sa, byte[] data)
        {
            try
            {
                ICryptoTransform cryptoTransform = sa.CreateDecryptor();                
                byte[] outBlock = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                return outBlock;
            }
            catch (Exception ex)
            {
                throw new CryptoException($"Encryption Error. Base Error:{ex.Message}");
            }
        }
    }
}
