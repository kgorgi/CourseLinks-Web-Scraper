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

    class CourseExtracter
    {
        const string rootUrl = "https://web.uvic.ca/calendar2017-09/CDs/";
        const string slash = "/";
        const string urlEnd = ".html";
        private static Regex coursePattern = new Regex("[A-Z]{2,4} \\d{3}");

        private string _fieldOfStudy;
        private string _courseNum;
        private string _courseId;

        private HtmlDocument content;

        private List<PreReq> _dependencies;

        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            this._fieldOfStudy = fieldOfStudy;
            this._courseNum = courseNum;
            this._courseId = this._fieldOfStudy + " " + this._courseNum;

            // Get Webpage
            // TODO Try Catch
            HtmlWeb web = new HtmlWeb();
            string requestUrl = rootUrl + fieldOfStudy + slash + courseNum + urlEnd;
            this.content = web.Load(requestUrl); 
        }

        public List<PreReq> ProcessCourse(DependencyType type)
        {
            this._dependencies = new List<PreReq>();

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
            var absolutePreReq = new PreReqAbsolute();

            // Check if Dependencies Exist
            if (coursesHtml == null)
            {
                this.LogError("No " + filterClass + "s");
                return null;
            }

            //Process Dependencies
            foreach (HtmlNode listItem in coursesHtml)
            {
                string rawText = PrepareForProcessing(listItem.InnerHtml);
                string rawTextLower = rawText.ToLowerInvariant();

                if (this.GetOtherDependencies(rawText, rawTextLower) || 
                    this.GetOfDependencies(rawText, rawTextLower) ||
                    this.GetOrDependencies(rawText, rawTextLower) ||
                    this.GetAbsoluteDependencies(rawText, rawTextLower, absolutePreReq))
                    continue;

                this.LogError("Unhandled List Dependency:\n" + listItem.InnerHtml);
            }

            if (absolutePreReq?.courseIds?.Count > 0)
            {
                this._dependencies.Add(absolutePreReq);
            }

            List<PreReq> list = _dependencies;
            this._dependencies = null;
            return list;
        }

        /* SPECIFIC PROCESSING HELPER METHODS */
        private bool GetOtherDependencies(string text, string textLower)
        {
            bool foundDependency = false;
            if (text.Contains("permission of"))
            {
                // Permission of faulty/department
                this.LogMessage("Permission of Faulty or Department");
                foundDependency = true;
            }
            else if (text.Contains("academic writing requirement"))
            {
                // Academic Writing Requirement
                this.LogMessage("Academic Writing Requirement");
                foundDependency = true;
            }
            else if (text.Contains("standing"))
            {
                // Year Requirement
                this.LogMessage("Year Requirement");
                foundDependency = true;
            }
            else if (text.Contains("gpa"))
            {
                // GPA Requirement
                this.LogMessage("GPA Requirement");
                foundDependency = true;
            }

            return foundDependency;
        }

        private bool GetAbsoluteDependencies(string text, string textLower, PreReqAbsolute absolutePreReq)
        {
            bool foundDependency = false;
            int absoluteCheck = textLower.IndexOf("and");
            if (absoluteCheck > 0)
            {
                // CORS A and CORS B
                this.LogMessage("Course A and Course B");

                string[] absCourses = ExtractCourses(text);
                foreach (var absCourse in absCourses)
                {
                    absolutePreReq.courseIds.Add(absCourse);
                }

                foundDependency = true;
            }

            MatchCollection absoluteMatches = coursePattern.Matches(text);
            if (absoluteMatches.Count == 1)
            {
                // Single Course
                this.LogMessage("Only One Course");

                absolutePreReq.courseIds.Add(ExtractCourses(text)[0]);
                this.LogMessage(absolutePreReq.courseIds.ToString());
                foundDependency = true;
            }

            return foundDependency;
        }

        private bool GetOrDependencies(string text, string textLower)
        {
            bool foundDependency = false;

            if (text.IndexOf("or") > 0)
            {
                // CORS A or CORS B
               this.LogMessage("Course A or Course B");

                //string[] OneOfCourses = HandleListItem(listItem);

                //var numberOfCoursesPreReq = new PreReqNumberOfCourses
                //{
                //    courseIds = new HashSet<string>(OneOfCourses),
                //    NumberOfCourses = 1
                //};

                //dependencies.Add(numberOfCoursesPreReq);
                foundDependency = true;
            }

            return foundDependency;
        }

        private bool GetOfDependencies(string text, string textLower)
        {
            bool foundDependency = false;
            if (text.IndexOf("units of") > -1)
            {
                // N units of subset of courses
                this.LogMessage("N units of a list of courses");

                var numberOfUnitsPreReq = new PreReqNumberOfUnits()
                {
                    // TODO Fix
                    // courseIds = new HashSet<string>(HandleListItem(listItem))
                };

                this._dependencies.Add(numberOfUnitsPreReq);
                foundDependency = true;
            } else if (text.IndexOf("of") > -1)
            {
                // N courses of subset of courses
                this.LogMessage("N courses of a list of courses");
                foundDependency = true;
            }

            return foundDependency;
        }

        /* GENERAL PROCESSING HELPER METHODS */
        private string PrepareForProcessing(string str)
        {
            this.LogMessage("Preparing for Proccessing:\n" + str);
            string noTags = RemoveHtmlTags(str);

            // Remove appended ";and, ";or", etc
            int semiColonCheck = noTags.IndexOf(";");
            if (semiColonCheck > -1)
            {
                noTags = noTags.Substring(0, semiColonCheck - 1);
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
        private static string[] ExtractCourses(string text)
        {
            // Regex [A-Z]{2,4} \d{3}
            MatchCollection matchCourses = coursePattern.Matches(text);
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

            var acceptableTags = new String[] { "strong", "em", "u" };

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
