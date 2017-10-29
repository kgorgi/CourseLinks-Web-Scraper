using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using WebCrawler.DataModel;

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
                PreReqs = new List<PreReq>
                {
                    new PreReqAbsolute
                    {
                         courseIds = new HashSet<string>
                         {
                             "ENG210"
                         }
                    },
                    new PreReqNumberOfCourses(1)
                    {                        
                         courseIds = new HashSet<string>
                         {
                             "ENG150",
                             "MATH160"
                         }
                    }
                },
                CoReqs = new List<PreReq>
                {
                    new PreReqAbsolute
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

            string expectedJson = "{\"CourseCode\":\"255\",\"FieldOfStudy\":\"Math\",\"MarkUp\":\"HTML CONTENT HERE\",\"PreReqs\":[{\"UserFriendlyCondition\":\"Absolute\",\"courseIds\":[\"ENG210\"]},{\"NumberOfCourses\":1,\"UserFriendlyCondition\":\"Courses\",\"courseIds\":[\"ENG150\",\"MATH160\"]}],\"CoReqs\":[{\"UserFriendlyCondition\":\"Absolute\",\"courseIds\":[\"SENG321\"]}]}";

            // ----- Expected Result -----

            Assert.AreEqual(expectedJson, jsonSting);
        }
        
    }
}
