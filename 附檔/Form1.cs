using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        String IDcard="";
        String ID = "";
        bool add = true;
        bool sub = false;
        public Form1()
        {
            InitializeComponent();
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            timer1.Start();

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private byte[] hexStringToByte(string hexString)
        {

            byte[] returnedBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnedBytes.Length; i++)
                returnedBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnedBytes;

        }

        //將數值位元組轉成16進位字串

        private String byteToHexString(byte[] buffer)

        {

            String hexString = "";

            for (int i = 0; i < buffer.Length; i++)
                hexString += buffer[i].ToString("X2"); //將資料先轉16進位

            return hexString;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            try
            {
                if (!serialPort1.IsOpen)
                    serialPort1.Open();

                byte[] bufferSend;
                bufferSend = hexStringToByte("55");
                serialPort1.Write(bufferSend, 0, bufferSend.Length);
               
                //等待接收
                System.Threading.Thread.Sleep(150);
                byte[] bufferReceive = new byte[serialPort1.BytesToRead];
                serialPort1.Read(bufferReceive, 0, bufferReceive.Length);
                
                ID = byteToHexString(bufferReceive);
                String check = byteToHexString(bufferReceive).Substring(0,2);
                //this.textBoxReceive.Text = "ID = "+ ID + "IDcard = " + IDcard + "check = " + check +"\r\n" + this.textBoxReceive.Text;
                if (check == "86")
                {
                    if (ID != IDcard)
                    {
                        IDcard = ID;
                        //輸入指令
                        bufferSend = hexStringToByte("520400");
                        serialPort1.Write(bufferSend, 0, bufferSend.Length);

                        //等待接收
                        System.Threading.Thread.Sleep(150);
                        bufferReceive = new byte[serialPort1.BytesToRead];
                        serialPort1.Read(bufferReceive, 0, bufferReceive.Length);

                        //16轉換10
                        String receive = byteToHexString(bufferReceive).Substring(2, 4);
                        int r = Convert.ToInt32(receive, 16);

                        int num = 0;
                        String s = textBoxTransmit.Text.ToString();
                        //金額框轉換
                        if (!String.IsNullOrEmpty(s))
                        {
                            num = int.Parse(s);
                        }

                        int f = 0;
                        //計算
                        if (add == true && r + num < 10000)
                        {
                            r += num;
                        }
                        else if (sub == true && r > num)
                        {
                            r -= num;
                        }
                        else if(add == true && r + num > 10000)
                        {
                            f = 1;
                        }
                        else if(sub == true && r < num)
                        {
                            f = 2;
                        }

                        receive = r.ToString("X4");
                        /*receive = Convert.ToString(r, 16);
                        int length = 4 - receive.Length;
                        while (receive.Length < 4)
                        {
                            receive = "0" + receive;
                        }*/

                        //輸入指令
                        s = "570400" + receive + "0000000000000000000000000000";

                        bufferSend = hexStringToByte(s);
                        serialPort1.Write(bufferSend, 0, bufferSend.Length);
                        System.Threading.Thread.Sleep(150);
                        bufferReceive = new byte[serialPort1.BytesToRead];
                        serialPort1.Read(bufferReceive, 0, bufferReceive.Length);
                        if (f == 1)
                            this.textBoxReceive.Text = ">加值金額已達上限,餘額: " + r + "元\r\n" + this.textBoxReceive.Text;
                        else if (f == 2)
                            this.textBoxReceive.Text = ">餘額不足,餘額: " + r + "元\r\n" + this.textBoxReceive.Text;
                        else if (add == true)
                            this.textBoxReceive.Text = ">加值" + num + "元，餘額:" + r + "元\r\n" + this.textBoxReceive.Text;
                        else if (sub == true)
                            this.textBoxReceive.Text = ">扣款" + num + "元，餘額:" + r + "元\r\n" + this.textBoxReceive.Text;
                        System.Threading.Thread.Sleep(100);
                    }
                }
                else
                    IDcard = "";
            }
            catch
            {
            }

            timer1.Start();
        }

        private void textBoxTransmit_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            add = true;
            sub = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            sub = true;
            add = false;
        }
    }
}
