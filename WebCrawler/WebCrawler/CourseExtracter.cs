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

        private void LogError(string error)
        {
            string courseId = this._fieldOfStudy + " " + this._courseNum + ":";
            Console.Error.WriteLine("-------------------");
            Console.Error.WriteLine(courseId);
            Console.Error.WriteLine(error);
            Console.Error.WriteLine("-------------------");
        }

        private void LogExecption(Exception ex)
        {
            this.LogError(ex.Message);
        }

        private void GetCourses(bool onlyPrereqs)
        {
            string filterClass = onlyPrereqs ? "prereq" : "precoreq";
            string filter = "//ul[@class='" + filterClass + "']//li";

            HtmlNodeCollection coursesHtml = this.content.DocumentNode.SelectNodes(filter);

            if(coursesHtml == null)
            {
                this.LogError("No " + (onlyPrereqs ? "Pre-requisites" : "Pre/Co-Requisites"));
                return;
            }

            // TODO Rename course to listItem (after merge with Amandeep)
            foreach (HtmlNode course in coursesHtml)
            {
                string innerHtml = course.InnerHtml;

                if (innerHtml.Contains("permission of"))
                {
                    // Permission of faulty
                    Console.WriteLine("Permission of faulty");
                    Console.WriteLine(course.InnerHtml);
                    continue;
                }

                if (innerHtml.Contains("Academic Writing Requirement"))
                {
                    // Permission of faulty
                    Console.WriteLine("Academic Writing Requirement");
                    Console.WriteLine(course.InnerHtml);
                    continue;
                }

                if (innerHtml.IndexOf("units of") > -1)
                {
                    // N units of subset of courses
                    Console.WriteLine("N units of CORS A, CORS B, etc");
                    Console.WriteLine(course.InnerHtml);
                    continue;
                }

                int absoluteCheck = innerHtml.IndexOf("and");
                if (absoluteCheck > 0 && absoluteCheck < innerHtml.Length - 4)
                {
                    // CORS A and CORS B
                    Console.WriteLine("A and B");
                    Console.WriteLine(course.InnerHtml);
                    continue;
                }

                int orCheck = innerHtml.IndexOf("or");
                if(orCheck > 0 && orCheck < innerHtml.Length - 3)
                {
                    // CORS A or CORS B
                    Console.WriteLine("A or B");
                    Console.WriteLine(course.InnerHtml);
                    continue;
                }

                HtmlNodeCollection singleLinkCheck = course.SelectNodes("a[@href]");
                if( singleLinkCheck != null && singleLinkCheck.Count == 1)
                {
                    // Cors A; and.
                    Console.WriteLine("A Only");
                    Console.WriteLine(course.InnerHtml);
                    continue;
                }


                

                throw new Exception("Unhandled List Dependency:\n" + course.InnerHtml);
            }
        }

        private static string[] HandleListItem(HtmlNode html) {
            LinkedList <string> courseList = new LinkedList<string>();

            HtmlNodeCollection links = html.SelectNodes("a[@href]");

            if (links != null)
            {
                foreach (HtmlNode link in links)
                {
                    courseList.AddLast(link.InnerHtml);
                }
            }
            
            return courseList.ToArray();
        }

    }
}
