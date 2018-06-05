using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Drawing.Drawing2D;

namespace Serial
{
    class Ccontrols
    {
        //新建label控件
        public static void creatLabel(ref Label lab, string LabelName, short width, short height, Control parent, short top, short left)
        {
            lab = new Label();
            lab.Name = LabelName;//控件名字
            lab.Location = new Point(left, top);//窗体位置
            lab.Visible = true;   //控件可见
            lab.Width = width;    //宽
            lab.Height = height;  //高
            lab.Text = lab.Name;  //控件显示内容
            parent.Controls.Add(lab);//添加到容器
            lab.ForeColor = Color.DarkBlue;//控件字体设置为蓝色
        }



        // 生成界面
        public void creatLabUI(ref Label[] lab_Name, ref Label[] lab_Value,
                               short SizeName_W, short SizeName_H, short SizeValue_W, short SizeValue_H,
                               Control parent, short CreatCount, short ColumnsCount, short top_offerSet, short left_offerSet)
        {
            // Local_Top 控件Y轴, InName_Left 名字显示X轴, InValue_Left  显示值控件X轴
            short i, Local_Top = 20, InName_Left = 25, InValue_Left = 0;  //(25,20)初始位置
            string InName, InValue;

            for (i = 0; i < CreatCount; i++)
            {

                InName = "InName" + Convert.ToString(i);
                InValue = "InValue" + Convert.ToString(i);


                if (i != 0)//非初始位置
                {
                    if (i % ColumnsCount == 0)   //ColumnsCount  列计数  每行显示多少个数据  此处是每行显示到最多，需要换行
                    {
                        Local_Top = (short)(Local_Top + top_offerSet);//下一行Y值为当前Y值  +  行距
                        InName_Left = 25;                             //行首开始  和初始行对齐
                    }
                    else
                    {
                        InName_Left = (short)(InName_Left + left_offerSet);  //如果不是每行的最大列，则行Y不变,  X则为当前X加上列距
                    }
                }


                InValue_Left = (short)(InName_Left + SizeName_W);//显示值控件的X轴(名字控件的X加这个控件的宽度)  Y轴和名字显示控件一致

                creatLabel(ref lab_Name[i], InName, SizeName_W, SizeName_H, parent, Local_Top, InName_Left);
                creatLabel(ref lab_Value[i], InValue, SizeValue_W, SizeValue_H, parent, Local_Top, InValue_Left);

            }
        }

        //公共过程：刷新label值
        public void RefreshAnalog(Label lab, int value, string unit)
        {
            if (value == -32768)
            {
                lab.Text = "故障";
                lab.ForeColor = Color.Red;
            }
            else if (value == -32767)
            {
                lab.Text = "未检测";
            }
            else
            {
                lab.ForeColor = Color.DarkBlue;


                lab.Text = Convert.ToString((float)value / 10);

                lab.Text = lab.Text + " " + unit;
            }
        }

        public void RefreshAnalogEEV(Label lab, short value, string unit)
        {
            if (value == -32768)
            {
                lab.Text = "故障";
                lab.ForeColor = Color.Red;
            }
            else if (value == -32767)
            {
                lab.Text = "未检测";
            }
            else
            {
                lab.ForeColor = Color.DarkBlue;

                lab.Text = Convert.ToString((float)value);

                lab.Text = lab.Text + " " + unit;
            }
        }

    }
}
