using HIO.Backend;
using HIO.Core;
using System.Diagnostics;
using System.Windows;

namespace HIO.Extentions
{
    public class TExtention06a : TViewModelBase, IExtention
    {
        public TExtention06a()
        {
   
        }
        private TExtention06aView _Form;

        public bool IsFormOpen
        {
            get
            {
                return _Form != null;
            }
        }
        public Window Form
        {
            get
            {
                return _Form;
            }

        }
        public bool IsClosed { get; private set; }
        public bool Show()
        {
          
                IsClosed = false;
                _Form = new TExtention06aView();
                _Form.DataContext = this;
                _Form.Closing += _Form_Closing;
                return _Form.ShowDialog() ?? false;
            
        }


        private void _Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosed = true;
         
        }

        public void Close() {
            try
            {
                _Form.DialogResult = true;
                if (!IsClosed)
                    _Form?.Close();
                _Form = null;
            }
            catch  {
             
            }
        }


        
    }
    
}
