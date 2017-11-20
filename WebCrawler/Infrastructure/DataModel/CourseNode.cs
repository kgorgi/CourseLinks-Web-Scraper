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
        public List<Dependency> PreReqs { get; set; }

        [JsonProperty]
        public List<Dependency> CoReqs { get; set; }

        [JsonProperty]
        public List<Dependency> PreOrCoReqs { get; set; }

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

    public enum DependencyCondition
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
        Grade,

        [Description("Must satisfy this statement")]
        Statement,

        // Collection of various Pre-Reqs. Any one of the collections must be satisfied.
        [Description("Must satisfy one of the sections")]
        Or
    }

    public abstract class Dependency
    {
        [JsonIgnore]
        public abstract DependencyCondition condition { get; }

        [JsonProperty]
        public string UserFriendlyCondition => condition.GetDescription();
    }

    public abstract class DependencyWithIds : Dependency
    {
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

    public sealed class DependencyAbsoluteText : Dependency
    {
        public override DependencyCondition condition => DependencyCondition.Statement;
    }

    public sealed class DependencyAbsolute : DependencyWithIds
    {
        public override DependencyCondition condition => DependencyCondition.Absolute;
    }

    public sealed class DependencyNumberOfUnits : DependencyWithIds
    {
        [JsonProperty]
        public float Units { get; set; }

        public override DependencyCondition condition => DependencyCondition.Credits;

        public DependencyNumberOfUnits()
        {
            Units = 0f;
        }

        public DependencyNumberOfUnits(float units)
        {
            if (units < 1)
            {
                throw new InvalidDataException("Units cannot be zero or negative.");
            }

            this.Units = units;
        }
    }

    public sealed class DependencyNumberOfCourses : DependencyWithIds
    {
        [JsonProperty]
        public int NumberOfCourses { get; set; }

        public override DependencyCondition condition => DependencyCondition.Courses;

        public DependencyNumberOfCourses()
        {
            this.NumberOfCourses = 1;
        }

        public DependencyNumberOfCourses(int nCourses)
        {
            if (nCourses < 1)
            {
                throw new InvalidDataException("Number of courses cannot be zero or negative.");
            }

            this.NumberOfCourses = nCourses;
        }
    }

    public sealed class DependencyWithOrCourses : Dependency
    {
        public override DependencyCondition condition => DependencyCondition.Or;

        public List<List<Dependency>> sections;
    }
}
