using HIO.Backend.IconURL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HIO.Backend
{
    class DataBase
    {
        string dbPath, db_way, ConnectionString;
        EncodingForBase64 encode = new EncodingForBase64();
        readonly object _lockDB = new object();
        public DataBase()
        {
            dbPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            db_way = dbPath + "\\HIOdb.sqlite";
            ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
        }

        public void Update_LastUsed_User(string rowid)
        {
            try
            {

                List<LoginPack> lp = new List<LoginPack>();
                DataTable DB = new DataTable();


                var sql = string.Format("update \"main\".\"logins\" set   last_used='{0}',counter=counter+1 where rowid='{1}'   ", DateTime.Now.ToString("yyyyMMddHHmmssfff"), rowid);

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))

                        command.ExecuteNonQuery();

                    connect.Close();


                }
            }
            catch (Exception ex)
            {
          
            }

        }
        public List<string> GetListUrlsWithoutIcon()
        {
            try
            {

                List<string> url = new List<string>();
                string sql = "";


                DataTable DB = new DataTable();
                sql = string.Format("select distinct origin_url from logins where   image_data IS  NULL or length(image_data)==0  ");
                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        if (DB.Rows.Count == 0) return null;
                        int rows = DB.Rows.Count;
                        for (int i = 0; i < rows; i++)
                        {

                            url.Add((DB.Rows[i][0] == DBNull.Value) ? string.Empty : DB.Rows[i][0].ToString());
                        }
                        adapter.Dispose();
                        connect.Close();
                    }
                }
                return url;
            }

            catch (Exception ex)
            {
              
                return null;
            }

        }
        public void FillIcon(string url)
        {
            try
            {
                lock (_lockDB)
                {
                    byte[] imageData = GetIconFromUrl(encode.Base64Decode(url));


                    var sql = string.Format("update \"main\".\"logins\" set   image_data=@image_byte_array where origin_url='{0}'   ", url);


                    using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                    {
                        connect.Open();
                        using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                        {
                            command.Parameters.Add("@image_byte_array", DbType.Binary, imageData.Length);
                            command.Parameters["@image_byte_array"].Value = imageData;
                            command.ExecuteNonQuery();


                        }
                        connect.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            
            }



        }
        //public void UpdateIconWebsite()
        //{
        //    try
        //    {

        //        string url = "";
        //        string sql = "";
        //        try
        //        {

        //            DataTable DB = new DataTable();
        //            sql = string.Format("select distinct origin_url from logins where   image_data IS  NULL or length(image_data)==0 limit 1 ");
        //            using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
        //            {
        //                connect.Open();
        //                using (SQLiteCommand command = new SQLiteCommand(sql, connect))
        //                {
        //                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
        //                    adapter.Fill(DB);
        //                    if (DB.Rows.Count == 0) return;
        //                    url = (DB.Rows[0][0] == DBNull.Value) ? string.Empty : DB.Rows[0][0].ToString();

        //                    adapter.Dispose();
        //                    connect.Close();
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {

       
        //        }



        //        byte[] imageData = GetIconFromUrl(encode.Base64Decode( url));


        //        sql = string.Format("update \"main\".\"logins\" set   image_data=@image_byte_array where origin_url='{0}'   ", url);


        //        using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
        //        {
        //            connect.Open();
        //            using (SQLiteCommand command = new SQLiteCommand(sql, connect))
        //            {
        //                command.Parameters.Add("@image_byte_array", DbType.Binary, imageData.Length);
        //                command.Parameters["@image_byte_array"].Value = imageData;
        //                command.ExecuteNonQuery();


        //            }
        //            connect.Close();
        //        }





        //    }
        //    catch (Exception ex)
        //    {
    
        //    }

        //}

        private byte[] GetIconFromUrl(string url)
        {
            byte[] imageData = new byte[0];


            Converts conv = new Converts();
            Image _imageData = null;
            try
            {

                _imageData = Favicon.GetFromUrl(url).Icon;



                if (_imageData != null)
                {
                    byte[] tmpImageData = conv.ImageToByteArray(_imageData);
                    Array.Resize(ref imageData, tmpImageData.Length);
                    Array.Copy(tmpImageData, imageData, tmpImageData.Length);

                }

                return imageData;
            }
            catch (Exception ex)
            {

                return imageData;
            }

        }

        public bool insertToDatabase(string rowid, string url, string username, string appid, string title)
        {


            try
            {

                DataTable DB = new DataTable();

                string _url = url;
                int i = _url.IndexOf("//");
                if (i > 0) _url = _url.Substring(i + 2);


                var sql = string.Format("INSERT INTO \"main\".\"logins\" (  origin_url,username_value,title,appid,last_used,rowid) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')  ", encode.Base64Encode(_url), encode.Base64Encode(username), encode.Base64Encode(title), encode.Base64Encode(appid.ToLower()), "", rowid);


                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))

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
        public int getLastId()
        {

            try
            {
                string db_field = "logins";
                List<LoginPack> lp = new List<LoginPack>();
                DataTable DB = new DataTable();


                var sql = string.Format("SELECT MAX(rowid) FROM {0}", db_field);
                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        adapter.Dispose();
                    }
                    connect.Close();
                    return Int32.Parse(DB.Rows[0][0].ToString()) + 1;

                }
            }
            catch (Exception ex)
            {

           
                return -1;
            }
        }
        public List<LoginFieldS> getInfoFromDB(string colName, string username, string title)
        {
            List<LoginFieldS> lp = new List<LoginFieldS>();
            try
            {

                Debug.Print("getInfoFromDB");
                string db_field = "logins";
                string sql = "";
                string colNameWithoutSlash = "";
                DataTable DB = new DataTable();
                if (colName.Substring(colName.Length - 1) == "/")
                    colNameWithoutSlash = colName.Substring(0, colName.Length - 1);
                else
                    colNameWithoutSlash = colName;
                if (colName.Substring(colName.Length - 1) == "/")
                    colNameWithoutSlash = colName.Substring(0, colName.Length - 1);
                else
                    colNameWithoutSlash = colName;
                if (title != "")
                {

                    sql = string.Format("SELECT DISTINCT * FROM {0} where ( origin_url=\"{1}\" or origin_url=\"{2}/\") and username_value=\"{3}\" and title=\"{4}\" ", db_field, encode.Base64Encode(colNameWithoutSlash), encode.Base64Encode(colNameWithoutSlash), encode.Base64Encode(username), encode.Base64Encode(title));
                }
                else if ((username == "" || username == null) && colName != "*")
                {

                    sql = string.Format("SELECT DISTINCT * FROM {0} where  origin_url=\"{1}\" or origin_url=\"{2}/\" ", db_field, encode.Base64Encode(colNameWithoutSlash), encode.Base64Encode(colNameWithoutSlash));
                }
                else if (colName == "*" && username != "")
                {
                    //sql = string.Format("SELECT  DISTINCT * FROM {0} where username_value LIKE '{1}%' ", db_field, username.ToString());
                }
                else if (colName == "*")
                {
                    sql = string.Format("SELECT  DISTINCT * FROM {0} ", db_field);
                }

                else
                {
                    sql = string.Format("SELECT DISTINCT * FROM {0} where ( origin_url=\"{1}\" or origin_url=\"{2}/\") and username_value=\"{3}\"", db_field, encode.Base64Encode(colNameWithoutSlash), encode.Base64Encode(colNameWithoutSlash), encode.Base64Encode(username));
                    // sql = string.Format("SELECT DISTINCT * FROM {0} where (action_url=\"{1}\" or origin_url=\"{2}\" or origin_url=\"{3}/\") and username_value=\"{4}\"", db_field, (action == "" || action == null) ? "{}" : action, colNameWithoutSlash, colNameWithoutSlash, username);
                }


                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(DB);
                    int rows = DB.Rows.Count;
                    for (int i = 0; i < rows; i++)
                    {

                        //byte[] byteArray = (byte[])DB.Rows[i][3];
                        // byte[] decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                        lp.Add(new LoginFieldS { url = (DB.Rows[i][0] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[i][0].ToString()), userName = (DB.Rows[i][2] == DBNull.Value) ? string.Empty : encode.Base64Decode((string)DB.Rows[i][2]), title = (DB.Rows[i][4] == DBNull.Value) ? string.Empty : encode.Base64Decode((string)DB.Rows[i][4]), appID = (DB.Rows[i][5] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[i][5].ToString()), last_used = (DB.Rows[i][6] == DBNull.Value) ? string.Empty : DB.Rows[i][6].ToString(), rowid = (DB.Rows[i][7] == DBNull.Value) ? string.Empty : DB.Rows[i][7].ToString(), imageData = (DB.Rows[i][8] == DBNull.Value) ? null : (byte[])DB.Rows[i][8] });
                    }
                    adapter.Dispose();
                    connect.Close();
                }
                // Writer.Close();
                return lp;
            }
            catch (Exception ex)
            {


            
                return lp;
            }

        }
        public bool CheckExistUser(string username, string url)
        {
            try
            {
                LoginFieldS lp = new LoginFieldS();
                DataTable DB = new DataTable();
                var sql = string.Format("select username_value from \"main\".\"logins\" where origin_url=\"{0}\"  ", encode.Base64Encode(url));

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(DB);
                    int rows = DB.Rows.Count;
                    for (int i = 0; i < rows; i++)
                    {
                        if (encode.Base64Decode(DB.Rows[i][0].ToString()) == username) return true;
                    }
                    adapter.Dispose();
                    connect.Close();
                    return false;
                }


            }
            catch (Exception ex)
            {
             
                return false;
            }


        }
        public LoginFieldS GetDataFromID(byte[] iD)
        {
            try
            {
                LoginFieldS lp = new LoginFieldS();
                DataTable DB = new DataTable();
                int idRecord = BitConverter.ToInt32(iD, 0);
                var sql = string.Format("select * from \"main\".\"logins\" where rowid={0}  ", idRecord);

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(DB);
                    int rows = DB.Rows.Count;

                    lp = new LoginFieldS { url = (DB.Rows[0][0] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[0][0].ToString()), action = (DB.Rows[0][1] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[0][1].ToString()), userName = (DB.Rows[0][2] == DBNull.Value) ? string.Empty : encode.Base64Decode((string)DB.Rows[0][2].ToString()), title = (DB.Rows[0][4] == DBNull.Value) ? string.Empty : encode.Base64Decode((string)DB.Rows[0][4]), appID = (DB.Rows[0][5] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[0][5].ToString()), rowid = (DB.Rows[0][7] == DBNull.Value) ? string.Empty : DB.Rows[0][7].ToString() };

                    adapter.Dispose();
                    connect.Close();
                    return lp;
                }


            }
            catch (Exception ex)
            {
            
                return null;
            }


        }
        public void AddDeviceInfo(byte[] mac, string name)
        {


            try
            {

                DataTable DB = new DataTable();
                var sql = string.Format("INSERT INTO \"main\".\"Devices\" (  name,mac,datetime) VALUES (@DEVNAME,@MAC_BYTE,'{0}')  ", DateTime.Now.ToString("yyyyMMddHHmmssfff"));


                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, connect);
                    command.Parameters.Add("@MAC_BYTE", DbType.Binary, mac.Length);
                    command.Parameters["@MAC_BYTE"].Value = mac;
                    command.Parameters.Add("@DEVNAME", DbType.String, name.Length);
                    command.Parameters["@DEVNAME"].Value = name;

                    command.ExecuteNonQuery();
                    connect.Close();
                }


            }
            catch (Exception ex)
            {
           
            }



        }
        public byte[] GetLastDeviceMac()
        {
            try
            {

                Commands cmd = new Commands();
                DataTable DB = new DataTable();
                var sql = string.Format("select mac from \"main\".\"Devices\"  ");
                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                    {
                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                        {
                            adapter.Fill(DB);
                            adapter.Dispose();
                        }
                        command.Dispose();
                    }
                    if (DB.Rows.Count > 0)
                    {
                        return (byte[])DB.Rows[0][0];
                    }
                    connect.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
              
                return null;
            }
        }
        public void CheckData()
        {


            try
            {
                Commands cmd = new Commands();
                List<LoginFieldS> lp = new List<LoginFieldS>();
                DataTable DB = new DataTable();
                var sql = string.Format("select * from \"main\".\"Devices\"  ");
                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        if (DB.Rows.Count > 0)
                        {
                            byte[] mac = (byte[])DB.Rows[0][1];
                            if (Array.TrueForAll(HIOStaticValues.blea.mac, value => (value == 0x00)) == true)
                                return;
                            if (mac.SequenceEqual(HIOStaticValues.blea.mac))
                                return;
                            using (SQLiteCommand command2 = new SQLiteCommand("delete from \"main\".\"Devices\" ", connect))
                            {

                                command2.ExecuteNonQuery();
                                AddDeviceInfo(HIOStaticValues.blea.mac, Encoding.UTF8.GetString(HIOStaticValues.blea.name).Replace("\0", ""));
                                HIOStaticValues.tmain.TabManager.ActiveTab = HIOStaticValues.tmain.AccountManager;
                                command2.Dispose();
                            }
                        }
                        else
                        {
                            AddDeviceInfo(HIOStaticValues.blea.mac, Encoding.UTF8.GetString(HIOStaticValues.blea.name).Replace("\0", ""));
                            HIOStaticValues.tmain.TabManager.ActiveTab = HIOStaticValues.tmain.AccountManager;
                        }
                        adapter.Dispose();
                        command.Dispose();
                    }
                    connect.Close();

                }

            }
            catch (Exception ex)
            {
           
            }


        }

        public List<LoginFieldS> ReadData()
        {

            try
            {
                List<LoginFieldS> lp = new List<LoginFieldS>();
                DataTable DB = new DataTable();
                var sql = string.Format("select * from \"main\".\"logins\"  ");
                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        int rows = DB.Rows.Count;
                        for (int i = 0; i < rows; i++)
                        {

                            lp.Add(new LoginFieldS { url = (DB.Rows[i][0] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[i][0].ToString()), userName = (DB.Rows[i][2] == DBNull.Value) ? string.Empty : encode.Base64Decode((string)DB.Rows[i][2]), title = (DB.Rows[i][4] == DBNull.Value) ? string.Empty : encode.Base64Decode((string)DB.Rows[i][4]), appID = (DB.Rows[i][5] == DBNull.Value) ? string.Empty : encode.Base64Decode(DB.Rows[i][5].ToString()), rowid = (DB.Rows[i][7] == DBNull.Value) ? string.Empty : DB.Rows[i][7].ToString(), last_used = (DB.Rows[i][6] == DBNull.Value) ? string.Empty : DB.Rows[i][6].ToString(), imageData = (DB.Rows[i][8] == DBNull.Value) ? null : (byte[])DB.Rows[i][8] });
                        }
                        adapter.Dispose();
                    }
                    connect.Close();
                    return lp;
                }

            }
            catch (Exception ex)
            {
            
                return null;
            }





        }
        //public bool UpdateRowID(string rowIdNew, string rowIdOld)
        //{

        //    try
        //    {
        //        DataTable DB = new DataTable();

        //        sql = string.Format("update \"main\".\"logins\" set   rowid='{0}' where rowid='{1}'   ", rowIdNew, rowIdOld);

        //        using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
        //        {
        //            connect.Open();
        //            SQLiteCommand command = new SQLiteCommand(sql, connect);
        //            command.ExecuteNonQuery();

        //            connect.Close();
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //       
        //        return false;
        //    }


        //}
        public bool UpdateDBFromRowID(string rowId, string url, string appID, string Title, string username)
        {

            try
            {
                DataTable DB = new DataTable();
                string _url = url;
                string sql = "";
                int i = _url.IndexOf("//");
                if (i > 0) _url = _url.Substring(i + 2);
                if (appID != "")
                    sql = string.Format("update \"main\".\"logins\" set   origin_url='{0}',appid='{1}',title='{2}',username_value='{3}' where rowid='{4}'   ", encode.Base64Encode(url), encode.Base64Encode(appID), encode.Base64Encode(Title), encode.Base64Encode(username), rowId);
                else sql = string.Format("update \"main\".\"logins\" set   origin_url='{0}',title='{1}',username_value='{2}' where rowid='{3}'   ", encode.Base64Encode(url), encode.Base64Encode(Title), encode.Base64Encode(username), rowId);

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
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
        public bool deleteFromDatabase(string rowid)
        {
            try
            {
                DataTable DB = new DataTable();
                string sql = "";
                if (rowid == "-1")
                    sql = string.Format("delete from \"main\".\"logins\" ");
                else
                    sql = string.Format("delete from \"main\".\"logins\"  where rowid='{0}'   ", rowid);

                using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                {
                    connect.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connect))
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

    }
}
