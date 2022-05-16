using ClientPetri;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using ClientPetri2.Properties;
using System.Threading;
using System.Reflection;
using WebSocketSharp.Net.WebSockets;
using System.Net;

namespace ClientPetri2
{
    class Client
    {
        private string wsAddress;
        private WebSocketSharp.WebSocket ws;
        private Random rnd = new Random();
        private byte serverProtocol = 19;

        public double specX = 7500, specY = 7500;
        private byte chunk;
        private int maxchunk = 5600;

        public double X, Y;
        public uint ID;

        private WebClient wc = new WebClient();

        private Queue<byte[]> messages = new Queue<byte[]>();
        private double angle;

        private Mutex _dataMutex;

        public bool spawned = true;

        public double mergeX, mergeY;

        private uint previd;

        private int mergeCount = 4;

        public int MapMaxX;
        public int MapMaxY;

        const uint T1 = 665;
        const uint T2 = 99999998;

        public List<string> chat = new List<string>();

        public Client(string wsAddress)
        {
            this.wsAddress = wsAddress;
            _dataMutex = new Mutex();
        }
        
        public void Connect()
        {
            ws = new WebSocketSharp.WebSocket(wsAddress);
            ws.Origin = "http://petridish.pw";
            ws.OnMessage += Ws_OnMessage;
            ws.OnClose += (s, e) => {
                //chat.Add("Соединение закрыто!");
                Thread.Sleep(10000);
                Connect();
            };
            ws.OnOpen += (s, e) => {
                SendRaw(CreateToken(254, T1));
                SendRaw(CreateToken(255, T2));
                SendRaw(new byte[] { 253, 7 });
                SendRaw(new byte[] { 79, 1 });
                SendRaw(SendNick("PetriDish.pw"));
                SendRaw(new byte[] { 77 });
                SendRaw(new byte[] { 78, 0, 0, 0, 0 });
                SendRaw(new byte[] { 247, 0, 0, 0, 0 });
                SendRaw(SendChat("***playerenter***"));
                SendRaw(SendChat("***playerenter***"));
                //wc.DownloadData("http://data3.petri-dish.ru/engine/formobile/sticker_management.txt");

            };
            ws.Connect();
        }

        private void SendRaw(byte[] raw)
        {
            if (ws.ReadyState == WebSocketState.Open)
            {
                ws.Send(raw);
            }
        }

        public void Chat(string msg)
        {
            SendRaw(SendChat(msg + " :ru"));
        }

        public void Respawn()
        {
            SendRaw(new byte[] { 2 });
        }

