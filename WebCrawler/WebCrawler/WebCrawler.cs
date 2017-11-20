using System;
using System.Collections.Generic;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class WebCrawler 
    {
        static void Main(string[] args)
        {
            string lineRead = Console.ReadLine();
            while(lineRead != "EXIT")
            {
                Console.Clear();
                string[] course = lineRead.Split(' ');
                var courseExtractor = new CourseExtracter(course[0], course[1]);
                List <Dependency> prereqs = courseExtractor.ProcessCourse(DependencyType.Prereq);
                List<Dependency> coreqs = courseExtractor.ProcessCourse(DependencyType.Coreq);
                List<Dependency> precoreqs = courseExtractor.ProcessCourse(DependencyType.Precoreq);
                lineRead = Console.ReadLine();
            }
        }
    }
}
