using HIO.Backend.Firefox.Cryptography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HIO.Backend.Firefox
{
    class Firefox
    {
        public List<LoginFieldS> FetchPassword()
        {
            try
            {
                byte[] privateKey = new byte[24];
                bool loginsFound = false, signonsFound = false;
                string signonsFile = string.Empty, loginsFile = string.Empty; ;
                ErrorHandle eh = new ErrorHandle();
                string MasterPwd = string.Empty;
                string filePath = string.Empty;
                Converts conv = new Converts();
                // Read berkeleydb
                DataTable dt = new DataTable();
                Asn1Der asn = new Asn1Der();
                List<LoginFieldS> lp = new List<LoginFieldS>();
                var ffPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists( ffPath))
                    return lp;
                var dirs = Directory.GetDirectories(ffPath);
                foreach (string dir in dirs)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(dir, "signons.sqlite");
                        if (files.Length > 0)
                        {
                            filePath = dir;
                            signonsFile = files[0];
                            signonsFound = true;
                        }

                        // find logins.json file
                        files = Directory.GetFiles(dir, "logins.json");
                        if (files.Length > 0)
                        {
                            filePath = dir;
                            loginsFile = files[0];
                            loginsFound = true;
                        }



                        if (!loginsFound && !signonsFound)
                        {
                            eh.ErrorFunc("File signons & logins not found.");
                            continue;

                        }


                        if (filePath == string.Empty)
                        {

                            eh.ErrorFunc("Mozilla not found.");
                            continue;



                        }
                        // Check if files exists
                        if (!File.Exists(Path.Combine(filePath, "key3.db")))
                            privateKey = CheckKey4DB(dir, MasterPwd);
                        else
                            privateKey = CheckKey3DB(dir, MasterPwd);
                        if (privateKey == null || privateKey.Length == 0)
                        {
                            eh.ErrorFunc("Private key return null");
                            continue;

                        }


                        // decrypt username and password

                        FFLogins ffLoginData;


                        DataBase dbLocal = new DataBase();
                        if (signonsFound)
                        {

                            using (SQLiteConnection cnn = new SQLiteConnection("Data Source=" + Path.Combine(filePath, "signons.sqlite")))
                            {
                                cnn.Open();
                                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                                mycommand.CommandText = "select hostname, encryptedUsername, encryptedPassword, guid, encType from moz_logins;";
                                SQLiteDataReader reader = mycommand.ExecuteReader();
                                dt.Load(reader);
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                try
                                {
                                    Asn1DerObject user = asn.Parse(Convert.FromBase64String(row["encryptedUsername"].ToString()));
                                    Asn1DerObject pwd = asn.Parse(Convert.FromBase64String(row["encryptedPassword"].ToString()));
                                    string hostname = row["hostname"].ToString();
                                    string decryptedUser = TripleDESHelper.DESCBCDecryptor(privateKey, user.objects[0].objects[1].objects[1].Data, user.objects[0].objects[2].Data);
                                    string decryptedPwd = TripleDESHelper.DESCBCDecryptor(privateKey, pwd.objects[0].objects[1].objects[1].Data, pwd.objects[0].objects[2].Data);

                                    string username = Regex.Replace(decryptedUser, @"[^\u0020-\u007F]", "").GetUTF8String(64);
                                    string password = Regex.Replace(decryptedPwd, @"[^\u0020-\u007F]", "").GetUTF8String(64);
                                    string title = HIOStaticValues.getTitleNameURI(hostname).GetUTF8String(64);
                                    string url = HIOStaticValues.getDomainNameURI(hostname).GetUTF8String(256);

                                    if (dbLocal.getInfoFromDB(url, username, "").Count == 0)
                                    {

                                        lp.Add(new LoginFieldS { url = url, userName = username, password = password, title = title });
                                    }
                                }
                                catch (Exception ex)
                                {
                                   
                                    continue;
                                }
                            }

                        }
                        if (loginsFound)
                        {
                            using (StreamReader sr = new StreamReader(Path.Combine(filePath, "logins.json")))
                            {
                                string json = sr.ReadToEnd();
                                ffLoginData = JsonConvert.DeserializeObject<FFLogins>(json);
                            }

                            foreach (LoginData loginData in ffLoginData.logins)
                            {
                                try
                                {
                                    Asn1DerObject user = asn.Parse(Convert.FromBase64String(loginData.encryptedUsername));
                                    Asn1DerObject pwd = asn.Parse(Convert.FromBase64String(loginData.encryptedPassword));
                                    string hostname = loginData.hostname;
                                    string decryptedUser = TripleDESHelper.DESCBCDecryptor(privateKey, user.objects[0].objects[1].objects[1].Data, user.objects[0].objects[2].Data);
                                    string decryptedPwd = TripleDESHelper.DESCBCDecryptor(privateKey, pwd.objects[0].objects[1].objects[1].Data, pwd.objects[0].objects[2].Data);




                                    string username = Regex.Replace(decryptedUser, @"[^\u0020-\u007F]", "").GetUTF8String(64);
                                    string password = Regex.Replace(decryptedPwd, @"[^\u0020-\u007F]", "").GetUTF8String(64);
                                    string title = HIOStaticValues.getTitleNameURI(hostname).GetUTF8String(64);
                                    string url = HIOStaticValues.getDomainNameURI(hostname).GetUTF8String(256);

                                    if (dbLocal.getInfoFromDB(url, username, "").Count == 0)
                                    {

                                        lp.Add(new LoginFieldS { url = url, userName = username, password = password, title = title });

                                    }
                                }
                                catch (Exception ex)
                                {
                                   
                                    continue;
                                }

                            }
                        }
                    }
                    catch (Exception ex) {
                       
                        continue;
                    }
                }


                return lp;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private byte[] CheckKey4DB(string dir, string masterPwd)
        {
            try
            {
                ErrorHandle eh = new ErrorHandle();
                Asn1Der asn = new Asn1Der();
                byte[] item2 = new byte[] { };
                byte[] item1 = new byte[] { };
                byte[] a11 = new byte[] { };
                byte[] a102 = new byte[] { };
                string query = "SELECT item1,item2 FROM metadata WHERE id = 'password'";
                GetItemsFromQuery(dir, ref item1, ref item2, query);
                Asn1DerObject f800001 = asn.Parse(item2);
                MozillaPBE CheckPwd = new MozillaPBE(item1, Encoding.ASCII.GetBytes(""), f800001.objects[0].objects[0].objects[1].objects[0].Data);
                CheckPwd.Compute();

                string decryptedPwdChk = TripleDESHelper.DESCBCDecryptor(CheckPwd.Key, CheckPwd.IV, f800001.objects[0].objects[1].Data);

                if (!decryptedPwdChk.StartsWith("password-check"))
                {

                    eh.ErrorFunc("Master password is wrong !");
                    return null;
                }

                query = "SELECT a11,a102 FROM nssPrivate";
                GetItemsFromQuery(dir, ref a11, ref a102, query);
                var decodedA11 = asn.Parse(a11);
                var entrySalt = decodedA11.objects[0].objects[0].objects[1].objects[0].Data;
                var cipherT = decodedA11.objects[0].objects[1].Data;

                return decrypt3DES(item1, masterPwd, entrySalt, cipherT);
            }
            catch (Exception ex)
            {
               
                return null;

            }
        }

        private byte[] decrypt3DES(byte[] item1, string masterPwd, byte[] entrySalt, byte[] cipherT)
        {
            try
            {
                var sha1 = SHA1.Create("sha1");
                var hp = sha1.ComputeHash(item1);
                Array.Resize(ref hp, 40);
                Array.Copy(entrySalt, 0, hp, 20, 20);

                var pes = entrySalt.Concat(Enumerable.Range(1, 20 - entrySalt.Length).Select(b => (byte)0).ToArray()).ToArray();
                Array.Resize(ref pes, 40);
                Array.Copy(entrySalt, 0, pes, 20, 20);
                var chp = sha1.ComputeHash(hp);
                var hmac = HMACSHA1.Create();
                hmac.Key = chp;
                var k1 = hmac.ComputeHash(pes);
                Array.Resize(ref pes, 20);

                var tk = hmac.ComputeHash(pes);
                Array.Resize(ref tk, 40);
                Array.Copy(entrySalt, 0, tk, 20, 20);
                var k2 = hmac.ComputeHash(tk);
                Array.Resize(ref k1, 40);
                Array.Copy(k2, 0, k1, 20, 20);
                var iv = k1.Skip(k1.Length - 8).ToArray();
                var key = k1.Take(24).ToArray();
                return TripleDESHelper.DESCBCDecryptorByte(key, iv, cipherT).Take(24).ToArray();
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        private void GetItemsFromQuery(string dir, ref byte[] item1, ref byte[] item2, string query)
        {
            DataTable dt = new DataTable();

            var db_way = dir + "\\key4.db";
            var ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
            var sql = string.Format(query);
            using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
            {
                connect.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                {
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(dt);

                    int rows = dt.Rows.Count;
                    for (int i = 0; i < rows; i++)
                    {
                        Array.Resize(ref item2, ((byte[])dt.Rows[i][1]).Length);
                        Array.Copy((byte[])dt.Rows[i][1], item2, ((byte[])dt.Rows[i][1]).Length);
                        Array.Resize(ref item1, ((byte[])dt.Rows[i][0]).Length);
                        Array.Copy((byte[])dt.Rows[i][0], item1, ((byte[])dt.Rows[i][0]).Length);
                    }
                    adapter.Dispose();
                    connect.Close();
                }

            }
        }

        private byte[] CheckKey3DB(string filePath, string MasterPwd)
        {
            try
            {
                Converts conv = new Converts();
                ErrorHandle eh = new ErrorHandle();
                Asn1Der asn = new Asn1Der();
                BerkeleyDB db = new BerkeleyDB(Path.Combine(filePath, "key3.db"));

                // Verify MasterPassword
                PasswordCheck pwdCheck = new PasswordCheck((from p in db.Keys
                                                            where p.Key.Equals("password-check")
                                                            select p.Value).FirstOrDefault().Replace("-", ""));

                string GlobalSalt = (from p in db.Keys
                                     where p.Key.Equals("global-salt")
                                     select p.Value).FirstOrDefault().Replace("-", "");


                MozillaPBE CheckPwd = new MozillaPBE(conv.ConvertHexStringToByteArray(GlobalSalt), Encoding.ASCII.GetBytes(MasterPwd), conv.ConvertHexStringToByteArray(pwdCheck.EntrySalt));
                CheckPwd.Compute();
                string decryptedPwdChk = TripleDESHelper.DESCBCDecryptor(CheckPwd.Key, CheckPwd.IV, conv.ConvertHexStringToByteArray(pwdCheck.Passwordcheck));

                if (!decryptedPwdChk.StartsWith("password-check"))
                {

                    eh.ErrorFunc("Master password is wrong !");
                    return null;
                }

                // Get private key
                string f81 = (from p in db.Keys
                              where !p.Key.Equals("global-salt")
                              && !p.Key.Equals("Version")
                              && !p.Key.Equals("password-check")
                              select p.Value).FirstOrDefault().Replace("-", "");

                Asn1DerObject f800001 = asn.Parse(conv.ConvertHexStringToByteArray(f81));

                MozillaPBE CheckPrivateKey = new MozillaPBE(conv.ConvertHexStringToByteArray(GlobalSalt), Encoding.ASCII.GetBytes(MasterPwd), f800001.objects[0].objects[0].objects[1].objects[0].Data);
                CheckPrivateKey.Compute();

                byte[] decryptF800001 = TripleDESHelper.DESCBCDecryptorByte(CheckPrivateKey.Key, CheckPrivateKey.IV, f800001.objects[0].objects[1].Data);

                Asn1DerObject f800001deriv1 = asn.Parse(decryptF800001);

                Asn1DerObject f800001deriv2 = asn.Parse(f800001deriv1.objects[0].objects[2].Data);



                byte[] privateKey = new byte[24];

                if (f800001deriv2.objects[0].objects[3].Data.Length > 24)
                {
                    Array.Copy(f800001deriv2.objects[0].objects[3].Data, f800001deriv2.objects[0].objects[3].Data.Length - 24, privateKey, 0, 24);
                }
                else
                {
                    privateKey = f800001deriv2.objects[0].objects[3].Data;
                }
                return privateKey;
            }
            catch (Exception ex)
            {
               
                return null;
            }
        }
    }

    class FFLogins
    {
        public long nextId { get; set; }
        public LoginData[] logins { get; set; }
        public string[] disabledHosts { get; set; }
        public int version { get; set; }
    }
    class LoginData
    {
        public long id { get; set; }
        public string hostname { get; set; }
        public string url { get; set; }
        public string httprealm { get; set; }
        public string formSubmitURL { get; set; }
        public string usernameField { get; set; }
        public string passwordField { get; set; }
        public string encryptedUsername { get; set; }
        public string encryptedPassword { get; set; }
        public string guid { get; set; }
        public int encType { get; set; }
        public long timeCreated { get; set; }
        public long timeLastUsed { get; set; }
        public long timePasswordChanged { get; set; }
        public long timesUsed { get; set; }
    }
}

