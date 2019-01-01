using HIO.Extentions;
using HIO.ViewModels;
using HIO.ViewModels.Settings;
using Mighty.HID;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace HIO.Backend
{


    public class LoginFieldsBytes
    {
        public byte[] recordKey = new byte[32];
        public byte[] url = new byte[256];
        public byte[] userName = new byte[64];
        public byte[] password = new byte[64];
        public byte isPassFlag = new byte();
        public byte[] title = new byte[64];
        public byte[] appID = new byte[64];
        public byte[] rowID = new byte[2];
        public byte TU = new byte();
    }
    public enum Ins : byte
    {
        SET_RSSI_TH=0x20,
        GET_RSSI_TH =0x10,
        SET_CNTR_TH=0x13,
        GET_CNTR_TH=0x12,
        AMISYNCED = 0x20,
        SYNC = 0x30,
        SYNC_ACK = 0x31,
        UPDATE = 0x40,
        INSERT = 0x50,
        DELETE = 0x60,
        GETPASSWORD = 0x70,
        SCAN = 0x30,
        RESET_DEVICE = 0x30,
        BOND = 0x31,
        UNBOUND_ALL = 0x40,
        UNBOUND = 0x50,
        GET_CONNECTION_STATUS = 0x20,
        GET_VERSION = 0x10,
        BATTERY_STATUS = 0x20,
        GET_SWVERSION = 0x10,
        RESET_SYNC_RECORD = 0x80,
        GET_DEVICE_NAME = 0x22,
        GET_BONDED_LIST = 0x60,
        SET_DEVICE_NAME = 0x21,
        ENABLE_DEVICE_PIN = 0x90,
        DISABLE_DEVICE_PIN = 0x91,
        CHANGE_DEVICE_PIN = 0x92,
        CHECK_DEVICE_PIN = 0x93
    }
    public enum Type : byte
    {
        BRIDGE_CONFIG = 0xc6,
        BRIDGE_INFO = 0xc5,
        PASS_MANAGE_CMD = 0xd0,
        EVENTS = 0xc3,
        FOB_CONFIG = 0xc1,
        FOB_INFO = 0xc0

    }
    public enum Source:int
    {
        WINDOWS = 0x01,
        CHROME = 0x02,
        FIREFOX = 0x03,
        EDGE = 0x04


    }

    class RecPacketJsonByte
    {
        public byte[] iD = new byte[2];
        public byte[] data = new byte[448];
    }

    class RecPacketChain
    {
        public byte[] data = new byte[0];
        public byte[] sw = new byte[2];
    }

    public class BridgeandSwitch : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern short GetKeyState(int keyCode);
        [DllImport("user32")]
        public static extern void LockWorkStation();
        static AutoResetEvent _signal = new AutoResetEvent(false);
        RecPacketChain rpc;
        int sequencePacket = -1;

        static InterceptMouse im = new InterceptMouse();
        const int PACKETUSERBUF = 451;
        const int PACKETBLEBUF = 38;
        bool BAD_SEQUENCE = false; // check bad sequence into insert and update
        bool NOT_CONNECT = false; // check connection
        int sequencePacketCounter = -1;
        public byte[] versionDB = new byte[4];
        public IDevice dev = new HIDDev();


        WindowsTools win = new WindowsTools();
        public IDeviceInfo devInfo;
        const long NOTCONNECTED = unchecked((int)0x8007048f);
        ErrorHandle ehlog = new ErrorHandle();

        public void Dispose()
        {
            dev.Close();

            GC.SuppressFinalize(this);


        }
        public bool UnBondSwitch(byte[] mac)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {

                     Trace.WriteLine("UnBondSwitch");
                    List<byte[]> psp = PrepringSendPack(mac, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_CONFIG, (byte)Ins.UNBOUND);
                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }

                    if (rpc == null)
                        return false;


                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result UnBondSwitch: " + StatusWord.sw[swResult]);

                    // check record data
                    if (swResult == StatusWord.SW_NO_ERROR)
                    {
                        return true;

                    }
                    return false;
                }
            }
            catch (Exception ex)
            {

               
                return false;
            }
            finally
            {
                rpc = null;
            }
        }
        public bool EraseAllDataSwitch()
        {
            try
            {

                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("EraseAllDataSwitch");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.RESET_DEVICE);
                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(9000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }

                    if (rpc == null)
                        return false;



                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result EraseAllDataSwitch: " + StatusWord.sw[swResult]);

                    // check record data
                    if (swResult == StatusWord.SW_NO_ERROR)
                        return true;

                    return false;
                }
            }
            catch (Exception ex)
            {

                
                return false;
            }
            finally
            {
                rpc = null;
            }
        }
        public bool UnBondAllSwitch()
        {
            try
            {

                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("UnBondSwitch");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_CONFIG, (byte)Ins.UNBOUND_ALL);
                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }

                    if (rpc == null)
                    {
                        return false;
                        // dev.Close();
                        // bool a = deviceListAndOpen();
                        // dev.Open(devInfo);
                    }
                    else if (rpc.sw.SequenceEqual(new byte[] { 0x90, 0x00 }))
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {

                
                return false;
            }
            finally
            {
                rpc = null;
            }
        }
        private static void winAPP()
        {

            StringBuilder szClassName = new StringBuilder(256);
            //run mouse click check
            //im.run();
            System.Windows.Forms.Application.Run();

        }
        private void processClick(string winTitle, TMain tMain)
        {
            try
            {
                 Trace.WriteLine("processClick");
                if (winTitle == "chrome")
                {
                    HIOStaticValues.SOURCE =(int) Source.CHROME;
                    Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "WHICH" }, { "DATA", "" } };
                    Write(dicData, Source.CHROME);
                }
                else if (winTitle == "firefox") {
                    HIOStaticValues.SOURCE = (int)Source.FIREFOX;
                    Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "WHICH" }, { "DATA", "" } };
                    Write(dicData, Source.FIREFOX);

                }
                else if (winTitle == "microsoftedge")
                {
                    HIOStaticValues.SOURCE = (int)Source.EDGE;
                    Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "WHICH" }, { "DATA", "" } };
                    Write(dicData, Source.EDGE);

                }
                else
                {

                    ProcessMessage pm = new ProcessMessage();

                    if (im.checkClick() == 1)
                    {

                        string jsonStr = "{\"CMD\":\"GETUSER\",\"url\":\"" + winTitle + "\",\"username\":\"\",\"action\":\"\",\"title\":\"" + winTitle + "\"}";
                        pm.ProcessRecieveMessage((JObject)JsonConvert.DeserializeObject<JObject>(jsonStr), Source.WINDOWS);

                    }
                    else if (im.checkClick() == 2)
                    {

                        string jsonStr = "{\"CMD\":\"GETPASS\",\"url\":\"" + winTitle + "\",\"username\":\"" + HIOStaticValues.username + "\",\"action\":\"\",\"title\":\"" + winTitle + "\"}";
                        pm.ProcessRecieveMessage((JObject)JsonConvert.DeserializeObject<JObject>(jsonStr), Source.WINDOWS);

                    }



                }
            }
            catch (Exception ex)
            {
                

            }
        }
        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }
        public bool CheckSwitchEvents(TMain tMain)
        {
            try
            {
                Trace.WriteLine("BAS Open Connection");

                HIOStaticValues._signalCheckDevice.Set(); //FREE
                ///////////////START Application///////////////////////////////
                //Thread threadApp = null;
                //threadApp = new Thread(() => winAPP());
                //threadApp.SetApartmentState(ApartmentState.STA);
                //threadApp.IsBackground = true;
                //threadApp.Start();
                ////////////////////////////////////////////////////////////////
                do
                {
                     Trace.WriteLine("befor start event read");
                    byte[] recBufData = new byte[65];
                    Array.Clear(recBufData, 0, recBufData.Length);
                    if (!dev.CanRead)
                    {
                        return false;

                    }

                    ////read data
                    Thread threadReadDevice = new Thread(() => ThreadReadDevice(ref recBufData), 0);
                    threadReadDevice.Priority = ThreadPriority.Highest;
                    threadReadDevice.SetApartmentState(ApartmentState.MTA);
                    threadReadDevice.IsBackground = true;
                    threadReadDevice.Start();
                    threadReadDevice.Join();
                    Trace.WriteLine("Rec:\n" + ByteArrayToString(recBufData));
                     Trace.WriteLine("after start event read");
                    //while (threadReadDevice.IsAlive) ;
                    if (rpc != null && rpc.sw != null && rpc.sw.All(b => b == 0xff))
                    {
                        rpc = null;
                        return false;
                    }

                    if (Array.TrueForAll(recBufData, value => (value == 0x00)) == true)
                    {
                        rpc = null;
                        continue;
                    }
                    if (recBufData[5] == 0xcc) //check if event
                        continue;

                    else if (recBufData[5] != 0xc3) //check if event
                    {

                        rpc = AnalyzeRecPacks(recBufData);
                        if (rpc == null)
                        {
                            //  _signal.Set();
                            continue;
                        }


                        switch (BitConverter.ToString(rpc.sw).Replace("-", "").ToUpper())
                        {


                            case "6901":
                                rpc = null;
                                continue;
                            case "6D00":
                                NOT_CONNECT = true;
                                break;
                            case "9580":
                                BAD_SEQUENCE = true;
                                Array.Reverse(rpc.data);
                                Array.Resize(ref rpc.data, 4);
                                sequencePacketCounter = BitConverter.ToInt32(rpc.data, 0);
                                continue;

                        }

                    }
                    else //events
                    {
                        if (HIOStaticValues.SYNC_ON || HIOStaticValues.IMPORT_ON)
                            continue;
                        if (HIOStaticValues.eventQ.Count > 0)
                        {
                            HIOStaticValues.popUp("Please wait...\nReceiving password.");
                            continue;
                        }
                        string appTitle = WindowsTools.GetProcessName().ToLower() ;
                        switch (recBufData[10])
                        {


                            case 0x10: //click

                                if (appTitle == "HIO")
                                    continue;
                                if (HIOStaticValues.AdminExtention.Extention06a.IsFormOpen)
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    { HIOStaticValues.AdminExtention.Extention06a.Close(); }));
                                   
                                Thread threadClickProcess = new Thread(() => processClick(appTitle, tMain));
                                threadClickProcess.SetApartmentState(ApartmentState.STA);
                                threadClickProcess.IsBackground = true;
                                threadClickProcess.Start();
                                break;
                            case 0x20: //long click
                                if (appTitle == "chrome")
                                    longClick(Source.CHROME);
                                else if (appTitle == "firefox")
                                    longClick(Source.FIREFOX);
                                else if (appTitle == "microsoftedge")
                                    longClick(Source.EDGE);
                                else longClick(Source.WINDOWS);
                                break;
                            case 0x30: //double click 
                                if (appTitle == "HIO") continue;
                                if (HIOStaticValues.AdminExtention.Extention06a.IsFormOpen)
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    { HIOStaticValues.AdminExtention.Extention06a.Close(); }));
                                if (appTitle == "chrome")
                                {
                                    HIOStaticValues.SOURCE =(int) Source.CHROME;
                                    Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "MENU" }, { "DATA", "" } };

                                    Write(dicData, (Source.CHROME));

                                }

                                else if (appTitle == "firefox") {
                                    HIOStaticValues.SOURCE = (int)Source.FIREFOX;
                                    Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "MENU" }, { "DATA", "" } };
                                    Write(dicData, (Source.FIREFOX));
                                }
                                else if (appTitle == "microsoftedge")
                                {
                                    HIOStaticValues.SOURCE = (int)Source.EDGE;
                                    Dictionary<string, string> dicData = new Dictionary<string, string> { { "CMD", "MENU" }, { "DATA", "" } };
                                    Write(dicData, (Source.EDGE));
                                }
                                else
                                {
                                    ProcessMessage pm = new ProcessMessage();
                                    string jsonStr = "{\"CMD\":\"JUSTMENU\",\"url\":\"" + appTitle + "\",\"username\":\"\",\"action\":\"\",\"title\":\"" + appTitle + "\"}";
                                    pm.ProcessRecieveMessage((JObject)JsonConvert.DeserializeObject<JObject>(jsonStr), Source.WINDOWS);
                                }
                                break;
                            case 0x40: //say hello

                                continue;
                            case 0x60:
                                HIOStaticValues.commandQ.Add(() =>
                                {
                                    Commands ic = new Commands();
                                    ic.IsConnection();

                                });
                                break;
                            case 0x70:
                                HIOStaticValues.CONNECTIONBHIO = false;
                                Trace.WriteLine("******* CheckSwitchEvent");
                                break;
                            case 0x80: //lock
                                if (!HIOStaticValues.ISLOCK)
                                    LockWorkStation();
                                break;

                        }
                        continue;
                    }
                    Trace.WriteLine("Set receive");
                    _signal.Set();
                    Thread.Sleep(300);
                    _signal.Reset();
                } while (true);

            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        private void longClick(Source source)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    TExtentionGenPass gp = new TExtentionGenPass();
                    gp.Initialize(source);
                    gp.Show(false, true);
                }
                catch (Exception ex)
                {
                    
                }
            }));
        }

        double _RestOfProgressBar = 100;
        private RecPacketChain AnalyzeRecPacks(byte[] data)
        {
            int _index = 0;
            if (BAD_SEQUENCE)
                _RestOfProgressBar = 100 - HIOStaticValues.ProgressPercent;
             Trace.WriteLine("AnalyzeRecPacks");
            //LoadingClass lc = new LoadingClass();
            RecPacketChain listRecPackets = new RecPacketChain();
            int index = 0;
            try
            {

                sequencePacket = -1;

                byte[] cid = new byte[4];
                byte[] len = new byte[3];

                Array.Copy(data, 1, cid, 0, 4);
                if (cid.Equals(new byte[] { 0x00, 0x00, 0x00, 0x01 })) return null;
                Array.Copy(data, 6, len, 0, 3);
                if (!Enum.IsDefined(typeof(Type), data[5])) return null;
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(len);

                int lenPack = (len[2] << 16) | (len[1] << 8) | len[0];
                Array.Resize(ref listRecPackets.data, lenPack); //set size for data
                index = 24 - 8;
                if (lenPack > 16)
                {
                    byte[] sequenceByte = new byte[3];
                    byte[] packRec = new byte[65];
                    /////////////////////////////////////////        

                    //////////////////////////////////////////
                    Array.Copy(data, 9, listRecPackets.data, 0, index);
                    do
                    {
                        if (HIOStaticValues.SYNC_ON && _index > 451 && packRec[5] != 0xcc)
                        {
                            HIOStaticValues.ProgressPercent += ((100 / (lenPack / 451.0)) * 5 / 10);// * (index / 451);
                            _index -= 451;
                        }



                        dev.Read(packRec);


                       















                        if (packRec[5] == 0xcc) //check if event
                            continue;
                        Trace.WriteLine("Rec:\n" + Converts.ByteArrayToString(packRec));
                        // if (!packRec.Skip(1).Take(4).ToArray().SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x01 })) break;
                        Array.Copy(packRec.Skip(5).Take(3).ToArray(), sequenceByte, sequenceByte.Length);
                        Array.Reverse(sequenceByte);
                        int sequence = (sequenceByte[2] << 16) | (sequenceByte[1] << 8) | sequenceByte[0];
                        Trace.WriteLine(sequence);
                        Trace.WriteLine(sequencePacket);
                        if (sequence != ++sequencePacket)
                        {
                            _index = 0;
                            BAD_SEQUENCE = true;
                            //SwitchAckSync(new byte[] { 0x00, 0x00 }, false);
                            dev.Close();
                            dev.Open(devInfo);
                            if (index > 0)
                                Array.Resize(ref listRecPackets.data, index); //set size for data
                            Trace.WriteLine("Bad sequence while recieve packet.");
                            if (rpc == null)
                                return listRecPackets;
                            else return rpc;

                        }
                        if ((lenPack - index) > 17)
                        {

                            packRec.Skip(8).Take(17).ToArray().CopyTo(listRecPackets.data, index);
                        }
                        else
                        { //LAST PACKET

                            packRec.Skip(8).Take(lenPack - index).ToArray().CopyTo(listRecPackets.data, index);

                        }
                        index += 17;
                        _index += 17;


                    } while (lenPack > index);
                }
                else
                {

                    Array.Copy(data, 9, listRecPackets.data, 0, lenPack);
                }

                if (lenPack == 0)
                    return null;
                //get status word 
                listRecPackets.data.Skip(lenPack - 2).Take(2).ToArray().CopyTo(listRecPackets.sw, 0);

                Array.Resize(ref listRecPackets.data, lenPack - 2); //set size for data

                return listRecPackets;
            }
            catch (Exception ex)
            {
                
                if (index > 0)
                {
                    Array.Resize(ref listRecPackets.data, index); //set size for data
                    return listRecPackets;
                }
                return null;
            }
            finally
            {
            }
        }



        /*
         opration 0 Insert data
         opration 1 Update data
         
         */
        private byte[] PrepringPackToBytes(LoginFieldS data, int opration = 0)
        {
            try
            {
                byte[] pack = new byte[547];
                int withIDIndex = 0;
                int withRKIndex = 0;
                LoginFieldsBytes lfb = new LoginFieldsBytes();
                Converts conv = new Converts();
                if (opration == 1)
                {
                    withIDIndex = 2;
                    withRKIndex = 32;
                    int rowidInt = Int32.Parse(data.rowid);
                    byte[] rowidByteArray = BitConverter.GetBytes(rowidInt).ToArray();
                    lfb.rowID = rowidByteArray;
                    Array.Copy(lfb.rowID, 0, pack, 0, 2);
                    Array.Reverse(pack, 0, 2);
                    lfb.recordKey = preparingRecordKey(rowidByteArray);
                    Array.Resize(ref lfb.recordKey, 32);
                    Array.Copy(lfb.recordKey, 0, pack, 0 + withIDIndex, 32);
                }

                lfb.url = conv.StringToByteArray(data.url.ToLower());
                Array.Resize(ref lfb.url, 256);
                Array.Copy(lfb.url, 0, pack, withIDIndex + withRKIndex, 256);


                lfb.appID = conv.StringToByteArray((data.appID == null) ? "" : data.appID.ToLower());
                Array.Resize(ref lfb.appID, 64);
                Array.Copy(lfb.appID, 0, pack, withRKIndex + 256 + withIDIndex, 64);


                lfb.title = conv.StringToByteArray(data.title);
                Array.Resize(ref lfb.title, 64);
                Array.Copy(lfb.title, 0, pack, 256 + withIDIndex + withRKIndex + 64, 64);


                lfb.userName = conv.StringToByteArray(data.userName);
                Array.Resize(ref lfb.userName, 64);
                Array.Copy(lfb.userName, 0, pack, 256 + withIDIndex + withRKIndex + 64 + 64, 64);
                lfb.password = conv.StringToByteArray(data.password);
                Array.Resize(ref lfb.password, 64);
                Array.Copy(lfb.password, 0, pack, 256 + withIDIndex + withRKIndex + 64 + 64 + 64, 64);
                if (opration == 1)
                {
                    pack[256 + withIDIndex + withRKIndex + 64 + 64 + 64 + 64] = data.isPassFlag;
                    Array.Resize(ref pack, 547);
                }
                else
                {
                    Array.Resize(ref pack, 512);

                }
                return pack;
            }
            catch (Exception ex)
            {

                
                return null;
            }
        }
        /// <summary>
        /// Delete Specific record from FOB
        /// </summary>
        /// <param name="ID">Record ID</param>
        /// <returns></returns>

        public string DeleteFromSwitch(byte[] ID)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("DeleteFromSwitch");
                    byte[] pack = new byte[34];
                    byte[] prk = preparingRecordKey(ID);
                    Array.Resize(ref ID, 2);
                    Array.Reverse(ID);
                    Array.Copy(ID, 0, pack, 0, 2);
                    Array.Copy(prk, 0, pack, 2, 32);

                    List<byte[]> psp = PrepringSendPack(pack, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.DELETE);

                    int seqCounter = 0;
                    do
                    {

                        sendDataChain(seqCounter, psp);
                        CancellationTokenSource _cts = new CancellationTokenSource();
                        Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                        threadTimeout.SetApartmentState(ApartmentState.STA);
                        threadTimeout.Start();
                        _signal.WaitOne();
                        if (threadTimeout.IsAlive)
                        {
                            _cts.Cancel();
                        }
                        if (rpc == null)
                        {
                            ErrorHandle eh = new ErrorHandle();
                            return StatusWord.SW_RECEIVE_TIMEOUT;
                        }
                        string swResult = CheckStatusWord(rpc.sw);
                        Trace.WriteLine("Result DeleteFromSwitch: " + StatusWord.sw[swResult]);

                        // check record data
                        if (swResult == StatusWord.SW_NO_ERROR)
                        {
                            return swResult;

                        }

                        else if (rpc.sw.SequenceEqual(new byte[] { 0x66, 0x00 }))
                        {
                            seqCounter = 0;
                            continue;

                        }

                        else if (rpc.sw.SequenceEqual(new byte[] { 0x95, 0x80 }))
                        {
                            Array.Reverse(rpc.data);
                            Array.Resize(ref rpc.data, 4);


                            seqCounter = BitConverter.ToInt32(rpc.data, 0) + 1;
                            continue;

                        }
                        return swResult;
                    } while (true);

                }
            }
            catch (Exception ex)
            {

                
                return StatusWord.SW_RECEIVE_TIMEOUT;
            }
            finally
            {
                rpc = null;
            }

        }
        /// <summary>
        /// Receive password from FOB
        /// </summary>
        /// <param name="rowId">Record ID</param>
        /// <returns>Password value</returns>
        public StatusPassword GetPassFromSwitch(byte[] rowId)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                    Trace.WriteLine("GetPassFromSwitch");
                    HIOStaticValues.eventQ.Enqueue("click");
                    StatusPassword sp = new StatusPassword();
                     Trace.WriteLine("GetPassFromSwitch");
                    byte[] pack = new byte[34];
                    byte[] prk = preparingRecordKey(rowId);
                    Array.Resize(ref rowId, 2);
                    Array.Reverse(rowId);
                    Array.Copy(rowId, 0, pack, 0, 2);
                    Array.Copy(prk, 0, pack, 2, 32);

                    List<byte[]> psp = PrepringSendPack(pack, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.GETPASSWORD);

                    int seqCounter = 0;


                    sendDataChain(seqCounter, psp);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    // check record data
                    if (rpc == null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            HIOStaticValues.popUp("Something went wrong!\nPlease try again");
                        }));


                        return new StatusPassword { pass = "", statusWord = null };
                    }
                    else if (rpc.sw.SequenceEqual(new byte[] { 0x6A, 0xF0 }))
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            HIOStaticValues.popUp("Something went wrong!\nPlease try again");
                        }));
                        return new StatusPassword { pass = "", statusWord = null };
                    }
                    else if (!rpc.sw.SequenceEqual(new byte[] { 0x90, 0x00 }))
                    {
                        return new StatusPassword { pass = "", statusWord = rpc.sw };
                    }

                    else
                    {
                        // UnicodeEncoding passwordEnc = new UnicodeEncoding();
                        UTF8Encoding passwordEnc = new UTF8Encoding();
                        string pass = passwordEnc.GetString(rpc.data.ToArray());

                        return new StatusPassword { pass = pass.Replace("\0", string.Empty), statusWord = rpc.sw };
                    }
                }
            }
            catch (Exception ex)
            {

                
                return new StatusPassword { pass = "", statusWord = null }; ;
            }
            finally
            {
                if (HIOStaticValues.eventQ.Count > 0)
                    HIOStaticValues.eventQ.Dequeue();
                rpc = null;
            }

        }

        private void threadTimeoutFunc(int miliSec, CancellationTokenSource cts)
        {
            try
            {
                int checkSeqLocal = -1;
                do
                {
                    checkSeqLocal = sequencePacket;
                    Thread.Sleep(miliSec);
                    if (cts.IsCancellationRequested) return;

                } while (checkSeqLocal != sequencePacket);
                Trace.WriteLine("Set timeout");
                _signal.Set();
                _signal.Reset();
            }
            catch { }
        }
        private bool sendDataChain(int sequence, List<byte[]> psp)
        {
            try
            {
                 Trace.WriteLine("sendDataChain");
                for (int i = sequence; i < psp.Count; i++)
                {

                    dev.Write(psp[i]);
                    if (BAD_SEQUENCE)
                    {
                        i = sequencePacketCounter;
                        BAD_SEQUENCE = false;
                        sequencePacketCounter = -1;
                    }
                    else if (NOT_CONNECT)
                    {
                        NOT_CONNECT = false;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                
                return false;
            }
        }
        /// <summary>
        /// Insert record to FOB
        /// </summary>
        /// <param name="lf">record</param>
        /// <returns>
        ///  record ID 
        /// </returns>
        public int InsertToSwitch(LoginFieldS lf)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                    HIOStaticValues.CounterTimerIcon = 0; //set to zero for start  searching icon url
                     Trace.WriteLine("InsertToSwitch");
                    lf.userName = (lf.userName == null) ? "" : lf.userName;
                    lf.appID = (lf.appID == null) ? "" : lf.appID;
                    DataBase db = new DataBase();
                    byte[] recordDataPacket = PrepringPackToBytes(lf);
                    int seqCounter = 0;
                    //send record data
                    List<byte[]> psp = PrepringSendPack(recordDataPacket, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.INSERT);
                    do
                    {
                        //  mut.ReleaseMutex();

                        sendDataChain(seqCounter, psp);
                        Trace.WriteLine("abort" + HIOStaticValues.AbortThread);
                        CancellationTokenSource _cts = new CancellationTokenSource();
                        Thread threadTimeout = new Thread(() => threadTimeoutFunc(6000, _cts));
                        threadTimeout.SetApartmentState(ApartmentState.STA);
                        threadTimeout.Start();
                        _signal.WaitOne();
                        if (threadTimeout.IsAlive)
                        {
                            _cts.Cancel();
                        }
                        Trace.WriteLine("abort" + HIOStaticValues.AbortThread);
                        if (rpc == null)
                            return -2;
                        string swResult = CheckStatusWord(rpc.sw);
                        Trace.WriteLine("Result InsertToSwitch: " + StatusWord.sw[swResult]);

                        if (swResult == StatusWord.SW_NO_ERROR)
                        {

                            byte[] rowid = rpc.data;
                            Array.Resize(ref rowid, 4);
                            return BitConverter.ToInt32(rowid, 0);
                        }

                        else if (rpc.sw.SequenceEqual(new byte[] { 0x66, 0x00 }))
                        {
                            seqCounter = 0;
                            continue;

                        }

                        else if (rpc.sw.SequenceEqual(new byte[] { 0x95, 0x80 }))
                        {
                            Array.Reverse(rpc.data);
                            Array.Resize(ref rpc.data, 4);
                            seqCounter = BitConverter.ToInt32(rpc.data, 0) + 1;
                            continue;

                        }
                        else if (swResult == StatusWord.SW_MEM_IS_FULL)
                            return -3;


                        return -2;
                    } while (true);

                }
            }
            catch (Exception ex)
            {

                
                return -2;
            }
            finally
            {
                rpc = null;
            }


        }


        public byte[] preparingRecordKey(byte[] rowid)
        {
            try
            {
                SHA256 sha256 = SHA256Managed.Create();
                DataBase db = new DataBase();
                LoginFieldS lf = db.GetDataFromID(rowid);

                return sha256.ComputeHash(UnicodeEncoding.UTF8.GetBytes(lf.url + lf.appID + lf.title + lf.userName));
            }
            catch (Exception ex)
            {
                
                return null;
            }


        }
        public int UpdateSwitch(string ID, string url, string appID, string title, string username, string password, byte flagPass)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("UpdateSwitch");
                    HIOStaticValues.CounterTimerIcon = 0; //set to zero for start  searching icon url
                    LoginFieldS lf = new LoginFieldS();
                    int rowidInt = 0;
                    Int32.TryParse(ID, out rowidInt);
                    byte[] rowidByteArray = BitConverter.GetBytes(rowidInt).ToArray();

                    lf.recordKey = preparingRecordKey(rowidByteArray);

                    lf.appID = appID;
                    lf.password = password;
                    lf.title = title;
                    lf.url = url;
                    lf.userName = username;
                    lf.rowid = ID;
                    lf.isPassFlag = flagPass;
                    byte[] sendBufData = new byte[65];

                    byte[] pack = PrepringPackToBytes(lf, 1);

                    List<byte[]> psp = PrepringSendPack(pack, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.UPDATE);

                    int seqCounter = 0;
                    do
                    {
                        sendDataChain(seqCounter, psp);
                        CancellationTokenSource _cts = new CancellationTokenSource();
                        Thread threadTimeout = new Thread(() => threadTimeoutFunc(7000, _cts));
                        threadTimeout.SetApartmentState(ApartmentState.STA);
                        threadTimeout.Start();
                        // check record data
                        _signal.WaitOne();
                        if (threadTimeout.IsAlive)
                        {
                            _cts.Cancel();
                        }
                        if (rpc == null)
                            return -1;
                        string swResult = CheckStatusWord(rpc.sw);
                        Trace.WriteLine("Result UpdateSwitch: " + StatusWord.sw[swResult]);
                        if (swResult == StatusWord.SW_NO_ERROR)
                        {
                            return 0;
                        }

                        else if (swResult == StatusWord.SW_BAD_SEQUENCE)
                        {
                            Array.Reverse(rpc.data);
                            Array.Resize(ref rpc.data, 4);


                            seqCounter = BitConverter.ToInt32(rpc.data, 0) + 1;
                            continue;

                        }
                        return -1;
                    } while (true);
                }
            }
            catch (Exception ex)
            {

                
                return -2;
            }
            finally
            {
                rpc = null;
            }

        }
        public List<byte[]> PrepringSendPack(byte[] data, byte[] p1p2, byte type, byte ins)
        {
            try
            {

                byte[] Pack = new byte[25];
                byte[] id = new byte[2];
                int indexData = 0;
                int seqCounter = 0;
                bool firstPack = true;
                const int sendBufDatatempLen = 25;
                byte[] sendBufDatatemp = new byte[25];
                List<byte[]> sendBufData = new List<byte[]>();
                //reportid
                sendBufDatatemp[0] = 0x00;
                //hid interface   
                Array.Copy(new byte[] { 0x00, 0x00, 0x00, 0x01 }, 0, sendBufDatatemp, 1, 4); //cid               
                do
                {
                    if (firstPack)
                    {
                        sendBufDatatemp[5] = type;
                        //len
                        byte[] len = BitConverter.GetBytes(data.Length + 7);
                        Array.Resize(ref len, 3);
                        Array.Reverse(len);
                        Array.Copy(len, 0, sendBufDatatemp, 6, 3);
                        //data
                        sendBufDatatemp[9] = 0x00;
                        sendBufDatatemp[10] = ins;
                        Array.Copy(p1p2, 0, sendBufDatatemp, 11, 2);
                        //data apdu
                        //len data
                        byte[] lenAPDU = new byte[3];
                        lenAPDU = BitConverter.GetBytes(data.Length);
                        Array.Resize(ref lenAPDU, 3);
                        Array.Reverse(lenAPDU);
                        Array.Copy(lenAPDU, 0, sendBufDatatemp, 13, 3);
                        //end len data

                        ///////
                        Array.Copy(data, indexData, sendBufDatatemp, 16, (data.Length > 8) ? (sendBufDatatempLen - 16) : (data.Length));
                        //get index data
                        indexData += sendBufDatatempLen - 16;
                        Array.Resize(ref sendBufDatatemp, 65);
                        sendBufData.Add(sendBufDatatemp.ToArray());

                        firstPack = false;
                    }
                    else
                    {
                        Array.Clear(sendBufDatatemp, 5, sendBufDatatemp.Length - 5);
                        byte[] seqCounterByte = BitConverter.GetBytes(seqCounter++);
                        Array.Resize(ref seqCounterByte, 3);
                        Array.Reverse(seqCounterByte);
                        Array.Copy(seqCounterByte, 0, sendBufDatatemp, 5, 3);
                        Array.Copy(data, indexData, sendBufDatatemp, 8, (data.Length - indexData > sendBufDatatempLen - 8) ? (sendBufDatatempLen - 8) : (data.Length - indexData));
                        indexData += sendBufDatatempLen - 8;
                        sendBufData.Add(sendBufDatatemp.ToArray());
                    }
                } while (indexData <= data.Length);



                return sendBufData;
            }
            catch (Exception ex)
            {

                
                return null;
            }

        }
        private void SendAckThread(byte[] rowID)
        {
            bool ret = SwitchAckSync(rowID, true);

        }
        public int SyncSwitch()
        {
             Trace.WriteLine("SyncSwitch");


            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                    HIOStaticValues.CounterTimerIcon=0; //set to zero for start  searching icon url
                    LoginFieldsBytes lfb = new LoginFieldsBytes();
                    RecPacketJsonByte rpj = new RecPacketJsonByte();
                    // byte[] recBufData = new byte[65];
                    byte[] Pack = new byte[0];
                    DataBase db = new DataBase();
                    if (!dev.CanWrite) return -1;
                    List<byte[]> sendBufData = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.SYNC);
                    dev.Write(sendBufData[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();

                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                        return -1;
                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result Sync: " + StatusWord.sw[swResult]);
                    if (StatusWord.sw[swResult] == StatusWord.SW_NO_ERROR) return -1;
                    if (rpc.sw.SequenceEqual(new byte[] { 0x90, 0x00 }) && rpc.data.Length == 0)
                    {
                        Thread threadSendAck = new Thread(() => SendAckThread(new byte[] { 0xff, 0xff }));
                        threadSendAck.SetApartmentState(ApartmentState.STA);
                        threadSendAck.Start();

                        threadSendAck.Join(10000);
                        db.deleteFromDatabase("-1");
                        return 1;
                    }
                    Array.Resize(ref Pack, rpc.data.Length);
                    Array.Copy(rpc.data, Pack, rpc.data.Length);
                    if (Pack.Length < PACKETUSERBUF)
                        return -1;

                    for (int i = 0; i < Pack.Length; i += PACKETUSERBUF)
                    {

                        HIOStaticValues.ProgressPercent += ((100 / (Pack.Length / 451.0)) * 5 / 10);
                        byte[] tempRowID = new byte[2];
                        Array.Copy(Pack, i, lfb.rowID, 0, 2);
                        Array.Copy(lfb.rowID, 0, tempRowID, 0, 2);
                        Array.Reverse(tempRowID);
                        Array.Resize(ref tempRowID, 4);
                        lfb.TU = Pack[i + 2];
                        Array.Copy(Pack, i + 3, lfb.url, 0, 256);
                        Array.Copy(Pack, i + 3 + 256, lfb.appID, 0, 64);
                        Array.Copy(Pack, i + 3 + 256 + 64, lfb.title, 0, 64);
                        Array.Copy(Pack, i + 3 + 256 + 64 + 64, lfb.userName, 0, 64);
                        switch (lfb.TU)
                        {
                            case 0x01:
                                db.UpdateDBFromRowID(BitConverter.ToInt32(tempRowID, 0).ToString(), UnicodeEncoding.UTF8.GetString(lfb.url).Replace("\0", ""), UnicodeEncoding.UTF8.GetString(lfb.appID).Replace("\0", ""), UnicodeEncoding.UTF8.GetString(lfb.title).Replace("\0", ""), UnicodeEncoding.UTF8.GetString(lfb.userName).Replace("\0", ""));
                                break;
                            case 0x02:
                                db.deleteFromDatabase(BitConverter.ToInt32(tempRowID, 0).ToString());

                                db.insertToDatabase(BitConverter.ToInt32(tempRowID, 0).ToString(), UnicodeEncoding.UTF8.GetString(lfb.url).Replace("\0", ""), UnicodeEncoding.UTF8.GetString(lfb.userName).Replace("\0", ""), UnicodeEncoding.UTF8.GetString(lfb.appID).Replace("\0", ""), UnicodeEncoding.UTF8.GetString(lfb.title).Replace("\0", ""));
                                break;
                            case 0x03:
                                db.deleteFromDatabase(BitConverter.ToInt32(tempRowID, 0).ToString());
                                break;
                        }
                        if (Pack.Length / PACKETUSERBUF == (i / PACKETUSERBUF) + 1)
                        {
                            rpc = null;
                            Thread threadSendAck = new Thread(() => SendAckThread(lfb.rowID));
                            threadSendAck.SetApartmentState(ApartmentState.STA);
                            threadSendAck.Start();
                            threadSendAck.Join(10000);

                        }
                    }
                    return 1;
                }
            }
            catch (Exception err)
            {

               
                return -3;
            }
            finally
            {
                rpc = null;
            }

        }


        private bool SwitchAckSync(byte[] p, bool returnVal)
        {
            try
            {
                 Trace.WriteLine("SwitchAckSync");
                var c = new Converts();
                Trace.WriteLine($"SwitchAckSync: {BitConverter.ToString(p)}");
                byte[] recData = new byte[65];
                byte[] data = PrepringSendPack(p, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.SYNC_ACK)[0];
                Array.Resize(ref data, 65);
                //bool ret = false;


                var ret = dev.Write(data);
                if (returnVal)
                {
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(9000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null) { return false; }
                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result SwitchAckSync: " + StatusWord.sw[swResult]);
                    if (swResult != StatusWord.SW_NO_ERROR) return false;
                    return true;

                }
                else return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
            finally
            {
                rpc = null;
            }
        }


        public int AmISyncedSwitch()
        {
            lock (HIOStaticValues.Cmdlocker)
            {
                try
                {

                     Trace.WriteLine("AmISyncedSwitch");
                    byte[] sendBufData = new byte[65];
                    sendBufData = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.AMISYNCED)[0];
                    dev.Write(sendBufData);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        return -2;
                    }

                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result AmISyncedSwitch: " + StatusWord.sw[swResult]);
                    if (rpc.data[0] == 0x1 && swResult == StatusWord.SW_NO_ERROR)
                    {
                        BAD_SEQUENCE = false;
                        return 1;
                    }
                    else if (rpc.data[0] == 0x0) return 0;
                    else if (swResult == StatusWord.SW_RECEIVE_TIMEOUT) return -2;
                    return -1;


                }

                catch (Exception err)
                {

                    
                    return -3;
                }
                finally
                {
                    rpc = null;
                }
            }
        }


        public bool deviceListAndOpen()
        {
            try
            {
                Trace.WriteLine("DirectBluetooth="+ HIOStaticValues.DirectBluetooth);
                if (HIOStaticValues.DirectBluetooth)
                {
                    var deviceInfo = BLEBrowser.FindConnectedDevice();
                    if (deviceInfo != null)
                    {
                        if (HIOStaticValues.BaS.devInfo is BLEDeviceInfo bleDI && bleDI.Path == deviceInfo.Path)
                            return true;
                        devInfo = deviceInfo;
                        dev = new BLEDevice();
                        return dev.Open(devInfo);
                    }
                }

                //    Trace.WriteLine("deviceListAndOpen");
                ///Console.WriteLine("List of USB HID devices:");
                /* browse for hid devices */
                var devs = HIDBrowse.Browse();
                /* try to connect to first device */

                for (int i = 0; i < devs.Count; i++)
                {
                    if (devs[i].UsagePage == "0C1E")
                    {
                        /* connect */
                        devInfo = devs[i];
                        bool ret = dev.Open(devs[i]);
                        ///Console.WriteLine(ret);
                        return ret;
                    }
                }
                return false;
            }
            catch 
            {
                
                dev.Dispose();
                return false;
            }
            finally
            {


            }



        }
        public bool SetRSSITHBLE(byte value)
        {
            lock (HIOStaticValues.Cmdlocker)
            {
                try
                {
                    Trace.WriteLine("SetRSSITHBLE");
                    byte[] packet = PrepringSendPack(new byte[] { value }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.SET_CNTR_TH)[0];
                    Array.Resize(ref packet, 65);


                    dev.Write(packet);

                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        return false;
                    }

                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result SetRSSITHBLE: " + StatusWord.sw[swResult]);

                    if (swResult == StatusWord.SW_NO_ERROR)
                    {

                        return true;
                    }
                    else
                        return false;




                }
                catch
                {
                    return false;
                }
            }
        }
        public bool SetRSSITH(byte value) {
            lock (HIOStaticValues.Cmdlocker)
            {
                try
                {
                     Trace.WriteLine("SetRSSITH");
                    byte[] packet = PrepringSendPack(new byte[] {value }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_CONFIG, (byte)Ins.SET_RSSI_TH)[0];
                    Array.Resize(ref packet, 65);


                    dev.Write(packet);

                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        return false;
                    }

                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result SetRSSITH: " + StatusWord.sw[swResult]);

                    if (swResult == StatusWord.SW_NO_ERROR)
                    {

                        return true;
                    }
                    else
                        return false;
                  



                }
                catch
                {
                    return false;
                }
            }
        }
        public byte GetRSSITHBLE()
        {
            lock (HIOStaticValues.Cmdlocker)
            {
                try
                {
                     Trace.WriteLine("GetRSSITHBLE");
                    byte[] packet = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.GET_CNTR_TH)[0];
                    Array.Resize(ref packet, 65);


                    dev.Write(packet);

                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        return 0;
                    }

                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result GetRSSITHBLE: " + StatusWord.sw[swResult]);

                    if (swResult == StatusWord.SW_NO_ERROR)
                    {
                        if (rpc.data[0] == 0x0)
                        {
                            Trace.WriteLine("******* GetRSSITHBLE 1515");
                            return 0;
                        }
                        else
                        {



                            return rpc.data[0];
                        }
                    }
                    Trace.WriteLine("******* GetRSSITHBLE 1525");

                    return 0;



                }
                catch (Exception ex)
                {


                    return 0;
                }
                finally
                {
                    rpc = null;
                }


            }
        }
        public int GetRssi()
        {
            lock (HIOStaticValues.Cmdlocker)
            {
                if (HIOStaticValues.DirectBluetooth)
                {
                    if (dev is BLEDevice bleDev && (dev?.CanRead ?? false) && (dev?.CanWrite ?? false))
                    {
                        var conv = new Converts();
                        HIOStaticValues.blea.rssi = bleDev.GetSignalValue();

                        return 1;
                    }
                    return -1;
                }

                try
                {
                     Trace.WriteLine("GetRssi");
                    byte[] packet = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_INFO, (byte)Ins.GET_CONNECTION_STATUS)[0];
                    Array.Resize(ref packet, 65);


                    dev.Write(packet);

                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        return -2;
                    }

                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result GetRssi: " + StatusWord.sw[swResult]);

                    if (swResult == StatusWord.SW_NO_ERROR)
                    {
                        if (rpc.data[0] == 0x0)
                        {
                            Trace.WriteLine("******* GetRssi 1411");
                            return -1;
                        }
                        else
                        {

                            HIOStaticValues.blea.rssi = rpc.data[39];

                            return 1;
                        }
                    }
                    Trace.WriteLine("******* GetRssi 1429");

                    return -1;



                }
                catch (Exception ex)
                {
                    
                    if (ex.HResult == unchecked((int)0x8007048f))
                        return -2;
                    return -1;
                }
                finally
                {
                    rpc = null;
                }
            }
        }
        public int CheckConnection()
        {
            lock (HIOStaticValues.Cmdlocker)
            {
                if (HIOStaticValues.DirectBluetooth)
                {
                    if (dev is BLEDevice bleDev && (dev?.CanRead ?? false) && (dev?.CanWrite ?? false))
                    {
                        var conv = new Converts();
                        HIOStaticValues.blea.rssi = bleDev.GetSignalValue();
                        Array.Copy(bleDev.mac, 0, HIOStaticValues.blea.mac, 0, 6);
                        Array.Copy(bleDev.name, 0, HIOStaticValues.blea.name, 0, 32);
                        HIOStaticValues.tmain.SettingManager.MyHIOTitle = Encoding.UTF8.GetString(HIOStaticValues.blea.name).Replace("\0", "");
                        HIOStaticValues.tmain.SettingManager.MyHIOMac = "MAC: " + conv.ByteToChar(HIOStaticValues.blea.mac[0]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[1]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[2]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[3]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[4]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[5]);

                        // HIOStaticValues. blea.mac
                        HIOStaticValues.CONNECTIONBHIO = true;
                        return 1;
                    }
                    Trace.WriteLine("******* CheckConnection");
                    HIOStaticValues.CONNECTIONBHIO = false;
                    Array.Clear(HIOStaticValues.blea.mac, 0, HIOStaticValues.blea.mac.Length);
                    Array.Clear(HIOStaticValues.blea.name, 0, HIOStaticValues.blea.name.Length);
                    return -1;
                }

                try
                {
                     Trace.WriteLine("CheckConnection");
                    byte[] packet = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_INFO, (byte)Ins.GET_CONNECTION_STATUS)[0];
                    Array.Resize(ref packet, 65);


                    dev.Write(packet);

                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        Trace.WriteLine("******* CheckConnection rpc == null");
                        HIOStaticValues.CONNECTIONBHIO = false;
                        return -2;
                    }

                    string swResult = CheckStatusWord(rpc.sw);
                    Trace.WriteLine("Result CheckConnection: " + StatusWord.sw[swResult]);

                    if (swResult == StatusWord.SW_NO_ERROR)
                    {
                        if (rpc.data[0] == 0x0)
                        {
                            Trace.WriteLine("******* CheckConnection 1411");
                            HIOStaticValues.CONNECTIONBHIO = false;
                            Array.Clear(HIOStaticValues.blea.mac, 0, HIOStaticValues.blea.mac.Length);
                            Array.Clear(HIOStaticValues.blea.name, 0, HIOStaticValues.blea.name.Length);
                            return -1;
                        }
                        else
                        {
                            var conv = new Converts();
                            HIOStaticValues.blea.rssi = rpc.data[39];
                            Array.Copy(rpc.data, 1, HIOStaticValues.blea.mac, 0, 6);
                            Array.Copy(rpc.data, 7, HIOStaticValues.blea.name, 0, 32);
                            HIOStaticValues.tmain.SettingManager.MyHIOTitle = Encoding.UTF8.GetString(HIOStaticValues.blea.name).Replace("\0", "");
                            HIOStaticValues.tmain.SettingManager.MyHIOMac = "MAC: " + conv.ByteToChar(HIOStaticValues.blea.mac[0]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[1]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[2]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[3]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[4]) + ":" + conv.ByteToChar(HIOStaticValues.blea.mac[5]);
                            HIOStaticValues.CONNECTIONBHIO = true;
                            return 1;
                        }
                    }
                    Trace.WriteLine("******* CheckConnection 1429");
                    HIOStaticValues.CONNECTIONBHIO = false;
                    return -1;



                }
                catch (Exception ex)
                {
                    
                    if (ex.HResult == unchecked((int)0x8007048f))
                        return -2;
                    return -1;
                }
                finally
                {
                    rpc = null;
                }
            }
        }
        public bool SwitchResetSync()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchResetSync");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.PASS_MANAGE_CMD, (byte)Ins.RESET_SYNC_RECORD);

                    dev.Write(psp[0]);


                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(6000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();

                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null) return false;
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchResetSync: " + StatusWord.sw[swResult]);

                    if (swResult == StatusWord.SW_NO_ERROR)
                    {
                        DataBase db = new DataBase();
                        db.deleteFromDatabase("-1");
                        return true;
                    }
                    else return false;
                }
            }
            catch (Exception ex)
            {
                
                return false;
            }
            finally
            {
                rpc = null;
            }

        }
        public string SwitchGetVersion()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchGetVersion");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_INFO, (byte)Ins.GET_SWVERSION);
                    // end data field

                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                    {
                        return "";
                    }
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchGetVersion: " + StatusWord.sw[swResult]);
                    if (swResult == StatusWord.SW_NO_ERROR) return System.Text.Encoding.UTF8.GetString(rpc.data).Replace("\0", "");

                    return "";
                }
            }
            catch (Exception ex)
            {
                
                return "";
            }
            finally
            {
                rpc = null;
            }
        }
        public string GetVersionBridge()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("GetVersionBridge");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_INFO, (byte)Ins.GET_VERSION);
                    // end data field

                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }

                    /// Console.Write("\nget response");
                    //RecPacket rp = AnalyzRecRawPack(recBufData, (byte)Type.BRIDGE_INFO);
                    if (rpc == null)
                    {
                        return "";
                    }
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result GetVersionBridge: " + StatusWord.sw[swResult]);
                    if (swResult == StatusWord.SW_NO_ERROR) return System.Text.Encoding.UTF8.GetString(rpc.data).Replace("\0", "");

                    return "";
                }
            }
            catch (Exception ex)
            {
                
                rpc = null;
                return "";

            }
        }
        public byte BatteryStatus()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("BatteryStatus");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_INFO, (byte)Ins.BATTERY_STATUS);
                    // end data field

                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.IsBackground = true;
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    if (rpc == null)
                        return 0xff;

                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result BatteryStatus: " + StatusWord.sw[swResult]);
                    if (swResult == StatusWord.SW_NO_ERROR) return rpc.data[0];


                    return 0xff;

                }
            }
            catch (Exception ex)
            {
                
                return 0xff;
            }
            finally
            {
                rpc = null;
            }
        }
        public string SwitchGetDeviceName()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchGetDeviceName");

                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.GET_DEVICE_NAME);
                    // end data field

                    sendDataChain(0, psp);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null) return "";
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchGetDeviceName: " + StatusWord.sw[swResult]);
                    if (swResult == StatusWord.SW_NO_ERROR)
                    {

                        UTF8Encoding dataEnc = new UTF8Encoding();
                        string name = dataEnc.GetString(rpc.data.ToArray());
                        return name.Replace("\0", string.Empty);
                    }
                    return "";
                }
            }
            catch (Exception ex)
            {
                
                return "";
            }
            finally
            {
                rpc = null;
            }
        }
        public string SwitchDisableDevicePin(byte[] pin)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchDisableDevicePin");
                    //hid interface   
                    byte[] packet = new byte[6];
                    Array.Copy(pin, 0, packet, 0, 6);
                    List<byte[]> psp = PrepringSendPack(packet, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.DISABLE_DEVICE_PIN);
                    // end data field

                    sendDataChain(0, psp);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null) return StatusWord.SW_RECEIVE_TIMEOUT;
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchDisableDevicePin: " + StatusWord.sw[swResult]);
                    return swResult;
                }
            }
            catch (Exception ex)
            {
                
                return StatusWord.SW_RECEIVE_TIMEOUT;
            }
            finally
            {
                rpc = null;
            }
        }
        public CheckPinStatus SwitchCheckDevicePin()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchCheckDevicePin");
                    //hid interface   

                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.CHECK_DEVICE_PIN);
                    // end data field

                    sendDataChain(0, psp);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null) return CheckPinStatus.Disabled;
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchCheckDevicePin: " + StatusWord.sw[swResult]);

                    if (swResult != StatusWord.SW_NO_ERROR)
                        return CheckPinStatus.Disabled;

                    return (CheckPinStatus)rpc.data[0];
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
            finally
            {
                rpc = null;
            }
        }
        public string SwitchEnableDevicePin(byte[] pin)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchEnableDevicePin");
                    //hid interface   
                    byte[] packet = new byte[6];
                    Array.Copy(pin, 0, packet, 0, 6);
                    List<byte[]> psp = PrepringSendPack(packet, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.ENABLE_DEVICE_PIN);
                    // end data field

                    sendDataChain(0, psp);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null) return StatusWord.SW_RECEIVE_TIMEOUT;
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchEnableDevicePin: " + StatusWord.sw[swResult]);
                    return swResult;
                }
            }
            catch (Exception ex)
            {
                
                return StatusWord.SW_RECEIVE_TIMEOUT;
            }
            finally
            {
                rpc = null;
            }
        }
        public string SwitchSetDeviceName(string name)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchSetDeviceName");
                    //hid interface   
                    byte[] packet = new byte[32];
                    byte[] nameByteArray = name.GetUTF8Bytes(32).ToArray();
                    Array.Copy(nameByteArray, 0, packet, 0, (nameByteArray.Length < 32) ? nameByteArray.Length : 32);
                    
                    List<byte[]> psp = PrepringSendPack(packet, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.SET_DEVICE_NAME);
                    // end data field

                    sendDataChain(0, psp);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null) return StatusWord.SW_RECEIVE_TIMEOUT;
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchSetDeviceName: " + StatusWord.sw[swResult]);
                    return swResult;
                }
            }
            catch (Exception ex)
            {
                
                return StatusWord.SW_RECEIVE_TIMEOUT;
            }
            finally
            {
                rpc = null;
            }
        }
        public string SwitchChangeDevicePin(byte[] oldPin, byte[] newPin)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("SwitchChangeDevicePin");
                    //hid interface   
                    byte[] packet = new byte[12];

                    //data field
                    Array.Copy(oldPin, 0, packet, 0, 6);
                    Array.Copy(newPin, 0, packet, 6, 6);

                    List<byte[]> psp = PrepringSendPack(packet, new byte[] { 0x00, 0x00 }, (byte)Type.FOB_CONFIG, (byte)Ins.CHANGE_DEVICE_PIN);
                    // end data field

                    dev.Write(psp[0]);
                    dev.Write(psp[1]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(5000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null) return StatusWord.SW_RECEIVE_TIMEOUT;
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result SwitchSetPin: " + StatusWord.sw[swResult]);
                    return swResult;
                }
            }
            catch (Exception ex)
            {
                
                return StatusWord.SW_RECEIVE_TIMEOUT;
            }
            finally
            {
                rpc = null;
            }
        }
        public string BondBridge(byte[] mac, byte[] pin)
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("BondBridge");
                    //hid interface   
                    byte[] packet = new byte[12];

                    //data field
                    Array.Copy(mac, 0, packet, 0, 6);
                    Array.Copy(pin, 0, packet, 6, 6);

                    List<byte[]> psp = PrepringSendPack(packet, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_CONFIG, (byte)Ins.BOND);
                    // end data field

                    dev.Write(psp[0]);
                    dev.Write(psp[1]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(12000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.Reset();
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();

                    }
                    if (rpc == null)
                    {
                        return StatusWord.SW_RECEIVE_TIMEOUT;

                    }
                    string swResult = CheckStatusWord(rpc.sw);

                    Trace.WriteLine("Result BondBridge: " + StatusWord.sw[swResult]);
                    return swResult;
                }
            }
            catch (Exception ex)
            {
                
                return StatusWord.SW_RECEIVE_TIMEOUT;
            }
            finally
            {
                rpc = null;
            }
        }

        public object ThreadReadDevice(ref byte[] packet)
        {
            try
            {

                dev.Read(packet);
                return null;
            }
            catch (DeviceIsDisposedException)
            {
                Trace.WriteLine("ThreadReadDevice: DeviceIsDisposedException");
                rpc = new RecPacketChain { sw = new byte[] { 0xff, 0xff } };
                return null;
            }
            catch (Exception ex)
            {
                
                if (ex.HResult == NOTCONNECTED)
                {
                    dev.Close();
                    return null;
                }
                
                return null;
            }
            finally
            {


                // dev.fileStream.Close();
            }


        }
        public List<TDevice> ScanBluetooth()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                     Trace.WriteLine("ScanBluetooth");
                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_CONFIG, (byte)Ins.SCAN);


                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(3000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data

                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }
                    //get data from ble

                    if (rpc == null) return null;
                    string swResult = CheckStatusWord(rpc.sw);
                    if (swResult != StatusWord.SW_NO_ERROR)
                    {
                        Trace.WriteLine("Result ScanBluetooth: " + StatusWord.sw[swResult]);
                        return null;

                    }

                    List<TDevice> arp = AnalyzeBluetoothListPacks(rpc.data);
                    return arp;

                }
            }
            catch (Exception ex)
            {
                
                return null;
            }
            finally
            {
                rpc = null;
            }
        }


        private List<TDevice> AnalyzeBluetoothListPacks(byte[] rawData)
        {
            try
            {
                 Trace.WriteLine("AnalyzeBluetoothListPacks");
                Commands ic = new Commands();
                List<TDevice> bl = new List<TDevice>();
                int counter = 0;

                if (Array.TrueForAll(rawData, value => (value == 0x00)) == true) return null;


                for (int i = 0; i < rawData.Length; i += 39, counter++)
                {
                    bl.Add(new TDevice(HIOStaticValues.tmain?.SettingManager, Encoding.UTF8.GetString(rawData.Skip(i + 6).Take(32).ToArray()).Replace("\0", ""), ic.GetSignalStatus(rawData[i + 38]), rawData.Skip(i).Take(6).ToArray()));
                }
                return bl;
            }
            catch (Exception ex)
            {
                
                return null;

            }
        }


        public void Write(Dictionary<string, string> dataDic, Source source, bool filledFrom = false)
        {

            try
            {
                bool CapsLock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                // Create a new order 
                SendData sentOrder = new SendData();
                // Connect to a queue on the local computer.
               

                switch (source)
                {

                    case Source.CHROME: //chrome
                        if (!MessageQueue.Exists(HIOStaticValues.PATHMSGCHROME))
                            MessageQueue.Create(HIOStaticValues.PATHMSGCHROME);
                        MessageQueue chromeQueue = new MessageQueue(HIOStaticValues.PATHMSGCHROME);

                        /////////////////////
                        if (filledFrom)
                            win.SetActiveWindowTitle();
                        if (filledFrom == true)
                        {
                            //   JArray jA = JArray.Parse("[" + data.ToString() + "]");

                            if (!dataDic.Keys.Any(k => k == "USER"))
                            {
                                //fill pass
                                Thread.Sleep(300);
                                //  string password = "";
                                string passText = Convert.ToString(dataDic["DATA"]);
                                //   password = passText.Replace("{", "[OPENBRACET]").Replace("}", "[CLOSEBRACET]").Replace("%", "{%}").Replace("+", "{+}").Replace("(", "{(}").Replace(")", "{)}").Replace("[OPENBRACET]", "{{}").Replace("[CLOSEBRACET]", "{}}");
                                //  SendKeys.SendWait("^{a}");
                                //    SendKeys.SendWait("{DELETE}");
                                // if (CapsLock) SendKeys.SendWait("{CAPSLOCK}" + password);
                                // else SendKeys.SendWait(password);
                                HIOStaticValues.sim.Keyboard.TextEntry(passText);
                                HIOStaticValues.username = "";
                                
                            }

                            else
                            {
                                Thread.Sleep(300);
                                string usernameText = Convert.ToString(dataDic["USER"]);
                                HIOStaticValues.sim.Keyboard.TextEntry(usernameText);
                               
                            }


                        }
                        /////////////////////
                        // Create a new order and set values.
                        sentOrder.label = "Chrome"; //set header
                        sentOrder.data = JsonConvert.SerializeObject(dataDic, Formatting.None); ;
                        //  set values.
                        chromeQueue.Send(sentOrder);
                        chromeQueue.Purge();

                        break;
                    case Source.WINDOWS: //windows
                        string dataField = "";
                        //  JArray jArray = JArray.Parse("[" + data + "]");
                        if (!dataDic.Keys.Any(k => k == "USERS"))
                        {
                            dataField = Convert.ToString(dataDic["DATA"]);
                            HIOStaticValues.username = "";
                        }
                        else
                            dataField = Convert.ToString(dataDic["USER"]);

                        win.SetActiveWindowTitle();
                        if (CapsLock) SendKeys.SendWait("{CAPSLOCK}" + dataField);
                        else SendKeys.SendWait(dataField);

                        break;
                    case Source.FIREFOX: //firefox
                        if (!MessageQueue.Exists(HIOStaticValues.PATHMSGFF))
                            MessageQueue.Create(HIOStaticValues.PATHMSGFF);
                        MessageQueue ffQueue = new MessageQueue(HIOStaticValues.PATHMSGFF);

                        if (filledFrom)
                            win.SetActiveWindowTitle();
                        if (filledFrom == true)
                        {
                            if (!dataDic.Keys.Any(k => k == "USER"))
                            {
                                //fill pass
                                Thread.Sleep(300);
                                string passText = Convert.ToString(dataDic["DATA"]);
                                HIOStaticValues.sim.Keyboard.TextEntry(passText);
                                HIOStaticValues.username = "";
                            }

                            else
                            {
                                Thread.Sleep(300);
                                string usernameText = Convert.ToString(dataDic["USER"]);
                                HIOStaticValues.sim.Keyboard.TextEntry(usernameText);
                                Thread.Sleep(1000);
                            }


                        }
                        /////////////////////
                      
                       
                        sentOrder.label = "FIREFOX"; //set header
                        sentOrder.data = JsonConvert.SerializeObject(dataDic, Formatting.None); ;

                        // Send the Order to the queue.
                        ffQueue.Send(sentOrder);
                        ffQueue.Purge();


                        break;
                    case Source.EDGE: //edge
                        if (!MessageQueue.Exists(HIOStaticValues.PATHMSGEDGE))
                            MessageQueue.Create(HIOStaticValues.PATHMSGEDGE);
                        MessageQueue edgeQueue = new MessageQueue(HIOStaticValues.PATHMSGEDGE);

                        if (filledFrom)
                            win.SetActiveWindowTitle();
                        if (filledFrom == true)
                        {
                            if (!dataDic.Keys.Any(k => k == "USER"))
                            {
                                //fill pass
                                Thread.Sleep(300);
                                string passText = Convert.ToString(dataDic["DATA"]);
                                HIOStaticValues.sim.Keyboard.TextEntry(passText);
                                HIOStaticValues.username = "";
                            }

                            else
                            {
                                Thread.Sleep(300);
                                string usernameText = Convert.ToString(dataDic["USER"]);
                                HIOStaticValues.sim.Keyboard.TextEntry(usernameText);
                                Thread.Sleep(1000);
                            }


                        }
                        /////////////////////


                        sentOrder.label = "EDGE"; //set header
                        sentOrder.data = JsonConvert.SerializeObject(dataDic, Formatting.None); ;

                        // Send the Order to the queue.
                        edgeQueue.Send(sentOrder);
                        edgeQueue.Purge();


                        break;
                }

            }
            catch (Exception ex)
            {
                
            }
            finally
            {

            }

        }

        private void threadTimeoutResponse()
        {
            HIOStaticValues._signalRec.WaitOne();

        }
        public List<BluetoothList> GetBondedListBridge()
        {
            try
            {
                lock (HIOStaticValues.Cmdlocker)
                {
                    List<BluetoothList> listBLE = new List<BluetoothList>();
                     Trace.WriteLine("GetBondedListBridge");
                    byte[] Pack = new byte[0];

                    List<byte[]> psp = PrepringSendPack(new byte[] { }, new byte[] { 0x00, 0x00 }, (byte)Type.BRIDGE_CONFIG, (byte)Ins.GET_BONDED_LIST);
                    // end data field

                    dev.Write(psp[0]);
                    CancellationTokenSource _cts = new CancellationTokenSource();
                    Thread threadTimeout = new Thread(() => threadTimeoutFunc(10000, _cts));
                    threadTimeout.SetApartmentState(ApartmentState.STA);
                    threadTimeout.Start();
                    // check record data
                    _signal.WaitOne();
                    if (threadTimeout.IsAlive)
                    {
                        _cts.Cancel();
                    }

                    if (rpc == null)
                    {
                        return null;

                    }
                    string swResult = CheckStatusWord(rpc.sw);
                    if (swResult != StatusWord.SW_NO_ERROR)
                    {
                        Trace.WriteLine("Result GetBondedListBridge: " + StatusWord.sw[swResult]);
                        return null;

                    }
                    Pack = rpc.data;

                    for (int i = 0; i < Pack.Length; i += PACKETBLEBUF)
                    {
                        byte[] macTemp = new byte[6];
                        byte[] nameTemp = new byte[32];

                        Array.Copy(Pack, i, macTemp, 0, 6);
                        Array.Copy(Pack, i + 6, nameTemp, 0, 32);
                        listBLE.Add(new BluetoothList { mac = macTemp, name = nameTemp });

                    }
                    if (listBLE.Count > 0) return listBLE;
                    return null;
                }
            }
            catch (Exception ex)
            {
                
                return null;
            }
            finally
            {

                rpc = null;
            }

        }
        private string CheckStatusWord(byte[] sw)
        {

            switch (BitConverter.ToString(sw).Replace("-", "").ToUpper())
            {
                case StatusWord.SW_NO_ERROR:
                    break;
                case StatusWord.SW_CONDITIONS_NOT_SATISFIED:
                    break;
                case StatusWord.SW_SYNC_IS_REQUIRED:
                    HIOStaticValues.popUp("Sync data is required.");

                    HIOStaticValues.commandQ.Add(() =>
                    {
                        DataBase db = new DataBase();
                        Commands cmd = new Commands();
                        HIOStaticValues.SYNC_ON = true;
                        db.deleteFromDatabase("-1");

                        cmd.Sync();
                        HIOStaticValues.SYNC_ON = false;

                    });

                    break;
                case StatusWord.SW_WRONG_DATA:
                    Trace.WriteLine("Something went wrong!\nYou may need to use 'Reset Sync' button to refresh the database.");
                    //HIOStaticValues.popUp("Something went wrong!\nYou may need to use 'Reset Sync' button to refresh the database.");
                    break;
                case StatusWord.SW_INS_NOT_SUPPORTED:
                    break;
                case StatusWord.SW_BRIDGE_INSUFFICIENT_MEMORY:
                    break;
                case StatusWord.SW_MEM_IS_FULL:
                    // if(HIOStaticValues.FormHide==true)
                    HIOStaticValues.popUp("There is not enough space available on HIO");
                    break;
                case StatusWord.SW_BUSY_STATE:
                    rpc = null;

                    break;
                case StatusWord.SW_PIN_NOT_VERIFIED:
                    break;
                case StatusWord.SW_RECEIVE_TIMEOUT:
                    break;
                case StatusWord.SW_BAD_SEQUENCE:
                    break;
                case StatusWord.SW_APP_NOT_INRODUCED:
                    break;
                case StatusWord.SW_BRIDGE_BOND_FULL:
                    break;
                case StatusWord.SW_BLE_CONNECTION_IS_NOT_AVAILABLE:
                    break;
                case StatusWord.SW_FS_WRITE_READ_ERROR:
                    break;
                case StatusWord.SW_DURING_INITIATING_BLE_CONNECTION:
                    break;

            }
            return BitConverter.ToString(sw).Replace("-", "").ToUpper();
        }
    }
}
