using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTrayApplication
{
    public static class PropertiesReader
    {

        public static List<(string, double)> GetData()
        {
            var products = TaskTrayApplication.Properties.Settings.Default.products.Split(';');
            var prices = TaskTrayApplication.Properties.Settings.Default.prices.Split(';')
                .Select(raw => double.TryParse(raw, out var res) ? res : 0);

            if (prices.Count() != products.Length)
            {
                products = new string[0];
                prices = new double[0];
                //throw new IndexOutOfRangeException("Length of prices and products is not equal. Error in settings.settings");
            }

            return products.Zip(prices, (product, price) => (product, price)).ToList();
        }
    }
}
