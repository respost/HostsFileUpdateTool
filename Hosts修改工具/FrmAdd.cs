using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

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
            //ip
            string ip = this.txtIp.Text.Trim();
            if (ip==string.Empty)
            {
                MessageBox.Show("IP不为能空");
                this.txtIp.Focus();
                return;
            }
            if (IsDomain(url))
            {
                Uri uri = new Uri(url);
                url = uri.Host;
            }
            try
            {
                using (FileStream fs = new FileStream(@"host.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    //开始写入
                    sw.Write(ip + " " + url);
                    sw.WriteLine();
                    //关闭
                    sw.Close();
                    //释放资源
                    sw.Dispose();
                }         
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败，异常：" + ex.Message);
                return;
            }
            MessageBox.Show("添加成功");
            this.txtDomain.Text = "";
            FrmMian.frmMain.LoadHostList();
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
