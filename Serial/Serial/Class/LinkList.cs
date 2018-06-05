using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Serial
{

    public class LinkListNode
    {
        public LinkListNode()
        {
            ReadFunc = 0;
            WriteFunc = 0;
            Num = 0;

            FirstAddr = 0;
            LastAddr = 0;
          
        }

        public int ReadFunc;
        public int WriteFunc;
        public int MonReadFunc;
        public int MonWriteFunc;
        public string DataType;

        public int Num;
      
        public int FirstAddr;
        public int LastAddr;

        public int Index;
        public string PageName;

        public int[] ViewRows = new int[100];  //配置视窗行
        public int[] ViewCells = new int[100];  //配置视窗行

        public string[] Unit = new string[100];    //单位
        public Int16[] Precision = new Int16[100];  //精度
        public string[] ReadNameConst = new string[100]; //名字
        public Label[] ReadName = new Label[100];  //名字文本框
        public Label[] ReadValue = new Label[100]; //值显示文本框
        public Int16[] ShowMode = new Int16[100];      //读时是显示方式  写是显示的默认值

        public LinkListNode Previous;//前一个
        public LinkListNode Next;// 后一个


    }

    public class LinkedList
    {
        public LinkedList() //构造函数  初始化
        {
            ListNodeCount = 0;
            Head = null;
            Tail = null;
        }
        public LinkListNode Head; // 头指针
        public LinkListNode Tail;// 尾指针  
        public LinkListNode Current;// 当前指针
        public int ListNodeCount; //链表数据的个数


        public void Append() //尾部添加数据 
        {
            LinkListNode NewNode = new LinkListNode();

            if (IsNull())

            //如果头指针为空
            {
                Head = NewNode;
                Tail = NewNode;
            }
            else
            {
                Tail.Next = NewNode;
                NewNode.Previous = Tail;
                Tail = NewNode;
            }
            Current = NewNode;
            ListNodeCount += 1;//链表数据个数加一
        }

        public void Delete()
        {
            //若为空链表
            if (!IsNull())
            {
                //若删除头
                if (IsBof())
                {
                    Head = Current.Next;
                    Current = Head;
                    ListNodeCount -= 1;
                    return;
                }

                //若删除尾
                if (IsEof())
                {
                    Tail = Current.Previous;
                    Current = Tail;
                    ListNodeCount -= 1;
                    return;
                }

                //若删除中间数据
                Current.Previous.Next = Current.Next;
                Current = Current.Previous;
                ListNodeCount -= 1;
                return;
            }
        }

        // 向后移动一个数据
        public void MoveNext()
        {
            if (!IsEof())
            {
                Current = Current.Next;
            }
            else
            {
                Current = Head;
            }
        }

        // 判断是否为空链表
        public bool IsNull()
        {
            if (ListNodeCount == 0)
                return true;

            return false;
        }

        // 判断是否为到达头部
        public bool IsBof()
        {
            if (Current == Head)
                return true;
            return false;

        }
        // 判断是否为到达尾  
        public bool IsEof()
        {
            if (Current == Tail)
                return true;
            return false;
        }

    }
}
