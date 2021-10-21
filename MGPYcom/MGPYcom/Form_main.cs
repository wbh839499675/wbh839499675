using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Threading;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Management;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MGPYcom
{
    public interface IView
    {
        void SetController(IController controller);

        //Open serial port event
        void OpenComEvent(Object sender, SerialPortEventArgs e);

        //Close serial port event
        void CloseComEvent(Object sender, SerialPortEventArgs e);

        //Serial port receive data event
        void ComReceiveDataEvent(Object sender, SerialPortEventArgs e);

    }
    public partial class Form_main : Form, IView
    {
        private IController controller;
        
        private int sendBytesCount = 0;
        private int receiveBytesCount = 0;
        //private String strSend = "";
        //当前路径
        private string curFilePath = "";

        //待复制并粘贴的文件\文件夹的源路径
        private string[] copyFilesSourcePaths = new string[200];

        //是否移动文件
        private bool isMove = false;

        private string fileComPort = "";
        public string curFilePathAT = "/usr";

        public string strSendText = "";

        private bool bRecv = false;
        private String strRecv1st = "";
        private int iUp = 1;
        private int iDown = 1;
        

        private List<string> listSendText = new List<string>(1024);
        private int iCMD = 0;

		//是否选择显示行号
        private bool lineShowSelect = false;

        //交互界面主题切换
        private bool cmdLineFormat = false;

        //默认窗口置顶
        private bool formOnTopStatus = true;

        //命令行交互使能
        private bool cmdLineEnable = false;

        //关键字匹配相关变量
        private int index = -1;
        private int startPos = -1;
        private int nextIndex = 0;

        public Form_main()
        {
            
            InitializeComponent();

        }
        private void statustimer_Tick(object sender, EventArgs e)
        {
            this.statusTimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
        public void SetController(IController controller)
        {
            this.controller = controller;
        }

        public void OpenComEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new Action<Object, SerialPortEventArgs>(OpenComEvent), sender, e);
                return;
            }

            if (e.isOpend)  //Open successfully
            {
                statuslabel.Text = comboBox_port.Text + " Opend";
                button_OpenPort.Text = "关闭串口";
                //sendbtn.Enabled = true;
                //autoSendcbx.Enabled = true;
                //autoReplyCbx.Enabled = true;

                comboBox_port.Enabled = false;
                comboBox_band.Enabled = false;
                label_portSelect.Enabled = false;
                label_portBaudrate.Enabled = false;
                this.toolStripButton_pause.Enabled = true;
                this.toolStripButton_pause.Image = MGPYcom.Properties.Resources.pause;
                this.toolStripButton_play.Enabled = false;
                this.toolStripButton_play.Image = MGPYcom.Properties.Resources.play_disable;
                this.button_OpenPort.Image = MGPYcom.Properties.Resources.closeCOM;
                cmdLineEnable = true;       //使能命令行交互
                //dataBitsCbx.Enabled = false;
                //stopBitsCbx.Enabled = false;
                //parityCbx.Enabled = false;
                //handshakingcbx.Enabled = false;
                //refreshbtn.Enabled = false;
            }
            else    //Open failed
            {
                statuslabel.Text = "Open failed !";
                //sendbtn.Enabled = false;
                //autoSendcbx.Enabled = false;
                //autoReplyCbx.Enabled = false;
            }
        }
        public void CloseComEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new Action<Object, SerialPortEventArgs>(CloseComEvent), sender, e);
                return;
            }

            if (!e.isOpend) //close successfully
            {
                statuslabel.Text = comboBox_port.Text + " Closed";
                button_OpenPort.Text = "打开串口";

                comboBox_port.Enabled = true;
                comboBox_band.Enabled = true;
                label_portSelect.Enabled = true;
                label_portBaudrate.Enabled = true;
                this.toolStripButton_pause.Enabled = false;
                this.toolStripButton_pause.Image = MGPYcom.Properties.Resources.pause_disable;
                this.toolStripButton_play.Enabled = true;
                this.toolStripButton_play.Image = MGPYcom.Properties.Resources.play_disable;
                this.button_OpenPort.Image = MGPYcom.Properties.Resources.openCOM;
                cmdLineEnable = false;       //关闭命令行交互
                //dataBitsCbx.Enabled = true;
                //stopBitsCbx.Enabled = true;
                //parityCbx.Enabled = true;
                //handshakingcbx.Enabled = true;
                //refreshbtn.Enabled = true;
            }
        }

        public string CustomToEscape(string str)
        {
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '\\': // 反斜杠
                        sb.Append("\\\\");
                        break;
                    case '/': // 正斜杠
                        sb.Append("\\/");
                        break;
                    case '\'': // 单引号
                        sb.Append("\\\'");
                        break;
                    case '\"': // 双引号
                        sb.Append("\\\"");
                        break;
                    case '\a': // 感叹号
                        sb.Append("\\a");
                        break;
                    case '\b': // 退格
                        //sb.Append(c);
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    // 空格是一个空白字符，ASCII码是32
                    // \0某种意义说是字符串结尾
                    case '\0':// \0，会终止对此字符串的所有操作
                        sb.Append("\\0");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    Invoke(new Action<Object, SerialPortEventArgs>(ComReceiveDataEvent), sender, e);
                }
                catch (System.Exception)
                {
                    //disable form destroy exception
                }
                return;
            }

            //String strRecv = Encoding.Default.GetString(e.receivedBytes);
            String strRecv = "";
            
            if (bRecv == false)//第一次尝试读取
            {
                strRecv1st = Encoding.Default.GetString(e.receivedBytes);
                bRecv = true;
                Thread.Sleep(500);
                return;
            }
            else
            {
                //wbh
                //strRecv = strRecv1st + Encoding.Default.GetString(e.receivedBytes);
                strRecv = Encoding.Default.GetString(e.receivedBytes);
                //bRecv = false;
                //strRecv1st = "";
            }
            //this.richTextBox1.AppendText(strRecv);
            //System.Diagnostics.Debug.WriteLine("received data");
            
            if (!strRecv.Contains("\a") && !strRecv.Contains("\b") && !strRecv.Contains("\000") && !strRecv.Contains("\0"))
            {
                //解决命令行允许过程中重启模组，引起工具异常的问题
                if (strRecv.Length <= 0)
                    return;

                strRecv = strRecv.Replace(strSendText,"");

                System.Diagnostics.Debug.WriteLine(strRecv);
                richTextBox1.AppendText(strRecv);
            }
            else
            {
                String strTmp = "";
                if (strRecv.Contains("\b"))
                {
                    string[] removeB = strRecv.Split(new string[] { "\b" }, StringSplitOptions.RemoveEmptyEntries);
                    if (removeB.Length >= 2)
                    {
                        for (int i = 0; i < removeB.Length - 1; i++)
                        {
                            removeB[i] = removeB[i].Substring(0, removeB[i].Length - 1);

                        }
                        foreach (string strP in removeB)
                        {
                            //richTextBox1.AppendText(strP);
                            strTmp = strTmp + strP;
                        }
                        //strRecv = removeB.ToString();
                        //strRecv = strRecv.Replace("\b", "");
                        
                    }

                }
                if (strRecv.Contains("\a"))
                {
                    strTmp = strTmp.Replace("\a", "");
                    //strRecv = strRecv.Replace("\a", "");
                }
                if (strRecv.Contains("\000"))
                {
                    strTmp = strTmp.Replace("\000", "");
                    //strRecv = strRecv.Replace("\000", "");

                }
                if (strRecv.Contains("\0"))
                {
                    strTmp = strTmp.Replace("\0", "");
                    //strRecv = strRecv.Replace("\000", "");

                }
                strTmp = strTmp.Replace(strSendText, "");
                richTextBox1.AppendText(strTmp);
            }

            receiveBytesCount += e.receivedBytes.Length;
            toolStripStatusRx.Text = "Received: " + receiveBytesCount.ToString();
        }
        private void Form_load(object sender, EventArgs e)
        {
            InitCom();
            //richTextBox1.KeyDown += new KeyEventHandler(richTextBox1_KeyDown);
            timer_monitor.Interval = 10;
        }

        private enum HardwareEnum
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。
 
            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }
        /// <summary>
        /// WMI取硬件信息
        /// </summary>
        /// <param name="hardType"></param>
        /// <param name="propKey"></param>
        /// <returns></returns>
        private static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        {
            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null && hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                        {
                            strs.Add(hardInfo.Properties[propKey].Value.ToString());
                        }
 
                    }
                    searcher.Dispose();
                }
 
                return strs.ToArray();
            }
            catch
            {
                return strs.ToArray();
            }
        }
 
        /// <summary>
        /// 串口信息
        /// </summary>
        /// <returns></returns>
        public static string[] GetSerialPort()
        {
            return MulGetHardwareInfo(HardwareEnum.Win32_PnPEntity, "Name");
        }


        public void InitCom()
        {
            comboBox_band.Items.Add("1200");//选择项1
            comboBox_band.Items.Add("2400");
            comboBox_band.Items.Add("4800");
            comboBox_band.Items.Add("9600");
            comboBox_band.Items.Add("14400");
            comboBox_band.Items.Add("19200");
            comboBox_band.Items.Add("38400");
            comboBox_band.Items.Add("57600");
            comboBox_band.Items.Add("115200");
            comboBox_band.Items.Add("230400");
            comboBox_band.Items.Add("256000");
            comboBox_band.Items.Add("460800");
            comboBox_band.Items.Add("921600");

            // set default baudrate to 115200
            comboBox_band.Text = "115200";

            string[] ArrayComPortsNames = SerialPort.GetPortNames();
            List<string> strList = new List<string>();

            ArrayComPortsNames = GetSerialPort();

            foreach (string strcom in ArrayComPortsNames)
            {
                if(strcom.Contains("MEIG USB AT "))
                {
                    fileComPort = strcom;
                }
            }
            fileComPort = fileComPort.Replace("MEIG USB AT (", "");
            fileComPort = fileComPort.Replace(")", "");
            if (ArrayComPortsNames.Length == 0)
            {
                //statuslabel.Text = "No COM found !";
                button_OpenPort.Enabled = false;
                
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comboBox_port.Items.Add(ArrayComPortsNames[i]);
                    
                }
                comboBox_port.Text = ArrayComPortsNames[0];
                
                button_OpenPort.Enabled = true;
               
            }
            // init richtextBox display
            //richTextBox1.Text = ">>>";
        }

        private void button_OpenPort_Click(object sender, EventArgs e)
        {
            if (button_OpenPort.Text == "打开串口")
            {
                richTextBox1.Clear();
                string[] sArray = comboBox_port.Text.Split('(');
                String strPort = "";
                strPort = sArray[1];
                strPort = strPort.Replace(")", "");

                controller.OpenSerialPort(strPort, comboBox_band.Text, "8", "One", "None", "None");
                //controller.OpenSerialPort(comboBox_port.Text, comboBox_band.Text, comboBox_data.Text,
                //                        comboBox_stop.Text, comboBox_parity.Text, comboBox_flowctrol.Text);
            }
            else
            {
                controller.CloseSerialPort();
            }
        }
        
        private void SendToPort(object sender, KeyEventArgs e)
        {
            if (cmdLineEnable == false)
            {
                return;
            }

            if (e.KeyCode == Keys.Up && listSendText.Count>0)
            {
                string[] a1 = Regex.Split(richTextBox1.Text, "\n", RegexOptions.IgnoreCase);
                //String str1 = richTextBox1.Lines[richTextBox1.Lines.Length - 1].Replace("\r\n","");
                String str2 = "";

                iCMD--;
                if (iCMD < 0)
                {
                    str2 = listSendText[0].ToString();
                    iCMD = 0;
                }
                else
                    str2 = listSendText[iCMD].ToString();
                
                //if (iUp < listSendText.Count)
                    
                    
                    //iUp++;
                if (a1[a1.Length - 1].Contains(">>>"))
                {
                    a1[a1.Length - 1] = ">>> " + str2;
                }
                else
                {
                    a1[a1.Length - 1] = str2;
                }
                string bbb = "";
                foreach (string aaa in a1)
                {
                    bbb = bbb+aaa+"\n";
                }
                bbb = bbb.Substring(0, bbb.LastIndexOf('\n')-2);
                richTextBox1.Text = bbb;
                bbb = "";
                //richTextBox1.Text = richTextBox1.Text.Replace(str1, str2);
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.ScrollToCaret();
            }
            if (e.KeyCode == Keys.Down && listSendText.Count > 0)
            {
                
                string[] a1 = Regex.Split(richTextBox1.Text, "\n", RegexOptions.IgnoreCase);
                //String str1 = richTextBox1.Lines[richTextBox1.Lines.Length - 1].Replace("\r\n","");
                //String str2 = listSendText[listSendText.Count -iUp+ iDown].ToString();
                String str2 = "";
                iCMD++;
                if (iCMD >= listSendText.Count)
                {
                    str2 = " \n";
                    iCMD = listSendText.Count;
                }
                else
                    str2 = listSendText[iCMD].ToString();
                //if (iDown < (listSendText.Count-iUp))
                    //iDown++;
                    
                if (a1[a1.Length - 1].Contains(">>>"))
                {
                    a1[a1.Length - 1] = ">>> " + str2;
                }
                else
                {
                    a1[a1.Length - 1] = str2;
                }
                string bbb = "";
                foreach (string aaa in a1)
                {
                    bbb = bbb + aaa + "\n";
                }
                bbb = bbb.Substring(0, bbb.LastIndexOf('\n') - 2);
                richTextBox1.Text = bbb;
                bbb = "";
                //richTextBox1.Text = richTextBox1.Text.Replace(str1, str2);
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.ScrollToCaret();
            }
            
            if (e.KeyCode == Keys.Control || e.KeyCode == Keys.Enter)//捕获回车键
            {
                string[] tempArray = richTextBox1.Lines;

                if (tempArray[tempArray.Length - 2] == "")
                {
                    //return;
                }

                String sendText = tempArray[tempArray.Length -2 ].Replace("\r\n","") + "\r\n";
                bool flag = false;
                if (sendText == null)
                {
                    //return;
                }
                sendText = sendText.Replace(">>> ", "");
                flag = controller.SendDataToCom(sendText);
                sendBytesCount += sendText.Length;
                
                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                    strSendText = sendText;
                    listSendText.Add(sendText);
                    iCMD = listSendText.Count;
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }
                //update status bar
                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();
            }
            if (e.KeyCode == Keys.Control || e.KeyCode == Keys.Tab) //捕获TAB键
            {
                int lineIndex = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart);
                string[] tempArray = richTextBox1.Lines;

                String sendText = tempArray[lineIndex].Replace("\r\n", "") + "\t";
                bool flag = false;

                sendText = sendText.Replace(">>> ", "");
                sendText = sendText.Replace("\t\t", "\t");
                flag = controller.SendDataToCom(sendText);

                sendBytesCount += sendText.Length;

                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                    strSendText = sendText;
                    listSendText.Add(sendText);
                    iCMD = listSendText.Count;
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }
                //update status bar
                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();
                
            }
            if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.A)
            {
                String sendText = "CTRL-A" + "\r\n";
                Byte[] bsend = new Byte[] { 1 };
                bool flag = false;
                if (sendText == null)
                {
                    return;
                }
                sendText = sendText.Replace(">>> ", "");
                //flag = controller.SendDataToCom(sendText);
                flag = controller.SendDataToCom(bsend);
                sendBytesCount += sendText.Length;
                
                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }

                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();
 
            }
            if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.B)
            {

                Byte[] bsend = new Byte[] { 2 };
                String sendText = "CTRL-B" + "\r\n";
                bool flag = false;
                if (sendText == null)
                {
                    return;
                }
                sendText = sendText.Replace(">>> ", "");
                //flag = controller.SendDataToCom(sendText);
                flag = controller.SendDataToCom(bsend);
                sendBytesCount += sendText.Length;

                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }

                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();

            }
            if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.C)
            {

                Byte[] bsend = new Byte[] { 3 };
                String sendText = "CTRL-C" + "\r\n";
                bool flag = false;
                if (sendText == null)
                {
                    return;
                }
                sendText = sendText.Replace(">>> ", "");
                //flag = controller.SendDataToCom(sendText);
                flag = controller.SendDataToCom(bsend);
                sendBytesCount += sendText.Length;

                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }

                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();

            }
            if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.D)
            {

                Byte[] bsend = new Byte[] { 4 };
                String sendText = "CTRL-D" + "\r\n";
                bool flag = false;
                if (sendText == null)
                {
                    return;
                }
                sendText = sendText.Replace(">>> ", "");
                //flag = controller.SendDataToCom(sendText);
                flag = controller.SendDataToCom(bsend);
                sendBytesCount += sendText.Length;

                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }

                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();

            }
            if (e.Modifiers.CompareTo(Keys.Control) == 0 && e.KeyCode == Keys.E)
            {

                Byte[] bsend = new Byte[] { 5 };
                String sendText = "CTRL-E" + "\r\n";
                bool flag = false;
                if (sendText == null)
                {
                    return;
                }
                sendText = sendText.Replace(">>> ", "");
                //flag = controller.SendDataToCom(sendText);
                flag = controller.SendDataToCom(bsend);
                sendBytesCount += sendText.Length;

                if (flag)
                {
                    statuslabel.Text = "Send OK !";
                }
                else
                {
                    statuslabel.Text = "Send failed !";
                }

                toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();

            }
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void 清除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }

        private void toolStripButton_clean_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void showLineNum()
        {
            Point p = this.richTextBox1.Location;
            int crntFirstIndex = this.richTextBox1.GetCharIndexFromPosition(p);
            int crntFirstLine = this.richTextBox1.GetLineFromCharIndex(crntFirstIndex);
            Point crntFirstPos = this.richTextBox1.GetPositionFromCharIndex(crntFirstIndex);
            p.Y += this.richTextBox1.Height;
            int crntLastIndex = this.richTextBox1.GetCharIndexFromPosition(p);
            int crntLastLine = this.richTextBox1.GetLineFromCharIndex(crntLastIndex);
            Point crntLastPos = this.richTextBox1.GetPositionFromCharIndex(crntLastIndex);
            Graphics g = this.pannel_lineNum.CreateGraphics();
            Font font = new Font(this.richTextBox1.Font, this.richTextBox1.Font.Style);
            SolidBrush brush = new SolidBrush(Color.Green);

            Rectangle rect = this.pannel_lineNum.ClientRectangle;
            brush.Color = this.pannel_lineNum.BackColor;

            g.FillRectangle(brush, 0, 0, this.pannel_lineNum.ClientRectangle.Width, this.pannel_lineNum.ClientRectangle.Height);
            brush.Color = Color.Green;

            int lineSpace = 0;
            if (crntFirstLine != crntLastLine)
            {
                lineSpace = (crntLastPos.Y - crntFirstPos.Y) / (crntLastLine - crntFirstLine);
            }
            else
            {
                lineSpace = Convert.ToInt32(this.richTextBox1.Font.Size);
            }
            int brushX = this.pannel_lineNum.ClientRectangle.Width - Convert.ToInt32(font.Size * 3);
            int brushY = crntLastPos.Y + Convert.ToInt32(font.Size * 0.21f);
            for (int i = crntLastLine; i >= crntFirstLine; i--)
            {
                g.DrawString((i + 1).ToString(), font, brush, brushX, brushY);
                brushY -= lineSpace;
            }
            g.Dispose();
            font.Dispose();
            brush.Dispose();
        }

        /// <summary>
        /// Auto scroll in receive richTextBox1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            showLineNum();
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            showLineNum();
        }

        private void toolStripButton_showNum_Click(object sender, EventArgs e)
        {
            int width = tabControl_mid.Width;
            int height = tabControl_mid.Height;
            if (lineShowSelect == false)
            {
                lineShowSelect = true;
                richTextBox1.Location = new System.Drawing.Point(40, 0);
                richTextBox1.Size = new System.Drawing.Size(width - 40, height);
                this.pannel_lineNum.Show();
                showLineNum();
            }
            else
            {
                lineShowSelect = false;
                this.pannel_lineNum.Hide();
                richTextBox1.Location = new System.Drawing.Point(0, 0);
                richTextBox1.Size = new System.Drawing.Size(width, height);
            }
        }

        //存储窗体左边目录区的图标在ImageList（具体是ilstDirectoryIcons）中的索引
        private class IconsIndexes
        {
            public const int FixedDrive = 0; //固定磁盘
            public const int CDRom = 1; //光驱
            public const int RemovableDisk = 2; //可移动磁盘
            public const int Folder = 3; //文件夹图标
            public const int RecentFiles = 4; //最近访问
        }

        //当前路径节点
        private DoublyLinkedListNode curPathNode = null;

        //双向链表节点类(用来存储用户的历史访问路径)
        class DoublyLinkedListNode
        {
            //保存的路径
            public string Path { set; get; }
            public DoublyLinkedListNode PreNode { set; get; }
            public DoublyLinkedListNode NextNode { set; get; }

        }

         //在右窗体中显示指定路径下的所有文件/文件夹
        public void ShowFilesList(string path, bool isRecord)
        {
            curPathNode = new DoublyLinkedListNode();
            //后退按钮可用
            //tsbtnBack.Enabled = true;

            //需要保存记录，则需要创建新的路径节点
            if (isRecord)
            {
                //保存用户的历史访问路径
                DoublyLinkedListNode newNode = new DoublyLinkedListNode();
                newNode.Path = path;
                curPathNode.NextNode = newNode;
                newNode.PreNode = curPathNode;

                curPathNode = newNode;
            }


            //开始数据更新
            lvwFiles.BeginUpdate();

            //清空lvwFiles
            lvwFiles.Items.Clear();

            if (path == "最近访问")
            {
                //获取最近使用的文件的路径的枚举集合
                var recentFiles = RecentFilesUtil.GetRecentFiles();

                foreach (string file in recentFiles)
                {
                    if (File.Exists(file))
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                        //为exe文件或无拓展名
                        if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                        {
                            //通过当前系统获得文件相应图标
                            Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                            //因为不同的exe文件一般图标都不相同，所以不能按拓展名存取图标，应按文件名存取图标
                            ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                            item.ImageKey = fileInfo.Name;
                        }
                        //其他文件
                        else
                        {
                            if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                            {
                                Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                                //因为类型（除了exe）相同的文件，图标相同，所以可以按拓展名存取图标
                                ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                            }

                            item.ImageKey = fileInfo.Extension;
                        }

                        item.Tag = fileInfo.FullName;
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.SubItems.Add(fileInfo.Extension + "文件");
                        item.SubItems.Add(AttributeForm.ShowFileSize(fileInfo.Length).Split('(')[0]);

                    }
                    else if (Directory.Exists(file))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(file);

                        ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                        item.Tag = dirInfo.FullName;
                        item.SubItems.Add(dirInfo.LastWriteTime.ToString());
                        item.SubItems.Add("文件夹");
                        item.SubItems.Add("");
                    }
                }
            }
            else
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                    FileInfo[] fileInfos = directoryInfo.GetFiles();

                    //删除ilstIcons(ImageList)中的exe文件的图标，释放ilstIcons的空间
                    foreach (ListViewItem item in lvwFiles.Items)
                    {
                        if (item.Text.EndsWith(".exe"))
                        {
                            ilstIcons.Images.RemoveByKey(item.Text);
                        }
                    }



                    //列出所有文件夹
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                        item.Tag = dirInfo.FullName;
                        item.SubItems.Add(dirInfo.LastWriteTime.ToString());
                        item.SubItems.Add("文件夹");
                        item.SubItems.Add("");
                    }

                    //列出所有文件
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                        //为exe文件或无拓展名
                        if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                        {
                            //通过当前系统获得文件相应图标
                            Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                            //因为不同的exe文件一般图标都不相同，所以不能按拓展名存取图标，应按文件名存取图标
                            ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                            item.ImageKey = fileInfo.Name;
                        }
                        //其他文件
                        else
                        {
                            if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                            {
                                Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                                //因为类型（除了exe）相同的文件，图标相同，所以可以按拓展名存取图标
                                ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                            }

                            item.ImageKey = fileInfo.Extension;
                        }

                        item.Tag = fileInfo.FullName;
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.SubItems.Add(fileInfo.Extension + "文件");
                        item.SubItems.Add(AttributeForm.ShowFileSize(fileInfo.Length).Split('(')[0]);
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            //更新当前路径
            curFilePath = path;

            //更新地址栏
            tscboAddress.Text = curFilePath;

            ////更新状态栏
            //tsslblFilesNum.Text = lvwFiles.Items.Count + " 个项目";

            //结束数据更新
            lvwFiles.EndUpdate();
        }
           

        private void tscboAddress_KeyDown(object sender, KeyEventArgs e)
        {
            //回车输入新地址
            if (e.KeyCode == Keys.Enter)
            {
                string newPath = tscboAddress.Text;

                if (newPath == "")
                {
                    return;
                }
                else if (!Directory.Exists(newPath))
                {
                    return;
                }

                ShowFilesList(newPath, true);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string newPath = tscboAddress.Text;

            if (newPath == "")
            {
                tscboAddress.Text = System.Environment.CurrentDirectory;
                newPath = tscboAddress.Text;
                //return;
            }
            else if (!Directory.Exists(newPath))
            {
                return;
            }

            ShowFilesList(newPath, true);

        }


        //打开文件/文件夹
        private void Open()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                string path = lvwFiles.SelectedItems[0].Tag.ToString();

                try
                {
                    //如果选中的是文件夹
                    if (Directory.Exists(path))
                    {
                        //打开文件夹
                        ShowFilesList(path, true);
                    }
                    //如果选中的是文件
                    else
                    {
                        //打开文件
                        Process.Start(path);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //激活某项事件(默认激活动作是“双击”)
        private void lvwFiles_ItemActivate(object sender, EventArgs e)
        {
            Open();
        }
        //检查文件名是否合法,文件名中不能包含字符\/:*?"<>|
        private bool IsValidFileName(string fileName)
        {
            bool isValid = true;

            //非法字符
            string errChar = "\\/:*?\"<>|";

            for (int i = 0; i < errChar.Length; i++)
            {
                if (fileName.Contains(errChar[i].ToString()))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }
        private void lvwFiles_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string newName = e.Label;

            //选中项
            ListViewItem selectedItem = lvwFiles.SelectedItems[0];

            //如果名称为空
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("文件名不能为空！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //显示时，恢复原来的标签
                e.CancelEdit = true;
            }
            //标签没有改动
            else if (newName == null)
            {
                return;
            }
            //标签改动了，但是最终还是和原来一样
            else if (newName == selectedItem.Text)
            {
                return;
            }
            //文件名不合法
            else if (!IsValidFileName(newName))
            {
                MessageBox.Show("文件名不能包含下列任何字符:\r\n" + "\t\\/:*?\"<>|", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //显示时，恢复原来的标签
                e.CancelEdit = true;
            }
            else
            {
                Computer myComputer = new Computer();

                //如果是文件
                if (File.Exists(selectedItem.Tag.ToString()))
                {
                    //如果当前路径下有同名的文件
                    if (File.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("当前路径下有同名的文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //显示时，恢复原来的标签
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameFile(selectedItem.Tag.ToString(), newName);

                        FileInfo fileInfo = new FileInfo(selectedItem.Tag.ToString());
                        string parentPath = Path.GetDirectoryName(fileInfo.FullName);
                        string newPath = Path.Combine(parentPath, newName);

                        //更新选中项的Tag
                        selectedItem.Tag = newPath;

                        //刷新左边的目录树
                        //LoadChildNodes(curSelectedNode);
                    }
                }
                //如果是文件夹
                else if (Directory.Exists(selectedItem.Tag.ToString()))
                {
                    //如果当前路径下有同名的文件夹
                    if (Directory.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("当前路径下有同名的文件夹！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //显示时，恢复原来的标签
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameDirectory(selectedItem.Tag.ToString(), newName);

                        DirectoryInfo directoryInfo = new DirectoryInfo(selectedItem.Tag.ToString());
                        string parentPath = directoryInfo.Parent.FullName;
                        string newPath = Path.Combine(parentPath, newName);

                        //更新选中项的Tag
                        selectedItem.Tag = newPath;

                        //刷新左边的目录树
                        //LoadChildNodes(curSelectedNode);
                    }
                }
            }
        }

        private void tsbtnUpArrow_Click(object sender, EventArgs e)
        {
            if (curFilePath == "")
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(curFilePath);

            //根目录诸如：C:\ 、D:\、E:\ 等
            //如果还没到达根目录
            if (directoryInfo.Parent != null)
            {
                ShowFilesList(directoryInfo.Parent.FullName, true);
            }
            //已经到达根目录，则停止
            else
            {
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox_port.Items.Clear();

            //string[] ArrayComPortsNames = SerialPort.GetPortNames();
            string[] ArrayComPortsNames = GetSerialPort();

            if (ArrayComPortsNames.Length == 0)
            {
                //statuslabel.Text = "No COM found !";
                button_OpenPort.Enabled = false;
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comboBox_port.Items.Add(ArrayComPortsNames[i]);
                }
                comboBox_port.Text = ArrayComPortsNames[0];
                button_OpenPort.Enabled = true;
            }
        }


        public void ShowFilesList_AT(string[] sArray)
        {
            curPathNode = new DoublyLinkedListNode();
            //后退按钮可用
            //tsbtnBack.Enabled = true;

            


            //开始数据更新
            lvwFiles_AT.BeginUpdate();

            //清空lvwFiles
            lvwFiles_AT.Items.Clear();
            /*
            if (sArray[0] != "/usr\r\n")
            {
                ListViewItem item0 = lvwFiles_AT.Items.Add("/usr");
                item0.Tag = "/usr";
                item0.SubItems.Add("文件夹");
                item0.SubItems.Add("");
            }
             * */
            foreach (String name in sArray)
            {

                
                if (name.Contains(","))
                {
                    string[] strArray = name.Split(',');
                    ListViewItem item = lvwFiles_AT.Items.Add(strArray[0]);
                    item.Tag = name;
                    item.SubItems.Add("文件");
                    item.SubItems.Add(strArray[1]);
                }
                else
                {
                    ListViewItem item = lvwFiles_AT.Items.Add(name);
                    item.Tag = name;
                    item.SubItems.Add("文件夹");
                    item.SubItems.Add("");
                }
                    

            }


            //结束数据更新
            lvwFiles_AT.EndUpdate();
            
        }
        public bool SendAtCmd( String sendText ,ref string[] sArray,String strSplit)
        {
            if ((fileComPort == null) || (fileComPort == ""))
            {
                MessageBox.Show("MEIG USB AT COM not connect", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            String recvText = "";
            SerialPort sp = new SerialPort();
            //sp.PortName = comboBox_filePort.Text;
            //sp.BaudRate = Convert.ToInt32(comboBox_fileBand.Text);
            sp.PortName = fileComPort;
            sp.BaudRate = 115200;
            sp.DataBits = Convert.ToInt16("8");
            sp.RtsEnable = true;
            sp.DtrEnable = true;

            try
            {
                sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
                sp.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
                sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
                sp.WriteTimeout = 1000; /*Write time out*/
                sp.Open();
                sp.Write(Encoding.Default.GetBytes(sendText), 0, Encoding.Default.GetBytes(sendText).Length);
                Thread.Sleep(1000);
            }
            catch (System.Exception)
            {
                MessageBox.Show(string.Format("{0}", sp.PortName) + "被占用！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //int len = sp.BytesToRead;
            //Byte[] data = new Byte[len];
            //sp.Read(data, 0, len);
            //recvText = Encoding.Default.GetString(data);
            try
            {
                int len = sp.BytesToRead;
                Byte[] data = new Byte[len];
                sp.Read(data, 0, len);
                recvText = Encoding.Default.GetString(data);
            }
            catch (System.Exception)
            {
                return false;
            }

            if (recvText.Contains("OK"))
            {
                recvText = recvText.Replace(sendText, "");
                recvText = recvText.Replace("OK", "");
                recvText = recvText.Replace("\r\n", "");
                recvText = recvText.Replace(" ", "");
                recvText = recvText.Replace("\"", "");
                recvText = recvText.Replace("+", "");
                recvText = recvText.Replace(":", "");
                sArray = Regex.Split(recvText, strSplit, RegexOptions.IgnoreCase);
                //recvText = sArray.ToString();
                sArray[0] = "/usr";
                //sArray[0] = sendText.Replace("AT+FSLSTFILE=", "");
                //sArray[0] = sArray[0].Replace("\"", "");
                foreach (String strp in sArray)
                {
                    if (strp != "")
                    {
                        //MessageBox.Show(strp);
                    }
                }
            }
            else
            {
                sp.Close();
                return false;
            }
            sp.Close();
            return true;
        }
        public bool SendAtCmd_NoSplit(String sendText, ref String getText)
        {
            String recvText = "";
            SerialPort sp = new SerialPort();
            //sp.PortName = comboBox_filePort.Text;
            //sp.BaudRate = Convert.ToInt32(comboBox_fileBand.Text);
            sp.PortName = fileComPort;
            sp.BaudRate = 115200;
            sp.DataBits = Convert.ToInt16("8");
            sp.RtsEnable = true;
            sp.DtrEnable = true;
            sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
            sp.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
            sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
            sp.WriteTimeout = 1000; /*Write time out*/
            sp.Open();
            sp.Write(Encoding.Default.GetBytes(sendText), 0, Encoding.Default.GetBytes(sendText).Length);
            //Thread.Sleep(1000);
            Thread.Sleep(10);
            int len = sp.BytesToRead;
            Byte[] data = new Byte[len];
            sp.Read(data, 0, len);
            recvText = Encoding.Default.GetString(data);
            if (recvText.Contains("OK"))
            {
                getText = recvText;
            }
            else
            {
                sp.Close();
                return false;
            }
            sp.Close();
            return true;
        }

        public bool SendAtCmd_file(String sendText, String fileName)
        {
            String recvText = "";
            SerialPort sp = new SerialPort();
            //sp.PortName = comboBox_filePort.Text;
            //sp.BaudRate = Convert.ToInt32(comboBox_fileBand.Text);
            sp.PortName = fileComPort;
            sp.BaudRate = 115200;
            sp.DataBits = Convert.ToInt16("8");
            sp.RtsEnable = true;
            sp.DtrEnable = true;
            sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
            sp.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
            sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
            sp.WriteTimeout = 1000; /*Write time out*/
            sp.Open();
            sp.Write(Encoding.Default.GetBytes(sendText), 0, Encoding.Default.GetBytes(sendText).Length);
            //Thread.Sleep(1000);
            Thread.Sleep(10);
            int len = sp.BytesToRead;
            Byte[] data = new Byte[len];
            sp.Read(data, 0, len);
            recvText = Encoding.Default.GetString(data);
            if (recvText.Contains(">"))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        byte[] buffur = new byte[fs.Length];
                        fs.Read(buffur, 0, (int)fs.Length);
                        sp.Write(buffur, 0, buffur.Length);
                        //System.Diagnostics.Debug.WriteLine("send data..................");

                        timer_monitor.Start();
                        progressBar_fileSend.Maximum = (int)fs.Length;
                        //progressBar_fileSend.Value = 80;
                        for (int i = 0; i < 600;i++ )
                        {
                            Thread.Sleep(100);
                            sp.Read(data, 0, len);
                            if (Encoding.Default.GetString(data).Contains("OK"))
                            {
                                break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                sp.Close();
                return false;
            }

            sp.Close();
            return true;
        }

        private void tsmiProperties1_Click(object sender, EventArgs e)
        {
            //右边窗体中没有文件/文件夹被选中
            if (lvwFiles.SelectedItems.Count == 0)
            {

                if (curFilePath == "最近访问")
                {
                    MessageBox.Show("不能查看当前路径的属性！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AttributeForm attributeForm = new AttributeForm(curFilePath);

                //显示当前文件夹的属性
                attributeForm.Show();
            }
            //右边窗体中有文件/文件夹被选中
            else
            {
                //显示被选中的第一个文件/文件夹的属性
                AttributeForm attributeForm = new AttributeForm(lvwFiles.SelectedItems[0].Tag.ToString());

                attributeForm.Show();
            }
        }

        private void tsmiNewFolder1_Click(object sender, EventArgs e)
        {
            if (curFilePath == "最近访问")
            {
                MessageBox.Show("不能在当前路径下新建文件夹！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int num = 1;
                string path = Path.Combine(curFilePath, "新建文件夹");
                string newFolderPath = path;

                while (Directory.Exists(newFolderPath))
                {
                    newFolderPath = path + "(" + num + ")";
                    num++;
                }

                Directory.CreateDirectory(newFolderPath);

                ListViewItem item = lvwFiles.Items.Add("新建文件夹" + (num == 1 ? "" : "(" + (num - 1) + ")"), IconsIndexes.Folder);

                //真正的路径
                item.Tag = newFolderPath;

                //刷新左边的目录树
                //LoadChildNodes(curSelectedNode);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsmiNewFile1_Click(object sender, EventArgs e)
        {
            if (curFilePath == "最近访问")
            {
                MessageBox.Show("不能在当前路径下新建文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            NewFileForm newFileForm = new NewFileForm(curFilePath, this);
            newFileForm.Show();
        }

        private void tsmiRename_Click(object sender, EventArgs e)
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                //模拟进行编辑标签，实质是为了通过代码触发LabelEdit事件
                lvwFiles.SelectedItems[0].BeginEdit();
            }
        }

        private void tsmiDelete1_Click(object sender, EventArgs e)
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("确定要删除吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    try
                    {
                        foreach (ListViewItem item in lvwFiles.SelectedItems)
                        {
                            string path = item.Tag.ToString();

                            //如果是文件
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            //如果是文件夹
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }

                            lvwFiles.Items.Remove(item);
                        }

                        //刷新左边的目录树
                        //LoadChildNodes(curSelectedNode);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void tsmiCut1_Click(object sender, EventArgs e)
        {
            //获得待复制文件的源路径
            if (lvwFiles.SelectedItems.Count > 0)
            {
                int i = 0;

                foreach (ListViewItem item in lvwFiles.SelectedItems)
                {
                    copyFilesSourcePaths[i++] = item.Tag.ToString();
                }

                isMove = false;
            }

            //准备移动
            isMove = true;
        }

        //通过递归，复制并粘贴文件夹（包含文件夹下的所有文件）
        //没有DirectoryInfo.CopyTo(string path)方法，需要自己实现
        private void CopyAndPasteDirectory(DirectoryInfo sourceDirInfo, DirectoryInfo destDirInfo)
        {
            //判断目标文件夹是否是源文件夹的子目录，是则给出错误提示，不进行任何操作
            for (DirectoryInfo dirInfo = destDirInfo.Parent; dirInfo != null; dirInfo = dirInfo.Parent)
            {
                if (dirInfo.FullName == sourceDirInfo.FullName)
                {
                    MessageBox.Show("无法复制！目标文件夹是源文件夹的子目录！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //创建目标文件夹
            if (!Directory.Exists(destDirInfo.FullName))
            {
                Directory.CreateDirectory(destDirInfo.FullName);
            }

            //复制文件并将文件粘贴到目标文件夹下
            foreach (FileInfo fileInfo in sourceDirInfo.GetFiles())
            {
                fileInfo.CopyTo(Path.Combine(destDirInfo.FullName, fileInfo.Name));
            }

            //递归复制并将子文件夹粘贴到目标文件夹下
            foreach (DirectoryInfo sourceSubDirInfo in sourceDirInfo.GetDirectories())
            {
                DirectoryInfo destSubDirInfo = destDirInfo.CreateSubdirectory(sourceSubDirInfo.Name);
                CopyAndPasteDirectory(sourceSubDirInfo, destSubDirInfo);
            }

        }

        //执行文件夹的“移动到”或“复制到”
        private void MoveToOrCopyToDirectoryBySourcePath(string sourcePath)
        {
            try
            {
                DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);

                //获取目的路径
                string destPath = Path.Combine(curFilePath, sourceDirectoryInfo.Name);

                //如果目的路径和源路径相同，则不执行任何操作
                if (destPath == sourcePath)
                {
                    return;
                }

                //移动文件夹到目的路径（当前是在执行“剪切+粘贴”操作）
                if (isMove)
                {
                    //若使用sourceDirectoryInfo.MoveTo(destPath)，则不支持跨磁盘移动文件夹

                    //通过递归，复制并粘贴文件夹（包含文件夹下的所有文件）
                    CopyAndPasteDirectory(sourceDirectoryInfo, new DirectoryInfo(destPath));

                    //删除源文件夹
                    Directory.Delete(sourcePath, true);

                }
                //粘贴文件夹到目的路径（当前是在执行“复制+粘贴”操作）
                else
                {
                    //通过递归，复制并粘贴文件夹（包含文件夹下的所有文件）
                    CopyAndPasteDirectory(sourceDirectoryInfo, new DirectoryInfo(destPath));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //执行文件的“移动到”或“复制到”
        private void MoveToOrCopyToFileBySourcePath(string sourcePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sourcePath);

                //获取目的路径
                string destPath = Path.Combine(curFilePath, fileInfo.Name);

                //如果目的路径和源路径相同，则不执行任何操作
                if (destPath == sourcePath)
                {
                    return;
                }

                //移动文件到目的路径（当前是在执行“剪切+粘贴”操作）
                if (isMove)
                {
                    fileInfo.MoveTo(destPath);
                }
                //粘贴文件到目的路径（当前是在执行“复制+粘贴”操作）
                else
                {
                    fileInfo.CopyTo(destPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void tsmiPaste1_Click(object sender, EventArgs e)
        {
            //没有待粘贴的文件
            if (copyFilesSourcePaths[0] == null)
            {
                return;
            }

            //当前路径无效
            if (!Directory.Exists(curFilePath))
            {
                return;
            }

            if (curFilePath == "最近访问")
            {
                MessageBox.Show("不能在当前路径下进行粘贴操作！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; copyFilesSourcePaths[i] != null; i++)
            {
                //如果是文件
                if (File.Exists(copyFilesSourcePaths[i]))
                {
                    //执行文件的“移动到”或“复制到”
                    MoveToOrCopyToFileBySourcePath(copyFilesSourcePaths[i]);
                }
                //如果是文件夹
                else if (Directory.Exists(copyFilesSourcePaths[i]))
                {
                    //执行文件夹的“移动到”或“复制到”
                    MoveToOrCopyToDirectoryBySourcePath(copyFilesSourcePaths[i]);
                }

            }

            //在右边窗体显示文件列表
            ShowFilesList(curFilePath, false);

            //刷新左边的目录树
            //LoadChildNodes(curSelectedNode);

            //置空
            copyFilesSourcePaths = new string[200];
        }

        private void tsmiCopy1_Click(object sender, EventArgs e)
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                int i = 0;

                foreach (ListViewItem item in lvwFiles.SelectedItems)
                {
                    copyFilesSourcePaths[i++] = item.Tag.ToString();
                }

                isMove = false;
            }
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void tsmiRefresh1_Click(object sender, EventArgs e)
        {
            ShowFilesList(curFilePath, false);
        }

        private void lvwFilesAT_ItemActivate(object sender, EventArgs e)
        {
            if (lvwFiles_AT.SelectedItems.Count > 0)
            {
                string path = lvwFiles_AT.SelectedItems[0].Tag.ToString();

                try
                {
                    //如果选中的是文件夹
                    if (!path.Contains(","))
                    {
                        //打开文件夹
                        //ShowFilesList_AT(path, true);
                        String strCMD = "AT+FSLSTFILE=";
                        string[] sArray = { };
                        strCMD = strCMD + "\""+path+"\""+"\r\n";
                        SendAtCmd(strCMD, ref sArray, "FSLSTFILE");
                        ShowFilesList_AT(sArray);

                        string[] sArrayP = { };
                        sArrayP = path.Split('/');
                        curFilePathAT = "";
                        for (int i = 1; i < sArrayP.Length-1; i++)
                        {
                            curFilePathAT = curFilePathAT + "/" + sArrayP[i];
                        }

                    }
                    //如果选中的是文件
                    else
                    {
                        //打开文件
                        //Process.Start(path);
                        
                        String strCMD = "AT+FSRDFILE=";
                        String strGet = "";
                        string[] sArr = path.Split(',');
                        strCMD = strCMD + "\"" + sArr[0] + "\"" + "\r\n";
                        SendAtCmd_NoSplit(strCMD, ref strGet);
                        FileShow fileshow = new FileShow(strGet);
                        fileshow.Show();
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lvwFilesAT_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string newName = e.Label;

            //选中项
            ListViewItem selectedItem = lvwFiles_AT.SelectedItems[0];

            //如果名称为空
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("文件名不能为空！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //显示时，恢复原来的标签
                e.CancelEdit = true;
            }
            //标签没有改动
            else if (newName == null)
            {
                return;
            }
            //标签改动了，但是最终还是和原来一样
            else if (newName == selectedItem.Text)
            {
                return;
            }
            //文件名不合法
            else if (!IsValidFileName(newName))
            {
                MessageBox.Show("文件名不能包含下列任何字符:\r\n" + "\t\\/:*?\"<>|", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //显示时，恢复原来的标签
                e.CancelEdit = true;
            }
            else
            {
                Computer myComputer = new Computer();

                //如果是文件
                if (File.Exists(selectedItem.Tag.ToString()))
                {
                    //如果当前路径下有同名的文件
                    if (File.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("当前路径下有同名的文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //显示时，恢复原来的标签
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameFile(selectedItem.Tag.ToString(), newName);

                        FileInfo fileInfo = new FileInfo(selectedItem.Tag.ToString());
                        string parentPath = Path.GetDirectoryName(fileInfo.FullName);
                        string newPath = Path.Combine(parentPath, newName);

                        //更新选中项的Tag
                        selectedItem.Tag = newPath;

                        //刷新左边的目录树
                        //LoadChildNodes(curSelectedNode);
                    }
                }
                //如果是文件夹
                else if (Directory.Exists(selectedItem.Tag.ToString()))
                {
                    //如果当前路径下有同名的文件夹
                    if (Directory.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("当前路径下有同名的文件夹！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //显示时，恢复原来的标签
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameDirectory(selectedItem.Tag.ToString(), newName);

                        DirectoryInfo directoryInfo = new DirectoryInfo(selectedItem.Tag.ToString());
                        string parentPath = directoryInfo.Parent.FullName;
                        string newPath = Path.Combine(parentPath, newName);

                        //更新选中项的Tag
                        selectedItem.Tag = newPath;

                        //刷新左边的目录树
                        //LoadChildNodes(curSelectedNode);
                    }
                }
            }
        }


        private void cmsMain_Opening(object sender, CancelEventArgs e)
        {
            ToolStripDropDownItem ts = ((ToolStripDropDownItem)cmsMain.Items["发送至ToolStripMenuItem"]);
            ts.DropDownItems.Clear();
            for (int i = 0; i < lvwFiles_AT.Items.Count; i++)
            {
                if (!lvwFiles_AT.Items[i].Tag.ToString().Contains(","))
                {
                    ts.DropDownItems.Add(new ToolStripMenuItem(lvwFiles_AT.Items[i].Tag.ToString()));
                }
            }
        }

        

        private void 发送至ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
            ToolStripDropDownItem ts = ((ToolStripDropDownItem)cmsMain.Items["发送至ToolStripMenuItem"]);

            //MessageBox.Show(e.ClickedItem.Text);
            
            if (lvwFiles.SelectedItems.Count > 0)
            {
                string path = lvwFiles.SelectedItems[0].Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);
                //MessageBox.Show(fileInfo.Length.ToString());
                try
                {
                    //如果选中的是文件夹
                    if (!Directory.Exists(path))
                    {
                        //MessageBox.Show(path);
                        //MessageBox.Show(lvwFiles.SelectedItems[0].Text);
                        
                        String strCMD = "AT+FSDWNFILE=";

                        strCMD = strCMD + "\"" + e.ClickedItem.Text + "/" + lvwFiles.SelectedItems[0].Text + "\"" + "," + fileInfo.Length.ToString() + "\r\n";
                        SendAtCmd_file(strCMD, path);
                    }
                    
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }


        public void refreshList()
        {
            String sendText = "AT+FSLSTFILE=\"/usr\"\r\n";

            //controller_file.OpenSerialPort(comboBox_filePort.Text, comboBox_fileBand.Text, "8", "One", "None", "None");
            //flag = controller_file.SendDataToCom(sendText);
            string[] sArray = { };

            if (SendAtCmd(sendText, ref sArray, "FSLSTFILE"))
            {
                ShowFilesList_AT(sArray);
            }
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            String sendText = "AT+FSLSTFILE=\"/usr\"\r\n";

             //controller_file.OpenSerialPort(comboBox_filePort.Text, comboBox_fileBand.Text, "8", "One", "None", "None");
            //flag = controller_file.SendDataToCom(sendText);
            string[] sArray = { };

            if (SendAtCmd(sendText, ref sArray, "FSLSTFILE"))
            {
                ShowFilesList_AT(sArray);
            }
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvwFiles_AT.SelectedItems.Count > 0)
            {
                string path = lvwFiles_AT.SelectedItems[0].Tag.ToString();

                try
                {
                    //如果选中的是文件夹
                    if (!path.Contains(","))
                    {
                        //打开文件夹
                        //ShowFilesList_AT(path, true);
                        String strCMD = "AT+FSLSTFILE=";
                        string[] sArray = { };
                        strCMD = strCMD + "\"" + path + "\"" + "\r\n";
                        SendAtCmd(strCMD, ref sArray, "FSLSTFILE");
                        ShowFilesList_AT(sArray);
                    }
                    //如果选中的是文件
                    else
                    {
                        //打开文件
                        //Process.Start(path);

                        String strCMD = "AT+FSRDFILE=";
                        String strGet = "";
                        string[] sArr = path.Split(',');
                        strCMD = strCMD + "\"" + sArr[0] + "\"" + "\r\n";
                        SendAtCmd_NoSplit(strCMD, ref strGet);
                        FileShow fileshow = new FileShow(strGet);
                        fileshow.Show();
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvwFiles_AT.SelectedItems.Count > 0)
            {
                string path = lvwFiles_AT.SelectedItems[0].Tag.ToString();

                try
                {
                    //如果选中的是文件
                    if (path.Contains(","))
                    {
                        //打开文件夹
                        //ShowFilesList_AT(path, true);
                        String strCMD = "AT+FSDELFILE=";
                        String strGet = "";
                        string[] sArr = path.Split(',');
                        strCMD = strCMD + "\"" + sArr[0] + "\"" + "\r\n";
                        SendAtCmd_NoSplit(strCMD, ref strGet);

                    }
                    else
                    {
                        bool ifind = false;
                        String sendText = "AT+FSLSTFILE=\"/usr\"\r\n";
                        string[] sArray = { };

                        SendAtCmd(sendText, ref sArray, "FSLSTFILE");
                        foreach (string fi in sArray)
                        {
                            if (fi.Contains("+"))
                            {
                                ifind = true;
                                break;
                            }
                        }
                        if (ifind == true)
                        {
                            MessageBox.Show("请先删除文件夹内的文件！");
                        }
                        else
                        {
                            String strCMD = "AT+FSRMDIR=";
                            String strGet = "";

                            strCMD = strCMD + "\"" + path + "\"" + "\r\n";
                            SendAtCmd_NoSplit(strCMD, ref strGet);
                        }
                    }
                    
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            refreshList();
        }



        private void 新建文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFolderForm newFolderForm = new NewFolderForm(curFilePath, this);
            newFolderForm.Show();

            
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            /*
            String sendText = "AT+FSLSTFILE=\""+curFilePathAT+"\"\r\n";

            //controller_file.OpenSerialPort(comboBox_filePort.Text, comboBox_fileBand.Text, "8", "One", "None", "None");
            //flag = controller_file.SendDataToCom(sendText);
            string[] sArray = { };

            if (SendAtCmd(sendText, ref sArray, "FSLSTFILE"))
            {
                ShowFilesList_AT(sArray);
            }
            */
        }
        private void tabControl_mid_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (this.tabControl_mid.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    {
                        string newPath = tscboAddress.Text;
                        if (newPath == "")
                        {
                            tscboAddress.Text = tscboAddress.Text = System.Environment.CurrentDirectory;
                            newPath = tscboAddress.Text;
                            ShowFilesList(newPath, true);
                        }
                        else if (!Directory.Exists(newPath))
                        {
                            ShowFilesList(newPath, true);
                        }
                    }
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_run_Click(object sender, EventArgs e)
        {
            //复位
            String sendText = "AT+TRB\r\n";
            string[] sArray = { };
            SendAtCmd(sendText, ref sArray, "TRB");
            button_OpenPort_Click(null, null);
        }

        private void toolStripButton_add_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO");
        }

        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            //删除指定文件
            if (lvwFiles_AT.SelectedItems.Count > 0)
            {
                try
                {
                    int index = this.lvwFiles_AT.SelectedItems[0].Index;
                    String file = lvwFiles_AT.SelectedItems[0].SubItems[0].Text.ToString();

                    String strCMD = "AT+FSDELFILE=";
                    String quote = "\"";
                    strCMD = strCMD  + quote + file + quote + "\r\n";
                    string[] sArray = { };
                    if (SendAtCmd(strCMD, ref sArray, "FSDELFILE"))
                    {
                        MessageBox.Show("文件删除成功");
                        toolStripButton_refresh_Click(null, null);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("请先选择删除的文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton_deleteall_Click(object sender, EventArgs e)
        {
            bool ret = true;
            foreach (ListViewItem item in lvwFiles_AT.Items)
            {
                string file = item.Text;

                String strCMD = "AT+FSDELFILE=";
                String quote = "\"";
                strCMD = strCMD + quote + file + quote + "\r\n";
                string[] sArray = { };
                ret |= SendAtCmd(strCMD, ref sArray, "FSDELFILE");
            }
            toolStripButton_refresh_Click(null, null);
            if (ret == true)
            {
                MessageBox.Show("文件删除成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void toolStripButton_format_Click(object sender, EventArgs e)
        {
            if (cmdLineFormat == false)
            {
                cmdLineFormat = true;
                this.richTextBox1.Font = this.fontDialog1.Font;
                this.richTextBox1.BackColor = Color.White;
                this.richTextBox1.ForeColor = Color.Black;
            }
            else if (cmdLineFormat == true)
            {
                cmdLineFormat = false;
                this.richTextBox1.ForeColor = Color.Green;
                this.richTextBox1.BackColor = Color.Black;
            }
        }

        private void 版本信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ///
            //“程序集版本：” +System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n";
            //“文件版本：” +Application.ProductVersion.ToString() + "\n";
            //“部署版本：” +System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            ///
            MessageBox.Show("工具当前版本："+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), "工具版本", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToolStripMenuItem_Website_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "iexplore.exe";   //IE浏览器，可以更换
            process.StartInfo.Arguments = "https://www.meigsmart.com/";
            process.Start();
        }

        private void ToolStripMenuItem_WikiOnline_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "iexplore.exe";
            process.StartInfo.Arguments = "https://meigpython.github.io/MeigPython/";
            process.Start();
        }

        private void ToolStripMenuItem_language_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void ToolStripMenuItem_cmdline_Click(object sender, EventArgs e)
        {

            this.tabControl_mid.SelectedTab = tabControl_mid.TabPages[0];
        }

        private void ToolStripMenuItem_fileScan_Click(object sender, EventArgs e)
        {
            this.tabControl_mid.SelectedTab = tabControl_mid.TabPages[1];
        }

        private void ToolStripMenuItem_download_Click(object sender, EventArgs e)
        {
            this.tabControl_mid.SelectedTab = tabControl_mid.TabPages[2];
        }

        private void ToolStripMenuItem_config_Click(object sender, EventArgs e)
        {
            this.tabControl_mid.SelectedTab = tabControl_mid.TabPages[3];
        }

        private void checkBox_logConfig_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_logConfig.Checked)
            {
                //MessageBox.Show("自动保存日志设置选中");
                label_maxSaveNum.Enabled = true;
                label_maxOccupancy.Enabled = true;
                textBox_maxSaveNum.Enabled = true;
                textBox_maxOccupancy.Enabled = true;
            }
            else
            {
                //MessageBox.Show("自动保存日志设置取消");
                label_maxSaveNum.Enabled = false;
                label_maxOccupancy.Enabled = false;
                textBox_maxSaveNum.Enabled = false;
                textBox_maxOccupancy.Enabled = false;
            }
        }

        private void checkBox_serialPortConfig_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_serialPortConfig.Checked)
            {
                label_parity.Enabled = true;
                label_data.Enabled = true;
                label_stop.Enabled = true;
                label_flowctrol.Enabled = true;
                comboBox_parity.Enabled = true;
                comboBox_data.Enabled = true;
                comboBox_stop.Enabled = true;
                comboBox_flowctrol.Enabled = true;
            }
            else
            {
                label_parity.Enabled = false;
                label_data.Enabled = false;
                label_stop.Enabled = false;
                label_flowctrol.Enabled = false;
                comboBox_parity.Enabled = false;
                comboBox_data.Enabled = false;
                comboBox_stop.Enabled = false;
                comboBox_flowctrol.Enabled = false;
            }
        }

        private void checkBox_editor_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_editor.Checked)
            {
                textBox_editorPath.Enabled = true;
                button_editorPatch.Enabled = true;
                label_editorParam.Enabled = true;
                textBox_editorParam.Enabled = true;
            }
            else
            {
                textBox_editorPath.Enabled = false;
                button_editorPatch.Enabled = false;
                label_editorParam.Enabled = false;
                textBox_editorParam.Enabled = false;
            }
            
        }

        private void checkBox_mpyCrossPath_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_mpyCrossPath.Checked)
            {
                textBox_mpyCrossPath.Enabled = true;
                button_mpyCrossPath.Enabled = true;
            }
            else
            {
                textBox_mpyCrossPath.Enabled = false;
                button_mpyCrossPath.Enabled = false;
            }
        }

        private void checkBox_modifyConfig_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_modifyConfig.Checked)
            {
                button_modifyConfig.Enabled = true;
            }
            else
            {
                button_modifyConfig.Enabled = false;
            }
        }

        private void button_modifyConfig_Click(object sender, EventArgs e)
        {
            //Process.GetCurrentProcess().Kill();//此方法完全奏效，绝对是完全退出。
        }

        private void checkBox_font_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_font.Checked)
            {
                button_font.Enabled = true;
            }
            else
            {
                button_font.Enabled = false;
            }
        }

        private void button_font_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                this.richTextBox1.Font = this.fontDialog1.Font;
                this.richTextBox1.Refresh();
            }
        }

        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);
        private void checkBox_disableWindowsSleep_CheckedChanged(object sender, EventArgs e)
        {
            const uint ES_SYSTEM_REQUIRED = 0x00000001;
            const uint ES_DISPLAY_REQUIRED = 0x00000002;
            const uint ES_CONTINUOUS = 0x80000000;
            if (checkBox_WindowsSleep.Checked)
            {
                //阻止系统休眠
                SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
            }
            else
            {
                //恢复系统休眠
                SetThreadExecutionState(ES_CONTINUOUS);
            }
        }

        private void button_openLogFolder_Click(object sender, EventArgs e)
        {
            tscboAddress.Text = System.Environment.CurrentDirectory;
            string path = tscboAddress.Text;
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void toolStripButton_saveLog_Click(object sender, EventArgs e)
        {
            /*
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Log File (*.txt;*.c) | *.txt; *.c" ;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {

                StreamWriter sw = File.AppendText(saveFileDialog.FileName);
                sw.Write(this.richTextBox1.Text);
                sw.Flush();//清理缓冲区
                sw.Close();//关闭文件
                //文件保存完成
                MessageBox.Show("日志保存完毕                                                             ", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            */
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "Log File (*.txt;*.c) | *.txt; *.c";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "log.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = saveFileDialog.FileName;
                System.IO.File.WriteAllText(fName, this.richTextBox1.Text);
            }
        }

        private void toolStripButton_setting_Click(object sender, EventArgs e)
        {
            this.tabControl_mid.SelectedTab = tabControl_mid.TabPages[3];
        }

        private void toolStripButton_onTop_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("窗口置顶");

            //if (toolStripButton_onTop.CheckOnClick)
            //{
                if (formOnTopStatus == false)
                {
                    formOnTopStatus = true;
                    this.toolStripButton_onTop.Image = MGPYcom.Properties.Resources.ontop;
                    this.toolStripButton_onTop.Text = "取消窗口置顶";
                    this.TopMost = true;
                }
                else
                {
                    formOnTopStatus = false;
                    this.toolStripButton_onTop.Image = MGPYcom.Properties.Resources.ontop_disable;
                    this.toolStripButton_onTop.Text = "窗口置顶";
                    this.TopMost = false;
                }  
            //}
        }

        private void toolStripButton_box_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_boxCmdAdd_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_pause_Click(object sender, EventArgs e)
        {
            if (cmdLineEnable == true)
            {
                this.cmdLineEnable = false;
                this.toolStripButton_pause.Enabled = false;
                this.toolStripButton_pause.Image = MGPYcom.Properties.Resources.pause;
                this.toolStripButton_play.Enabled = true;
                this.toolStripButton_play.Image = MGPYcom.Properties.Resources.play;

                this.richTextBox1.ReadOnly = true;
            }
        }

        private void toolStripButton_play_Click(object sender, EventArgs e)
        {
            if (cmdLineEnable == false)
            {
                this.cmdLineEnable = true;
                this.toolStripButton_play.Enabled = false;
                this.toolStripButton_play.Image = MGPYcom.Properties.Resources.play_disable;
                this.toolStripButton_pause.Enabled = true;
                this.toolStripButton_pause.Image = MGPYcom.Properties.Resources.pause;

                this.richTextBox1.ReadOnly = false;
            }
        }

        private void lvwFiles_AT_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (lvwFiles_AT.SelectedItems.Count > 0)
            {
                int index = this.lvwFiles_AT.SelectedItems[0].Index;
                string name = lvwFiles_AT.SelectedItems[0].SubItems[1].Text.ToString();
                string size = lvwFiles_AT.SelectedItems[0].SubItems[0].Text.ToString();
                //string name = lvwFiles_AT.Items[index].Name;
                //MessageBox.Show(name);
            }
        }

        /// <summary>
        /// 鼠标拖动某项至该控件的区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvwFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// 当拖动某项时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvwFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            lvwFiles.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        /// <summary>
        /// 拖动时拖着某项置于某行上方时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvwFiles_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// 鼠标拖动某项至该控件的区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvwFiles_AT_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        /// <summary>
        /// 结束拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvwFiles_AT_DragDrop(object sender, DragEventArgs e)
        {
            //触发文件发送
            if (lvwFiles.SelectedItems.Count > 0)
            {
                string path = lvwFiles.SelectedItems[0].Tag.ToString();
                FileInfo fileInfo = new FileInfo(path);

                try
                {
                    //如果选中的是文件夹
                    if (!Directory.Exists(path))
                    {
                        String strCMD = "AT+FSDWNFILE=";

                        strCMD = strCMD + "\"" + "usr" + "/" + lvwFiles.SelectedItems[0].Text + "\"" + "," + fileInfo.Length.ToString() + "\r\n";
                        if (SendAtCmd_file(strCMD, path) == true)
                        {
                            toolStripButton_refresh_Click(null, null);
                            MessageBox.Show("文件发送成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("文件发送失败，请重新发送", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        timer_monitor.Stop();
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ToolStripMenuItem_save_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(*.txt;*.c;*.py) | *.txt; *.c; *.py";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {

                StreamWriter sw = File.AppendText(saveFileDialog.FileName);
                sw.Write(this.richTextBox1.Text);
                sw.Flush();//清理缓冲区
                sw.Close();//关闭文件
                //文件保存完成
                MessageBox.Show("保存完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ToolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            //或者
            //Process.GetCurrentProcess().Kill();
        }

        private void toolStripButton_search_Click(object sender, EventArgs e)
        {
            Form searchForm = new searchForm();
            searchForm.Show();
        }

        //高亮关键字
        private void highLightText()
        {
            string[] keywords =
            {
                "and", "as", "assert", "break", "class", "continue",
                "def", "del", "elif", "else", "except",	"finally",
                "for", "from", "False", "global", "if",	"import",
                "in", "is",	"lambda", "nonlocal", "not", "None",
                "or", "pass", "raise",	"return", "try", "True",
                "while", "with", "yield"
            };
            string[] functions =
            {
                "isnull", "count", "sum"
            };
            string[] strings = { @"'((.|\n)*?)'" };
            string[] whiteSpace = { "\t", "\n", "   " };

            highLightText(keywords, Color.Blue);
            //highLightText(functions, Color.Magenta);
            //highLightText(strings, Color.Red);
            //highLightText(whiteSpace, Color.Black);
        }

        private void highLightText(string[] wordList, Color color)
        {
            foreach (string word in wordList)
            {
                Regex r = new Regex(word, RegexOptions.IgnoreCase);

                foreach (Match m in r.Matches(richTextBox1.Text))
                {
                    richTextBox1.Select(m.Index, m.Length);
                    richTextBox1.SelectionColor = color;
                }
            }
            this.richTextBox1.SelectionFont = new Font("宋体", 12, (FontStyle.Regular));
            this.richTextBox1.SelectionColor = Color.Green;
        }


        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            string keywords = "import";

            index = richTextBox1.Find(keywords, RichTextBoxFinds.WholeWord);
            startPos = index;

            if (index == -1)
                return;

            while (nextIndex != startPos)
            {
                richTextBox1.SelectionStart = index;
                richTextBox1.SelectionLength = keywords.Length;
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.SelectionFont = new Font("Times New Roman", (float)12, FontStyle.Regular);
                richTextBox1.Select(startPos + keywords.Length, 0);
                //richTextBox1.Focus();

                nextIndex = richTextBox1.Find(keywords, index + keywords.Length, RichTextBoxFinds.WholeWord);
                if (nextIndex == -1)
                    nextIndex = startPos;
                index = nextIndex;

            }
            */
        }

        private void timer_monitor_Tick(object sender, EventArgs e)
        {
            int size = 0;
            progressBar_fileSend.Value = progressBar_fileSend.Maximum - size;
        }

        /*
        //建立关键字
        public static List<string> AllClass()
        {
            List<string> list = new List<string>();
            string[] keywords =
            {
                "and", "as", "assert", "break", "class", "continue",
                "def", "del", "elif", "else", "except", "finally",
                "for", "from", "False", "global", "if", "import",
                "in", "is", "lambda", "nonlocal", "not", "None",
                "or", "pass", "raise",  "return", "try", "True",
                "while", "with", "yield"
            };

            //list.Add("import");
            //list.Add("from");
            for (int i = 0; i < keywords.Length; i++)
            {
                list.Add(keywords[i]);
            }

            return list;
        }

        public static string getLastWord(string str, int i)
        {
            string x = str;
            Regex reg = new Regex(@"\s+[a-z]+\s*", RegexOptions.RightToLeft);
            x = reg.Match(x).Value;

            Regex reg2 = new Regex(@"\s");
            x = reg2.Replace(x, "");
            return x;
        }

        //设定颜色
        public static void MySelect(System.Windows.Forms.RichTextBox tb, int i, string s, Color c, bool font)
        {
            tb.Select(i - s.Length, s.Length);
            tb.SelectionColor = c;
            //是否改变字体
            if (font)
                tb.SelectionFont = new Font("宋体", 12, (FontStyle.Regular));
            else
                tb.SelectionFont = new Font("宋体", 12, (FontStyle.Regular));
            //以下是把光标放到原来位置，并把光标后输入的文字重置
            tb.Select(i,0);
            tb.SelectionFont = new Font("宋体", 12, (FontStyle.Regular));
            tb.SelectionColor = Color.Green;
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            RichTextBox rich = (RichTextBox)sender;

            string s = getLastWord(rich.Text, rich.SelectionStart);

            if (AllClass().IndexOf(s) > -1)
            {
                //MySelect(rich, rich.SelectionStart, s, Color.CadetBlue, true);
                MySelect(rich, rich.SelectionStart, s, Color.Red, true);
            }
            else
            {
                //this.richTextBox1.Font = this.fontDialog1.Font;
                //this.richTextBox1.BackColor = Color.White;
                //this.richTextBox1.ForeColor = Color.Black;
            }
        }
        */
    }
}
