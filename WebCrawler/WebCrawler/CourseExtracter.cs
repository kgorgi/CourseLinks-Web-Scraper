using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class CourseExtracter
    {
        const string rootUrl = "https://web.uvic.ca/calendar2017-09/CDs/";
        const string slash = "/";
        const string urlEnd = ".html";        

        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            string requestUrl = rootUrl +  fieldOfStudy  + slash + courseNum +  urlEnd;

            string rawResponse;

            using (var wb = new WebClient())
            {
                rawResponse = wb.DownloadString(requestUrl);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawResponse);
            string filter = "//ul[@class='prereq']";

            var prereqs = htmlDoc.DocumentNode
                .SelectNodes(filter)
                .FirstOrDefault()?
                .InnerHtml;
                
        }
    }

}
