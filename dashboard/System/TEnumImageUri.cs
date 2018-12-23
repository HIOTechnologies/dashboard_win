using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TEnumImageUriAttribute : Attribute
    {
        public TEnumImageUriAttribute(string uri)
        {
            ImageUri = uri;
        }
        public string ImageUri { get; set; }

        public override string ToString()
        {
            return ImageUri;
        }
        public static string GetImageUri(Enum @enum)
        {
            return @enum.GetType()
                .GetField(@enum.ToString())
                .GetCustomAttributes(typeof(TEnumImageUriAttribute), false)
                ?.OfType<TEnumImageUriAttribute>()
                .FirstOrDefault()
                ?.ImageUri;
        }
    }
}
