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
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.IO;



namespace Serial
{
    public partial class MainForm : Form
    {
        public byte MailAddr = 0x40;  //默认通信地址1，可在串口设置界面更改
        public int SHOW = 0;
        public int NowMode = 0;

        private DataFlowForm dataflowform;
        public CQueue BuffQueue = new CQueue();
        Cparameter cparameter = new Cparameter();
        Ccontrols ccontrols = new Ccontrols();

        Modbus modbus = new Modbus();
        public static MainForm pMainWin = null;

        public List<byte> DataBuff = new List<byte>();
        public int DataBuffCrt = 0;

        //创建Excel 对象
        Excel.Application excelApp;//创建实例
        Excel.Application excelRead;//创建实例
        Excel.Workbook workBook;
        Excel.Worksheet workSheet;
        int ExcelOpen = 0;


        public LinkedList LinkList = new LinkedList();
       // public LinkListNode WriteNode = new LinkListNode();
       // public LinkListNode CopyNode = new LinkListNode();

        public MainForm()
        {

            this.StartPosition = FormStartPosition.CenterScreen;
            pMainWin = this;
            InitializeComponent();
            ConfigRead();
            GetPcSeriesInit();
            Configconfiguration();
            ConfigSet();


        }

        //将配置文件显示放到最后一页
        public void Configconfiguration()
        {
            System.Windows.Forms.TabPage tab = new System.Windows.Forms.TabPage();
            tab = config;

            this.tabControl1.Controls.Remove(config);
          
            this.tabControl1.Controls.Add(tab);
        }

      

