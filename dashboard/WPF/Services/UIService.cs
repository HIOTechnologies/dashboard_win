using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using HIO.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace HIO.WPF.Services
{
    public static class UIService
    {
        /// <summary>
        ///   A value indicating whether the UI is currently busy
        /// </summary>
        private static bool IsBusy;

        public static async Task Execute(Action action)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await SetBusyState(true);
                    action();
                }
                finally
                {
                    await SetBusyState(false);
                }
            });
        }

        /// <summary>
        /// Sets the busystate to busy or not busy.
        /// </summary>
        /// <param name="busy">if set to <c>true</c> the application is now busy.</param>
        private static async Task SetBusyState(bool busy)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Application.Current.Windows.OfType<TWindow>().ToList().ForEach(w =>
                {
                    w.IsBusy = busy;
                });

                if (busy != IsBusy)
                {
                    IsBusy = busy;
                    Mouse.OverrideCursor = busy ? Cursors.Wait : null;
                }
            }, DispatcherPriority.Send);
        }
    }
}
