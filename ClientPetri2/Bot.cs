using ClientPetri;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace ClientPetri2
{
    class Bot
    {
        private WebSocket ws;
        private Client client;
        private bool ok = false;
        public Bot(Client client)
        {
            this.client = client;
            ws = new WebSocket("ws://192.168.11.3:20000");
            ws.Connect();
            ws.OnClose += Ws_OnClose;
            ws.OnMessage += Ws_OnMessage;
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            Thread.Sleep(3000);
            ws.Connect();
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            /*ok = true;
            dynamic data = JsonConvert.DeserializeObject(e.Data);
            if (data.id == null && data.message == null)
            {
                if (client.following)
                {
                    client.specX = data.x;
                    client.specY = data.y;
                }
            } else
            {
                client.botid = data.id;
                if (client.previd != null)
                {
                    if (Blob.blobs.ContainsKey(client.previd.id) && client.previd.id != client.botid)
                    {
                        Blob.blobs[client.previd.id].SetColor(client.previd.color);
                    }
                    if (Blob.blobs.ContainsKey(data.id))
                    {
                        client.previd = new Id()
                        {
                            id = data.id,
                            color = Blob.blobs[data.id].color
                        };

                        Blob.blobs[data.id].color = "FFFFFF";
                    }
                }
            }*/
        }

        private void Send(object o)
        {
            if (!ok && o as Packet0 == null) return;
            if (ws.IsAlive)
            {
                ws.Send(JsonConvert.SerializeObject(o));
                
            }
        }
        public void Join(uint snum, uint tokena)
        {
            Send(new Packet0 { flag = 0, snurmd = snum, tokenad = tokena });
        }



        public void SendMouse(double x, double y)
        {
            Send(new Packet1 { flag = 1, x = x, y = y });
        }

        public void SendValue(int value)
        {
            Send(new Packet2 { flag = 2, value = value});
        }


        class Packet0
        {
            public int flag;
            public uint snurmd;
            public uint tokenad;
        }
        class Packet1
        {
            public int flag;
            public double x;
            public double y;
        }

        class Packet2
        {
            public int flag;
            public int value;
        }

        public class Id
        {
            public uint id;
            public string color;
        }
    }
}
