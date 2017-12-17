using System;
using UvicCourseCalendar.Infrastructure.CourseNodeRepo;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class WebCrawler
    {
        public static string[] _calendars = {
            //"calendar2017-09",
            "calendar2018-01" }; // Nice to have - Read a list from file.

        static void Main(string[] args)
        {
            foreach (var calendar in _calendars)
            {
                Console.WriteLine(string.Format("Start processing : {0}", calendar));

                CourseNodeRepo repo = new CourseNodeRepo(calendar);
                DownloadAllCourses(repo, calendar);
                //repo.Reload();
                //repo.PrintAllCourses();

                repo.PublishCoursesRelationInfo();

                Console.WriteLine(string.Format("End processing : {0}", calendar));
            }

            ReadyToExit();
        }

        private static void DownloadAllCourses(ICourseNodeRepo repo, string calendarName)
        {
             var allCourses = new string[] { "MATH 101" };

            //UvicCoursesDownloader uvicCoursesDownloader = new UvicCoursesDownloader(calendarName);
            //var allCourses = uvicCoursesDownloader.GetAllUniversityCourses();

            foreach (var courseId in allCourses)
            {
                var courseNode = DownloadCourse(courseId, calendarName);
                courseNode.PrintCourse();

                repo.AddCourse(courseNode);
            }

             repo.SaveCoursesToDisk();
        }

        public static CourseNode DownloadCourse(string courseId, string calendarName)
        {
            string[] course = courseId.Split(' ');
            var courseExtractor = new CourseExtracter(course[0], course[1], calendarName);
            var prereqs = courseExtractor.ProcessCourse(DependencyType.Prereq);
            var coreqs = courseExtractor.ProcessCourse(DependencyType.Coreq);
            var precoreqs = courseExtractor.ProcessCourse(DependencyType.Precoreq);

            CourseNode courseNode = new CourseNode()
            { FieldOfStudy = course[0], CourseCode = course[1], PreReqs = prereqs, CoReqs = coreqs, PreOrCoReqs = precoreqs, MarkUp = courseExtractor.GetHtmlContent() };

            courseNode.PrintCourse();

            return courseNode;
        }

        private static void ReadyToExit()
        {
            Console.WriteLine("All done. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
