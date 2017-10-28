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

        private string _fieldOfStudy;
        private string _courseNum;

        private HtmlDocument content;

        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            this._fieldOfStudy = fieldOfStudy;
            this._courseNum = courseNum;

            // Get Webpage
            HtmlWeb web = new HtmlWeb();
            string requestUrl = rootUrl + fieldOfStudy + slash + courseNum + urlEnd;
            this.content = web.Load(requestUrl);

            GetCourses(true);
            GetCourses(false);
            Console.ReadLine();
        }

        private void LogError(string error)
        {
            string errorMsgStart = this._fieldOfStudy + " " +  this._courseNum + ": ";
            Console.Error.WriteLine(errorMsgStart + error);
        }

        private void GetCourses(bool onlyPrereqs)
        {
            string filter = onlyPrereqs ? "//ul[@class='prereq']" : "//ul[@class='precoreq']";
            string reqsHTML = "";
            try
            {
                reqsHTML = this.content.DocumentNode.SelectNodes(filter).First().InnerHtml;
            }
            catch (ArgumentNullException)
            {
                string msg = onlyPrereqs ? "No Pre-requisites" : "No Pre/Co-Requisites";
                this.LogError(msg);
            }
            Console.WriteLine(reqsHTML);
        }
    }
}
