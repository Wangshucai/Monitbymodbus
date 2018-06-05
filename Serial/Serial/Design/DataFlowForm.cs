using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Serial
{
    public partial class DataFlowForm : Form
    {
        private MainForm frmMain = new MainForm();

        public DataFlowForm(MainForm parentt)
        {
            InitializeComponent();
            frmMain = parentt;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 这里写关闭窗体要执行的代码
            frmMain.SHOW = 0;
            //frmMain.Childflag = 0;
          
          //  base.OnClosing(e);
            this.Hide();
            e.Cancel = true;
        }


        private void DataFlowForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "暂停")
            {

                frmMain.SHOW = 0;
                button1.Text = "开始";

            }
            else
            {

                frmMain.SHOW = 1;
                button1.Text = "暂停";
            }
        }
    }
}
