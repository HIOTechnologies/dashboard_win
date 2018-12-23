using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace HIO.Backend
{
    class Backup
    {
        public List<LoginFieldS> LoadBackup() {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            string jsonString = "";

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                return ParsJson(filename);
            }
            return null;

        }

        private List<LoginFieldS> ParsJson(string filename)
        {
            string tempLine;

            List<LoginFieldS> lp = new List<LoginFieldS>();
            var json_serializer = new JavaScriptSerializer();
            using (StreamReader file = new System.IO.StreamReader(filename))
            {
               
                while ((tempLine = file.ReadLine()) != null)
                {
                    try
                    {
                        if (tempLine == "") continue;
                        var dataValue = (IDictionary<string, object>)json_serializer.DeserializeObject(tempLine);
                        string url = HIOStaticValues.getTitleNameURI(dataValue["url"].ToString()).GetUTF8String(256);
                        string username = dataValue["username"].ToString().GetUTF8String(64);
                        string password = dataValue["password"].ToString().GetUTF8String(64);
                        string title = HIOStaticValues.getTitleNameURI(dataValue["title"].ToString()).GetUTF8String(64);
                        string appid = dataValue["appid"].ToString().GetUTF8String(64);
                        string last_used = dataValue["last_used"].ToString().GetUTF8String(64);
                        int counter = int.Parse(dataValue["counter"].ToString());

                        lp.Add(new LoginFieldS { url = url, userName = username, password = password, title = title, appID = title, last_used = last_used, popularity = counter });
                    }
                    catch {
                        continue;
                    }
                }

            }
            return lp;
            

        }
    }
}
