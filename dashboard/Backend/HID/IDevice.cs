using System;

namespace Mighty.HID
{
    public interface IDevice : IDisposable
    {
        void Close();
        bool Open(IDeviceInfo dev);
        void Read(byte[] data);
        bool Write(byte[] data);
        bool CanRead { get; }
        bool CanWrite { get; }
    }
}