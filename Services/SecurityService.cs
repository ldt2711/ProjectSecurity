using System.Text;

namespace ProjectSecurity.Services
{
    public class SecurityService
    {

        // ===== Encrypt =====
        public string CaesarEncrypt(string text, int key)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                result.Append((char)(c + key));
            }

            return result.ToString();
        }

        public string VigenereEncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            return result.ToString();
        }

        // ===== Decrypt =====
        public string CaesarDecrypt(string text, int key)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                result.Append((char)(c - key));
            }

            return result.ToString();
        }

        public string VigenereDecrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            return result.ToString();
        }
    }
}