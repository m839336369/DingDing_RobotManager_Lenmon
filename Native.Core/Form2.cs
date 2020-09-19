using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Native.Core
{
    public partial class Form2 : Form
    {
        public bool p = false;
        public Form2(bool p)
        {
            this.p = p;
            InitializeComponent();
            if (p)
            {
                numericUpDown1.Value = Customize.config.pdb;
                numericUpDown2.Value = Customize.config.pdt;
            }
            else
            {
                numericUpDown1.Value = Customize.config.gdb;
                numericUpDown2.Value = Customize.config.gdt;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2_FormClosing(null, null);
            Close();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (p)
            {
                Customize.config.pdb = Convert.ToInt32(numericUpDown1.Value);
                Customize.config.pdt = Convert.ToInt32(numericUpDown2.Value);
            }
            else
            {
                Customize.config.gdb = Convert.ToInt32(numericUpDown1.Value);
                Customize.config.gdt = Convert.ToInt32(numericUpDown2.Value);
            }
        }
    }
}
