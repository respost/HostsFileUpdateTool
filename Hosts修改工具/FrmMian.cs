using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.Diagnostics;

namespace Hosts修改工具
{
    public partial class FrmMian : Form
    {
        //定义公共窗体
        public static FrmMian frmMain;
        //保存Hosts类的集合
        List<Hosts> hostList = new List<Hosts>();

        public FrmMian()
        {
            //加载嵌入资源
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();      
        }
        /// <summary>
        /// 加载嵌入资源中的全部dll文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }

        private void picPath_Click(object sender, EventArgs e)
        {
            //获取文件和路径名 一起显示在 txtbox 控件里
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.txtPath.SelectedText = dialog.FileName;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FrmAdd fa = new FrmAdd();
            //指定为公共窗体
            frmMain = this;
            fa.ShowDialog();
        }

        /// <summary>
        /// 读取根目录下的host.txt文件，把域名和ip加载到listView控件上
        /// </summary>
        public void LoadHostList()
        {
            //先清空
            hostList.Clear();
            this.listView1.Items.Clear();
            try
            {
                using (FileStream fs = new FileStream(@"host.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //创建读取器
                    StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                    //开始分割内容，保存到临时数组中
                    string content = sr.ReadToEnd().Trim();//读取内容 
                    if (string.IsNullOrEmpty(content))
                        return;
                    //先把换行替换成逗号
                    content = content.Replace("\r\n", ",");
                    //再以逗号分割
                    string[] temps = content.Split(new string[] { "," }, StringSplitOptions.None);
                    foreach (var item in temps)
                    {
                        //以空格分割
                        string[] arr = item.Split(' ');
                        Hosts hosts = new Hosts(arr[0], arr[1]);
                        hostList.Add(hosts);
                    }
                    ShowListView();
                    //关闭
                    sr.Close();
                    //释放资源
                    sr.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取host.txt文件失败，异常：" + ex.Message);
            }
        }

        private void ShowListView()
        {          
            int num = 1;
            foreach (Hosts hosts in hostList)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = num.ToString();//序号  
                lvi.SubItems.Add(hosts.Ip);//ip 
                lvi.SubItems.Add(hosts.Domain);//域名
                this.listView1.Items.Add(lvi);
                num++;
            }

            //让listView自适应内容宽度
            foreach (ColumnHeader item in this.listView1.Columns)
            {
                item.Width = -2;
            }
        }

        private void FrmMian_Load(object sender, EventArgs e)
        {
            //1.初始化皮肤
            Sunisoft.IrisSkin.SkinEngine se = this.skinEngine1;
            se.SkinAllForm = true;
            //2.读取Resources资源中的skin.ssk皮肤
            byte[] obj = (byte[])Hosts修改工具.Properties.Resources.ResourceManager.GetObject("skin");
            se.SkinStream = new MemoryStream(obj);
            this.txtPath.Text = @"C:\WINDOWS\system32\drivers\etc\hosts";
            //加载列表
            LoadHostList();

        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            string path = this.txtPath.Text.Trim();
            if (path == string.Empty)
            {
                MessageBox.Show("Hosts文件路径不能为空");
                this.txtPath.Focus();
                return;
            }
            //通常情况下Hosts文件是只读的，所以写入之前要取消只读
            File.SetAttributes(path, File.GetAttributes(path) & (~FileAttributes.ReadOnly));
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    //开始写入
                    foreach (var host in hostList)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("\r\n" + host.Ip);
                        sb.Append(" ");
                        sb.Append(host.Domain);
                        sw.Write(sb.ToString());
                    }
                    //关闭
                    sw.Close();
                    //释放资源
                    sw.Dispose();
                }               
            }
            catch (Exception ex)
            {
                MessageBox.Show("写入Hosts失败，异常：" + ex.Message);
                return;
            }
            MessageBox.Show("写入Hosts成功");
            //设置只读
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            string path = this.txtPath.Text.Trim();
            if (path == string.Empty)
            {
                MessageBox.Show("Hosts文件路径不能为空");
                this.txtPath.Focus();
                return;
            }
            //通常情况下Hosts文件是只读的，所以写入之前要取消只读
            string[] allLine = File.ReadAllLines(path);
            File.SetAttributes(path, File.GetAttributes(path) & (~FileAttributes.ReadOnly));
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    //开始写入
                    sw.WriteLine("# Copyright (c) 1993-2009 Microsoft Corp.");
                    sw.WriteLine("#");
                    sw.WriteLine("# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.");
                    sw.WriteLine("#");
                    sw.WriteLine("# This file contains the mappings of IP addresses to host names. Each");
                    sw.WriteLine("# entry should be kept on an individual line. The IP address should");
                    sw.WriteLine("# be placed in the first column followed by the corresponding host name.");
                    sw.WriteLine("# The IP address and the host name should be separated by at least one");
                    sw.WriteLine("# space.");
                    sw.WriteLine("#");
                    sw.WriteLine("# Additionally, comments (such as these) may be inserted on individual");
                    sw.WriteLine("# lines or following the machine name denoted by a '#' symbol.");
                    sw.WriteLine("#");
                    sw.WriteLine("# For example:");
                    sw.WriteLine("#");
                    sw.WriteLine("#      102.54.94.97     rhino.acme.com          # source server");
                    sw.WriteLine("#       38.25.63.10     x.acme.com              # x client host");
                    sw.WriteLine("# localhost name resolution is handled within DNS itself.");
                    sw.WriteLine("");
                    sw.WriteLine("#	127.0.0.1       localhost");
                    sw.WriteLine("#	::1             localhost");
                    //关闭
                    sw.Close();
                    //释放资源
                    sw.Dispose();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("还原Hosts失败，异常：" + ex.Message);
                return;
            }
            MessageBox.Show("还原Hosts成功");
            //设置为只读
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
        }
       
        /// <summary>
        /// 清空列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiClear_Click(object sender, EventArgs e)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(@"host.txt", FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite);            
            }
            catch (Exception ex)
            {
                MessageBox.Show("列表清空失败，异常：" + ex.Message);
                return;
            }
            finally
            {
                fs.Close();
            }
            //重新加载
            LoadHostList();
            MessageBox.Show("列表已清空");        
        }

        private void txtPath_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //双击打开打开指定的文件
            string v_OpenFilePath = this.txtPath.Text;
            if (v_OpenFilePath == string.Empty)
            {
                return;
            }
            //以记事本（指定程序）打开外部文档（指定文档）
            System.Diagnostics.Process.Start("notepad.exe", v_OpenFilePath);
        }

        private void labelUrl_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.zy13.net");
        }

    }
    /// <summary>
    /// 自定义一个Hosts类
    /// </summary>
    public class Hosts
    {
        //IP
        string ip;
        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }
        //域名
        string domain;
        public string Domain
        {
            get { return domain; }
            set { domain = value; }
        }
        public Hosts()
        {

        }
        // 具有两个参数的构造函数
        public Hosts(string _ip, string _domain)
        {
            this.ip = _ip;
            this.domain = _domain;
        }
    }
}
