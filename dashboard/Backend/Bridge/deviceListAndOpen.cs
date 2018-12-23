using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HIO.Backend;
using HIO.Extentions;
using HIO.ViewModels;
using HIO.ViewModels.Settings;

namespace HIO.Backend.Bridge
{
    class deviceListAndOpen
    {

        public void ConnectToBridge(TMain tMain)
        {
            try
            {
                while (true)
                {
                    Trace.WriteLine("B4 deviceListAndOpen");
                    bool ret = HIOStaticValues.BaS.deviceListAndOpen();
                    Trace.WriteLine($"After deviceListAndOpen: {ret}");
                    if (ret == true)
                    {

                        HIOStaticValues.CONNECTIONBRIDGE = true;
                        bool res = HIOStaticValues.BaS.CheckSwitchEvents(tMain);

                    }
                    else
                    {
                        //HIOStaticValues.EventCheckDevice.Reset();
                        HIOStaticValues.CONNECTIONBRIDGE = false;
                    }
                    Trace.WriteLine("Wait for listening device.");
                    HIOStaticValues.EventCheckDevice.Reset();  //wait for event
                    HIOStaticValues.EventCheckDevice.WaitOne(3000);  //wait for event
                                                                   //Thread.Sleep(500);
                                                                   //   HIOStaticValues.EventCheckDevice.Reset();  //wait for event
                    Trace.WriteLine("Open signal");
                }

            }
            catch (Exception ex)
            {
          
            }
        }

    }
}
