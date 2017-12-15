using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace UvicCourseCalendar.Infrastructure.DataModel
{
    public class CourseNode : IEquatable<CourseNode>
    {
        #region Properties

        [JsonIgnore]
        public string CourseId => FieldOfStudy.ToUpper() + " " + CourseCode.ToUpper();

        [JsonProperty]
        public string CourseCode { get; set; }

        [JsonProperty]
        public string FieldOfStudy { get; set; }

        [JsonProperty]
        public string MarkUp { get; set; }

        [JsonProperty]
        public ISet<string> PreReqs { get; set; }

        [JsonProperty]
        public ISet<string> CoReqs { get; set; }

        [JsonProperty]
        public ISet<string> PreOrCoReqs { get; set; }

        #endregion Properties

        #region Constructors

        public CourseNode()
        {
            PreReqs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CoReqs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            PreOrCoReqs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion Constructors

        #region Equality

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CourseNode)obj);
        }

        public bool Equals(CourseNode other)
        {
            if (other == null)
            {
                return false;
            }

            bool result = CourseId.Equals(other.CourseId, StringComparison.Ordinal);

            return result;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Constants.HashComputePrime) ^ CourseId.GetHashCode();

                return result;
            }
        }

        #endregion Equality

        #region Utility Functions

        public void PrintCourse()
        {
            PrintCourse(this);
        }

        public  static void PrintCourse(CourseNode course)
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("Printing Details for " + course.CourseId);

            Console.Write("\nPreReqs:");

            foreach (var courseId in course.PreReqs)
            {
                Console.Write(courseId + " ");
            }

            Console.Write("\nCoReqs:");

            foreach (var courseId in course.CoReqs)
            {
                Console.Write(courseId + " ");
            }

            Console.Write("\nPreReqs OR CoReqs:");

            foreach (var courseId in course.PreOrCoReqs)
            {
                Console.Write(courseId + " ");
            }

            Console.WriteLine();
        }

        #endregion Utility Functions
    }
}
