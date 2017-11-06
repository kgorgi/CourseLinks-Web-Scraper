using System.Collections.Generic;
using System.Text;
using System.Threading;
using HtmlAgilityPack;

namespace WebCrawler
{
    class CourseList
    {
        private static string _fieldsOfStudyIndexUrl = "https://web.uvic.ca/calendar2017-09/courses/index.html";
        public static string FieldOfStudyUrl 
        {
            get
            {
                return _fieldsOfStudyIndexUrl;
            }
        }

        private static string _startUrl = "https://web.uvic.ca/calendar2017-09/CDs/";
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

            for (int i = 0; i < fieldsOfStudyHtmlCollection.Count/2; i+=2)
            {
                listOfFields.Add(fieldsOfStudyHtmlCollection[i].InnerHtml);
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
            List<string> allPossibleCourses = new List<string>();

            List<string> fieldsOfStudy = GetFieldOfStudyList();

            for(int i = 0; i < fieldsOfStudy.Count; i++)
            {
                GetAllCoursesForFieldOfStudy(fieldsOfStudy[i], allPossibleCourses);
            }

            return allPossibleCourses;
        }
    }
}
