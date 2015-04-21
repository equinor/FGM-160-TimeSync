using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Xml;
using Modbus.Device;


namespace DCS_Reader
{
    class Program
    {
        private static int Poll_Interval = 1000; // ms

        private static String Selected_Com_Port = "COM4";
        private static int Selected_Baud_Rate = 9600;
        private static int Selected_Data_Bits = 8;
        private static Parity Selected_Parity = Parity.Even;
        private static StopBits Selected_Stop_Bits = StopBits.One;

        private static byte Selected_Slave_Address = 1;
        private static ushort[] Selected_Start_Register = {1016, 1040, 1060, 1080, 1100};
        private static ushort[] Selected_Number_Of_Registers = {12, 10, 14, 12, 14} ;

        static void Main(string[] args)
        {

            // Read clock from FGM160.
            SerialPort Com_Port = new SerialPort(Selected_Com_Port);
            Com_Port.BaudRate = Selected_Baud_Rate;
            Com_Port.DataBits = Selected_Data_Bits;
            Com_Port.Parity = Selected_Parity;
            Com_Port.StopBits = Selected_Stop_Bits;
            Com_Port.ReadTimeout = 5000;
            Com_Port.Open();

            IModbusMaster Modbus_Master = ModbusSerialMaster.CreateRtu(Com_Port);

            ushort[] Registers = { 0x44fb, 0xe000, 0x4040, 0x0000, 0x41f8, 0x0000, 0,
                                   0x4100, 0x0000, 0x4260, 0x0000, 0x4188, 0x0000, 0 };

            while (true)
            {
                try
                {
                    for (int i = 0; i < Selected_Start_Register.Length; i++)
                    {
                        Registers = Modbus_Master.ReadHoldingRegisters(Selected_Slave_Address,
                            Selected_Start_Register[i], Selected_Number_Of_Registers[i]);

                        Console.WriteLine(Registers);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading Modbus slave. {0}", e.Message);
                }
                System.Threading.Thread.Sleep(Poll_Interval);
            }
        }
    }
}
