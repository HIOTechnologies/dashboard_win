using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.Backend
{
    public class EncodingForBase64
    {
        public string Base64Encode(string plainText)
        {
            if (plainText == "" || plainText == null || plainText.Length == 0)
                return "";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            if (base64EncodedData == "" || base64EncodedData == null || base64EncodedData.Length == 0)
                return "";
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.UnicodeEncoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
