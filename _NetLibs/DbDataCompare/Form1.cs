using DbDataCompare.App;
using System;
using System.IO;
using System.Windows.Forms;

namespace DbDataCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var configPath = EnsureConfig();

            if (!File.Exists(configPath))
            {
                textBox1.Text = Properties.Resources.ConfigTemplate;
                File.WriteAllText(configPath, textBox1.Text);
            }
            else
            {
                textBox1.Text = File.ReadAllText(configPath);
            }

            webBrowser1.Navigate("about:blank");
        }

        private void btnLoadDefaultConfig_Click(object sender, EventArgs e)
        {
            var configPath = EnsureConfig();

            textBox1.Text = File.ReadAllText(configPath);
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            File.WriteAllText(EnsureConfig(), textBox1.Text);

            var noti = MyHtmlNoti.Start();

            MyRequest req = null;

            try
            {
                req = RequestParser.ParseRequestText(textBox1.Text);
            }
            catch (Exception ex)
            {
                noti.Error(ex, "解析报错");
            }

            if (req != null)
            {
                try
                {
                    new MyComparer().Execute(req, noti);
                }
                catch (Exception ex)
                {
                    noti.Error(ex, "执行报错");
                }
            }

            var html = noti.End();

            webBrowser1.Document.Write(html);

            tabControl1.SelectedIndex = 1;

            //System.Diagnostics.Debug.WriteLine(html);
        }

        private static string EnsureConfig()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "Resources");
            Directory.CreateDirectory(configPath);
            return Path.Combine(configPath, "config.txt");
        }
    }
}
