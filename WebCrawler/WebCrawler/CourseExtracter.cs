using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebCrawler
{
    class CourseExtracter
    {
        const string rootUrl = "https://web.uvic.ca/calendar2017-09/CDs/";
        const string slash = "/";
        const string urlEnd = ".html";

        private string errorMsgStart;
        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            this.errorMsgStart = fieldOfStudy + " " + courseNum + ": ";

            HtmlWeb web = new HtmlWeb();
            string requestUrl = rootUrl + fieldOfStudy + slash + courseNum + urlEnd;
            var htmlDoc = web.Load(requestUrl);

            string prereqFilter = "//ul[@class='prereq']";
            string prereqsHTML = "";
            try
            {
               prereqsHTML = htmlDoc.DocumentNode.SelectNodes(prereqFilter).First().InnerHtml;
            } catch (ArgumentNullException)
            {
                this.LogError("No Pre-requisites");
            }

            string precoreqsFilter = "//ul[@class='precoreq']";
            string precoreqsHTML = "";
            try
            {
                precoreqsHTML = htmlDoc.DocumentNode.SelectNodes(precoreqsFilter).First().InnerHtml;
            }
            catch (ArgumentNullException )
            {
                this.LogError("No Pre/Co-Requisites");
            }

            Console.WriteLine(prereqsHTML);
            Console.ReadLine();
        }

        private void LogError(string error)
        {
            Console.Error.WriteLine(this.errorMsgStart + error);
        }
    }
}
