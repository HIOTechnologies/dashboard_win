using System;
using System.Management;
using HIO.Backend;
namespace HIO
{
  

    class UsbNotification
    {
        ManagementScope scope;

        ManagementEventWatcher watcher;

        public void WatcherUSB() {
            scope = new ManagementScope("root\\CIMV2");

            scope.Options.EnablePrivileges = true;

            try

            {

                WqlEventQuery query = new WqlEventQuery();

                query.EventClassName = "__InstanceCreationEvent";

                query.WithinInterval = new TimeSpan(0, 0, 1);

                query.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";

                watcher = new ManagementEventWatcher(scope, query);

                watcher.EventArrived += new EventArrivedEventHandler(WaitForUSBChangeEvent);


                watcher.Start();

            }

            catch (ManagementException)

            {

            }
        }

        private void WaitForUSBChangeEvent(object sender, EventArrivedEventArgs e)
        {
            HIOStaticValues.EventCheckDevice.Set();
        }
    }
}
