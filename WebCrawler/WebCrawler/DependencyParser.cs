using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class DependencyParser
    {
        private string _text;
        private string _textLower;
        private Action<string> _logMessage;
        private DependencyAbsolute _absolutePreReq;
        private List<Dependency> _dependencies;

        public DependencyParser(string text, DependencyAbsolute absDependency, Action<string> logMethodCallback)
        {
            this._text = text;
            this._textLower = text.ToLower();
            this._absolutePreReq = absDependency;
            this._logMessage = logMethodCallback;
            this._dependencies = new List<Dependency>();
        }

        public List<Dependency> GetDependencies()
        {
            this.GetOtherDependencies();
            this.GetOfDependencies();
            this.GetOrDependencies();
            this.GetAbsoluteDependencies();

            return _dependencies;
        }

        /* SPECIFIC PROCESSING HELPER METHODS */
        private bool GetOtherDependencies()
        {
            bool foundDependency = false;
            if (_textLower.Contains("permission of"))
            {
                // Permission of faulty/department
                _logMessage("Permission of Faulty or Department");
                foundDependency = true;
            }
            else if (_textLower.Contains("academic writing requirement"))
            {
                // Academic Writing Requirement
                _logMessage("Academic Writing Requirement");
                foundDependency = true;
            }
            else if (_textLower.Contains("standing"))
            {
                // Year Requirement
                _logMessage("Year Requirement");
                foundDependency = true;
            }
            else if (_textLower.Contains("gpa"))
            {
                // GPA Requirement
                _logMessage("GPA Requirement");
                foundDependency = true;
            }

            return foundDependency;
        }

        private bool GetAbsoluteDependencies()
        {
            bool foundDependency = false;
            int absoluteCheck = _textLower.IndexOf("and");
            if (absoluteCheck > 0)
            {
                // CORS A and CORS B
                _logMessage("Course A and Course B");

                string[] absCourses = CourseExtracter.ExtractCourses(_text);
                foreach (var absCourse in absCourses)
                {
                    _absolutePreReq.courseIds.Add(absCourse);
                }

                foundDependency = true;
            }

            MatchCollection absoluteMatches = CourseExtracter.CoursePattern.Matches(_text);
            if (absoluteMatches.Count == 1)
            {
                // Single Course
                _logMessage("Only One Course");

                _absolutePreReq.courseIds.Add(CourseExtracter.ExtractCourses(_text)[0]);
                foundDependency = true;
            }

            return foundDependency;
        }

        private bool GetOrDependencies()
        {
            bool foundDependency = false;

            if (_textLower.IndexOf("or") > 0)
            {
                // CORS A or CORS B
                _logMessage("Course A or Course B");

                //string[] OneOfCourses = HandleListItem(listItem);

                //var numberOfCoursesPreReq = new PreReqNumberOfCourses
                //{
                //    courseIds = new HashSet<string>(OneOfCourses),
                //    NumberOfCourses = 1
                //};

                //dependencies.Add(numberOfCoursesPreReq);
                foundDependency = true;
            }

            return foundDependency;
        }

        private bool GetOfDependencies()
        {
            bool foundDependency = false;
            if (_textLower.IndexOf("units of") > -1)
            {
                // N units of subset of courses
                _logMessage("N units of a list of courses");

                var numberOfUnitsPreReq = new DependencyNumberOfUnits()
                {
                    // TODO Fix
                    // courseIds = new HashSet<string>(HandleListItem(listItem))
                };

                this._dependencies.Add(numberOfUnitsPreReq);
                foundDependency = true;
            }
            else if (_textLower.IndexOf("of") > -1)
            {
                // N courses of subset of courses
                _logMessage("N courses of a list of courses");
                foundDependency = true;
            }

            return foundDependency;
        }

    }
}
