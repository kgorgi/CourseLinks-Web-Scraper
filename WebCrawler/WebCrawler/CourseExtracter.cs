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
            // TODO Try Catch
            HtmlWeb web = new HtmlWeb();
            string requestUrl = rootUrl + fieldOfStudy + slash + courseNum + urlEnd;
            this.content = web.Load(requestUrl);

            GetCourses(true);
            GetCourses(false);
            Console.ReadLine();
        }

        private void LogError(string error, Exception ex = null)
        {
            string errorMsgStart = this._fieldOfStudy + " " + this._courseNum + ": ";
            Console.Error.WriteLine("-------------------");
            Console.Error.WriteLine(errorMsgStart + error);
            if (ex != null)
            {
                Console.Error.WriteLine(ex.Message);
            }
            
            Console.Error.WriteLine("-------------------");
        }

        private void GetCourses(bool onlyPrereqs)
        {
            string filterClass = onlyPrereqs ? "prereq" : "precoreq";
            string filter = "//ul[@class='" + filterClass + "']//li";

            try
            {
                HtmlNodeCollection coursesHtml = this.content.DocumentNode.SelectNodes(filter);

                foreach (HtmlNode course in coursesHtml)
                {
                    string innerHtml = course.InnerHtml;
                    int absoluteCheck = innerHtml.IndexOf("and");
                    if (absoluteCheck > 0 && absoluteCheck < innerHtml.Length * 0.75)
                    {
                        // CORS A and CORS B
                    }

                    int orCheck = innerHtml.IndexOf("or");
                    if(orCheck > 0 && orCheck < innerHtml.Length * 0.75)
                    {
                        // CORS A or CORS B
                    }

                    HtmlNodeCollection singleLinkCheck = course.SelectNodes("a[@href]");
                    if(singleLinkCheck.Count == 1)
                    {
                        // Cors A; and.
                    }
                }

            }
            catch (Exception ex)
            {
                string msg = onlyPrereqs ? "No Pre-requisites" : "No Pre/Co-Requisites";
                this.LogError(msg, ex);
            }
        }

        private static string[] HandleListItem(HtmlNode html) {
            LinkedList <string> courseList = new LinkedList<string>();

            HtmlNodeCollection links = html.SelectNodes("a[@href]");

            foreach (HtmlNode link in links)
            {
                courseList.AddLast(link.InnerHtml);
            }

            return courseList.ToArray();
        }

    }
}
