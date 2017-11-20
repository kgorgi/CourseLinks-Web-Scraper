using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace CourseInfoCollector
{
    [TestClass]
    public class CourseNodeTests
    {
        [TestMethod]
        public void CourseNodeValid_Serialize_Done()
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

            // ----- Action -----

            var jsonSting = JsonConvert.SerializeObject(courseNode);

            string expectedJson = "{\"CourseCode\":\"255\",\"FieldOfStudy\":\"Math\",\"MarkUp\":\"HTML CONTENT HERE\",\"PreReqs\":[{\"courseIds\":[\"ENG210\"],\"UserFriendlyCondition\":\"Absolute\"},{\"NumberOfCourses\":1,\"courseIds\":[\"ENG150\",\"MATH160\"],\"UserFriendlyCondition\":\"Courses\"}],\"CoReqs\":[{\"courseIds\":[\"SENG321\"],\"UserFriendlyCondition\":\"Absolute\"}],\"PreOrCoReqs\":null}";

            // ----- Expected Result -----

            Assert.AreEqual(expectedJson, jsonSting);
        }
        
    }
}
