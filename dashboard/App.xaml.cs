using HIO.Backend;
using HIO.Controls;
using HIO.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HIO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        DataBase db = new DataBase();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

            try
            {


                string appName;
                try
                {
                    appName = System.Windows.Application.ResourceAssembly.GetName().Name.ToString();
                }
                catch 
                {
                    appName = "HIO";
                }


                Pipe p = new Pipe();
                if (p.Client()) {
                    Shutdown();
                    return ;
                }

                TMain.Instance = new TMain();
                Task.Run(() => { p.Server(TMain.Instance); });
                Task.Run(() =>
                {
                    UsbNotification un = new UsbNotification();
                    un.WatcherUSB();
                });

                HIOStaticValues.InitializeNotifyIcon(TMain.Instance);

                DispatcherTimer dtIcon = new DispatcherTimer();
                dtIcon.Interval = TimeSpan.FromSeconds(3);
                dtIcon.Tick += DtIcon_Tick;
                dtIcon.Start();
                if(e.Args.Length>0 && e.Args[0]=="silent")
                TMain.Instance.Start(true);
                else TMain.Instance.Start(false);


            }
            finally
            {
                //Shutdown();
            }
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            try { Trace.TraceError(GetExceptionMessage(e.Exception)); }
            catch { }
        }

        private static string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
                return null;
            if (exception is AggregateException aex)
                return exception.ToString() + Environment.NewLine + string.Join(Environment.NewLine, aex.Flatten().InnerExceptions.Select(ex => GetExceptionMessage(ex)));
            else
                return exception.ToString() + Environment.NewLine + GetExceptionMessage(exception.InnerException);
        }

        private void DtIcon_Tick(object sender, EventArgs e)
        {
            try
            {
                (sender as DispatcherTimer).Stop();
                if (HIOStaticValues.SYNC_ON == false && HIOStaticValues.IMPORT_ON == false && HIOStaticValues.commandQ.IsEmpty && HIOStaticValues.CONNECTIONBHIO && HIOStaticValues.CounterTimerIcon<3)
                {
                    HIOStaticValues.CounterTimerIcon++; 
                    DataBase db = new DataBase();
                    var urls = db.GetListUrlsWithoutIcon();
                    var dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Interval = TimeSpan.FromSeconds(3);
                    dispatcherTimer.Tick += (a, b) => { FillIconTickAsync(a, b, urls); };
                    dispatcherTimer.Start();


                }
            
            }
            catch (Exception ex)
            {
               

            }
            finally {
                (sender as DispatcherTimer).Start();


            }
        }

        private async void FillIconTickAsync(object sender, EventArgs e, List<string> urls)
        {
            (sender as DispatcherTimer).Stop();
            if (HIOStaticValues.SYNC_ON == false && HIOStaticValues.IMPORT_ON == false && HIOStaticValues.CONNECTIONBHIO && HIOStaticValues.commandQ.IsEmpty)
            {
                var url = urls?.LastOrDefault();
                if (url != null)
                {
                    urls.Remove(url);
                   await Task.Run(new Action(()=> { db.FillIcon(url); }));
                }
              
            }
              (sender as DispatcherTimer).Start();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            TMessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }
    }
}
