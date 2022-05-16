using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPetri2
{
    class Program
    {
        static Window client;

        static void Main(string[] args)
        {
            client = new Window();
            client.Start();
        }
    }
}
