using System;
using System.Collections.Generic;
using UvicCourseCalendar.Infrastructure.CourseNodeRepo;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class WebCrawler
    {
        static void Main(string[] args)
        {
            CourseNodeRepo repo = new CourseNodeRepo();
            //DownloadAllCourses(repo);
            repo.Reload();
            //repo.PrintAllCourses();

            repo.PublishCoursesRelationInfo();
            
            ReadyToExit();

            //string lineRead = Console.ReadLine();
            //while (lineRead != "EXIT")
            //{
            //    Console.Clear();
            //    string[] course = lineRead.Split(' ');
            //    var courseExtractor = new CourseExtracter(course[0], course[1]);
            //    var prereqs = courseExtractor.ProcessCourse(DependencyType.Prereq);
            //    var coreqs = courseExtractor.ProcessCourse(DependencyType.Coreq);
            //    var precoreqs = courseExtractor.ProcessCourse(DependencyType.Precoreq);

            //    CourseNode courseNode = new CourseNode() { FieldOfStudy = course[0], CourseCode = course[1], PreReqs = prereqs, CoReqs = coreqs, PreOrCoReqs = precoreqs };
            //    PrintCourses(courseNode);

            //    //Console.WriteLine();

            //    lineRead = Console.ReadLine();
            //}
        }

        private static void DownloadAllCourses(ICourseNodeRepo repo)
        {
            var allCourses = new string[] { "CSC 320" };
            //var allCourses = CourseList.GetAllUniversityCourses();

            foreach (var courseId in allCourses)
            {
                var courseNode = DownloadCourse(courseId);
                courseNode.PrintCourse();

                repo.AddCourse(courseNode);
            }

            repo.SaveCoursesToDisk();                
        }

        public static CourseNode DownloadCourse(string courseId)
        {
            string[] course = courseId.Split(' ');
            var courseExtractor = new CourseExtracter(course[0], course[1]);
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
