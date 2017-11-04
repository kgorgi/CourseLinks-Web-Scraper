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
            string lineRead = Console.ReadLine();
            while(lineRead != "EXIT")
            {
                Console.Clear();
                string[] course = lineRead.Split(' ');
                var courseExtractor = new CourseExtracter(course[0], course[1]);
                List <PreReq> prereqs = courseExtractor.ProcessCourse(DependencyType.Prereq);
                List<PreReq> coreqs = courseExtractor.ProcessCourse(DependencyType.Coreq);
                List<PreReq> precoreqs = courseExtractor.ProcessCourse(DependencyType.Precoreq);
                lineRead = Console.ReadLine();
            }
        }
    }
}
