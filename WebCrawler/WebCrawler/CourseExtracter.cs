using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UvicCourseCalendar.Infrastructure.DataModel;

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
            List<PreReq> preReqs = new List<PreReq>();
            string filterClass = onlyPrereqs ? "prereq" : "precoreq";
            string filter = "//ul[@class='" + filterClass + "']//li";

            HtmlNodeCollection coursesHtml = this.content.DocumentNode.SelectNodes(filter);
            var absolutePreReq = new PreReqAbsolute();

            if (coursesHtml == null)
            {
                this.LogError("No " + (onlyPrereqs ? "Pre-requisites" : "Pre/Co-Requisites"));
                return;
            }

            // TODO Rename course to listItem (after merge with Amandeep)
            foreach (HtmlNode listItem in coursesHtml)
            {
                string innerHtml = listItem.InnerHtml;

                if (innerHtml.Contains("permission of"))
                {
                    // Permission of faulty
                    Console.WriteLine("Permission of faulty");
                    Console.WriteLine(listItem.InnerHtml);
                    continue;
                }

                if (innerHtml.Contains("Academic Writing Requirement"))
                {
                    // Permission of faulty
                    Console.WriteLine("Academic Writing Requirement");
                    Console.WriteLine(listItem.InnerHtml);
                    continue;
                }

                if (innerHtml.IndexOf("units of") > -1)
                {
                    // N units of subset of courses
                    Console.WriteLine("N units of CORS A, CORS B, etc");
                    Console.WriteLine(listItem.InnerHtml);
                    continue;
                }

                int absoluteCheck = innerHtml.IndexOf("and");
                if (absoluteCheck > 0 && absoluteCheck < innerHtml.Length - 4)
                {
                    // CORS A and CORS B
                    Console.WriteLine("A and B");
                    Console.WriteLine(listItem.InnerHtml);

                    string[] absCourses = HandleListItem(listItem);
                    foreach (var absCourse in absCourses)
                    {
                        absolutePreReq.courseIds.Add(absCourse);
                    }
                    continue;
                }

                int orCheck = innerHtml.IndexOf("or");
                if (orCheck > 0 && orCheck < innerHtml.Length - 3)
                {
                    // CORS A or CORS B
                    Console.WriteLine("A or B");
                    Console.WriteLine(listItem.InnerHtml);

                    string[] OneOfCourses = HandleListItem(listItem);

                    var numberOfCoursesPreReq = new PreReqNumberOfCourses
                    {
                        courseIds = new HashSet<string>(OneOfCourses),
                        NumberOfCourses = 1
                    };

                    preReqs.Add(numberOfCoursesPreReq);
                    continue;
                }

                HtmlNodeCollection singleLinkCheck = listItem.SelectNodes("a[@href]");
                if (singleLinkCheck != null && singleLinkCheck.Count == 1)
                {
                    // Cors A; and.
                    Console.WriteLine("A Only");
                    Console.WriteLine(listItem.InnerHtml);
                    absolutePreReq.courseIds.Add(HandleListItem(listItem)[0]);
                    continue;
                }

                throw new Exception("Unhandled List Dependency:\n" + listItem.InnerHtml);
            }

            if (absolutePreReq?.courseIds?.Count > 0)
            {
                preReqs.Add(absolutePreReq);
            }
        }

        private static string[] HandleListItem(HtmlNode html)
        {
            LinkedList<string> courseList = new LinkedList<string>();

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
