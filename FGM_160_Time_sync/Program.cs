using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Xml;
using Modbus.Device;
using Modbus.Utility;


namespace FGM_160_Time_sync
{
    class Program
    {
        private static bool Verbose = true;
        private static bool Read_Only = false;

        private static String   Selected_Com_Port      = "COM1";
        private static int      Selected_Baud_Rate     = 38400;
        private static int      Selected_Data_Bits     = 8;
        private static Parity   Selected_Parity        = Parity.None;
        private static StopBits Selected_Stop_Bits     = StopBits.Two;
        
        private static byte     Selected_Slave_Address = 1;
        private static ushort   Selected_Start_Register      = 1000;
        private static ushort   Selected_Number_Of_Registers = 12;

        private static TimeSpan Threshold = new TimeSpan(0, 5, 0);

        static void Main(string[] args)
        {
            // Parse command line arguments
            foreach (string arg in args)
            {
                if (arg.Equals(@"-v"))
                {
                    Verbose = true;
                }
                else if (arg.Equals(@"-r"))
                {
                    Read_Only = true;
                }
            }


            // Read config file.
            Read_Config("FGM_160_Time_sync.xml");

            // Read clock from FGM160.
            SerialPort Com_Port = new SerialPort(Selected_Com_Port);
            Com_Port.BaudRate = Selected_Baud_Rate;
            Com_Port.DataBits = Selected_Data_Bits;
            Com_Port.Parity = Selected_Parity;
            Com_Port.StopBits = Selected_Stop_Bits;
            Com_Port.ReadTimeout = 500;
            Com_Port.Open();

            IModbusSerialMaster Modbus_Master = ModbusSerialMaster.CreateRtu(Com_Port);

            ushort[] Registers = { 0x44fb, 0xe000, 0x4040, 0x0000, 0x41f8, 0x0000,
                                   0x4100, 0x0000, 0x4260, 0x0000, 0x4188, 0x0000 };
            try
            {
                Registers = Modbus_Master.ReadHoldingRegisters(Selected_Slave_Address,
                    Selected_Start_Register, Selected_Number_Of_Registers);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading Modbus slave. {0}", e.Message);
                //Environment.Exit(1);
            }

            float Year = ModbusUtility.GetSingle(Registers[0], Registers[1]);
            float Month = ModbusUtility.GetSingle(Registers[2], Registers[3]);
            float Day = ModbusUtility.GetSingle(Registers[4], Registers[5]);
            float Hour = ModbusUtility.GetSingle(Registers[6], Registers[7]);
            float Minute = ModbusUtility.GetSingle(Registers[8], Registers[9]);
            float Second = ModbusUtility.GetSingle(Registers[10], Registers[11]);

            DateTime Device_Time_Stamp = new DateTime((int)Year, (int)Month, (int)Day, (int) Hour, (int) Minute, (int) Second);
            DateTime Reference_Time_Stamp = DateTime.Now;
            TimeSpan Time_Difference = Reference_Time_Stamp - Device_Time_Stamp;

            if (Verbose)
            {
                Console.WriteLine("Device time: {0}", Device_Time_Stamp.ToString());
                Console.WriteLine("Reference time: {0}", Reference_Time_Stamp.ToString());
                Console.WriteLine("Time difference: {0}", Time_Difference.ToString());
            }


            // Compare clock from FGM160 with local time.

            if (Time_Difference.Duration() > Threshold)
            {
                Console.WriteLine("Difference is larger than threshold");
            }

            // If difference is more than threshold then write
            // local time to FGM160.

            Registers = new ushort[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};

            Modbus_Master.WriteMultipleRegisters(Selected_Slave_Address,
                Selected_Start_Register, Registers);

        }

        public static void Read_Config(string Config_File)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;

            XmlReader reader = XmlReader.Create(Config_File, settings);

            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Serial_Port")
                    {
                        Selected_Com_Port = reader.GetAttribute("COM");
                        Selected_Baud_Rate = XmlConvert.ToInt32(reader.GetAttribute("Baudrate"));
                        Selected_Data_Bits = XmlConvert.ToInt32(reader.GetAttribute("Data_Bits"));
                        Selected_Parity = (Parity) Enum.Parse(Selected_Parity.GetType(), reader.GetAttribute("Parity"));
                        Selected_Stop_Bits = (StopBits) Enum.Parse(Selected_Stop_Bits.GetType(), reader.GetAttribute("Stop_Bits"));
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Modbus_Slave")
                    {
                        Selected_Slave_Address = XmlConvert.ToByte(reader.GetAttribute("Slave_Address"));
                        Selected_Start_Register = XmlConvert.ToUInt16(reader.GetAttribute("Start_Register"));
                        Selected_Number_Of_Registers = XmlConvert.ToUInt16(reader.GetAttribute("Number_Of_Registers"));
                        Threshold = TimeSpan.FromSeconds(XmlConvert.ToDouble(reader.GetAttribute("Threshold")));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading config file. {0}", e.Message);
                Environment.Exit(1);
            }
        }
    }
}
