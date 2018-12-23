using HIO.Controls;
using HIO.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace HIO.Setup
{
    public class TSetupPage6 : TSetupPageBase
    {
        public TSetupPage6(TSetupWizard parent, double progressPercent) : base(parent, progressPercent)
        {
            Commands.AddCommand("Apply", Apply, () => CanApply);
        }

        public string Email
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                if (SetValue(value))
                {
                    Commands.Update();
                }
            }
        }

        public bool CanApply
        {
            get
            {
                return !Email.IsNullOrEmpty();
            }
        }

       

        private void Apply()
        {
            string url = "https://www.gethio.com/php/checkmail.php"; // Just a sample url
            WebClient wc = new WebClient();

            wc.QueryString.Add("email", Email);

            var data = wc.UploadValues(url, "POST", wc.QueryString);

            Message = $@"Confirm your email address to access HIO's features.
A confirmation message was sent to {Email}.";
     
         
        }

    }
}
