using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubsManager
{
    public partial class PopUpdateForm : Form
    {
        Form1 f1 = new Form1();
        string username;

        public PopUpdateForm()
        {
            InitializeComponent();
        }

        private void PopUpdateForm_Load(object sender, EventArgs e)
        {
            userTXTF2.Text = username;
        }

        public void Getusername(string user)
        {
            username = user;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            f1.PopUpUpdateform(username, (int)numericUpDownF2.Value);
            this.Close();
        }

        private void cancelBTN_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void userTXTF2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                f1.PopUpUpdateform(username, (int)numericUpDownF2.Value);
                this.Close();
            }
        }

    }
}
