using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class WebCrawler 
    {
        static void Main(string[] args)
        {
            //var courseExtracter = new CourseExtracter("ACAN", "225");
            //var courseExtracter = new CourseExtracter("CSC", "322");
            var preReqExtractor = new CourseExtracter("ENGR", "001", DependencyType.Prereq);
            var coReqExtractor = new CourseExtracter("ENGR", "001", DependencyType.Coreq);
            var precoReqExtractor = new CourseExtracter("ENGR", "001", DependencyType.Precoreq);
        }
    }
}
