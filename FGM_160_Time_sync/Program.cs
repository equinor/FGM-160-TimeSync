using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using Modbus.Device;


namespace FGM_160_Time_sync
{
    class Program
    {
        private static bool Verbose = false;

        private static String   Selected_Com_Port      = "COM1";
        private static int      Selected_Baud_Rate     = 38400;
        private static int      Selected_Data_Bits     = 8;
        private static Parity   Selected_Parity        = Parity.None;
        private static StopBits Selected_Stop_Bits     = StopBits.Two;
        private static byte     Selected_Slave_Address = 1;
        private static ushort   Selected_Start_Register = 1000;
        private static ushort Selected_Number_Of_Registers = 12;

        static void Main(string[] args)
        {
            // Parse command line arguments
            foreach (string arg in args)
            {
                if (arg.Equals(@"/v"))
                {
                    Verbose = true;
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
            Com_Port.ReadTimeout = 5000;
            Com_Port.Open();

            IModbusSerialMaster Modbus_Master = ModbusSerialMaster.CreateRtu(Com_Port);

            ushort[] Registers;
            try
            {
                Registers = Modbus_Master.ReadHoldingRegisters(Selected_Slave_Address,
                    Selected_Start_Register, Selected_Number_Of_Registers);
            }
            catch
            {

            }
            44u.GetType();

            // Compare clock from FGM160 with local time.
            // If difference is more than threshold then write
            // local time to FGM160.

            Registers = new ushort[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};

            Modbus_Master.WriteMultipleRegisters(Selected_Slave_Address,
                Selected_Start_Register, Registers);

        }

        public static void Read_Config(string Config_File)
        {

        }

    }
}
