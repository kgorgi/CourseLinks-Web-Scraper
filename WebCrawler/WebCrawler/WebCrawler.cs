using System;
using System.Collections.Generic;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class WebCrawler 
    {
        static void Main(string[] args)
        {
            var courseExtractor = new CourseExtracter("PHYS", "111");
            List<PreReq> prereqs = courseExtractor.ProcessCourse(DependencyType.Prereq);
            List<PreReq> coreqs = courseExtractor.ProcessCourse(DependencyType.Coreq);
            List<PreReq> precoreqs = courseExtractor.ProcessCourse(DependencyType.Precoreq);
            Console.ReadLine();
        }
    }
}
