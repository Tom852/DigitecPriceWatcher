using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace TaskTrayApplication
{
    public class TaskTrayApplicationContext : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        Configuration configWindow = new Configuration();
        PriceChecker priceChecker = new PriceChecker();
        Timer timer = new Timer();

        TimeSpan requestInterval = TimeSpan.FromMinutes(15);

        public TaskTrayApplicationContext()
        {
            MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            configWindow.OnOk += DoPriceCheck;

            notifyIcon.Icon = TaskTrayApplication.Properties.Resources.moneyIcon;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { configMenuItem, exitMenuItem });
            notifyIcon.Text = "request pending";
            notifyIcon.DoubleClick += ShowConfig;
            notifyIcon.Visible = true;

            timer.Interval = (int)requestInterval.TotalMilliseconds;
            timer.Tick += DoPriceCheck;
            timer.Start();

            DoPriceCheck(this, null);
        }

        private async void DoPriceCheck(object sender, EventArgs e)
        {
            try
            {
                var data = PropertiesReader.GetData();

                foreach (var (productUrl, priceLimit) in data)
                {

                    if (priceLimit == 0 || string.IsNullOrWhiteSpace(productUrl))
                    {
                        continue;
                    }


                    var currentPrice = await priceChecker.GetPriceOfProductAsync(productUrl);

                    if (currentPrice <= priceLimit)
                    {
                        DoAlertStuff(productUrl);
                    }
                }
                notifyIcon.Text = $"Price checker state ok. Last Check {DateTime.Now.ToString("t")}";

            }
            catch (Exception ex)
            {
                notifyIcon.Text = $"Error! {ex.Message}";
            }
        }

        private void DoAlertStuff(string url)
        {
            notifyIcon.Icon = TaskTrayApplication.Properties.Resources.alertIcon;

            var l = Path.Combine(Directory.GetCurrentDirectory(), "airRaid.mp3");

            WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();

            wplayer.URL = l;
            wplayer.controls.play();
            wplayer.settings.setMode("Loop", true);


            var altertBox = new AlertForm(url);
            altertBox.OnCloserino += ((_1, _2) =>
            {
                wplayer.controls.stop();
                notifyIcon.Icon = TaskTrayApplication.Properties.Resources.moneyIcon;
            });
            altertBox.ShowDialog();
        }

        void ShowConfig(object sender, EventArgs e)
        {
            // If we are already showing the window meerly focus it.
            if (configWindow.Visible)
                configWindow.Focus();
            else
                configWindow.ShowDialog();
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;
            timer.Stop();
            Application.Exit();
        }
    }
}
