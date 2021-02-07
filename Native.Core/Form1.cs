using Clansty.tianlang;
using Native.Csharp.App;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Native.Core
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Size = new Size(1420, 933);
            textBox4.Text = Customize.config.SMTP_User;
            textBox5.Text = Customize.config.SMTP_Pass;
            textBox6.Text = Customize.config.SMTP_Acieve;
            textBox7.Text = Customize.config.SMTP_Server;
            textBox8.Text = Customize.config.SMTP_Port;
            checkBox3.Checked = Customize.config.SMTP;
            checkBox4.Checked = Customize.config.SMTP_SSL;
            label4.Text = Customize.config.CSV_SavePath;
            checkBox2.Checked = Customize.config.CSV;
            var groupList = Robot.GetGroupList();
            foreach (var g in groupList)
            {
                var gc = g.Value<long>("uin");
                var gn = g.Value<string>("name");
                if (Customize.config.fList.ContainsKey(gc))
                {
                    var i = dataGridView7.Rows.Add(Customize.config.fList[gc], gc, null, gn, "");
                    Task.Run(async () =>
                    {
                        var wr = WebRequest.Create($"http://p.qlogo.cn/gh/{gc}/{gc}/0");
                        var res = await wr.GetResponseAsync();
                        dataGridView7.Rows[i].Cells[2].Value = Image.FromStream(res.GetResponseStream());
                    });
                }
                else
                {
                    var i = dataGridView7.Rows.Add(false, gc, null, gn, "*");
                    Task.Run(async () =>
                    {
                        var wr = WebRequest.Create($"http://p.qlogo.cn/gh/{gc}/{gc}/0");
                        var res = await wr.GetResponseAsync();
                        dataGridView7.Rows[i].Cells[2].Value = Image.FromStream(res.GetResponseStream());
                    });
                }
            }
            textBox3.Lines = Customize.config.f;

        }

        private void button11_Click(object sender, EventArgs e)
        {
            new Form2(true).ShowDialog();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            new Form2(false).ShowDialog();
        }


        private void button8_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView7.Rows.Count; i++)
            {
                dataGridView7.Rows[i].Cells[0].Value = true;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView7.Rows.Count; i++)
            {
                dataGridView7.Rows[i].Cells[0].Value = false;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView7.Rows.Count; i++)
            {
                dataGridView7.Rows[i].Cells[0].Value = !(bool)dataGridView7.Rows[i].Cells[0].Value;
            }
        }

        public void open_saveFileDialog1()
        {
            saveFileDialog1.Filter = "All files(*.*)|*.*";
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.FileName = "关键字记录集";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                label4.Text = saveFileDialog1.FileName;
                Customize.config.CSV_SavePath = label4.Text;
            }
        }


        private void button18_Click_1(object sender, EventArgs e)
        {
            Thread t = new Thread(open_saveFileDialog1);
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);//缺少这句话，就会出错误。
            t.Start();
        }

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            Customize.config.CSV = ((CheckBox)sender).Checked;
        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {
            Customize.config.SMTP = ((CheckBox)sender).Checked;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            Customize.config.member_enter_send = ((ComboBox)sender).SelectedIndex;
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            Customize.config.member_leave_send = ((ComboBox)sender).SelectedIndex;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Customize.config.SMTP_User = textBox4.Text;
            Customize.config.SMTP_Pass = textBox5.Text;
            Customize.config.SMTP_Acieve = textBox6.Text;
            Customize.config.SMTP = checkBox3.Checked;
            Customize.config.SMTP_Server = textBox7.Text;
            Customize.config.SMTP_Port = textBox8.Text;
            Customize.config.SMTP_SSL = checkBox4.Checked;
            Customize.config.f = textBox3.Lines;
            var d = new Dictionary<long, bool>();
            d = new Dictionary<long, bool>();
            for (var i = 0; i < dataGridView7.Rows.Count; i++)
            {
                d.Add(long.Parse(dataGridView7.Rows[i].Cells[1].Value.ToString()), (bool)dataGridView7.Rows[i].Cells[0].Value);
            }
            Customize.config.fList = d;
            Customize.config.f = textBox3.Lines;
            var cfg = JsonConvert.SerializeObject(Customize.config);
            File.WriteAllText(Customize.configPath, cfg);
        }
    }
}
