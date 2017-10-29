using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UvicCourseCalendar.Infrastructure;

namespace UvicCourseCalendar.Infrastructure.DataModel
{
    public class CourseNode : IEquatable<CourseNode>
    {
        #region Properties

        [JsonIgnore]
        public string CourseId => FieldOfStudy + CourseCode;

        [JsonProperty]
        public string CourseCode { get; set; }

        [JsonProperty]
        public string FieldOfStudy { get; set; }

        [JsonProperty]
        public string MarkUp { get; set; }

        [JsonProperty]
        public List<PreReq> PreReqs { get; set; }

        [JsonProperty]
        public List<PreReq> CoReqs { get; set; }

        #endregion Properties

        #region Constructors

        public CourseNode()
        {

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
    }

    public enum PreReqCondition
    {
        // Number of courses required from a list of courses
        [Description("Courses")]
        Courses,

        // Number of units requires from a list of courses
        [Description("Credits")]
        Credits,

        // All courses required in a list
        [Description("Absolute")]
        Absolute,

        // Grade required for each course in a list
        [Description("Grade")]
        Grade
    }

    public abstract class PreReq
    {
        [JsonIgnore]
        public abstract PreReqCondition condition { get; }

        [JsonProperty]
        public string UserFriendlyCondition => condition.GetDescription();
        
        private ISet<string> _courseIds;

        [JsonProperty]
        public ISet<string> courseIds
        {
            get
            {
                if (_courseIds == null)
                {
                    _courseIds = new HashSet<string>();
                }

                return _courseIds;
            }
            set
            {
                _courseIds = value;
            }
        }
    }

    public sealed class PreReqAbsolute : PreReq
    {
        public override PreReqCondition condition => PreReqCondition.Absolute;
    }

    public sealed class PreReqNumberOfUnits : PreReq
    {
        [JsonProperty]
        public float Units { get; set; }

        public override PreReqCondition condition => PreReqCondition.Credits;

        public PreReqNumberOfUnits()
        {
            Units = 0f;
        }

        public PreReqNumberOfUnits(float units)
        {
            if (units < 1)
            {
                throw new InvalidDataException("Units cannot be zero or negative.");
            }

            this.Units = units;
        }
    }

    public sealed class PreReqNumberOfCourses : PreReq
    {
        [JsonProperty]
        public int NumberOfCourses { get; set; }

        public override PreReqCondition condition => PreReqCondition.Courses;

        public PreReqNumberOfCourses()
        {
            this.NumberOfCourses = 1;
        }

        public PreReqNumberOfCourses(int nCourses)
        {
            if (nCourses < 1)
            {
                throw new InvalidDataException("Number of courses cannot be zero or negative.");
            }

            this.NumberOfCourses = nCourses;
        }
    }    
}
