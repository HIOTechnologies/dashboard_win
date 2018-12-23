using HIO.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HIO.Backend
{
    class Pipe
    {
        public void Server(TMain main)
        {
            do
            {
                try
                {
                    using (NamedPipeServerStream pipeServer =
                    new NamedPipeServerStream("CheckRunApp", PipeDirection.Out))
                    {

                        pipeServer.WaitForConnection();
                        try
                        {

                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => { main.Show(); }));
                        }
                        // Catch the IOException that is raised if the pipe is broken
                        // or disconnected.
                        catch (IOException e)
                        {

                        }

                    }
                }
                catch (Exception ex) {
                    if (ex.HResult == -2147024665)
                        Thread.Sleep(1000);
                        continue;
                }
            } while (true);
        }


        public bool Client()
        {
            try
            {
                using (NamedPipeClientStream pipeClient =
                    new NamedPipeClientStream(".", "CheckRunApp", PipeDirection.In))
                {

                    // Connect to the pipe or wait until the pipe is available.

                    pipeClient.Connect(500);


                    return true;
                }

            }
            catch (Exception ex) { return false; }
        }



    }
}
