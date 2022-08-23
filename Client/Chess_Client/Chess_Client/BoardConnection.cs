using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Chess_Client
{
    public partial class BoardConnection : Form
    {
        BluetoothClient client;
        public BoardConnection(BluetoothClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            //client = new BluetoothClient();

            try
            {
                BluetoothDeviceInfo device = null;
                foreach (var dev in client.DiscoverDevices())
                {
                    richTextBox1.Text += dev.DeviceName + "\n";
                    if (dev.DeviceName.Contains("ESP32test")) //TBD proper name
                    {
                        device = dev;
                        break;
                    }
                }


                if(device == null)
                {
                    richTextBox1.Text += "Device not found";
                    return;
                }
                //return;

                if (!device.Authenticated)
                {
                    BluetoothSecurity.PairRequest(device.DeviceAddress, "1234");
                }

                device.Refresh();
                //System.Diagnostics.Debug.WriteLine(device.Authenticated);

                client.Connect(device.DeviceAddress, BluetoothService.SerialPort);

                richTextBox1.Text += "Board Connected";
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "***Exception***";
                richTextBox1.Text += ex.Message;
                richTextBox1.Text += ex.StackTrace;
            }
        }
    }
}
