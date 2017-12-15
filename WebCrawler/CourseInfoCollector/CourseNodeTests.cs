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
                PreReqs = new HashSet<string>
                {
                    "ENG210"
                },
                PreOrCoReqs = new HashSet<string>
                {
                    "ENG150",
                    "MATH160"
                },
                CoReqs = new HashSet<string>
                {
                    "SENG321"
                }
            };

            // ----- Action -----

            var jsonSting = JsonConvert.SerializeObject(courseNode);
            string expectedJson = "{\"CourseCode\":\"255\",\"FieldOfStudy\":\"Math\",\"PreReqs\":[\"ENG210\"],\"CoReqs\":[\"SENG321\"],\"PreOrCoReqs\":[\"ENG150\",\"MATH160\"]}";

            // ----- Expected Result -----

            Assert.AreEqual(expectedJson, jsonSting);
        }

    }
}
