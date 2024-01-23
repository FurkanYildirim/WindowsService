using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Configuration;
using S7.Net;
using System.Timers;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        Plc plc;
        Timer timer = new Timer();
        private Dictionary<int, string> previousValues;
        EventLog eventLog = new EventLog();
        public Service1()
        {
            InitializeComponent();
            previousValues = new Dictionary<int, string>();
           
            eventLog.Source = "PLC Service";
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                InitializeTimer();
                CpuType cpu = new CpuType();
                string plcType = ConfigurationManager.AppSettings["PlcType"];
                if (plcType.Equals("S71500"))
                {
                    cpu = CpuType.S71500;
                }
                else if (plcType.Equals("S71200"))
                {
                    cpu = CpuType.S71200;
                }

                // Değerleri app.config dosyasından al
                string plcIpAddress = ConfigurationManager.AppSettings["PlcIpAddress"];
                short rack = Convert.ToInt16(ConfigurationManager.AppSettings["PlcRack"]);
                short slot = Convert.ToInt16(ConfigurationManager.AppSettings["PlcSlot"]);

                plc = new Plc(cpu, plcIpAddress, rack, slot);
                plc.Open();

                
            }
            catch (Exception ex)
            {
               
            }
        }

        protected override void OnStop()
        {
            plc?.Close();
        }
        private void InitializeTimer()
        {

           
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Elapsed += test;


        }

        private void test(object sender, ElapsedEventArgs e)
        {
            if (plc.IsConnected)
            {
                string value_1 = (plc.Read(DataType.DataBlock, 1, 0, VarType.Int, 1)).ToString();
                string value_2 = (plc.Read(DataType.DataBlock, 1, 2, VarType.Int, 1)).ToString();
                string value_3 = (plc.Read(DataType.DataBlock, 1, 4, VarType.Int, 1)).ToString();
                string value_4 = (plc.Read(DataType.DataBlock, 1, 6, VarType.Int, 1)).ToString();
                string value_5 = (plc.Read(DataType.DataBlock, 1, 8, VarType.Int, 1)).ToString();

                // Önceki değerleri kontrol et
                if (!previousValues.ContainsKey(1) || previousValues[1] != value_1 ||
                    !previousValues.ContainsKey(2) || previousValues[2] != value_2 ||
                    !previousValues.ContainsKey(3) || previousValues[3] != value_3 ||
                    !previousValues.ContainsKey(4) || previousValues[4] != value_4 ||
                    !previousValues.ContainsKey(5) || previousValues[5] != value_5)
                {

                    // Önceki değerleri güncelle
                    previousValues[1] = value_1;
                    previousValues[2] = value_2;
                    previousValues[3] = value_3;
                    previousValues[4] = value_4;
                    previousValues[5] = value_5;
                    eventLog.WriteEntry("Value 1: " + value_1 + " Value 2: " + value_2 + " Value 3: " + value_3 + " Value 4: " + value_4 + " Value 5: " + value_5);
                }
            }
            else
            {

            }
        }

       
    }
}
