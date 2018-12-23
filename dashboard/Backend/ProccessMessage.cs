using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WindowsInput;
using HIO.Extentions;
using HIO.ViewModels;

using HIO.Core;
using HIO.ViewModels.Accounts;

namespace HIO.Backend
{
    class ProcessMessage
    {

        /// <summary>
        /// For Extensions 
        /// </summary>
        /// <param name="data">recieve data from browser</param>
        /// <param name="source">check refrence of data</param>
        /// <param name="tMain">parent handler</param>

        public void ProcessRecieveMessage(JObject data, Source source)
        {

            try
            {
                if (data == null)
                    return;
                Converts conv = new Converts();
                Dictionary<string, string> dicData = new Dictionary<string, string> ();
                string url = "", title = "", message = "", username = "";
                TCheckingData(data, ref url, ref title, ref message, ref username);
             
                DataBase db = new DataBase();
                if (HIOStaticValues.CONNECTIONBHIO == false)
                {
                    dicData.Add("CMD", "CONNCETION");
                    dicData.Add( "DATA", "false"  );
  

                    HIOStaticValues.BaS.Write(dicData, source);
                    if (message != "INIT" && message != "SUBMIT")
                    {
                        HIOStaticValues.popUp("HIO is not connected!");
                      
                    }
                    return;
                }
                if (message != "INIT" && !HIOStaticValues.TPinStatus()) 
                     return;

                
                if (HIOStaticValues.CheckSyncingData())
                    return;
                
                switch (message)
                {
                    case "exit":
                        //Environment.Exit(0);
                        break;
                    case "MENUCHROME":
                        List<LoginFieldS> listUsers = db.getInfoFromDB("*", "", "");
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            HIOStaticValues.AdminExtention.CloseAll();
                            HIOStaticValues.AdminExtention.ExtentionMenu.Initialize(url, title);
                            HIOStaticValues.AdminExtention.ExtentionMenu.Show();

                        }));

                        break;

                  
                    //case "DASHBOARD":
                    //    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        TMain.Instance.Show();
                    //    }));
                    //    break;
                    //case "MANAGEALLUSER":
                    //    List<LoginFieldS> lnFields = db.getInfoFromDB("*", "", "");
                    //    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //    {


                    //        HIOStaticValues.AdminExtention.CloseAll();
                    //        HIOStaticValues.AdminExtention.Extention08.Initialize(lnFields);
                    //        HIOStaticValues.AdminExtention.Extention08.Show();

                    //    }));

                    //    break;
                    //case "ADDUSER":
                        
