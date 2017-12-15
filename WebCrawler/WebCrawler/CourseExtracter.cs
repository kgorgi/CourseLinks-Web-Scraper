using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UvicCourseCalendar.Infrastructure.DataModel;

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
        const string rootUrl = "https://web.uvic.ca/calendar2018-01/";
        private string coursesUrls => rootUrl + "CDs/";
        const string slash = "/";
        const string urlEnd = ".html";
        public static Regex CoursePattern = new Regex("[A-Z]{2,4} \\d{3}");

        private string _fieldOfStudy;
        private string _courseNum;
        private string _courseId;

        private HtmlDocument content;
        private string htmlContent;

        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            this._fieldOfStudy = fieldOfStudy;
            this._courseNum = courseNum;
            this._courseId = this._fieldOfStudy + " " + this._courseNum;

            // Get Webpage
            // TODO Try Catch
            HtmlWeb web = new HtmlWeb();
            string requestUrl = coursesUrls + fieldOfStudy + slash + courseNum + urlEnd;
            this.content = web.Load(requestUrl);

        }

        public string GetHtmlContent()
        {
            string filter = "//section[@id='content']";

            HtmlNode htmlNode = this.content.DocumentNode.SelectNodes(filter).FirstOrDefault();
            foreach (var a in htmlNode.Descendants("a"))
            {
                // Resolve courses links
                if (CoursePattern.Matches(a.InnerHtml).Count == 1)
                {
                    string existing = a.Attributes["href"].Value.Replace("../", "");
                    a.Attributes["href"].Value = coursesUrls + existing;
                }
            }           
           
            return htmlNode.InnerHtml;
        }

        public ISet<string> ProcessCourse(DependencyType type)
        {
            ISet<string> relatedCourses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // this.LogMessage(type.ToString());
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

            HtmlNodeCollection coursesHtml = this.content.DocumentNode.SelectNodes(filter);

            // Check if Dependencies Exist
            if (coursesHtml == null)
            {
                return relatedCourses;
            }

            //Process Dependencies
            foreach (HtmlNode htmlNode in coursesHtml)
            {
                var coursesFound = CourseExtracter.ExtractCourses(htmlNode);
                foreach (var courseFound in coursesFound)
                {
                    relatedCourses.Add(courseFound);
                }
            }

            return relatedCourses;
        }


        /* GENERAL PROCESSING HELPER METHODS */
        private string PrepareForProcessing(string str)
        {
            // this.LogMessage("Preparing for Proccessing:\n" + str);
            string noTags = RemoveHtmlTags(str);

            // Remove appended ";and, ";or", etc
            int semiColonCheck = noTags.IndexOf(";");
            if (semiColonCheck > -1)
            {
                noTags = noTags.Substring(0, semiColonCheck);
            }

            return noTags;
        }

        /* LOGGING */
        private void LogMessage(string message)
        {
            String timeStamp = DateTime.Now.ToLongTimeString();
            Console.WriteLine(timeStamp + ": " + this._courseId);
            Console.WriteLine(message);
        }

        private void LogError(string error)
        {
            String timeStamp = DateTime.Now.ToLongTimeString();
            Console.WriteLine(timeStamp + ": " + this._courseId + " ERROR ");
            Console.WriteLine(error);
        }

        private void LogExeception(Exception ex)
        {
            this.LogError(ex.Message);
        }

        /* STATIC HELPER METHODS */
        public static ISet<string> ExtractCourses(HtmlNode htmlNode)
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

        // https://stackoverflow.com/questions/12787449/html-agility-pack-removing-unwanted-tags-without-removing-content
        private static string RemoveHtmlTags(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var acceptableTags = new String[] { };

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (!acceptableTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
            }

            return document.DocumentNode.InnerHtml;
        }
    }
}
