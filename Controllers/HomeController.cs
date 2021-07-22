using BarcodeCombined.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BarcodeCombined.Controllers
{
    public class HomeController : Controller
    {
        static HttpClient client = new HttpClient();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(stock_export stock_Export)
        {
            TempData["barcode"] = stock_Export.Bar_code;
            if (stock_Export.Bar_code != null)
            {
                return RedirectToAction("ProcessDetails");
            }
            else
            {
                return View();
            }
        }
        public async Task<ActionResult> ProcessDetails()
        {
            string barcode = "";
            if (TempData.ContainsKey("barcode"))
                barcode = TempData["barcode"].ToString();

            IList<stock_export> stock_s = new List<stock_export>();
            var productresult = await GetDetails(barcode);
            if (productresult != null)
            {
                foreach (stock_export stock in productresult)
                {
                    stock_export se = new stock_export();
                    se.Bar_code = stock.Bar_code;
                    se.Stock_code = stock.Stock_code;
                    se.Stock_description = stock.Stock_description;
                    stock_s.Add(se);
                }
            }
            return View(stock_s);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public async Task<IList<stock_export>> GetDetails(string barcode)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var barcodeInput = "";
            string username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["password"];
            string url = ConfigurationManager.AppSettings["url"];
            string companyname = ConfigurationManager.AppSettings["companyname"];
            if (barcode != null)
            {
                barcodeInput = barcode;
            }

            var str = "Report/Stock Export?" + string.Format("CompanyName={0}&UserName={1}&password={2}&IBarcode={3}", companyname, username, password, barcodeInput);
            client.BaseAddress = new Uri(url);
            var json = "";
            HttpResponseMessage response = await client.GetAsync(str);
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync();
            }

            JObject productSearch = JObject.Parse(json);

            // get JSON result objects into a list
            IList<JToken> results = productSearch["stock_export"].Children().ToList();

            // serialize JSON results into .NET objects
            IList<stock_export> searchResults = new List<stock_export>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                stock_export searchResult = result.ToObject<stock_export>();
                searchResults.Add(searchResult);
            }
            return searchResults;
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}