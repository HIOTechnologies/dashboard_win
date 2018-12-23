using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HIO.Controls
{
    public class TSetupPageContentControl : ContentControl
    {
        static TSetupPageContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TSetupPageContentControl), new FrameworkPropertyMetadata(typeof(TSetupPageContentControl)));
        }
    }
}
