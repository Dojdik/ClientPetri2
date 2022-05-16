using ClientPetri2;
using ClientPetri2.Properties;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClientPetri
{
    internal class Blob
    {
        public static float ratio = 1;

        public static Dictionary<uint, Blob> blobs = new Dictionary<uint, Blob>();
        public static List<Blob> list = new List<Blob>();
        public static List<uint> ids = new List<uint>();
        public static uint myids = 0;
        public static Font font;
        public static float colorFactor = 0.5f;


        public bool destroyed = false;
        public uint id;
        public float ox;
        public float oy;
        public float x;
        public float y;
        public float nx;
        public float ny;
        public short oSize;
        public short nSize;
        public short size;
        public string color;

        public byte r, g, b;


        public string name;
        public int sticker;
        public int skin;
        public int cellType;
        public DateTime updateTime;
        public bool isVirus;
        public bool isAgitated;
        public double updateCode;
        public byte flags;
        public bool ownCell;
        public static bool renderFood = true;
         
        Client client;

        public Blob(Client client, uint id, int x, int y, short size, string color, string name)
        {
            this.client = client;
            this.id = id;
            this.x = x;
            this.y = y;
            this.ox = x;
            this.oy = y;
            this.size = size;
            SetColor(color);

            this.name = name;
        }

        public void setName(string name)
        {

            this.name = name;
        }

        public void SetColor(string color)
        {
            this.color = color;

            int offset = 0;

            this.r = Convert.ToByte(color[offset++].ToString() + color[offset++].ToString(), 16);
            this.g = Convert.ToByte(color[offset++].ToString() + color[offset++].ToString(), 16);
            this.b = Convert.ToByte(color[offset++].ToString() + color[offset++].ToString(), 16);
        }

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static byte[] ChangeColorBrightness(int r, int g, int b, float correctionFactor)
        {
            float red = (float)r;
            float green = (float)g;
            float blue = (float)b;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new byte[] { (byte)red, (byte)green, (byte)blue };
        }

        public void Draw(RenderWindow window, int i)
        {
            if (cellType == 1 && !Blob.renderFood)
            {
                return;
            }


            // Draw outer dark circle
            CircleShape shape = new CircleShape(size, 40);

            if (font == null)
                font = new Font(Resources.dejavuSans);

            Text text = new Text(name, font, (uint)(10 + 0.07 * this.size));


            text.FillColor = Color.White;
            var textRect = text.GetLocalBounds();
            text.Origin = new Vector2f(textRect.Left + textRect.Width / 2.0f, textRect.Top + textRect.Height / 2.0f);
            text.Position = new Vector2f(x, y);
            window.Draw(text);

            if (cellType != 3 && cellType != 1)
            {
                Text text2 = new Text(this.size * this.size / 100 + "", font, (uint)(10 + 0.04 * this.size));
                textRect = text2.GetLocalBounds();
                text2.Origin = new Vector2f(textRect.Left + textRect.Width / 2.0f, textRect.Top + textRect.Height / 2.0f);
                text2.Position = new Vector2f(x, y + this.size * 0.45f);
                window.Draw(text2);
            }

            float diff = 34 + size / 100;
            // Draw inner circle
            shape.Radius -= diff;
            shape.OutlineThickness = 20;

            byte val = isVirus ? (byte)0 : (byte)50;
            byte[] rawc = ChangeColorBrightness(r, g, b, colorFactor);
            //Console.WriteLine("colorfactor:" + colorFactor);

            Color fillColor = new Color((byte)(r + val), (byte)(g + val), (byte)(b + val), (byte)(isVirus ? 255 : 0));
            Color outlineColor = new Color(rawc[0], rawc[1], rawc[2]);
            if (flags == 48)
            {
                outlineColor = Color.White;
            }

            shape.FillColor = fillColor;
            shape.OutlineColor = outlineColor;
            shape.Position = new Vector2f(x - (ushort)size + diff, y - (ushort)size + diff);
            window.Draw(shape);

            if (cellType == 0 && !isVirus)
            {
                double angle = Math.Atan2(this.ny - this.oy, this.nx - this.ox);
                // Draw inner circle
                shape.Radius = size / 4 - diff;
                shape.OutlineThickness = 10;

                shape.FillColor = fillColor;
                shape.OutlineColor = outlineColor;
                shape.Position = new Vector2f((x - (ushort)size / 4 + diff) + (float)Math.Cos(angle) * size, y - (ushort)size / 4 + diff + (float)Math.Sin(angle) * size);
                window.Draw(shape);
            }
        }

        public void destroy()
        {
            int i;
            /** @type {number} */
            i = 0;
            for (; i < list.Count; i++)
            {
                if (list[i] == this)
                {
                    list.RemoveAt(i);
                    break;
                }
            }
            blobs.Remove(this.id);
            //i = SoEwUtkGNV.indexOf(this);
            if (-1 != i)
            {
                /** @type {boolean} */
                //qweA = true;
                //SoEwUtkGNV.splice(i, 1);
            }
            i = ids.IndexOf(this.id);
            if (-1 != i)
            {
                myids--;
                ids.RemoveAt(i);
            }
            /** @type {boolean} */
            this.destroyed = true;
            //sprites.push(this);
        }

        public double updatePos(DateTime timestampLastDraw)
        {
            if (0 == this.id)
            {
                return 1;
            }
            /** @type {number} */
            var ratio = (timestampLastDraw - this.updateTime).TotalMilliseconds / 30;
            //var ratio = 1;
            if (0 > ratio)
            {
                /** @type {number} */
                ratio = 0;
            }
            else
            {
                if (1 < ratio)
                {
                    /** @type {number} */
                    ratio = 1;
                }
            }
            //this.getNameSize();
            if (this.destroyed && 1 <= ratio)
            {
                /*var index = sprites.indexOf(this);
                if (-1 != index)
                {
                    sprites.splice(index, 1);
                }*/
            }
            //this.x = nx;
            //this.y = ny;
            //this.x = (int)(ratio * (this.nx - this.ox) + this.ox);
            //this.y = (int)(ratio * (this.ny - this.oy) + this.oy);
            //this.size = (short)(ratio * (this.nSize - this.oSize) + this.oSize);
            this.x = this.ox + (this.nx - this.ox) * 0.3f;
            this.y = this.oy + (this.ny - this.oy) * 0.3f;
            this.size = this.nSize;
            return ratio;
        }
    }
}