using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using UvicCourseCalendar.Infrastructure.CourseNodeRepo;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace CourseInfoCollector
{
    [TestClass]
    public class CourseRepoTests
    {
        [TestMethod]
        public void CourseRepoWithOneCourse_Save_FileSaved()
        {
            // ----- Setup Environment -----

            CourseNode courseNode = new CourseNode
            {
                CourseCode = "255",
                FieldOfStudy = "Math",
                MarkUp = "HTML CONTENT HERE",
                PreReqs = new List<Dependency>
                {
                    new DependencyAbsolute
                    {
                         courseIds = new HashSet<string>
                         {
                             "ENG210"
                         }
                    },
                    new DependencyNumberOfCourses(1)
                    {                        
                         courseIds = new HashSet<string>
                         {
                             "ENG150",
                             "MATH160"
                         }
                    }
                },
                CoReqs = new List<Dependency>
                {
                    new DependencyAbsolute
                    {
                         courseIds = new HashSet<string>
                         {
                             "SENG321"
                         }
                    }
                }
            };

            CourseNodeRepo courseRepo = new CourseNodeRepo();

            courseRepo.AddCourse(courseNode);
            courseRepo.Save();
            courseRepo.Reload();
        }
        
    }
}
