namespace CryptoWeb
{
    /// <summary>
    /// An interface for further adding other types of encryption.
    /// </summary>
    public interface ICrypto
    {
        //Return current IV
        public byte[] GetIV();
        //Return current Key
        public byte[] GetKey();

        //Generate new IV and Key
        public void NewIVAndKey(string passwd, int keySize);
        //Clears object`s data
        public void Clear();

        //Encrypting data
        public byte[] Encrypt(byte[] data);

        //Decrypting data
        public byte[] Decrypt(byte[] data);
    }
}
