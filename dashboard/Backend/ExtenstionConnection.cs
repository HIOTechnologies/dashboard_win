using HIO.Controls;
using HIO.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
namespace HIO.Backend
{
    class ExtenstionConnection
    {
        public void ChromeConnectionSend(TMain tMain)
        {

            try
            {
                do
                {
                    // Connect to the a queue on the local computer.
                    if (!MessageQueue.Exists(HIOStaticValues.PATHMSGREC))
                        MessageQueue.Create(HIOStaticValues.PATHMSGREC);
                    if (!MessageQueue.Exists(HIOStaticValues.PATHMSGCHROME))
                        MessageQueue.Create(HIOStaticValues.PATHMSGCHROME);
                    if (!MessageQueue.Exists(HIOStaticValues.PATHMSGFF))
                        MessageQueue.Create(HIOStaticValues.PATHMSGFF);
                    if (!MessageQueue.Exists(HIOStaticValues.PATHMSGEDGE))
                        MessageQueue.Create(HIOStaticValues.PATHMSGEDGE);
                    MessageQueue myQueue = new MessageQueue(HIOStaticValues.PATHMSGREC);
                    MessageQueue QueueSendFF = new MessageQueue(HIOStaticValues.PATHMSGFF);
                    MessageQueue QueueSendChrome = new MessageQueue(HIOStaticValues.PATHMSGCHROME);
                    MessageQueue QueueSendEdge = new MessageQueue(HIOStaticValues.PATHMSGEDGE);
                    SendData sentOrder = new SendData();
                    sentOrder.label = "HIO"; //set header
                    sentOrder.data = "true";
                    // Set the formatter to indicate body contains an Order.
                    myQueue.Formatter = new XmlMessageFormatter(new System.Type[] { typeof(SendData) });

                    while (true)
                    {
                        try
                        {
                            // Receive and format the message. 
                            Message myMessage = myQueue.Receive();
                            myQueue.Purge();
                            SendData recData = (SendData)myMessage.Body;
                            if (recData.data == "true")
                            {
                                HIOStaticValues.checkRec = true;
                                HIOStaticValues._signalRec.Set();
                                continue;
                            }
                          
                            // Display message information.
                            if (recData.label == "Chrome")
                            {
                                QueueSendChrome.Send(sentOrder);
                                HIOStaticValues.SOURCE = (int)Source.CHROME;
                                Task.Run(() =>
                                {
                                    Debug.WriteLine(recData.data);
                                    ProcessMessage pm = new ProcessMessage();
                                    pm.ProcessRecieveMessage(JsonConvert.DeserializeObject<JObject>(recData.data), Source.CHROME);

                                });

                            }else if (recData.label == "Firefox")
                            {
                                QueueSendFF.Send(sentOrder);
                                HIOStaticValues.SOURCE = (int)Source.FIREFOX;
                                Task.Run(() =>
                                {
                                    Debug.WriteLine(recData.data);
                                    ProcessMessage pm = new ProcessMessage();
                                    pm.ProcessRecieveMessage(JsonConvert.DeserializeObject<JObject>(recData.data), Source.FIREFOX);

                                });

                            }
                        else if (recData.label == "Edge")
                        {
                            QueueSendEdge.Send(sentOrder);
                            HIOStaticValues.SOURCE = (int)Source.EDGE;
                            Task.Run(() =>
                            {
                                Debug.WriteLine(recData.data);
                                ProcessMessage pm = new ProcessMessage();
                                pm.ProcessRecieveMessage(JsonConvert.DeserializeObject<JObject>(recData.data), Source.EDGE);

                            });

                        }
                    }
                        catch (Exception ex)
                        {

                         
                        }


                    }
                } while (true);
            }
            catch (Exception ex)
            {
              

            }
        }
    }
}
