using System.Security.Cryptography;

namespace Info
{
    static class EasyMD5
    {
        // From https://stackoverflow.com/a/43647643/848627
        public static string GetMD5(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                var base64String = Convert.ToBase64String(hash);
                return base64String;
            }
        }
    }

}