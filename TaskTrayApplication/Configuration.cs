using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TaskTrayApplication
{
    public partial class Configuration : Form
    {
        public event EventHandler OnOk;

        private const int x_product = 12;
        private const int x_price = 590;
        private const int x_label = 650;
        private const int x_rmvBtn = 730;


        private List<System.Windows.Forms.TextBox> productBoxes = new List<TextBox>();
        private List<System.Windows.Forms.TextBox> priceBoxes = new List<TextBox>();
        private List<System.Windows.Forms.Button> rmvButtons = new List<Button>();
        private List<System.Windows.Forms.Label> currPrices = new List<Label>();

        private PriceChecker priceChecker = new PriceChecker();

        public Dictionary<string, string> Data { get; private set; }

        public Configuration()
        {
            InitializeComponent();
            var data = PropertiesReader.GetData();

            for (int i = 0; i < data.Count; i++)
            {
                AddTextBoxes();
            }
        }

        private void OnShow(object sender, EventArgs e)
        {
            var data = PropertiesReader.GetData();

            for (int i = 0; i < data.Count; i++)
            {
                productBoxes[i].Text = data[i].Item1;
                priceBoxes[i].Text = data[i].Item2.ToString();
            }

        }

        private void AddTextBoxes()
        {
            var i = this.productBoxes.Count;
            var heightPos = 69 + i * 26;

            var productBox = new TextBox();
            productBox.Location = new System.Drawing.Point(x_product, heightPos);
            productBox.Name = $"productBox_{i}";
            productBox.Size = new System.Drawing.Size(567, 20);
            productBox.TabIndex = (i + 10) * 2;
            productBox.TextChanged +=
                async (o, a) =>
                {
                    try
                    {
                        var index = this.productBoxes.IndexOf(o as TextBox);
                        currPrices[index].Text = "pending";
                        var cp = await priceChecker.GetPriceOfProductAsync((o as TextBox).Text);
                        currPrices[index].Text = "CHF " + cp;
                    }
                    catch
                    {
                    }
                };

            this.Controls.Add(productBox);
            this.productBoxes.Add(productBox);

            var priceBox = new TextBox();
            priceBox.Location = new System.Drawing.Point(x_price, heightPos);
            priceBox.Name = $"priceBox_{i}";
            priceBox.Size = new System.Drawing.Size(50, 20);
            priceBox.TabIndex = (i + 10) * 2 + 1;
            priceBox.KeyPress += priceBox_KeyPress;
            this.Controls.Add(priceBox);
            this.priceBoxes.Add(priceBox);



            var currPrice = new Label();
            currPrice.Location = new System.Drawing.Point(x_label, heightPos);
            currPrice.Size = new System.Drawing.Size(80, 20);
            currPrice.Text = "pending";
            currPrice.TextAlign = ContentAlignment.MiddleCenter;
            this.currPrices.Add(currPrice);
            this.Controls.Add(currPrice);

            var rmvBtn = new Button();
            rmvBtn.Location = new System.Drawing.Point(x_rmvBtn, heightPos);
            rmvBtn.Name = $"rmvBtn_{i}";
            rmvBtn.Size = new System.Drawing.Size(40, 20);
            rmvBtn.Text = " X ";
            rmvBtn.BackColor = Color.FromArgb(255,169,169);
            rmvBtn.TextAlign = ContentAlignment.MiddleCenter;
            rmvBtn.Click += (sender, args) => RemovelDovel(sender);
            this.rmvButtons.Add(rmvBtn);
            this.Controls.Add(rmvBtn);

        }

        private void RemovelDovel(object sender)
        {
            int removedIndex = this.rmvButtons.IndexOf(sender as Button);
            this.Controls.Remove(this.productBoxes[removedIndex]);
            this.Controls.Remove(this.priceBoxes[removedIndex]);
            this.Controls.Remove(this.rmvButtons[removedIndex]);
            this.Controls.Remove(this.currPrices[removedIndex]);
            this.productBoxes.RemoveAt(removedIndex);
            this.priceBoxes.RemoveAt(removedIndex);
            this.rmvButtons.RemoveAt(removedIndex);
            this.currPrices.RemoveAt(removedIndex);
            for (int i = removedIndex; i < this.productBoxes.Count(); i++)
            {
                var newHeight = this.productBoxes[i].Location.Y - 26;

                this.productBoxes[i].Location = new System.Drawing.Point(x_product, newHeight);
                this.priceBoxes[i].Location = new System.Drawing.Point(x_price, newHeight);
                this.currPrices[i].Location = new System.Drawing.Point(x_label, newHeight);
                this.rmvButtons[i].Location = new System.Drawing.Point(x_rmvBtn, newHeight);
            }
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            // only store if the user clicked "Save" / "Ok", but not on cancel
            if (this.DialogResult == DialogResult.OK)
            {

                TaskTrayApplication.Properties.Settings.Default.prices = string.Join(";", priceBoxes.Select(pb => double.TryParse(pb.Text, out var result) ? result : 0));
                TaskTrayApplication.Properties.Settings.Default.products = string.Join(";", productBoxes.Select(pb => pb.Text));
                TaskTrayApplication.Properties.Settings.Default.Save();
                OnOk?.Invoke(this, null);
            }
        }

        private void priceBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            var pressedChar = e.KeyChar;
            if (!char.IsControl(pressedChar) && !char.IsDigit(pressedChar) && (pressedChar != '.'))
            {
                e.Handled = true;
            }

            // only allow a single decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddTextBoxes();
        }
    }
}