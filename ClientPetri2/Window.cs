using ClientPetri;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientPetri2
{
    class Window
    {
        private RenderWindow window;
        private Client client;

        private Thread qkey;
        private Vector2f oldCenter;

        private string mytext = "";

        private uint oldViewWidth = 1600;
        private uint oldViewHeight = 800;

        private float fontSize = 25;
        private float fontSize2 = 35;
        private float zoom = 1;

        private bool space = false;
        

        public Window()
        {
            ContextSettings settings = new ContextSettings();
            settings.DepthBits = 24;
            settings.StencilBits = 0;
            settings.AntialiasingLevel = 4;
            settings.MajorVersion = 3;
            settings.MinorVersion = 0;
            window = new RenderWindow(new VideoMode(oldViewWidth, oldViewHeight), "PetriDish", Styles.Default, settings);
            //window.SetVerticalSyncEnabled(true);

            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
            window.KeyReleased += (sender, e) => { 
                if (e.Code == Keyboard.Key.Space)
                {
                    space = false;
                }
                if (e.Code == Keyboard.Key.R)
                {
                    client.Respawn();
                }
                if (qkey != null)
                {
                    qkey.Abort();
                    qkey = null;
                }
            };
            
            window.TextEntered += (sender, e) => { 
                if (typing)
                {
                    if (!char.IsControl(e.Unicode[0])) {
                        mytext += e.Unicode;
                    }
                }
            };
            window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMouseButtonPressed);
            window.MouseButtonReleased += Window_MouseButtonReleased;
            window.MouseWheelScrolled += Window_MouseWheelScrolled;
            window.Resized += Window_Resized;
            client = new Client("ws://overlimit2.petridish.pw:8080");
            //client = new Client("ws://overlimit2.petridish.pw:8080/connect?tynda=9jx7BonpadllT0A84O9%2BSqAiSwJ1n71t%2Fh5Q3UkPFsBYFkdS4xmtGnqotgDRiV1eaTtTGB8s%2BMRHWf0oug%2FwRabsIoG9NEJqXm%2FgbB1Mrd0%3D&serverpass=");
        }

        void Window_Resized(object sender, SizeEventArgs e)
        {
            uint ratio = oldViewWidth / oldViewHeight;
            viewWidth = e.Width * ratio;
            viewHeight = e.Height * ratio;
        }


        private void UpdateMouse()
        {
            Vector2f pos = window.MapPixelToCoords(Mouse.GetPosition(window));

            double x = pos.X;
            double y = pos.Y;

            //x = Math.Max(Math.Min(x, client.MapMaxX), 0);
            //y = Math.Max(Math.Min(y, client.MapMaxY), 0);

            client.SetCursor(x,y);

            
        }

        private void Window_MouseMoved(object sender, MouseMoveEventArgs e)
        {
            Vector2f fff = window.MapPixelToCoords(new Vector2i(e.X, e.Y));
            /*if (!client.following)
            {
                client.specX = fff.X;
                client.specY = fff.Y;
            }*/
            //client.bot.SendMouse(fff.X, fff.Y);
        }

        private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            Vector2f fff = window.MapPixelToCoords(new Vector2i(e.X, e.Y));
            client.specX = fff.X;
            client.specY = fff.Y;
            //client.bot.SendMouse(client.specX, client.specY);
        }

        private void Window_MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            zoom += e.Delta > 0 ? 0.015f : -0.015f;
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {

    /*        var dictkey = {
        '87': 21, //w
        '32': 17, //space
        '83': 24, //s
        '68': 23, //d
        '90': 26, //z
        '65': 25, //a
    };

        */
            if (!typing)
            {

                if (e.Code == Keyboard.Key.R)
                {
                    client.Split(2);
                }

                if (e.Code == Keyboard.Key.Q)
                {
                    if (qkey == null)
                    {
                        qkey = new Thread(() =>
                        {
                            while (Thread.CurrentThread.IsAlive)
                            {
                                client.Split(21);
                                Thread.Sleep(50);
                            }
                        });
                        qkey.Start();
                    }
                }

                if (e.Code == Keyboard.Key.W)
                {
                    client.Split(21);
                }

                if (e.Code == Keyboard.Key.Space)
                {
                    if (!space)
                    {
                        space = true;
                        client.Split(17);
                    }

                }

                if (e.Code == Keyboard.Key.S)
                {
                    client.Split(24);
                }

                if (e.Code == Keyboard.Key.D)
                {
                    client.Split(23);

                }

                if (e.Code == Keyboard.Key.Z)
                {
                    client.Split(26);
                }

                if (e.Code == Keyboard.Key.A)
                {
                    client.Split(25);
                }
                if (e.Code == Keyboard.Key.F)
                {
                    Blob.renderFood = !Blob.renderFood;
                }
            }
            else
            {
                if (e.Code == Keyboard.Key.Backspace)
                {
                    if (mytext.Length > 0)
                    {
                        mytext = mytext.Remove(mytext.Length - 1, 1);
                    }
                }
            }

            if (e.Code == Keyboard.Key.Enter)
            {
                typing = !typing;
                if (!typing && mytext.Length > 0)
                {
                    client.Chat(mytext);
                    mytext = "";
                }

            }
            if (e.Code == Keyboard.Key.Num1)
            {
                Blob.colorFactor -= 0.05f;
            }
            if (e.Code == Keyboard.Key.Num2)
            {
                Blob.colorFactor += 0.05f;
            }
            if (Blob.colorFactor > 1.0f) {
                Blob.colorFactor = 1.0f;
            }

        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            
        }

        public void Start()
        {
            viewWidth = oldViewWidth;
            viewHeight = oldViewHeight;

            client.Connect();

            window.SetVisible(true);
            window.Closed += new EventHandler(OnClosed);
            while (window.IsOpen)
            {
                Update();
                Display();
            }
        }

        float viewWidth;
        float viewHeight;

        private void Display()
        {
            float maxViewX = 0;
            float maxViewY = 0;
            float minViewX = 0;
            float minViewY = 0;
            /*int c = Blob.ids.Count;
            Blob my = null;
            if (c > 0)
            {
                my = Blob.blobs[Blob.ids[c - 1]];
            }*/
            window.Clear(new Color(20, 20, 20, 255));
            /*double distance = double.MaxValue;
            short siz = 0;
            bool danger = false;
            foreach (Blob blob in Blob.blobs.Values)
            {
                if (my != null && !Blob.ids.Contains(blob.id))
                {
                    

                    int lx = blob.x - my.x;
                    int ly = blob.y - my.y;


                    double diff = Math.Sqrt(lx * lx + ly * ly);
                    if (blob.size < my.size || blob.cellType == 3 && client.mergeX == 0 && client.mergeY ==0)
                    {

                        if (diff < distance && blob.size > siz)
                        {
                            siz = blob.size;
                            distance = diff;
                            
                            client.ID = blob.id;
                        }
                    } else if (blob.name.Length > 0)
                    {
                        if (my.size / blob.size * 100 < 70 && diff < 1000 + blob.size)
                        {
                            var angle = Math.Atan2(my.y - blob.y, my.x - blob.x);

                            var xx = Math.Cos(angle);
                            var yy = Math.Sin(angle);
                            
                            client.SetCursor(my.x + (1000 * xx), my.y + (1000 * yy));
                            if (my.size / blob.size * 100 < 45)
                            client.Split();
                            danger = true;
                        }
                    }
                }
                blob.Draw(window);
            }

            if (danger)
                client.ID = 0;


            if (Blob.ids.Count == 1)
            {
                client.mergeX = 0;
                client.mergeY = 0;
            }


            if (Blob.ids.Count > mergeCount)
            {
                
                if (client.mergeX == 0 && client.mergeY == 0)
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
                    client.SetCursor(x, y);
                    client.mergeX = x;
                    client.mergeX = y;
                }
            }
            else
            {

                if (previd != client.ID && !danger && client.mergeX == 0 && client.mergeY == 0)
                {
                    previd = client.ID;
                    client.Split();
                }

                if (Blob.blobs.ContainsKey(client.ID) && client.mergeX == 0 && client.mergeY == 0)
                {
                    client.SetCursor(Blob.blobs[client.ID].x, Blob.blobs[client.ID].y);

                }
            }*/
            int drawIndex = 0;
            foreach (Blob blob in Blob.blobs.Values)
            {
                if (minViewX > blob.x)
                {
                    minViewX = blob.x;
                }
                if (minViewY > blob.y)
                {
                    minViewY = blob.y;
                }
                if (blob.x > maxViewX)
                {
                    maxViewX = blob.x;
                }
                if (blob.y > maxViewY)
                {
                    maxViewY = blob.y;
                }
                blob.Draw(window, drawIndex++);
            }

                CircleShape shape = new CircleShape(50, 40);
            shape.FillColor = new Color(255, 255, 255, 255);
            shape.Position = new Vector2f((float)client.X - 50, (float)client.Y - 50);
            window.Draw(shape);

            int ci = 0;

            float newratio = Blob.ratio + (1 / (float)(Math.Pow(Math.Min(64.0f / totalSize, 1), 0.4)) - Blob.ratio) * 0.01f;

            Vector2f fontScale = new Vector2f(newratio, newratio);

            foreach (string c in client.chat)
            {
                Text t = new Text(c, Blob.font, (uint)(fontSize));
                t.Scale = fontScale;
                t.Position = window.MapPixelToCoords(new Vector2i(0, (int)(ci++ * fontSize)));
                t.FillColor = Color.White;
                window.Draw(t);
            }
            Text tt = new Text((typing ? "> " : "") + mytext, Blob.font, (uint)(fontSize));
            tt.Scale = fontScale;
            tt.Position = window.MapPixelToCoords(new Vector2i(0, (int)(ci * fontSize)));
            tt.FillColor = Color.Green;
            window.Draw(tt);

            int mass = TotalSize2();
            Text mycoords = new Text(string.Format("x:{0} y:{1} size:{2}", (int)window.GetView().Center.X, (int)window.GetView().Center.Y, mass)
            , Blob.font, (uint)(fontSize2));
            mycoords.Scale = new Vector2f(Blob.ratio, Blob.ratio);
            mycoords.Position = window.MapPixelToCoords(new Vector2i(0, (int)window.Size.Y - 24));
            mycoords.FillColor = Color.Cyan;
            window.Draw(mycoords);
            window.Display();
        }

        private int TotalSize()
        {
            int c = Blob.ids.Count;
            int size = 0;
            for (int i = 0; i < c; i++)
            {
                Blob myb = Blob.blobs[Blob.ids[i]];
                size += myb.size;
            }
            return size;
        }

        private int TotalSize2()
        {
            int c = Blob.ids.Count;
            int size = 0;
            for (int i = 0; i < c; i++)
            {
                if (Blob.blobs.ContainsKey(Blob.ids[i]))
                {
                    Blob myb = Blob.blobs[Blob.ids[i]];
                    size += myb.size * myb.size / 100;
                }
            }
            return size;
        }

        private void Update()
        {
            window.DispatchEvents();
            UpdateMouse();
            UpdateView();
            client.Update();
        }

        private void UpdateView()
        {
            int c = Blob.ids.Count;
            if (c == 0)
                return;

            double x = 0;
            double y = 0;
            totalSize = TotalSize();
            int size = totalSize;
            int count = 0;



            size /= c;

            for (int i = 0; i < c; i++)
            {
                Blob myb = Blob.blobs[Blob.ids[i]];
                if (myb.size / size * 100 < 5)
                {
                   //continue;
                }
                x += myb.x;
                y += myb.y;
                count++;
            }

            x /= count;
            y /= count;

            View view = window.GetView();

            view.Size = new Vector2f(viewWidth * Blob.ratio, viewHeight * Blob.ratio);
            Vector2f newCenter = new Vector2f((float)x, (float)y);
            if (!oldCenter.Equals(newCenter))
            {
                double total = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds;
                double totaldiff = total - oldTime;
                if (totaldiff < 0)
                {
                    totaldiff = 0;
                } else if (totaldiff > 3)
                {
                    totaldiff = 1;
                }
                //Console.WriteLine("totaldiff " + totaldiff);
                float viewX = oldCenter.X + (newCenter.X - oldCenter.X) * 0.1f;
                float viewY = oldCenter.Y + (newCenter.Y - oldCenter.Y) * 0.1f;
                view.Center = new Vector2f(viewX, viewY);
                oldTime = total;
                oldCenter = view.Center;
            }

            float newratio = Blob.ratio + (1 / (float)(Math.Pow(Math.Min(64.0f / totalSize, 1), 0.4)) - Blob.ratio) * 0.1f;



            Blob.ratio = newratio * zoom;

            window.SetView(view);
        }


        private double oldTime = 0;
        private bool typing;
        private int totalSize;

        private void OnClosed(object sender, EventArgs e)
        {
            window.Close();
        }
    }
}
