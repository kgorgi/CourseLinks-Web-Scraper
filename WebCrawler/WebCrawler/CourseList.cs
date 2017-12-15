using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace WebCrawler
{
    class CourseList
    {
        private static string _fieldsOfStudyIndexUrl = "https://web.uvic.ca/calendar2018-01/courses/index.html";
        public static string FieldOfStudyUrl 
        {
            get
            {
                return _fieldsOfStudyIndexUrl;
            }
        }

        private static string _startUrl = "https://web.uvic.ca/calendar2018-01/CDs/";
        private static string _endUrl = "/CTs.html";

        private static string GetCourseListUrl(string fieldOfStudy)
        {
            return _startUrl + fieldOfStudy + _endUrl;
        }

        private static HtmlWeb _htmlParser = new HtmlWeb();
        public static List<string> GetFieldOfStudyList()
        {
            HtmlDocument fieldsOfStudyHtml = _htmlParser.Load(FieldOfStudyUrl);

            string filter = "//tr[@onclick]//td//a";

            HtmlNodeCollection fieldsOfStudyHtmlCollection = fieldsOfStudyHtml.DocumentNode.SelectNodes(filter);

            List<string> listOfFields = new List<string>();

            bool skip = false;
            for (int i = 0; i < fieldsOfStudyHtmlCollection.Count; i++)
            {
                if(skip)
                {
                    skip = false;

                    continue;
                }

                listOfFields.Add(fieldsOfStudyHtmlCollection[i].InnerHtml);
                skip = true;
            }

            return listOfFields;
        }

        public static List<string> GetAllCoursesForFieldOfStudy(string fieldOfStudy, List<string> appendTo = null)
        {
            string coursesIndexUrl = GetCourseListUrl(fieldOfStudy);

            HtmlDocument courseListHtml = _htmlParser.Load(coursesIndexUrl);

            string filter = "//tr[@onclick]//td//a";

            HtmlNodeCollection coursesHtmlCollection = courseListHtml.DocumentNode.SelectNodes(filter);

            List<string> listOfCourses = appendTo != null ? appendTo : new List<string>();

            for (int i = 0; i < coursesHtmlCollection.Count; i+= 2)
            {
                listOfCourses.Add(fieldOfStudy + " " + coursesHtmlCollection[i].InnerHtml);
            }

            return listOfCourses;
        }

        public static List<string> GetAllUniversityCourses()
        {
            Console.WriteLine("Start downloading courses list from " + _fieldsOfStudyIndexUrl);
            List<string> allPossibleCourses = new List<string>();

            List<string> fieldsOfStudy = GetFieldOfStudyList();

            for(int i = 0; i < fieldsOfStudy.Count; i++)
            {
                GetAllCoursesForFieldOfStudy(fieldsOfStudy[i], allPossibleCourses);
            }

            Console.WriteLine("End downloading courses list from " + _fieldsOfStudyIndexUrl);

            return allPossibleCourses;
        }
    }
}
