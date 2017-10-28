using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.DataModel
{
    public enum PreReqCondition
    {
        Courses,
        Credits,
        Absolute,
        Grade
    }

    public abstract class PreReq
    {
       public abstract PreReqCondition condition { get; }

        public ISet<string> courseIds { get; set; }
    }

    public sealed class PreReqAbsolute : PreReq
    {
        public override PreReqCondition condition => PreReqCondition.Absolute;
    }

    public sealed class PreReqNumberOfUnits : PreReq
    {
        int Units;

        public override PreReqCondition condition => PreReqCondition.Credits;
    }

    public sealed class PreReqNumberOfCourses : PreReq
    {
        int NumberOfCourses;

        public override PreReqCondition condition => PreReqCondition.Courses;
    }

    public class CourseNode :IEquatable<CourseNode>
    {
        public string CourseId { get; set; }

        public string CourseCode { get; set; }

        public string FieldOfStudy { get; set; }

        public string MarkUp { get; set; }

        public List<PreReq> PreReqs { get; set; }

        public List<PreReq> CoReqs { get; set; }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CourseNode)obj);
        }

        public bool Equals(CourseNode other)
        {
            if(other == null)
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
    }
}
