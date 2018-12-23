using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.HID
{
    public interface IDeviceInfo
    {
        string Path { get; }
        //TODO: rename
        string UsagePage { get; }
    }
}
