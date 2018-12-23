using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.HID
{
    public class HIDInfo : IDeviceInfo
    {
        /* device path */
        public string Path { get; private set; }
        /* vendor ID */
        public short Vid { get; private set; }
        /* product id */
        public short Pid { get; private set; }
        /* usb product string */
        public string Product { get; private set; }
        /* usb manufacturer string */
        public string Manufacturer { get; private set; }
        /* usb serial number string */
        public string SerialNumber { get; private set; }
        /* usagepage string */
        public string UsagePage { get; private set; }
        /* constructor */
        public HIDInfo(string product, string serial, string manufacturer,
            string path, short vid, short pid, string usagepage)
        {
            /* copy information */
            Product = product;
            SerialNumber = serial;
            Manufacturer = manufacturer;
            Path = path;
            Vid = vid;
            Pid = pid;
            UsagePage = usagepage;
        }
    }

    public class BLEDeviceInfo : IDeviceInfo
    {
        public BLEDeviceInfo(string name, string path, string usagePage = "0C1E")
        {
            Path = path;
            UsagePage = usagePage;
            Name = name.PadRight(32, '\0');
        }
        public string Path { get; private set; }

        public string UsagePage
        {
            get; private set;
        }

        public string Name { get; set; }
    }
}