        private byte[] SendChat(string str)
        {
            MemoryStream ms = new MemoryStream(2 + 2 * str.Length);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)99);
            bw.Write((byte)0);
            for (int i = 0; i < str.Length; i++)
            {
                bw.Write((ushort)str[i]);
            }
            bw.Close();
            byte[] data = ms.ToArray();
            ms.Close();
            return data;
        }

        private byte[] SendNick(string nick)
        {
            nick += "::::::::::3";
            MemoryStream ms = new MemoryStream(1 + 2 * nick.Length);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)0);
            for (int i = 0; i < nick.Length; i++)
            {
                bw.Write((ushort)nick[i]);
            }
            bw.Close();
            byte[] data = ms.ToArray();
            ms.Close();
            return data;
        }

        private byte[] CreateToken(byte id, uint value)
        {
            MemoryStream ms = new MemoryStream(5);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(id);
            bw.Write(value);
            bw.Close();
            byte[] data = ms.ToArray();
            ms.Close();
            return data;
        }

        public void SetCursor(double x, double y)
        {
            //X = Math.Max(Math.Min(x, 15000), 0);
            //Y = Math.Max(Math.Min(y, 15000), 0);

            X = x;
            Y = y;
            SendRaw(updateMousePosition(x, y));
        }

        private byte[] updateMousePosition(double x, double y)
        {
            MemoryStream ms = new MemoryStream(21);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)16);
            bw.Write(Math.Floor(x));
            bw.Write(Math.Floor(y));
            //Console.ForegroundColor = ConsoleColor.DarkMagenta;
            //Console.WriteLine((double)Math.Floor(x) + "::::::" + (double)Math.Floor(y));
            //Console.ResetColor();
            bw.Write((int)0);
            bw.Close();
            byte[] data = ms.ToArray();
            ms.Close();
            return data;
        }

        public void Split(byte id)
        {
            SendRaw(new byte[] { id });
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            messages.Enqueue(e.RawData);
        }

        private void OnCellEaten(Blob blob1, Blob blob2, DateTime timestampLastDraw)
        {
            blob2.destroy();

            blob2.ox = blob2.x;

            blob2.oy = blob2.y;

            blob2.oSize = blob2.size;

            blob2.nx = blob1.x;

            blob2.ny = blob1.y;

            blob2.nSize = blob2.size;

            /** @type {number} */

            blob2.updateTime = timestampLastDraw;
        }

        private void qweR(BinaryReader reader)
        {
            //ids = [];
            /** @type {number} */
            DateTime timestampLastDraw = DateTime.Now;
            /** @type {number} */
            var rand = rnd.NextDouble();
            /** @type {boolean} */
            bool qweA = false;
            var blobsEaten = 0;
            if (serverProtocol >= 18)
            {
                blobsEaten = reader.ReadInt32();
                //offset += 4;
            }
            else
            {
                blobsEaten = reader.ReadUInt16();
                //offset += 2;
            }
            /** @type {number} */
            var i = 0;
            for (; i < blobsEaten; ++i)
            {
                var blob1i = reader.ReadUInt32();
                var blob2i = reader.ReadUInt32();
                //offset += 8;
                if (Blob.blobs.ContainsKey(blob1i))
                {
                    if (Blob.blobs.ContainsKey(blob2i))
                    {
                        Blob blob1 = Blob.blobs[blob1i];
                        Blob blob2 = Blob.blobs[blob2i];
                        //if (Blob.ids.Count == 1)
                        //{
                        //Console.WriteLine(Blob.ids.Count + ":" + Blob.ids.Contains(blob2.id) + ":" + !Blob.ids.Contains(blob1.id) + "::::" + blob1.name);
                        //}
                        if (Blob.ids.Count == 1 && Blob.ids.Contains(blob2.id) && !Blob.ids.Contains(blob1.id) && blob1.name.Length > 0)
                        {
                            // SendRaw(SendChat(string.Format("{0} отойди от меня и не трогай больше! :en", blob1.name)));
                        }

                        if (blob1.cellType != 1)
                        {
                            if (blob2.cellType != 1)
                            {
                                //console.log(blob1,blob2);
                            }
                        }
                        OnCellEaten(blob1, blob2, timestampLastDraw);

                        blob2.destroy();
                        blob2.ox = blob2.x;
                        blob2.oy = blob2.y;
                        blob2.oSize = blob2.size;
                        blob2.nx = blob1.x;
                        blob2.ny = blob1.y;
                        blob2.nSize = blob2.size;
                        /** @type {number} */
                        blob2.updateTime = timestampLastDraw;
                    }
                }


            }



            /** @type {number} */
            i = 0;
            for (; ; )
            {
                //console.log('sss ' + serverProtocol);
                uint id = reader.ReadUInt32();
                //offset += 4;
                var activeCell = false;
                if (0 == id)
                {
                    break;
                }
                ++i;
                var cellType = -1;
                var tbdteam = 0;
                var tbdbaza = false;

                if (serverProtocol >= 17)
                {
                    cellType = reader.ReadByte();
                    //offset += 1;
                }

                int x = -1, y = -1;

                if (serverProtocol <= 15)
                {
                    x = reader.ReadInt16();
                    //offset += 2;
                    y = reader.ReadInt16();
                    // offset += 2;
                }

                if (serverProtocol >= 16)
                {
                    //            var x = reader.getUint32(offset, true);
                    x = reader.ReadInt32();
                    //offset += 4;
                    //            var y = reader.getUint32(offset, true);
                    y = reader.ReadInt32();
                    //offset += 4;
                }

                var size = reader.ReadInt16();
                //console.log(size);
                //offset += 2;
                var sticker = 0;
                var skin = 0;
                var ownCell = false;
                if ((serverProtocol >= 12) && (serverProtocol < 15))
                {
                    sticker = reader.ReadByte();
                    //offset += 1;
                }
                if (serverProtocol >= 15)
                {
                    sticker = reader.ReadInt32();
                    //offset += 4;
                }
                if (serverProtocol >= 14)
                {
                    skin = reader.ReadInt32();
                    //offset += 4;
                }
                byte colorb = reader.ReadByte();
                var flags = reader.ReadByte();
                var isVirus = reader.ReadByte();
                /** @type {string} */
                string color = Convert.ToString(colorb << 16 | flags << 8 | isVirus, 16);
                for (; 6 > color.Length;)
                {
                    /** @type {string} */
                    color = "0" + color;
                }


                //globalFoodCache[color]++; 

                flags = reader.ReadByte();
                /** @type {boolean} */
                isVirus = !!((flags & 1) != 0) ? (byte)1 : (byte)0;
                /** @type {boolean} */
                var isAgitated = false;
                if ((flags & 2) != 0)
                {
                    reader.ReadBytes(4);
                    //offset += 4;
                }
                if ((flags & 4) != 0)
                {
                    reader.ReadBytes(8);
                    //offset += 8;
                }
                if ((flags & 8) != 0)
                {
                    reader.ReadBytes(16);
                    //offset += 16;
                }
                if ((flags & 32) != 0)
                {
                    ownCell = true;
                    if (Blob.ids.IndexOf(id) == -1)
                    {
                        //console.log(id);
                        Blob.ids.Add(id);

                        //console.log('pushed', id, ownCell);
                        //				if (isSpectating) {
                        //					setSpectate(false); // trying to fix clicks
                        //				}
                    }
                }
                var coronamask = 0;
                if (serverProtocol >= 19)
                {
                    coronamask = reader.ReadByte();
                }

                //console.log(coronamask);

                ushort readChar;
                /** @type {string} */
                var name = "";
                for (; ; )
                {
                    readChar = reader.ReadUInt16();
                    //offset += 2;
                    if (0 == readChar)
                    {
                        break;
                    }
                    name += (char)readChar;
                }


                /*if ((flags & 64) != 0)
                {
                    if (recievedgamemode != 77)
                    {
                        activeCell = true;
                    }
                    if (recievedgamemode == 77)
                    {
                        //			console.log('hurt64', name);
                        tbdteam = 1;
                    }
                }
                else
                {
                    if (recievedgamemode == 77)
                    {
                        //	console.log('hurt64', name);
                        tbdteam = 2;
                    }
                }

                if (flags & 128)
                {
                    if (recievedgamemode == 77)
                    {
                        tbdbaza = true;
                        //	console.log('hurt128', name);
                    }
                }*/

                /** @type {null} */
                Blob blob = null;
                if (Blob.blobs.ContainsKey(id))
                {
                    blob = Blob.blobs[id];
                    blob.updatePos(timestampLastDraw);
                    blob.ox = blob.x;
                    blob.oy = blob.y;
                    blob.oSize = blob.size;
                    blob.color = color;
                    blob.sticker = sticker;
                    blob.skin = skin;
                    blob.ownCell = ownCell;
                    //blob.activeCell = activeCell;
                    //blob.tbdteam = tbdteam;
                    //blob.tbdbaza = tbdbaza;
                    //blob.coronamask = coronamask;
                    //console.log('check1', id, blob.ownCell);
                    //if (squareskins.indexOf(skin) != -1) { blob.isSquareSkin = true;  }
                    blob.cellType = cellType;
                    /*if ((window.selmode == "ARENA") || (window.selmode == "FATBOY-ARENA")) {
                        if (ELnHfjshok[0]) {
                            for (var i = 0, leng = ELnHfjshok.length; i < leng; i++) {
                                if (i in ELnHfjshok) {
                                    if (blob.id == ELnHfjshok[i].id) {
                                        if (window.selmode == "ARENA") { blob.color = "#eb4b00"; }
                                        if (window.selmode == "FATBOY-ARENA") { blob.color = "#eb4b00"; }
                                    }
                                }
                            }

                        }
                    }*/

                    /*if (ELnHfjshok[0]) {
                        //myx = ~~(ELnHfjshok[0].x);
                        //myy = ~~(ELnHfjshok[0].y);
                        mycolo = ELnHfjshok[0].color;
                    }*/

                }
                else
                {
                    blob = new Blob(this, id, x, y, size, color, name);
                    Blob.list.Add(blob);
                    Blob.blobs[id] = blob;
                    blob.sticker = sticker;
                    blob.skin = skin;
                    blob.ownCell = ownCell;
                    // blob.activeCell = activeCell;
                    //blob.tbdteam = tbdteam;
                    //blob.tbdbaza = tbdbaza;
                    //blob.coronamask = coronamask;
                    //console.log('check2', id, blob.ownCell);
                    //if (squareskins.indexOf(skin) != -1) { blob.isSquareSkin = true;  }
                    blob.cellType = cellType;
                }
                /** @type {boolean} */
                blob.isVirus = isVirus == 1;
                /** @type {boolean} */
                blob.isAgitated = isAgitated;
                blob.nx = x;
                blob.ny = y;
                blob.nSize = size;
                /** @type {number} */
                blob.updateCode = rand;
                /** @type {number} */
                blob.updateTime = timestampLastDraw;
                blob.flags = flags;
                blob.ownCell = ownCell;
                // blob.activeCell = activeCell;
                //blob.tbdteam = tbdteam;
                //blob.tbdbaza = tbdbaza;
                //blob.coronamask = coronamask;
                //console.log('check3', id, blob.ownCell);
                //	    console.log(blob.name,blob.cellType,blob.skin);




                if (name.Length > 0)
                {
                    blob.setName(name);



                }

                /*if (blob.cellType != 1) {
                    //console.log('cell', blob);
                }
                var uuy = window.selmode;
                if ((blob.size == 708) && !(name) && (uuy == "BLACKHOLE")) {
                    console.log("i see the hole", blob);
                    blob.skin = 1926;
                    blob.setName('blackhole');

                }
                if ((blob.size > 158) && !(name) && (recievedgamemode == 113)) { blob.skin = 24927; }
                if (blob.skin == 37830) { blob.setName('покрышка'); }


                if (blob.skin == 1926) {
                    //console.log("i see the hole2",blob);
                    blob.setName('black hole');
                    blob.isTransparentSkin = true;
                    blob.isInvisibleNick = true;

                }

                if ((blob.isVirus) && (recievedgamemodeExtended != 17) && (recievedgamemodeExtended != 10) && (recievedgamemodeExtended != 30) && (recievedgamemodeExtended != 11) && (!isCustomBombc)) {
                    blob.skin = 63895;
                    if (blob.size > 140) {
                        blob.skin = 209904;
                        blob.isTransparentSkin = true;
                        if (recievedgamemode == 99) {
                            blob.skin = 220673;
                        }
                    }
                }

                if ((blob.isVirus) && (recievedgamemodeExtended == 30)) {
                    if (blob.skin) {
                        if (blob.skin > 0) {
                            //blob.skin = blob.skin;
                            blob.egg = true;
                            blob.isTransparentSkin = true;
                        }
                    }
                    else {
                        blob.skin = 63895;
                    }
                }*/


                //&& (blob.isVirus) // 63895 76868 84969
                //86608 podarok
                //84969 ng!!!!!
                //63895 radioactive
                //109090 2 goda
                //209902 209903 209904 burger


                //if (-1 != Blob.ids.indexOf(id))
                //{
                //    /*if (-1 == ELnHfjshok.indexOf(blob))
                //    {
                //        /** @type {string} */
                //        //document.getElementById("menuoverlay").style.display = "none";
                //        if (ELnHfjshok.length < 1)
                //        {
                //            //hide();
                //        }

                //        ELnHfjshok.push(blob);
                //        if (1 == ELnHfjshok.length)
                //        {
                //            offsetX = blob.x;
                //            offsetY = blob.y;
                //            //ResetChart();
                //            score = 0;
                //            OnGameStart(ELnHfjshok);
                //            settednick = ELnHfjshok[0].name;
                //            //console.log(ELnHfjshok[0].color);
                //            //if (uuy == "ARENA") { ELnHfjshok[0].color = "#eb4b00";  }
                //            //if (uuy == "FATBOY-ARENA") { ELnHfjshok[0].color = "#eb4b00";  }
                //            //console.log(ELnHfjshok[0].color);

                //        }
                //    }
                //}
            }
            var qweT = reader.ReadUInt32();
            //offset += 4;
            /** @type {number} */
            i = 0;
            for (; i < qweT; i++)
            {
                var id = reader.ReadUInt32();
                //offset += 4;
                if (Blob.blobs.ContainsKey(id))
                {
                    Blob blob = Blob.blobs[id];
                    if (null != blob)
                    {
                        blob.destroy();
                    }
                }
            }
            if (Blob.ids.Count == 0)
            {
                if (this.spawned)
                {
                    ID = 0;
                    Console.WriteLine("Respawn!");
                    spawned = false;
                    Respawn();
                }
            }
            else
            {
                spawned = true;
            }
        }

        private static string ReadStringPetri(BinaryReader reader)
        {
            string text = "";
            ushort code;
            while ((code = reader.ReadUInt16()) != 0)
            {
                text += (char)code;
            }
            return text;

        }

        public void Update()
        {
            _dataMutex.WaitOne();
            while (messages.Count > 0)
            {
                byte[] data = messages.Dequeue();
                if (data != null)
                {
                    MemoryStream packet = new MemoryStream(data);
                    BinaryReader reader = new BinaryReader(packet);
                    byte packetID = reader.ReadByte();
                    Console.WriteLine("packet " + packetID);
                    switch (packetID)
                    {
                        case 16:
                            qweR(reader);
                            break;
                        case 17:
                            specX = reader.ReadSingle();
                            specY = reader.ReadSingle();
                            chunk = reader.ReadByte();

                            Console.WriteLine("x:{0} y:{1} c:{2}", specX, specY, chunk);
                            break;
                        case 32:
                            uint id = reader.ReadUInt32();
                            Blob.myids++;
                            break;
                        case 64:
                            var minX = reader.ReadDouble();
                            var minY = reader.ReadDouble();
                            var maxX = reader.ReadDouble();
                            var maxY = reader.ReadDouble();

                            MapMaxX = (int)maxX;
                            MapMaxY = (int)maxY;
                            break;
                        case 99:
                            PushChat(reader);
                            break;
                        case 92:
                            chat.Add("Превышено количество соединений!!!");
                            break;
                        default:
                            Console.WriteLine("UNKNOWN PACKET " + packetID);
                            break;
                    }

                }
            }
            //Logic();
            _dataMutex.ReleaseMutex();
        }

        private void PushChat(BinaryReader reader)
        {
            string userlevel = "";
            string userlevel_season = "";

            var pwd = 0;

            var flags = reader.ReadByte();
            if (flags == 5)
            {
                pwd = 1;
            }
            // for future expansions
            if ((flags & 2) != 0)
            {
                long offset = reader.BaseStream.Position;
                reader.BaseStream.Seek(2, SeekOrigin.Begin);
                byte[] rawuserlevel = reader.ReadBytes(4).Reverse().ToArray();
                userlevel = BitConverter.ToUInt32(rawuserlevel, 0).ToString();
                reader.BaseStream.Seek(offset + 4, SeekOrigin.Begin);

            }
            if ((flags & 4) != 0)
            {
                long offset = reader.BaseStream.Position;
                reader.BaseStream.Seek(2, SeekOrigin.Begin);
                byte[] rawuserlevel = reader.ReadBytes(4).Reverse().ToArray();
                userlevel = " (" + BitConverter.ToUInt32(rawuserlevel, 0) + ")";
                byte[] rawuserlevel_season = reader.ReadBytes(4).Reverse().ToArray();
                userlevel_season = " (" + BitConverter.ToUInt32(rawuserlevel_season, 0) + ")";
                reader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
            }
            if ((flags & 8) != 0)
            {
                reader.ReadBytes(16);
            }

            byte r = reader.ReadByte(),
                g = reader.ReadByte(),
                b = reader.ReadByte();
            //    color = (r << 16 | g << 8 | b).toString(16);

            string color = Convert.ToString((1 << 24) + (r << 16) + (g << 8) + b, 16).Remove(1, 1);

            //        while (color.length > 6) {
            //          color = '0' + color;
            //    }
            //  color = '#' + color;

            var dt = DateTime.Now;
            string label = string.Format("[{0}] {1}{2} {4} >> {5}",
                dt.ToLongTimeString(), userlevel, userlevel_season,
                color, ReadStringPetri(reader), ReadStringPetri(reader));


            string[] segments = label.Split(' ');
            List<string> lines = new List<string>();
            int wrap = 0;
            string line = "";
            for (int i = 0; i < segments.Length; i++)
            {
                line += segments[i] + " ";
                wrap += segments[i].Length;
                if (wrap > 30)
                {
                    lines.Add(line);
                    wrap = 0;
                    line = "   ";
                }
            }
            if (line != "   ")
                lines.Add(line);

            foreach (var l in lines)
            {
                chat.Add(l);
                if (chat.Count > 20)
                {
                    chat.RemoveAt(0);
                }
            }

            //console.log('[CHAT] ' + dt.toLocaleTimeString() + userlevel + userlevel_season + color + " >>> " + getString() + ": " + getString());
        }

        private void Logic()
        {
           /* int c = Blob.ids.Count;
            Blob my = null;
            if (c > 0)
            {
                my = Blob.blobs[Blob.ids[c - 1]];
            }
            double distance = double.MaxValue;
            short siz = 0;
            bool danger = false;
            foreach (Blob blob in Blob.blobs.Values)
            {
                if (my != null && !Blob.ids.Contains(blob.id))
                {


                    int lx = blob.x - my.x;
                    int ly = blob.y - my.y;


                    double diff = Math.Sqrt(lx * lx + ly * ly);
                    if (blob.size < my.size || blob.cellType == 3 && mergeX == 0 && mergeY == 0)
                    {

                        if (diff < distance && blob.size > siz)
                        {
                            siz = blob.size;
                            distance = diff;

                            ID = blob.id;
                        }
                    }
                    else if (blob.name.Length > 0)
                    {
                        if (my.size / blob.size * 100 < 70 && diff < 1000 + blob.size)
                        {
                            var angle = Math.Atan2(my.y - blob.y, my.x - blob.x);

                            var xx = Math.Cos(angle);
                            var yy = Math.Sin(angle);

                            SetCursor(my.x + (1000 * xx), my.y + (1000 * yy));
                            if (my.size / blob.size * 100 < 45)
                                Split();
                            danger = true;
                        }
                    }
                }
            }

            if (danger)
                ID = 0;


            if (Blob.ids.Count == 1)
            {
                mergeX = 0;
                mergeY = 0;
            }


            if (Blob.ids.Count > mergeCount)
            {

                if (mergeX == 0 && mergeY == 0)
                {
                    int x = 0;
                    int y = 0;

                    foreach (uint blobid in Blob.ids)
                    {
                        x += Blob.blobs[blobid].x;
                        y += Blob.blobs[blobid].y;
                    }
                    x /= Blob.ids.Count;
                    y /= Blob.ids.Count;
                    SetCursor(x, y);
                    mergeX = x;
                    mergeX = y;
                }
            }
            else
            {

                if (previd != ID && !danger && mergeX == 0 && mergeY == 0)
                {
                    previd = ID;
                    Split();
                }

                if (Blob.blobs.ContainsKey(ID) && mergeX == 0 && mergeY == 0)
                {
                    SetCursor(Blob.blobs[ID].x, Blob.blobs[ID].y);

                }
            }*/
        }
    }
}
