using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace Serial
{
    public partial class MainForm : Form
    {
        public int SHOW = 0;
        public int NowMode = 0;
        private DataFlowForm dataflowform;

        public MainForm()
        {
            InitializeComponent();
            GetPcSeriesInit();
        }

      

        //************************以下为串口部分*********************************************************
        private void StringToHex(string str, params byte[] OutBuffer)
        {
            int index = 0;
            str = str.Replace(" ", "");//删除所有空格
            for (int i = 0; i < str.Length - 2;)
            {
                if (i == 0) { i += 2; } else { i += 3; }
                str = str.Insert(i, " ");
                str = str.Trim();//消首尾多余空格
            }
            string[] hexValuesSplit = str.Split(' ');
            foreach (String hex in hexValuesSplit)
            {
                OutBuffer[index] = Convert.ToByte(hex, 16);
                index++;
            }
        }

        public string HexToString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    for (int j = 0; j < 1; j++)
                    {
                        strB.Append(bytes[i].ToString("X2") + " ");
                    }
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        //串口数据发送
        private void Series_DataSend(string str, bool HexFlag)
        {
            try
            {
                if (HexFlag)
                {
                    str = str.Replace(" ", "");
                    byte[] WriteBuffer = new byte[(str.Length / 2) + (str.Length % 2)];
                    StringToHex(str, WriteBuffer);
                    Serial.Write(WriteBuffer, 0, WriteBuffer.Length);
                }
                else
                {
                    Serial.Write(str);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "未能找到任何可识别的数字。")
                {
                    MessageBox.Show("非法字符,0-9,A-F,a-f");
                }
                else
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        private void Series_DataSend(params byte[] OutBuffer)
        {
            try
            {
                Serial.Write(OutBuffer, 0, OutBuffer.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        //打开串口
        bool OpenPort()
        {
            try
            {
                Serial.PortName = comboBox_PortName.Text;
                Serial.BaudRate = Convert.ToInt32(comboBox_BaudRate.Text);
                Serial.DataBits = Convert.ToInt32(comboBox_DataBits.Text);
                Serial.Parity = (comboBox_Parity.Text == "None") ? Parity.None : ((comboBox_Parity.Text == "Odd") ? Parity.Odd : Parity.Even);
                Serial.StopBits = (comboBox_StopBits.Text == "1") ? StopBits.One : StopBits.Two;

                Serial.ReadBufferSize = 0x100000;
                Serial.Open();
                Serial.DiscardInBuffer();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return false;
            }
            return true;
        }
        //关闭串口
        bool ClosePort()
        {
            try
            {
                Serial.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return false;
            }
            return true;
        }
        private void ComboxEnableSet(bool Set)
        {
            comboBox_PortName.Enabled = Set;
            comboBox_BaudRate.Enabled = Set;
            comboBox_DataBits.Enabled = Set;
            comboBox_Parity.Enabled = Set;
            comboBox_StopBits.Enabled = Set;
            textBox1.Enabled = Set;
            textBox2.Enabled = Set;
            radioButton1.Enabled = Set;
            radioButton2.Enabled = Set;
            radioButton3.Enabled = Set;

        }

        //串口初始化
        private void GetPcSeriesInit()
        {
            string[] sAllPort = null;
            try
            {

                radioButton1.Checked = true;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                textBox2.Text = "1";


                comboBox_BaudRate.Text = "9600";
                comboBox_Parity.Text = "None";
                comboBox_StopBits.Text = "One";
                comboBox_DataBits.Text = "8";

                sAllPort = SerialPort.GetPortNames();
                int i = sAllPort.Length;
                for (; i > 0; i--)
                {
                    comboBox_PortName.Items.Add(sAllPort[i - 1]);
                }
                comboBox_PortName.Text = comboBox_PortName.Items[0].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }


            Thread thread = new Thread(new ThreadStart(() => { Serial.DataReceived += new SerialDataReceivedEventHandler(Series_DataReceived); }), 0);
            thread.IsBackground = true;
            thread.Start();
        }
        //串口接收
        private void Series_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string str;
            try
            {
               // System.Threading.Thread.Sleep(150);

                while (Serial.IsOpen && Serial.BytesToRead > 0)
                {
                    int length = Serial.BytesToRead;
                    byte[] buff = new byte[length];
                    Serial.Read(buff, 0, length);

                    str = HexToString(buff);

                    if (SHOW == 1)
                    {
                        Invoke(new MethodInvoker(
                                                      () =>
                                                      {
                                                          dataflowform.textBox1.AppendText(DateTime.Now.ToString("[RX_HH:mm:ss] ") + str + "\r\n");
                                                          dataflowform.textBox1.ForeColor = Color.DarkBlue;
                                                      }));
                    }

                              
                }

            }
            catch{}
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Serial.IsOpen)
            {
                try
                {
                    if (ClosePort())
                    {
                        ComboxEnableSet(true);
                        button1.Text = "打开串口";
                        MessageBox.Show("串口关闭成功");
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    if (radioButton1.Checked)
                    {
                        NowMode = 1;
                    }
                    else if (radioButton2.Checked)
                    {
                        NowMode = 2;
                    }
                    else if (radioButton3.Checked)
                    {
                        NowMode = 3;
                    }
                    else
                    {
                        NowMode = 0;
                    }

                    if (OpenPort())
                    {

                        ComboxEnableSet(false);
                        button1.Text = "关闭串口";
                        MessageBox.Show("串口打开成功");
                    }
                }
                catch { }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (MessageBox.Show("将要关闭窗体，是否继续", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataFlowForm dataflowset = new DataFlowForm(this);

            dataflowform = dataflowset;
            dataflowform.Text = "通信数据流";
            dataflowform.Show();
            SHOW = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //************************以上为串口部分*********************************************************
    }
}
