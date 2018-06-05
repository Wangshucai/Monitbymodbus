using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

//parameter :参数
//OPER = operating  操作

namespace Serial
{
    class Cparameter
    {
        public  byte SET_STA_NUM = 5;
        public  byte OPER_STA_NUM = 14;                   //机组运行状态数量
        public  byte ERR_STA_NUM = 52;
        public  byte DO_STA_NUM = 16;
        public  byte OPER2_STA_NUM = 30;

        public string[] OperNameConst = new string[14] { "远程开关机：", "运行模式：", "压缩机频率：", "EEV开度：",
                                                          "告警保护：","回水温度：","出水温度：","吸气压力：",
                                                          "环境温度：","排气温度：","吸气温度：","化霜温度：","冷媒进口温度：","水箱温度："};

        public Label[] OperName = new Label[14];  //名称显示控件
        public Label[] OperValue = new Label[14]; //显示状态值控件

        public string[] ERRNameConst = new string[52] { "预留：","其他故障","15V过欠压：","压缩机驱动故障：","PFC温度传感器异常：","软件保护：","IPM温度传感器异常","压缩机型号不匹配：","IPM故障：","压缩机过流：",
                                                        "IPM高温：","PFC故障：","直流母线过压：","直流母线欠压：","AC过欠压：", "AC过流：","电流检测电路故障：","失步故障：","缺相故障：","启动失败：","PFC过热：",
                                                         "IPM硬件故障：","模块复位", "AC频率异常：", "出水T传感器故障：", "进水T传感器故障：","吸气T传感器故障：","低压传感器故障：","化霜T传感器故障：","排气T传感器故障：",
                                                         "水箱温度传感器故障：","环境传感器故障：","化霜：","EEPROM故障：","水流开关保护：","辅助电加热保护：","超环温保护：","低频保护：","冷媒进传感器故障：","冷媒出传感器故障：","拨码错误：",
                                                         "制冷防冻结保护：","制热防高温保护：","排气保护：","高压保护：","低压保护：","驱动板通讯故障：","手操器通讯故障：","室内外机通讯故障：","扩展板通讯故障：","辅路出传感器故障：","辅路进传感器故障："};

        public Label[] ERRName = new Label[52];  //名称显示控件
        public Label[] ERRValue = new Label[52]; //显示状态值控件


        public string[] DONameConst = new string[16] {  "压缩机加热带：", "冷凝风机高速：", "冷凝风机中速：","冷凝风机低速：","定速压缩机：","经济器电磁阀：",
                                                          "底盘加热带：","水泵：","四通阀：","液管电磁阀：","故障：","电加热：","预留：","预留：","预留：","预留："};

        public Label[] DOName = new Label[16];  //名称显示控件
        public Label[] DOValue = new Label[16]; //显示状态值控件

        public string[] OPER2NameConst = new string[30] { "低饱和温度：","辅出口温度：","辅进口温度：", "内机DI1：","内机DI2：",
                                                          "外机DI1：","外机DI2：","型号：","实际频率：","目标频率：","计算能力：","排气过热度：",
                                                          "吸气过热度：","EEV步数：","辅EEV步数：","remote状态：","压缩机电流：","AC电流：",
                                                          "IPM温度：", "PFC温度：", "AC电压：", "冬季防冻保护等级：", "排气保护等级：","压缩机保护等级：",
                                                        "IPM温度保护等级：", "PFC温度保护等级：", "AC电流保护等级：", "低压防冻保护等级：",  "驱动类型：","故障："};



        public Label[] OPER2Name = new Label[30];  //名称显示控件
        public Label[] OPER2Value = new Label[30]; //显示状态值控件


        public ushort[] SetValue = new ushort[5] { 2, 0, 12, 41, 0 };//状态设定 30001
        public List<byte> ShowInformation = new List<byte>();  
        public int ShowInformationCrt;

    }


    class CparameterControl
    {

        //发送和接收标志
       
        public byte CONTROL_FLAG;
        public byte RECEIVE_FLAG;

        //控制标识码

        public readonly byte GET_OPER_REG_STA = 0x01;//  状态读取
        public readonly byte GET_SET_REG_STA = 0x02;//  设定读标志
        public readonly byte SET_SET_REG_STA = 0x03;//  设定写标志

        public readonly byte GET_ERR_REG_STA = 0x04;  //  读取故障标志 
        public readonly byte GET_DO_REG_STA = 0x05; //    读DO标志
        public readonly byte GET_OPER2_REG_STA = 0x06;//  读取保持寄存器2标志
        public readonly byte GET_SOFTWARE_REG_STA = 0x07;//  读取保持寄存器2标志
    }
}
