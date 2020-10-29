using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;//引入文件功能

namespace Hosts修改工具
{
    public partial class FrmAdd : Form
    {
        public FrmAdd()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.txtDomain.Text = "";
            this.txtIp.Text = "";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //域名
            string url = this.txtDomain.Text.Trim();
            if (url == string.Empty)
            {
                MessageBox.Show("域名不能为空");
                this.txtDomain.Focus();
                return;
            }
            if (IsDomain(url))
            {
                Uri uri = new Uri(url);
                url = uri.Host;
            }
            //ip
            string ip = this.txtIp.Text.Trim();
            if (ip==string.Empty)
            {
                MessageBox.Show("IP不为能空");
                this.txtIp.Focus();
                return;
            }
            //1.创建文件流
            string path = "host.txt";
            FileStream fs = new FileStream(path,FileMode.Append);
            //2.创建写入器
            StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);
            //3.开始写入
            bool result = false;//标识是否写入成功
            try
            {
                sw.Write(ip + " " + url);
                sw.WriteLine();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("写入失败，异常："+ex.Message);
                result = false;
            }
            finally
            {
                //4.关闭写入器
                if (sw != null)
                    sw.Close();
                //5.关闭文件流
                if (fs != null)
                    fs.Close();
            }
            if (result==true)
            {
                MessageBox.Show("添加成功");
                this.txtDomain.Text = "";
                FrmMian.frmMain.LoadHostList();
            }
            else
            {
                MessageBox.Show("添加失败");
                return;
            }
        }
        /// <summary>
        /// 验证域名是否合法
        /// </summary>
        /// <param name="str">指定字符串</param>
        /// <returns></returns>
        public static bool IsDomain(string str)
        {
            string pattern = @"^(http://|https://)([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?.*$";
            Regex reg = new Regex(pattern);
            return reg.IsMatch(str);
        }
    }
}
