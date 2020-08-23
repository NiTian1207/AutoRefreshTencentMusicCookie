using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace AutoRefreshTencentMusicCookie
{
    public partial class Form1 : Form
    {
        ChromiumWebBrowser webBrowser = null;

        public Form1()
        {
            InitializeComponent();
            var setting = new CefSettings();
            setting.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36 Edg/80.0.361.111";
            Cef.Initialize(setting);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser = new ChromiumWebBrowser("https://y.qq.com");
            webBrowser.AddressChanged += WebBrowser_AddressChanged;
            webBrowser.Dock = DockStyle.Fill;
            Controls.Add(webBrowser);
            webBrowser.Load("y.qq.com");
            if (File.Exists("Setting.ini"))
            {
                StreamReader sr = new StreamReader("Setting.ini");
                textBox3.Text = sr.ReadToEnd();
                sr.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetCookies());
        }

        private string GetCookies()
        {
            string Cookies = "";
            var cookiemanager = webBrowser.GetCookieManager();
            Task<List<CefSharp.Cookie>> cookies = cookiemanager.VisitAllCookiesAsync();
            List<CefSharp.Cookie> Result = cookies.Result;
            foreach (CefSharp.Cookie c in Result)
            {
                //if (c.Name == "pgv_pvi" ||
                //    c.Name == "RK" ||
                //    c.Name == "ptcz" ||
                //    c.Name == "pgv_pvid" ||
                //    c.Name == "pac_uid" ||
                //    c.Name == "XWINDEXGREY" ||
                //    c.Name == "ts_uid" ||
                //    c.Name == "psrf_qqunionid" ||
                //    c.Name == "psrf_qqopenid" ||
                //    c.Name == "psrf_qqrefresh_token" ||
                //    c.Name == "psrf_qqaccess_token" ||
                //    c.Name == "yq_index" ||
                //    c.Name == "sd_userid" ||
                //    c.Name == "sd_cookie_crttime" ||
                //    c.Name == "hibext_instdsigdipv2" ||
                //    c.Name == "ts_refer" ||
                //    c.Name == "psrf_access_token_expiresAt" ||
                //    c.Name == "uin" ||
                //    c.Name == "skey" ||
                //    c.Name == "yqq_stat" ||
                //    c.Name == "pgv_si" ||
                //    c.Name == "pgv_info" ||
                //    c.Name == "ts_last"
                //    )
                Cookies += c.Name + "=" + c.Value + "; ";
            }
            return Cookies.Substring(0, Cookies.Length - 2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RefreshServerCookie(textBox3.Text);
        }

        private void RefreshServerCookie(string api)
        {
            webBrowser.Load("https://y.qq.com/?forceUpdateCookie=1");
            while (true)
            {
                if (!webBrowser.IsLoading)
                { break; }
            }
            Encoding myEncoding = Encoding.GetEncoding("utf-8");
            string cookies = GetCookies();
            cookies = "{\"data\":\"" + cookies + "\"}";
            byte[] myByte = myEncoding.GetBytes(cookies);//数据转码

            string responseResult = string.Empty;//储存结果
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(api + "user/setCookie");//实例化
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = myByte.Length;
            req.GetRequestStream().Write(myByte, 0, myByte.Length);//写入请求
            HttpWebResponse myRespond = null;
            try
            {
                myRespond = (HttpWebResponse)req.GetResponse();//接收结果
            }
            catch
            { }

            if (myRespond != null && myRespond.StatusCode == HttpStatusCode.OK)
            {
                StreamReader sr = new StreamReader(myRespond.GetResponseStream());
                responseResult = sr.ReadToEnd();
                sr.Close();
            }
            myRespond.Close();
            textBox1.Text += responseResult + DateTime.Now.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //System.Timers.Timer t = new System.Timers.Timer();
            //t.Interval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
            //t.Elapsed += RefreshServerCookie;
            //t.AutoReset = true;
            //t.Enabled = true;
            //t.Start();
            //textBox1.Text += "TimerStart\r\n";
        }

        private void WebBrowser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            textBox2.Invoke(new Action(() => { textBox2.Text = e.Address; }));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists("Setting.ini"))
            {
                File.Delete("Setting.ini");
            }
            StreamWriter sw = new StreamWriter("Setting.ini");
            sw.WriteLine(textBox3.Text);
            sw.Flush();
            sw.Close();
        }
    }
}
