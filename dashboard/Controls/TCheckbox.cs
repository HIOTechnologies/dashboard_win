using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HIO.Controls
{
    public class TCheckbox : CheckBox
    {
        static TCheckbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TCheckbox), new FrameworkPropertyMetadata(typeof(TCheckbox)));
        }

    }
}
