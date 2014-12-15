using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Device;


namespace FGM_160_Time_sync
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("hello world");

            // Read config file.

            // Read clock from FGM160.

            // Compare clock from FGM160 with local time.
            // If difference is more than threshold then write
            // local time to FGM160.

        }
    }
}
