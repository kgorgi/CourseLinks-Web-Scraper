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
        private const string rootUrl = "https://web.uvic.ca/calendar2017-09/CDs/";
        private const string middleUrl = "/";
        private const string urlEnd = ".html";
        public static Regex CoursePattern = new Regex("[A-Z]{2,4} \\d{3}");

        private string _fieldOfStudy;
        private string _courseNum;
        private string _courseId;

        private HtmlDocument content;

        private List<Dependency> _dependencies;

        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            this._fieldOfStudy = fieldOfStudy;
            this._courseNum = courseNum;
            this._courseId = this._fieldOfStudy + " " + this._courseNum;

            // Get Webpage
            // TODO Try Catch
            HtmlWeb web = new HtmlWeb();
            string requestUrl = rootUrl + fieldOfStudy + middleUrl + courseNum + urlEnd;
            this.content = web.Load(requestUrl); 
        }

        public List<Dependency> ProcessCourse(DependencyType type)
        {
            this._dependencies = new List<Dependency>();
            this.LogMessage(type.ToString());
            // Determine HTML Class
            string filterClass = "prereq";

            if (type == DependencyType.Coreq)
            {
                filterClass = "coreq";
            } else if (type == DependencyType.Precoreq)
            {
                filterClass = "precoreq";
            }

            // Filter Based on Class
            string filter = "//ul[@class='" + filterClass + "']//li";
            
            HtmlNodeCollection coursesHtml = this.content.DocumentNode.SelectNodes(filter);
            var absolutePreReq = new DependencyAbsolute();

            // Check if Dependencies Exist
            if (coursesHtml == null)
            {
                return null;
            }

            //Process Dependencies
            foreach (HtmlNode listItem in coursesHtml)
            {
                string rawText = PrepareForProcessing(listItem.InnerHtml);
                Action<string> logCallBack = LogMessage;

                DependencyParser dependencyParser = new DependencyParser(rawText, absolutePreReq, logCallBack);
                List<Dependency> foundDependencies = dependencyParser.GetDependencies();
                if (foundDependencies.Count > 0)
                {
                    _dependencies.AddRange(foundDependencies);
                }
                else
                {
                    this.LogError("Unhandled List Dependency:\n" + listItem.InnerHtml);
                }
            }

            if (absolutePreReq?.courseIds?.Count > 0)
            {
                this._dependencies.Add(absolutePreReq);
            }

            List<Dependency> list = _dependencies;
            this._dependencies = null;
            return list;
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
        public  static string[] ExtractCourses(string text)
        {
            // Regex [A-Z]{2,4} \d{3}
            MatchCollection matchCourses = CoursePattern.Matches(text);
            string[] courses = new string[matchCourses.Count];

            for(int i = 0; i < matchCourses.Count; i++)
            {
                courses[i] = matchCourses[i].ToString();
            }

            return courses;
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
