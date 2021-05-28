using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Diagnostics;

namespace PID
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        public MainWindow()
        {
            InitializeComponent();
            
            serialPort = new SerialPort();
            FindAvaPorts();
            Button_connect.Content = "connect";
            TextBox_showreceive.IsReadOnly = true;
        }

        private void Button_connect_Click(object sender, RoutedEventArgs e)
        {
            if (!serialPort.IsOpen)
            {       //判断端口是否已经被打开
                InitPorts();
                //如果没有打开，打开端口
                try
                {
                    serialPort.Open();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("port is under using!");
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("port is used !");
                }
                if (serialPort.IsOpen)
                {
                    Button_connect.Content = "Disconnect";
                }
            }
            else
            {
                Button_connect.Content = "connect";
                serialPort.Close();
            }
        }

        private void FindAvaPorts()
        {
            ComboBox_portselect.ItemsSource = SerialPort.GetPortNames();
            ComboBox_portselect.SelectedIndex = 0;
        }

        private void InitPorts()
        {
            serialPort.PortName = ComboBox_portselect.SelectedItem.ToString();
            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceHandler);    //注册事件处理函数
        }
        public string recedata="";
        public void DataReceHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            string str = port.ReadExisting();
            recedata += str;
            if (recedata.EndsWith('\n'))
            {
                this.TextBox_showreceive.Dispatcher.Invoke(new Action(() =>
                {
                    //string str = port.ReadExisting();
                    this.TextBox_showreceive.Text = recedata;
                    //this.TextBox_showreceive.ScrollToEnd();
                    if (recedata.StartsWith('e')) {
                        string strerror = recedata.Substring(1,8);
                        short error;
                        UInt32 buffnum = UInt32.Parse(strerror, System.Globalization.NumberStyles.HexNumber);
                        error = (short)(buffnum - 0xFFFF0000); 

                        this.TextBox_error.Text = error.ToString();
                    }
                    recedata = "";
                }));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*从文本框获取角度数据，并发送，角度数据格式为“0000”*/
            if (serialPort.IsOpen && (string)Button_connect.Content == "Disconnect") {  //判断端口是否连接  
                string str = TextBox_angle.Text;
                if (str.Length == 0) {
                    MessageBox.Show("no data!");
                    return;
                }
                int num;
                if (!int.TryParse(str, out num)) {      //判断输入数据是否为数字
                    MessageBox.Show("please input numbers!");
                    return;
                }
                if (num > 180 || num<0) {        //判断舵机角度数据是否在0-180之间
                    MessageBox.Show("please input 0-180");
                    return;
                }
                str = num.ToString();       //从num转回，去除例如001这种形式多余的0

                if (num < 100) {               //数据不足3位，则补足3位
                    str = str.Insert(0, "0");
                }
                if (num < 10) {
                    str = str.Insert(0, "0"); //数据不足3位，补足3位
                }

                str = str.Insert(0, "DA");      //D用于检查数据有效，A用于标识当前数据位角度数据
                serialPort.WriteLine(str);
            }
        }

        private void Button_Ki_Click(object sender, RoutedEventArgs e)
        {
            //获取并发送Ki数据，格式为0.00
            if (serialPort.IsOpen && (string)Button_connect.Content == "Disconnect")
            {  //判断端口是否连接  
                string str = TextBox_Ki.Text;
                if (str.Length == 0)
                {
                    MessageBox.Show("no data!");
                    return;
                }
                double num;
                if (!double.TryParse(str, out num))
                {      //判断输入数据是否为数字
                    MessageBox.Show("please input numbers!");
                    return;
                }
                if (num >= 10 || num < 0)
                {        //判断Kp数据是否在0-10之间(最大四位，9.00)
                    MessageBox.Show("please input 0-10");
                    return;
                }
                str = num.ToString("0.00");       //从num转回，去除例如001这种形式多余的0，并格式化为0.00格式
                if (str == "-0.00")
                    str = "0.00";
                str = str.Insert(0, "Di");      //D用于检查数据有效，i用于标识当前数据为ki数据
                serialPort.WriteLine(str);
            }
        }

        private void Button_Kd_Click(object sender, RoutedEventArgs e)
        {
            /*从文本框获取并发送Kd数据，格式为"00.0"*/
            if (serialPort.IsOpen && (string)Button_connect.Content == "Disconnect")
            {  //判断端口是否连接  
                string str = TextBox_Kd.Text;
                if (str.Length == 0)
                {
                    MessageBox.Show("no data!");
                    return;
                }
                double num;
                if (!double.TryParse(str, out num))
                {      //判断输入数据是否为数字
                    MessageBox.Show("please input numbers!");
                    return;
                }
                if (num >= 100 || num < 0)
                {        //判断Kd数据是否在0-100之间(最大四位，99.0)
                    MessageBox.Show("please input 0-100");
                    return;
                }
                str = num.ToString("00.0");       //从num转回，去除例如001这种形式多余的0，并格式化为00.0格式
                if (str == "-00.0")
                    str = "00.0";
                str = str.Insert(0, "Dd");      //D用于检查数据有效，d用于标识当前数据为kd数据
                serialPort.WriteLine(str);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            /*从文本框获取并发送Kp数据，格式为"00.0"*/
            if (serialPort.IsOpen && (string)Button_connect.Content == "Disconnect")
            {  //判断端口是否连接  
                string str = TextBox_Kp.Text;
                if (str.Length == 0)
                {
                    MessageBox.Show("no data!");
                    return;
                }
                double num;
                if (!double.TryParse(str, out num))
                {      //判断输入数据是否为数字
                    MessageBox.Show("please input numbers!");
                    return;
                }
                if (num >= 100 || num < 0)
                {        //判断Kp数据是否在0-100之间(最大四位，99.0)
                    MessageBox.Show("please input 0-100");
                    return;
                }
                str = num.ToString("00.0");       //从num转回，去除例如001这种形式多余的0，并格式化为00.0格式
                if (str == "-00.0")
                    str = "00.0";
                str = str.Insert(0, "Dp");      //D用于检查数据有效，p用于标识当前数据为kp数据
                serialPort.WriteLine(str);
            }
        }
    }
}
