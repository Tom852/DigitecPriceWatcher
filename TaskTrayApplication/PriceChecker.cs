using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TaskTrayApplication
{
    public class PriceChecker
    {
        private const string pattern = "<meta property=\\\"product:price:amount\\\" content=\\\"(\\d+\\.\\d{2})\\\"/>";

        public async Task<double> GetPriceOfProductAsync(string url)
        {
            var rawHtml = await ReadPageAsync(url);
            var price = ExtractPrice(rawHtml);
            return price;
        }

        private double ExtractPrice(string pageSourceCode)
        {
            Regex r = new Regex(pattern);
            var match = r.Match(pageSourceCode);
            if (match.Success)
            {
                if (double.TryParse(match.Groups[1].Value, out var result))
                {
                    return result;
                }
            }

            throw new Exception("Error parsing page");

        }

        private async Task<string> ReadPageAsync(string url)
        {
            return await Task.Run(() =>
            {

                // so richtig funzht das nicht.... digitec schickt iwie alte daten? ka... mega weird. glaube aber nicht, dass ich dagegen viel machen könnte.

                try
                {
                    HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    var webRequest = WebRequest.Create(url);
                    webRequest.CachePolicy = noCachePolicy;
                    WebHeaderCollection headers = new WebHeaderCollection();
                    headers.Add("Cache-Control: max-age=0");
                    webRequest.Headers = headers;
                    using (var response = webRequest.GetResponse())
                    using (var content = response.GetResponseStream())
                    using (var reader = new StreamReader(content))
                    {
                        var strContent = reader.ReadToEnd();
                        return strContent;
                    }
                }

                catch (Exception e)
                {
                    var ex = e;
                    throw new Exception("Die Url konnte nicht abgefragt werden");
                }
            });


        }
    }
}
