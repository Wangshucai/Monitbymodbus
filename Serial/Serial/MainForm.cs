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
        public byte MailAddr = 0x40;  //默认通信地址1，可在串口设置界面更改
        public int SHOW = 0;
        public int NowMode = 0;
        private DataFlowForm dataflowform;
        public CQueue BuffQueue = new CQueue();
        Modbus modbus = new Modbus();
        public static MainForm pMainWin = null;

        public List<byte> DataBuff = new List<byte>();
        public int DataBuffCrt = 0;


        public MainForm()
        {

            this.StartPosition = FormStartPosition.CenterScreen;
            pMainWin = this;
            InitializeComponent();
            GetPcSeriesInit();
            Configconfiguration();

        }


        public void Configconfiguration()
        {
            System.Windows.Forms.TabPage tab = new System.Windows.Forms.TabPage();
            tab = config;

            System.Windows.Forms.TabPage tabPage = new System.Windows.Forms.TabPage()
            {
                Text = "显示"
            };
            GroupBox  Config = new System.Windows.Forms.GroupBox();
            Config.Width = 1085;
            Config.Height = 520;
            Config.BackColor = Color.LightCyan;
            tabPage.Controls.Add(Config);

            DataGridView dataGridView = new DataGridView();

            tabPage.Controls.Add(dataGridView);
            this.tabControl1.Controls.Remove(config);
            this.tabControl1.Controls.Add(tabPage);
            this.tabControl1.Controls.Add(tab);
        }














        private static object objlock = new object();
        private static Mutex mutex = new Mutex();
        //串口接收数据，全部读取后解析
        private void Series_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

           // lock (objlock)
            {
                try
                {
                    while (Serial.IsOpen && Serial.BytesToRead > 0)
                    {
                        int length = Serial.BytesToRead;
                        byte[] buff = new byte[length];
                        Serial.Read(buff, 0, length);

                        for (int i = 0; i < length; i++)
                        {
                            mutex.WaitOne();

                            BuffQueue.EnQueue(buff[i]);

                            mutex.ReleaseMutex();

                        }
                        ;
                    }

                }
                catch { }
            }



        

        }
        //解析和校验串口数据
        public void ReadBuff()
        {

            try
            {
                string readdata = "";
                UInt64 EERCount = 0;
                byte data = 0;
                ushort Local_CHKSUM = 0;
                int queuecount = 0;
                while (true)
                {
                    Thread.Sleep(1);

                    mutex.WaitOne();

                    queuecount = BuffQueue.QueueCount;

                    mutex.ReleaseMutex();

                    if (queuecount > 0)
                    {
                        if (DataBuffCrt < 7)
                        {
                            mutex.WaitOne();
                            data = (byte)BuffQueue.DeQueue();
                            mutex.ReleaseMutex();


                            if (data != 2147483647)
                            {
                                DataBuff.Insert(DataBuffCrt, data);
                                DataBuffCrt++;
                            }
                           
                        }
                        else
                        {
                            modbus.checksum = modbus.Crc16(DataBuff, 0, (ushort)(DataBuffCrt - 3));
                            Local_CHKSUM = (ushort)(modbus.TwoToWord(DataBuff[DataBuffCrt - 1], DataBuff[DataBuffCrt - 2]));

                            if (DataBuffCrt > 100)
                            {
                                if (data == MailAddr)
                                {
                                    DataBuffCrt = 0;
                                    DataBuff.Clear();
                                    DataBuff.Insert(DataBuffCrt, data);
                                    DataBuffCrt++;
                                }
   
                            }

                            if (modbus.checksum == Local_CHKSUM)
                            {
                                try
                                {
                                    EERCount++;
                                    
                                    byte[] showdata = new byte[DataBuffCrt];

                                    for (int num = 0; num < DataBuffCrt; num++)
                                    {
                                        showdata[num] = DataBuff[num];
                                    }

                                    readdata = HexToString(showdata);

                                    DataBuff.Clear();
                                    DataBuffCrt = 0;


                                    try
                                        {

                                            Invoke(new MethodInvoker(
                                                                    () =>
                                                                    {
                                                                        label3.Text = EERCount.ToString();
                                                                        label4.Text = queuecount.ToString();
                                                                        if (SHOW == 1)
                                                                        {
                                                                            dataflowform.textBox1.Font = new System.Drawing.Font("宋体", 14);
                                                                            dataflowform.textBox1.AppendText(DateTime.Now.ToString("[RX_HH:mm:ss] ") + readdata + "\r\n");
                                                                            dataflowform.textBox1.ForeColor = Color.DarkBlue;
                                                                        }
                                                                    }));
                                        }
                                        catch
                                        {


                                        }





                                   

                                }
                                catch
                                {

                                }






                            }
                            else
                            {
                                mutex.WaitOne();
                                data = (byte)BuffQueue.DeQueue();
                                mutex.ReleaseMutex();

                                if (data != 2147483647)
                                {
                                    DataBuff.Insert(DataBuffCrt, data);
                                    DataBuffCrt++;
                                }
                               
                            }

                        }


                    }

                }

            }
            catch {


            }
















          
           

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


            ThreadStart ReadThread = new ThreadStart(ReadBuff);
            Thread Read = new Thread(ReadThread);
            Read.IsBackground = true;
            Read.Start();


        }
        //串口接收
    

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
            SHOW = 1;
            dataflowform.Show();
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

       

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void config_Click(object sender, EventArgs e)
        {

        }
        static int flag = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
          
            DateTimePicker NowTime = new DateTimePicker();

            NowTime.Format = DateTimePickerFormat.Long;
            NowTime.Format = DateTimePickerFormat.Time;
            label5.Text = NowTime.Value.ToString();
            if (flag == 0)
            {
                flag = 1;
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].Cells[0].Value = NowTime.Value.ToString();
            }
            else
            {
                dataGridView1.Rows[0].Cells[0].Value = NowTime.Value.ToString();
            }
         

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //************************以上为串口部分*********************************************************
    }
}
