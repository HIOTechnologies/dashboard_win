using System;
using System.IO;
using Microsoft.Win32.SafeHandles;
using HIO.Backend;
using System.Text;
using System.Diagnostics;

namespace Mighty.HID
{
    public class HIDDev : IDevice
    {
        /* device handle */
        private IntPtr handle;
        /* stream */
        private FileStream _fileStream;
        /* stream */

        private FileStream fileStream
        {
            get { return _fileStream; }
            /* do not expose this setter */
            set { _fileStream = value; }
        }
        /* dispose */
        public void Dispose()
        {
            try
            {

                /* deal with file stream */
                if (_fileStream != null)
                {
                    /* close stream */
                    _fileStream.Close();
                    /* get rid of object */
                    _fileStream = null;
                    /* close handle */
                    if (handle != IntPtr.Zero)
                        Native.CloseHandle(handle);
                }
            }
            catch (Exception ex)
            {

                handle = IntPtr.Zero;
            }
        }

        /* open hid device */
        public bool Open(IDeviceInfo dev)
        {
            /* safe file handle */
            try
            {
                SafeFileHandle sHandle;
                /* opens hid device file */

                handle = Native.CreateFile(dev.Path,
                   Native.GENERIC_WRITE | Native.GENERIC_READ,
                     Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                    IntPtr.Zero, Native.OPEN_EXISTING, Native.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);
                /* whops */
                if (handle == Native.INVALID_HANDLE_VALUE)
                {
                    return false;
                }

                /* build up safe file handle */
                sHandle = new SafeFileHandle(handle, false);
                /* prepare stream - async */
                _fileStream = new FileStream(sHandle, FileAccess.ReadWrite,
                32, true);

                /* report status */
                return true;
            }
            catch (Exception ex)
            {
              
                return false;
            }
        }

        /* close hid device */
        public void Close()
        {
            try
            {
                /* deal with file stream */
                if (_fileStream != null)
                {
                    /* close stream */
                    _fileStream.Close();
                    /* get rid of object */
                    _fileStream = null;
                }

                /* close handle */
                Native.CloseHandle(handle);
            }
            catch (Exception ex)
            {
            }
        }

        /* write record */
        public bool Write(byte[] data)
        {
            try
            {
                Trace.WriteLine("Send HID:\n" + Converts.ByteArrayToString(data));
                /* write some bytes */
                _fileStream?.Write(data, 0, data.Length);
                /* flush! */
                _fileStream?.Flush();

                return true;
            }
            catch (Exception ex)
            {

              
                return false;
            }

        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        /* read record */
        public void Read(byte[] data)
        {


            /* get number of bytes */
            int n = 0, bytes = data.Length;

            /* read buffer */
            while (n != bytes)
            {
                /* read data */
                int rc = _fileStream.Read(data, n, bytes - n);




                /* update pointers */
                n += rc;
            }



        }

        public bool CanRead
        {
            get
            {
                return _fileStream?.CanRead ?? false;
            }
        }

        public bool CanWrite
        {
            get
            {
                return _fileStream?.CanWrite ?? false;
            }
        }
    }
}
