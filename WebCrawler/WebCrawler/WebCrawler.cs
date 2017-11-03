using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class WebCrawler 
    {
        static void Main(string[] args)
        {
            var courseExtractor = new CourseExtracter("PHYS", "111", DependencyType.Prereq);
            List<PreReq> prereqs = courseExtractor.ProcessCourse(DependencyType.Prereq);
            List<PreReq> coreqs = courseExtractor.ProcessCourse(DependencyType.Coreq);
            List<PreReq> precoreqs = courseExtractor.ProcessCourse(DependencyType.Precoreq);
            Console.ReadLine();
        }
    }
}
