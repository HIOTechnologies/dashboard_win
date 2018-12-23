using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HIO.Controls
{
    /// <summary>
    /// Interaction logic for TMessageBoxView.xaml
    /// </summary>
    public partial class TMessageBoxView
    {
        public TMessageBoxView()
        {
            InitializeComponent();
        }
        public override bool canCloseonDC
        {
            get
            {
                return false;
            }

        }

        public MessageBoxResult Result
        {
            get { return (MessageBoxResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(MessageBoxResult), typeof(TMessageBoxView), new PropertyMetadata(MessageBoxResult.None));


    }
}
