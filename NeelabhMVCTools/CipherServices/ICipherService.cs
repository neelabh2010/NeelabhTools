namespace NeelabhMVCTools.CipherServices
{
    public interface ICipherService
    {
        string Encrypt(string input, string userTransKey = "");
        string Decrypt(string cipherText, string userTransKey = "");
        public string UserTransKey(bool IsDateType = false);
    }
}
