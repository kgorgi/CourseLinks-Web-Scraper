using System.Text.RegularExpressions;

namespace WebCrawler
{
    public static class CourseExtractHelper
    {
        public static Regex CoursePattern = new Regex("[A-Z]{2,4} \\d{3}");

        /* STATIC HELPER METHODS */
        public static string[] ExtractCourses(string text)
        {
            // Regex [A-Z]{2,4} \d{3}
            MatchCollection matchCourses = CoursePattern.Matches(text);
            string[] courses = new string[matchCourses.Count];

            for (int i = 0; i < matchCourses.Count; i++)
            {
                courses[i] = matchCourses[i].ToString();
            }

            return courses;
        }
    }
}