                    //    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        HIOStaticValues.AdminExtention.CloseAll();
                    //        HIOStaticValues.AdminExtention.Extention02.Show(new TAccountItem { Url = url, Name = title });
                    //    }));
                    //    break;
                    case "READYDATA":
                        //ready for fill username
                        dicData.Add("CMD", "USER");
                        dicData.Add("USER", HIOStaticValues.username);
                        HIOStaticValues.BaS.Write(dicData, source, true);
                        break;
                    case "READYDATAPASS":
                        //ready for fill password
                        //  Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "USER" }, { "USER", HIOStaticValues.username } };
                        dicData.Add("CMD", "PASS");
                        dicData.Add("DATA", HIOStaticValues.password);
                        HIOStaticValues.BaS.Write(dicData, source, true);
                        break;
                    case "INIT":
                        dicData.Add("CMD", "CONNCETION");
                        dicData.Add("DATA", HIOStaticValues.CONNECTIONBHIO.ToString().ToLower());
                        HIOStaticValues.BaS.Write(dicData, source);
                        break;
                    case "MANAGEUSER":
                        break;
                    case "CANNOTFIND":
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                            HIOStaticValues.popUp("HIO could not find the login form." +
                                "\nMove the cursor inside the Username or Password field.");
                        }));
                        break;
                    //case "GENERATEPASSWORDNOFILL":


                    //    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //    {

                    //        try
                    //        {
                    //            HIOStaticValues.AdminExtention.CloseAll();
                    //            HIOStaticValues.AdminExtention.Extentiongp.Show(false,false);

                    //        }
                    //        catch (Exception ex)
                    //        {

                    //            ;
                    //        }
                    //    }));

                    //    break; 
                    case "GENERATEPASSWORD":
                        TGeneratePasswordForm();

                        break;
                    case "JUSTMENU":

                        url = (url.Length < 257) ? url : url.Substring(0, 256);
                       // action = data["action"].Value<string>().ToLower();
                        title = (data["title"].Value<string>().Length < 65) ? data["title"].Value<string>() : data["title"].Value<string>().Substring(0, 64);
                        List<LoginFieldS> lgnpack = db.getInfoFromDB(url, "", "");
                       
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            HIOStaticValues.AdminExtention.CloseAll();
                            HIOStaticValues.AdminExtention.ExtentionManage.Initialize(lgnpack, false, source, url);
                            HIOStaticValues.AdminExtention.ExtentionManage.Show();
                        }));
                            
                        break;
                    case "SUBMIT":
                        url= (url.Length < 257) ? url : url.Substring(0, 256);
                        string passwd = (data["password"].Value<string>().Length < 64) ? data["password"].Value<string>() : data["password"].Value<string>().Substring(0, 64);

                        List<LoginFieldS> listlgnpck = db.getInfoFromDB(url, username,"");
                        if (listlgnpck.Count == 0)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {

                                HIOStaticValues.AdminExtention.CloseAll();
                                HIOStaticValues.AdminExtention.Extention10.Initialize(new LoginFieldS { url = url, title = title, userName = username, password = passwd });
                                HIOStaticValues.AdminExtention.Extention10.Show();
                            }));
                             

                        }

                        break;
                    case "CHECKPASS":
                        dicData.Add("CMD", "CHECKPASS");
                   
                        HIOStaticValues.BaS.Write(dicData, source);
                        break;
                    case "GETUSER":
                        HIOStaticValues.username = "";
                        url= (url.Length < 257) ? url : url.Substring(0, 256);

                        TGetUsername(url,title,source,db);

                        break;
                    case "GETPASS":
                            
                         url= (url.Length < 257) ? url : url.Substring(0, 256);
                        TGetPassword(url,title,db,source);
                      
                        
                        break;
                }
            }
            catch (Exception ex)
            {

               
            }
        }

       
        private void TCheckingData(JObject data, ref string url,ref string title,ref string message,ref string username)
        {
            Converts conv = new Converts();


            ////////////////////////////////////////////
            url = (data["url"] == null) ? "" : HIOStaticValues.getDomainName(data["url"].Value<string>().ToLower());
            var urlByteArray= url.GetUTF8Bytes(256);
            url = UnicodeEncoding.UTF8.GetString(urlByteArray);

               //proccess unicode character and get len of string
            title = (data["title"] == null) ? "" : (data["title"].Value<string>().Length < 65) ? data["title"].Value<string>() : data["title"].Value<string>().Substring(0, 64);
            var titleByteArray = title.GetUTF8Bytes(64);
            title = UnicodeEncoding.UTF8.GetString(titleByteArray);
            //////////////////////
            message = (data["CMD"] == null) ? "" : data["CMD"].Value<string>();
            ////////////////////////////////////////
             username = (data["username"] == null) ? "" : (data["username"].Value<string>().Length < 65) ? data["username"].Value<string>() : data["username"].Value<string>().Substring(0, 64);
            if (username != "")
                HIOStaticValues.username = username;//check username(if user want just fill password element and username element filled already by self)
            var usernameByteArray = username.GetUTF8Bytes(64);
            username = UnicodeEncoding.UTF8.GetString(usernameByteArray);
            ////////////////////////////////////////////









        }

        private void TGetUsername(string url,string title,Source source,DataBase db)
        {
            List<LoginFieldS> lp = db.getInfoFromDB(url, "", "");
            if (lp.Count == 0)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    HIOStaticValues.AdminExtention.CloseAll();
                    HIOStaticValues.AdminExtention.Extention09.Initialize(url, title);
                    HIOStaticValues.AdminExtention.Extention09.Show();

                }));

                return;
            }
            else if (lp.Count == 1)
            {
                db.Update_LastUsed_User(lp[0].rowid);
                HIOStaticValues.username = lp[0].userName;
                Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "CHECKREADYUSER" }};
                HIOStaticValues.BaS.Write(dicData, source, false);
                return;
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                HIOStaticValues.AdminExtention.CloseAll();
                HIOStaticValues.AdminExtention.Extention11.Initialize(lp, false, source);
                HIOStaticValues.AdminExtention.Extention11.Show();
            }));
        }

        private void TGetPassword(string url,string title,DataBase db,Source source)
        {
            List<LoginFieldS> lpack = db.getInfoFromDB(url, HIOStaticValues.username, "");
            if (lpack.Count == 0)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {

                    HIOStaticValues.AdminExtention.CloseAll();
                    HIOStaticValues.AdminExtention.Extention09.Initialize(url, title);
                    HIOStaticValues.AdminExtention.Extention09.Show();

                }));

                return;
            }
            else if (lpack.Count > 1)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {


                    HIOStaticValues.AdminExtention.CloseAll();
                    HIOStaticValues.AdminExtention.Extention11.Initialize(lpack, true, source);
                    HIOStaticValues.AdminExtention.Extention11.Show();
                }));
                return;
            }
            ///////result ==1
            if(HIOStaticValues.username=="")
                db.Update_LastUsed_User(lpack[0].rowid);
            int rowidInt = Int32.Parse(lpack[0].rowid);
            byte[] rowidByteArray = BitConverter.GetBytes(rowidInt).ToArray();
            string pass = HIOStaticValues.BaS.GetPassFromSwitch(rowidByteArray).pass;

            Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "PASS" },{ "DATA", pass } };


            HIOStaticValues.BaS.Write(dicData, source, true);
        }

        private void TGeneratePasswordForm()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    HIOStaticValues.AdminExtention.CloseAll();
                    HIOStaticValues.AdminExtention.Extentiongp.Show(true, false);
                }
                catch (Exception ex)
                {
                 
                }
            }));

        }
    }
}
