using CaptecTechCheck.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CaptecTechCheck.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HomePageModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = DownloadPageAsync(model.URL).Result;
                    var sanatisedResult = StripHTML(result);
                    List<CloudModel> wordsCloud = WordCount(sanatisedResult);
                    model.cloudModel = wordsCloud.OrderBy(a => Guid.NewGuid()).ToList(); 
                }
                else
                {
                    ModelState.AddModelError("URL", "There is an Error with URL");
                }
            }
            catch
            {
                ModelState.AddModelError("URL", "Error checking URL");
            }
            return View(model);
        }

        private HomePageModel GenerateWordCloud(List<CloudModel> wordsCloud)
        {
            var model = new HomePageModel();
            model.cloudModel = wordsCloud;
            return model;
        }

        private List<CloudModel> WordCount(string sanatisedResult)
        {
            var punctuation = sanatisedResult.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = sanatisedResult.Split().Select(x => x.Trim(punctuation)).Where(x=> x != string.Empty);
            List<CloudModel> cloudModel = (from x in words
                     where x.Length > 3
                     group x by x into g
                     let count = g.Count()
                     orderby count descending
                     select new CloudModel { cloudWord = g.Key, cloudWordCount = count }).Take(30).ToList();
            return cloudModel;
        }



        public async Task<string> DownloadPageAsync(string url)
        {

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            using (HttpContent content = response.Content)
            {
                string result = await content.ReadAsStringAsync();
                return result;
            }
        }

        public string StripHTML(string input)
        {
            //HtmlAgilityPack
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(input);
            htmlDoc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());

            string returnText = htmlDoc.DocumentNode.InnerText;
            returnText = Regex.Replace(returnText, @"\r|\n?|\n", "");
            returnText = Regex.Replace(returnText, @"\t", " ");
            return returnText.Trim();
        }
    }
}
