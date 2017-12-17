using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UvicCourseCalendar.Infrastructure;

namespace WebCrawler
{
    public enum DependencyType
    {
        Prereq,
        Coreq,
        Precoreq
    }

    public class CourseExtracter
    {
        private string _calendarName;
        private string _fieldOfStudy;
        private string _courseNum;
        private HtmlDocument _content;
        private static Regex CoursePattern = new Regex("[A-Z]{2,4} \\d{3}");
        
        private string coursesUrls => Constants.UvicRootUrl + "/" + _calendarName + "/CDs/";

        private string  requestUrl => coursesUrls + _fieldOfStudy + "/" + _courseNum + ".html";        

        public CourseExtracter(string fieldOfStudy, string courseNum, string calendarName)
        {
            this._fieldOfStudy = fieldOfStudy;
            this._courseNum = courseNum;
            this._calendarName = calendarName;

            // Get Webpage
            HtmlWeb web = new HtmlWeb();
            this._content = web.Load(requestUrl);
        }

        public string GetHtmlContent()
        {
            string filter = "//section[@id='content']";        
            HtmlNode htmlNode = this._content.DocumentNode.SelectNodes(filter).FirstOrDefault();

            foreach (var a in htmlNode.Descendants("a"))
            {
                var absPathParts = requestUrl.Split('/').ToList();
                absPathParts.Remove(absPathParts.Last()); // Need to remove html page

                string relPath = a.Attributes["href"].Value;
                string finalPath = relPath;
                if (relPath.StartsWith("../"))
                {
                    var relPathParts = relPath.Split('/');

                    for (int relPos = 0; relPos < relPathParts.Length; relPos++)
                    {
                        if (relPathParts[relPos].Contains(".."))
                        {
                            absPathParts.Remove(absPathParts.Last());
                            continue;
                        }

                        absPathParts.Add(relPathParts[relPos]);
                    }

                    finalPath = string.Join("/", absPathParts);
                }

                a.Attributes["href"].Value = finalPath;
            }           
           
            return htmlNode.InnerHtml;
        }

        public ISet<string> ProcessCourse(DependencyType type)
        {
            ISet<string> relatedCourses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Determine HTML Class
            string filterClass = "prereq";

            if (type == DependencyType.Coreq)
            {
                filterClass = "coreq";
            }
            else if (type == DependencyType.Precoreq)
            {
                filterClass = "precoreq";
            }

            // Filter Based on Class
            string filter = "//ul[@class='" + filterClass + "']//li";

            HtmlNodeCollection coursesHtml = this._content.DocumentNode.SelectNodes(filter);

            // Check if Dependencies Exist
            if (coursesHtml == null)
            {
                return relatedCourses;
            }

            //Process Dependencies
            foreach (HtmlNode htmlNode in coursesHtml)
            {
                var coursesFound = ExtractCourses(htmlNode);
                foreach (var courseFound in coursesFound)
                {
                    relatedCourses.Add(courseFound);
                }
            }

            return relatedCourses;
        }
        
        private ISet<string> ExtractCourses(HtmlNode htmlNode)
        {
            var coursesFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            HtmlNodeCollection links = htmlNode.SelectNodes("a[@href]");

            if (links == null)
            {
                return coursesFound;
            }

            foreach (var item in links)
            {
                string courseName = item.InnerHtml;

                // Regex [A-Z]{2,4} \d{3}
                if (CoursePattern.Matches(courseName).Count == 1)
                {
                    coursesFound.Add(courseName);
                }
            }

            return coursesFound;
        }        
    }
}
