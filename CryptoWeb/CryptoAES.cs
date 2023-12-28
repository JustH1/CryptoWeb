using System.Security.Cryptography;
using System.Text;

namespace Crypto.cs
{

    interface ICryptoAES
    {
        //Return current IV
        public byte[] GetIV();
        //Return current Key
        public byte[] GetKey();

        //Generate new IV and Key
        public void NewIVAndKey(byte[] iv);
        //Clears object`s data
        public void Clear();

        //Encrypting data
        public byte[] Encrypt(byte[] data);

        //Decrypting data
        public byte[] Decrypt(byte[] data);
    }

    class CryptoAES : ICryptoAES
    {
        private AesCng AESObj;
        private byte[] IV;
        private byte[] Key;

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
            AESObj.GenerateIV();
            AESObj.GenerateKey();
        }

        
        public byte[] GetIV() { return AESObj.IV; }
        public byte[] GetKey() { return AESObj.Key; }


        public void NewIVAndKey(byte[] iv) { AESObj.GenerateIV(); AESObj.GenerateKey(); }
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


        public byte[] Encrypt(byte[] data) {return Encrypt(data);}
        public byte[] Decrypt(byte[] data) { return Decrypt(data); }

        private byte[] Encrypt(SymmetricAlgorithm sa, byte[] data)
        {
            try
            {
                ICryptoTransform cryptoTransform = sa.CreateEncryptor();
                byte[] outblock = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                return outblock;
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
                byte[] outblock = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                return outblock;
            }
            catch (Exception ex)
            {
                throw new CryptoException($"Encryption Error. Base Error:{ex.Message}") { ErrorCode = CryptoException.ErrorCodeEnum.EncryptionError };
            }
        }
    }   
}
