using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace WebCrawler
{
    class UvicCoursesDownloader
    {
        private const string _endUrl = "/CTs.html";
        private string _calendarName;
        private HtmlWeb _htmlParser;

        private string _startUrl => string.Format("https://web.uvic.ca/{0}/CDs/", _calendarName);

        private string _fieldsOfStudyIndexUrl => string.Format("https://web.uvic.ca/{0}/courses/index.html", _calendarName);

        public UvicCoursesDownloader(string calendarName)
        {
            if (string.IsNullOrEmpty(calendarName))
            {
                throw new ArgumentNullException(nameof(calendarName));
            }

            _calendarName = calendarName;
            _htmlParser = new HtmlWeb();
        }

        private string GetCourseListUrl(string fieldOfStudy)
        {
            return _startUrl + fieldOfStudy + _endUrl;
        }
        
        public List<string> GetFieldOfStudyList()
        {
            HtmlDocument fieldsOfStudyHtml = _htmlParser.Load(_fieldsOfStudyIndexUrl);

            string filter = "//tr[@onclick]//td//a";

            HtmlNodeCollection fieldsOfStudyHtmlCollection = fieldsOfStudyHtml.DocumentNode.SelectNodes(filter);

            List<string> listOfFields = new List<string>();

            bool skip = false;
            for (int i = 0; i < fieldsOfStudyHtmlCollection.Count; i++)
            {
                if (skip)
                {
                    skip = false;

                    continue;
                }

                listOfFields.Add(fieldsOfStudyHtmlCollection[i].InnerHtml);
                skip = true;
            }

            return listOfFields;
        }

        public List<string> GetAllCoursesForFieldOfStudy(string fieldOfStudy, List<string> appendTo = null)
        {
            string coursesIndexUrl = GetCourseListUrl(fieldOfStudy);

            HtmlDocument courseListHtml = _htmlParser.Load(coursesIndexUrl);

            string filter = "//tr[@onclick]//td//a";

            HtmlNodeCollection coursesHtmlCollection = courseListHtml.DocumentNode.SelectNodes(filter);

            List<string> listOfCourses = appendTo != null ? appendTo : new List<string>();

            for (int i = 0; i < coursesHtmlCollection.Count; i += 2)
            {
                listOfCourses.Add(fieldOfStudy + " " + coursesHtmlCollection[i].InnerHtml);
            }

            return listOfCourses;
        }

        public List<string> GetAllUniversityCourses()
        {
            Console.WriteLine("Start downloading courses list from " + _fieldsOfStudyIndexUrl);
            List<string> allPossibleCourses = new List<string>();

            List<string> fieldsOfStudy = GetFieldOfStudyList();

            for (int i = 0; i < fieldsOfStudy.Count; i++)
            {
                GetAllCoursesForFieldOfStudy(fieldsOfStudy[i], allPossibleCourses);
            }

            Console.WriteLine("End downloading courses list from " + _fieldsOfStudyIndexUrl);

            return allPossibleCourses;
        }
    }
}
