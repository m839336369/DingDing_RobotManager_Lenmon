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
            textBox9.Text = Customize.config.Manager_Group_Invite_QQ;
            comboBox2.SelectedIndex = Customize.config.Manager_Group_Invite_Request;
            comboBox3.SelectedIndex = Customize.config.Manager_QQ_Request;
            comboBox4.SelectedIndex = Customize.config.member_enter_send;
            comboBox5.SelectedIndex = Customize.config.member_leave_send;
            if (!Customize.running)
            {
                MessageBox.Show("机器人不存在或已删除，此功能将不会工作");
            }
            genabled.Checked = Customize.config.gWelcome.enabled;
            checkBox1.Checked = Customize.config.pWelcome.enabled;
            textBox1.Text = Customize.config.gWelcome.words;
            textBox2.Text = Customize.config.pWelcome.words;
            var groupList = Robot.GetGroupList();
            foreach (var g in groupList)
            {
                var gc = g.Value<long>("uin");
                var gn = g.Value<string>("name");
                if (Customize.config.gList.ContainsKey(gc))
                {
                    var i = dataGridView2.Rows.Add(Customize.config.gList[gc], gc, null, gn, "");
                    Task.Run(async () => 
                    {
                        var wr = WebRequest.Create($"http://p.qlogo.cn/gh/{gc}/{gc}/0");
                        var res = await wr.GetResponseAsync();
                        dataGridView2.Rows[i].Cells[2].Value = Image.FromStream(res.GetResponseStream());
                    });                    
                }
                else
                {
                    var i = dataGridView2.Rows.Add(false, gc, null, gn, "*");
                    Task.Run(async () =>
                    {
                        var wr = WebRequest.Create($"http://p.qlogo.cn/gh/{gc}/{gc}/0");
                        var res = await wr.GetResponseAsync();
                        dataGridView2.Rows[i].Cells[2].Value = Image.FromStream(res.GetResponseStream());
                    });
                }
                if (Customize.config.pList.ContainsKey(gc))
                {
                    var i = dataGridView4.Rows.Add(Customize.config.pList[gc], gc, null, gn, "");
                    Task.Run(async () =>
                    {
                        var wr = WebRequest.Create($"http://p.qlogo.cn/gh/{gc}/{gc}/0");
                        var res = await wr.GetResponseAsync();
                        dataGridView4.Rows[i].Cells[2].Value = Image.FromStream(res.GetResponseStream());
                    });
                }
                else
                {
                    var i = dataGridView4.Rows.Add(false, gc, null, gn, "*");
                    Task.Run(async () =>
                    {
                        var wr = WebRequest.Create($"http://p.qlogo.cn/gh/{gc}/{gc}/0");
                        var res = await wr.GetResponseAsync();
                        dataGridView4.Rows[i].Cells[2].Value = Image.FromStream(res.GetResponseStream());
                    });
                }
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
                var a = comboBox1.Items.Add($"{gc}({gn})");
                if (gc == Customize.config.fg)
                    comboBox1.SelectedIndex = a;
            }
            foreach (var r in Customize.config.gKws)
            {
                dataGridView1.Rows.Add(r.Key, r.Value);
            }
            foreach (var r in Customize.config.pKws)
            {
                dataGridView5.Rows.Add(r.Key, r.Value);
            }
            dataGridView2.Sort(newg, System.ComponentModel.ListSortDirection.Descending);
            dataGridView4.Sort(新, System.ComponentModel.ListSortDirection.Descending);
            radioButton1.Checked = !Customize.config.realF;
            radioButton2.Checked = Customize.config.realF;
            textBox3.Lines = Customize.config.f;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            button9_Click(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView4.Rows.Count; i++)
            {
                dataGridView4.Rows[i].Cells[0].Value = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView4.Rows.Count; i++)
            {
                dataGridView4.Rows[i].Cells[0].Value = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView4.Rows.Count; i++)
            {
                dataGridView4.Rows[i].Cells[0].Value = !(bool)dataGridView4.Rows[i].Cells[0].Value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView2.Rows.Count; i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView2.Rows.Count; i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < dataGridView2.Rows.Count; i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = !(bool)dataGridView2.Rows[i].Cells[0].Value;
            }
        }

        int pediting = -1;
        int gediting = -1;

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (gediting != -1)
                {
                    var r = "";
                    for (var i = 0; i < dataGridView3.Rows.Count - 1; i++)
                    {
                        r += dataGridView3.Rows[i].Cells[0].Value.ToString();
                        if (i != dataGridView3.Rows.Count - 2)
                            r += Customize.split;
                    }
                    dataGridView1.Rows[gediting].Cells[1].Value = r;
                }
                dataGridView3.Rows.Clear();
                var obj = dataGridView1.Rows[e.RowIndex].Cells[1].Value;
                var str = obj?.ToString() ?? "";
                var ss = str.Split(new string[] { Customize.split }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var i in ss)
                {
                    dataGridView3.Rows.Add(i);
                }
                gediting = e.RowIndex;
            }
            catch
            {
            }
        }

        private void dataGridView5_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (pediting != -1)
                {
                    var r = "";
                    for (var i = 0; i < dataGridView6.Rows.Count - 1; i++)
                    {
                        r += dataGridView6.Rows[i].Cells[0].Value.ToString();
                        if (i != dataGridView6.Rows.Count - 2)
                            r += Customize.split;
                    }
                    dataGridView5.Rows[pediting].Cells[1].Value = r;
                }
                dataGridView6.Rows.Clear();
                var obj = dataGridView5.Rows[e.RowIndex].Cells[1].Value;
                var str = obj?.ToString() ?? "";
                var ss = str.Split(new string[] { Customize.split }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var i in ss)
                {
                    dataGridView6.Rows.Add(i);
                }
                pediting = e.RowIndex;
            }
            catch
            {
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (pediting != -1)
            {
                var r = "";
                for (var i = 0; i < dataGridView6.Rows.Count - 1; i++)
                {
                    r += dataGridView6.Rows[i].Cells[0].Value.ToString();
                    if (i != dataGridView6.Rows.Count - 2)
                        r += Customize.split;
                }
                dataGridView5.Rows[pediting].Cells[1].Value = r;
            }
            if (gediting != -1)
            {
                var r = "";
                for (var i = 0; i < dataGridView3.Rows.Count - 1; i++)
                {
                    r += dataGridView3.Rows[i].Cells[0].Value.ToString();
                    if (i != dataGridView3.Rows.Count - 2)
                        r += Customize.split;
                }
                dataGridView1.Rows[gediting].Cells[1].Value = r;
            }
            Customize.config.gWelcome.enabled = genabled.Checked;
            Customize.config.pWelcome.enabled = checkBox1.Checked;
            Customize.config.gWelcome.words = textBox1.Text;
            Customize.config.pWelcome.words = textBox2.Text;
            Customize.config.SMTP_User = textBox4.Text;
            Customize.config.SMTP_Pass = textBox5.Text;
            Customize.config.SMTP_Acieve = textBox6.Text;
            Customize.config.SMTP = checkBox3.Checked;
            Customize.config.SMTP_Server = textBox7.Text;
            Customize.config.SMTP_Port = textBox8.Text;
            Customize.config.SMTP_SSL = checkBox4.Checked;
            Customize.config.Manager_Group_Invite_QQ = textBox9.Text;
            Customize.config.Manager_Group_Invite_Request = comboBox2.SelectedIndex;
            Customize.config.Manager_QQ_Request = comboBox3.SelectedIndex;
            Customize.config.member_enter_send = comboBox4.SelectedIndex;
            Customize.config.member_leave_send = comboBox5.SelectedIndex;
            var d = new Dictionary<long, bool>();
            for (var i = 0; i < dataGridView2.Rows.Count; i++)
            {
                d.Add(long.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString()), (bool)dataGridView2.Rows[i].Cells[0].Value);
            }
            Customize.config.gList = d;
            d = new Dictionary<long, bool>();
            for (var i = 0; i < dataGridView4.Rows.Count; i++)
            {
                d.Add(long.Parse(dataGridView4.Rows[i].Cells[1].Value.ToString()), (bool)dataGridView4.Rows[i].Cells[0].Value);
            }
            Customize.config.pList = d;
            d = new Dictionary<long, bool>();
            for (var i = 0; i < dataGridView7.Rows.Count; i++)
            {
                d.Add(long.Parse(dataGridView7.Rows[i].Cells[1].Value.ToString()), (bool)dataGridView7.Rows[i].Cells[0].Value);
            }
            Customize.config.fList = d;
            try
            {
                var k = new Dictionary<string, string>();
                for (var i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    k.Add(dataGridView1.Rows[i].Cells[0].Value.ToString(), dataGridView1.Rows[i].Cells[1].Value.ToString());
                }
                Customize.config.gKws = k;
                k = new Dictionary<string, string>();
                for (var i = 0; i < dataGridView5.Rows.Count - 1; i++)
                {
                    k.Add(dataGridView5.Rows[i].Cells[0].Value.ToString(), dataGridView5.Rows[i].Cells[1].Value.ToString());
                }
                Customize.config.pKws = k;
            }
            catch
            {
                MessageBox.Show("回复语不能为空");
            }
            Customize.config.realF = radioButton2.Checked;
            Customize.config.f = textBox3.Lines;
            try
            {
                Customize.config.fg = long.Parse(comboBox1.Text.GetLeft("("));
            }
            catch
            {
                Customize.config.fg = 0;
            }
            var cfg = JsonConvert.SerializeObject(Customize.config);
            File.WriteAllText(Customize.configPath, cfg);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            button9_Click(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
                textBox1.AppendText("[at]");
            else if (gediting != -1)
                if (dataGridView3.Rows[dataGridView3.SelectedCells[0].RowIndex].Cells[0].Value is null)
                    dataGridView3.Rows[dataGridView3.SelectedCells[0].RowIndex].Cells[0].Value = "[at]";
                else
                    dataGridView3.Rows[dataGridView3.SelectedCells[0].RowIndex].Cells[0].Value = dataGridView3.Rows[dataGridView3.SelectedCells[0].RowIndex].Cells[0].Value.ToString() + "[at]";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            new Form2(true).ShowDialog();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            new Form2(false).ShowDialog();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.ronsir.com/atchat");
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

        private void button16_Click(object sender, EventArgs e)
        {
            button9_Click(null, null);
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
    }
}
