using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskTrayApplication
{
    public partial class AlertForm : Form
    {
        public event EventHandler OnCloserino;
        public AlertForm(string url)
        {
            InitializeComponent();
            whichProductLabel.Text = url;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnCloserino?.Invoke(this, null);
            Close();
        }
    }
}