        private void ConfigRead()
        {
            int IndexRead = 1, RowsCount = 0;
            string data = "", addr = "",tabPageName = "";
           

            excelRead = new Excel.Application();//创建实例
            string currentPath = Directory.GetCurrentDirectory();
            string filename = currentPath + "\\" + "config.xlsx";   //需要在此添加一个文件夹

            excelRead.Visible = false; excelRead.UserControl = true;

            Excel.Workbook ConfigExcel = excelRead.Application.Workbooks.Open(filename, Missing.Value, true, Missing.Value, Missing.Value, Missing.Value,
             Missing.Value, Missing.Value, Missing.Value, true, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            int SheetNum = ConfigExcel.Worksheets.Count;


            GroupBox[] groupBox = new System.Windows.Forms.GroupBox[SheetNum];

            for (int i = 0; i < SheetNum; i++)
            {

                Excel.Worksheet ConfigSheet = (Excel.Worksheet)ConfigExcel.Worksheets.get_Item(i + 1);
                RowsCount = ConfigSheet.UsedRange.Cells.Rows.Count; //得到行数


                Excel.Range temp = ConfigSheet.Cells[1, 2];
                data = temp.FormulaLocal;

                if (Convert.ToInt32(data) != 16)
                {
                    LinkList.Append();
                    LinkList.Current.Num = RowsCount - 1;

                    LinkList.Current.Index = IndexRead;
                    IndexRead++;

                    try
                    {

                       temp = ConfigSheet.Cells[2, 3];
                       addr = temp.FormulaLocal;
                       LinkList.Current.DataType = addr.Substring(0,2);

                       SetMonFunc(LinkList.Current, LinkList.Current.DataType);

                       addr = addr.Substring(2, addr.Length-2);
                   
                        int value = Convert.ToInt32(data);
                        LinkList.Current.ReadFunc = value;
                        value = Convert.ToInt32(addr);
                        LinkList.Current.FirstAddr = value;

                        LinkList.Current.LastAddr = value + RowsCount - 2;
                    }
                    catch { MessageBox.Show("功能码或地址错误！", "提示"); }

                    for (int t = 0; t < RowsCount - 1; t++)
                    {
                        var txt = ((Excel.Range)ConfigSheet.Cells[t + 2, 4]).Text.ToString();
                        LinkList.Current.ReadNameConst[t] = txt;


                        try
                        {
                            LinkList.Current.Precision[t] = Convert.ToInt16((ConfigSheet.Cells[t + 2, 5]).Text);
                        }
                        catch
                        {
                            MessageBox.Show("精度错误！", "提示");
                        }

                        txt = ((Excel.Range)ConfigSheet.Cells[t + 2, 6]).Text.ToString();
                        LinkList.Current.Unit[t] = txt;

                        try
                        {
                            LinkList.Current.ShowMode[t] = Convert.ToInt16((ConfigSheet.Cells[t + 2, 7]).Text);
                        }
                        catch
                        {
                            MessageBox.Show("显示方式数据错误！", "提示");
                        }

                    }



                    try
                    {

                        tabPageName = ConfigSheet.Name;
                        LinkList.Current.PageName = tabPageName;

                        System.Windows.Forms.TabPage tabPage = new System.Windows.Forms.TabPage()
                        {
                            Text = tabPageName
                        };
                        groupBox[i] = new System.Windows.Forms.GroupBox();
                        groupBox[i].Width = 1085;
                        groupBox[i].Height = 520;
                        groupBox[i].BackColor = Color.LightCyan;
                        tabPage.Controls.Add(groupBox[i]);
                        this.tabControl1.Controls.Add(tabPage);

                        CreatShowUI(ref LinkList.Current.ReadNameConst, ref LinkList.Current.ReadName, ref LinkList.Current.ReadValue, groupBox[i], (short)LinkList.Current.Num);

                    }
                    catch
                    {
                        MessageBox.Show("页名错误！", "提示");
                    }

                }
                else   //写寄存器的操作
                {
                   
                }

            }

           
            excelRead.Quit();
            excelRead = null;
            ConfigExcel = null;

            GC.Collect();

        }


        public void ConfigSet()
        {
            LinkListNode data = new LinkListNode();
            int[] Num_X = new int[4] { 0,0,0,0};

            data = LinkList.Head;
  
            for (int i = 0;i< LinkList.ListNodeCount; i++)
            {
                if (data.DataType == "0X")
                {
                    Num_X[0] += data.Num;
                }
                else if (data.DataType == "1X")
                {
                    Num_X[1] += data.Num;
                }
                else if (data.DataType == "3X")
                {
                    Num_X[2] += data.Num;
                }
                else if (data.DataType == "4X")
                {
                    Num_X[3] += data.Num;
                }
                data = data.Next;
            }

          
            dataGridView1.Rows.Add(ReturnLagNum(Num_X));
         
            for (int i = 0; i < ReturnLagNum(Num_X); i++)
            {
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }

            data = LinkList.Head;
            Num_X[0] = 0; Num_X[1] = 0; Num_X[2] = 0; Num_X[3] = 0;
            for (int i = 0; i < LinkList.ListNodeCount; i++)
            {
                if (data.DataType == "0X")
                {
                    for (int num_0x = 0;num_0x < data.Num; num_0x ++)
                    {
                        dataGridView1.Rows[Num_X[0]].Cells[0].Value = data.DataType +(data.FirstAddr + num_0x);
                        dataGridView1.Rows[Num_X[0]].Cells[0].ReadOnly = true;
                        dataGridView1.Rows[Num_X[0]].Cells[1].Value = data.ReadNameConst[num_0x];
                        dataGridView1.Rows[Num_X[0]].Cells[1].ReadOnly = true;

                        data.ViewRows[num_0x] = Num_X[0];
                        data.ViewCells[num_0x] = 2;
                        dataGridView1.Rows[data.ViewRows[num_0x]].Cells[data.ViewCells[num_0x]].Value = 0;




                        Num_X[0]++;
                    }
                }
                else if (data.DataType == "1X")
                {
                    for (int num_1x = 0; num_1x < data.Num; num_1x++)
                    {
                        dataGridView1.Rows[Num_X[1]].Cells[3].Value = data.DataType + (data.FirstAddr + num_1x);
                        dataGridView1.Rows[Num_X[1]].Cells[3].ReadOnly = true;
                        dataGridView1.Rows[Num_X[1]].Cells[4].Value = data.ReadNameConst[num_1x];
                        dataGridView1.Rows[Num_X[1]].Cells[4].ReadOnly = true;

                        data.ViewRows[num_1x] = Num_X[1];
                        data.ViewCells[num_1x] = 5;
                        dataGridView1.Rows[data.ViewRows[num_1x]].Cells[data.ViewCells[num_1x]].Value = 0;


                        Num_X[1]++;
                    }
                }
                else if (data.DataType == "3X")
                {
                    for (int num_3x = 0; num_3x < data.Num; num_3x++)
                    {
                        dataGridView1.Rows[Num_X[2]].Cells[6].Value = data.DataType + (data.FirstAddr + num_3x);
                        dataGridView1.Rows[Num_X[2]].Cells[6].ReadOnly = true;
                        dataGridView1.Rows[Num_X[2]].Cells[7].Value = data.ReadNameConst[num_3x];
                        dataGridView1.Rows[Num_X[2]].Cells[7].ReadOnly = true;

                        data.ViewRows[num_3x] = Num_X[2];
                        data.ViewCells[num_3x] = 8;
                        dataGridView1.Rows[data.ViewRows[num_3x]].Cells[data.ViewCells[num_3x]].Value = 0;


                        Num_X[2]++;
                    }
                }
                else if (data.DataType == "4X")
                {
                    for (int num_4x = 0; num_4x < data.Num; num_4x++)
                    {
                        dataGridView1.Rows[Num_X[3]].Cells[9].Value = data.DataType + (data.FirstAddr + num_4x);
                        dataGridView1.Rows[Num_X[3]].Cells[9].ReadOnly = true;
                        dataGridView1.Rows[Num_X[3]].Cells[10].Value = data.ReadNameConst[num_4x];
                        dataGridView1.Rows[Num_X[3]].Cells[10].ReadOnly = true;

                        data.ViewRows[num_4x] = Num_X[3];
                        data.ViewCells[num_4x] = 11;
                        dataGridView1.Rows[data.ViewRows[num_4x]].Cells[data.ViewCells[num_4x]].Value = 0;


                        Num_X[3]++;
                    }
                }
                data = data.Next;
            }


        }

        public int ReturnLagNum(int[] data)
        {         
            int lagnum = 0;

            lagnum = data[0] > data[1] ? data[0] : data[1];
            lagnum = lagnum > data[2] ? lagnum : data[2];
            lagnum = lagnum > data[3] ? lagnum : data[3];

            return lagnum;
        }


        public void SetMonFunc(LinkListNode linkListNode,string func)
        {
            if (linkListNode.DataType == "0X")
            {
                linkListNode.MonReadFunc = 1;
                linkListNode.MonWriteFunc = 15;
            } else if (linkListNode.DataType == "1X")
            {
                linkListNode.MonReadFunc = 2;
            } else if (linkListNode.DataType == "3X")
            {
                linkListNode.MonReadFunc = 4;
            } else if (linkListNode.DataType == "4X")
            {
                linkListNode.MonReadFunc = 3;
                linkListNode.MonWriteFunc = 16;
            }
        }


        public void MonDataAnalysis(string request, string respond)
        {
            byte[] Request = new byte[request.Length / 3];
            byte[] Respond = new byte[respond.Length / 3];
            StringToHex(request, Request);
            StringToHex(respond, Respond);




        }


        public void CreatShowUI(ref string[] ReadNameConst, ref Label[] lab_Name, ref Label[] lab_Value, Control parent, short DataNum)
        {

            ccontrols.creatLabUI(ref lab_Name, ref lab_Value,
                                      120, 20, 80, 20, parent, DataNum, 4, 35, 250);
            for (int i = 0; i < DataNum; i++)
            {
                if (lab_Name[i] != null)
                    lab_Name[i].Text = ReadNameConst[i] + ":";

                if (lab_Value[i] != null)
                    lab_Value[i].Text = "未检测";
            }
            //label7.Text = "通讯状态：断开";
            //label7.ForeColor = Color.Red;
            //label7.Font = new System.Drawing.Font("宋体", 9);


        }


        private static Mutex mutex = new Mutex();
        //串口接收数据，全部读取后解析
        private void Series_DataReceived(object sender, SerialDataReceivedEventArgs e)
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
        //解析和校验串口数据
        public void ReadBuff()
        {

            try
            {
                int DataFlag = 0;
                string readdata = "";
                UInt64 EERCount = 0;
                byte data = 0;
                ushort Local_CHKSUM = 0;
                int queuecount = 0;
                string Request = "";
                string Respond = "";
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

                                    if (DataFlag == 0)
                                    {
                                        DataFlag = 1;
                                        Request = HexToString(showdata);
                                    } else if (DataFlag == 1)
                                    {
                                        Respond = HexToString(showdata);

                                        string[] check = new string[2];
                                        check[0] = Request.Substring(3, 2);
                                        check[1] = Respond.Substring(3, 2);
                                        if (check[0] == check[1])
                                        {
                                            DataFlag = 0;
                                            Invoke(new MethodInvoker(
                                                 () =>
                                                 {
                                                     MonDataAnalysis(Request, Respond);
                                                 }));
                                         
                                        }
                                        else
                                        {
                                            Request = Respond;
                                            Respond = "";
                                        }
                                    
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

          
            SHOW = 1;
            dataflowform.Show();
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            DataFlowForm dataflowset = new DataFlowForm(this);

            dataflowform = dataflowset;
            dataflowform.Text = "通信数据流";
        }

       

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void config_Click(object sender, EventArgs e)
        {

        }
       // static int flag = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
          
            DateTimePicker NowTime = new DateTimePicker();

            NowTime.Format = DateTimePickerFormat.Long;
            NowTime.Format = DateTimePickerFormat.Time;
            label5.Text = NowTime.Value.ToString();
  
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //************************以上为串口部分*********************************************************
    }
}
