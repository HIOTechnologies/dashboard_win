using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HIO.Backend.IconURL;
using HIO.ViewModels.Settings.NewDeviceAdding;
namespace HIO.Backend
{
    class Converts
    {
        public  byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    
     public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public Image byteArrayToImage(byte[] byteArrayIn)
        {

            using (var ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        public DrawingImage BitmapImageToDrawingImage(Image url)
        {

            double widthImage = 0;
            double heightImage = 0;
            var bitmapImage = new BitmapImage();
            using (var ms = new MemoryStream())
            {
                url.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);


                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
            }

            var visual = new DrawingVisual();

            widthImage = bitmapImage.Width;

            heightImage = bitmapImage.Width;
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapImage, new Rect(0, 0, widthImage, heightImage));
            }
            var image = new DrawingImage(visual.Drawing);
            return image;

        }
        public byte[] StringToByteArray(string p)
        {
            try
            {
                return Encoding.UTF8.GetBytes(p);
            }
            catch (Exception ex)
            {
               
                return new byte[0x0];
            }
        }
        public string GetPrettyDate(string dateInput)
        {
            if (dateInput == null || dateInput == "")
            {
                return "a long time ago";
            }
            DateTime d = DateTime.ParseExact(dateInput, "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
            // 1.
            // Get time span elapsed since the date.
            TimeSpan s = DateTime.Now.Subtract(d);

            // 2.
            // Get total number of days elapsed.
            int dayDiff = (int)s.TotalDays;

            // 3.
            // Get total number of seconds elapsed.
            int secDiff = (int)s.TotalSeconds;

            // 4.
            // Don't allow out of range values.
            if (dayDiff < 0 || dayDiff >= 31)
            {
                return null;
            }

            // 5.
            // Handle same-day times.
            if (dayDiff == 0)
            {
                // A.
                // Less than one minute ago.
                if (secDiff < 60)
                {
                    return "just now";
                }
                // B.
                // Less than 2 minutes ago.
                if (secDiff < 120)
                {
                    return "1 minute ago";
                }
                // C.
                // Less than one hour ago.
                if (secDiff < 3600)
                {
                    return string.Format("{0} minutes ago",
                        Math.Floor((double)secDiff / 60));
                }
                // D.
                // Less than 2 hours ago.
                if (secDiff < 7200)
                {
                    return "1 hour ago";
                }
                // E.
                // Less than one day ago.
                if (secDiff < 86400)
                {
                    return string.Format("{0} hours ago",
                        Math.Floor((double)secDiff / 3600));
                }
            }
            // 6.
            // Handle previous days.
            if (dayDiff == 1)
            {
                return "yesterday";
            }
            if (dayDiff < 7)
            {
                return string.Format("{0} days ago",
                    dayDiff);
            }
            if (dayDiff < 31)
            {
                return string.Format("{0} weeks ago",
                    Math.Ceiling((double)dayDiff / 7));
            }
            return null;
        }
        public int RSSItoSignal(byte rssi)
        {
            int signal = 256 - Convert.ToInt32(rssi);
            if (signal > 75)
                return 1;
            if (signal <= 75 && signal > 50)
                return 2;
            if (signal <= 50)
                return 3;

            else return 0;
        }
        public string ByteToChar(byte ba)
        {
            StringBuilder hex = new StringBuilder(2);
            hex.AppendFormat("{0:x2}", ba);
            return hex.ToString();
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }
        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.ToUpper();
            if (hex.Length % 2 == 1)
            {
                hex = '0' + hex;
            }
            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public byte[] StringNumbertoByteArray(string id)
        {
            int rowidInt = Int32.Parse(id);
            byte[] rowidByteArray = BitConverter.GetBytes(rowidInt).ToArray();
            return rowidByteArray;
        }
    }
}
