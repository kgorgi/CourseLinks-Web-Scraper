using System;
using System.Collections.Generic;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace WebCrawler
{
    public class DependencyParser
    {
        private string _text;
        private string _textLower;
        private Action<string> _logMessage;
        private List<Dependency> _dependencies;
        private string[] courseList;

        public DependencyParser(string text, Action<string> logMethodCallback)
        {
            this._text = text;
            this._textLower = text.ToLower();
            this._logMessage = logMethodCallback;
            this._dependencies = new List<Dependency>();
        }

        public List<Dependency> GetDependencies()
        {
            if(this.GetConditionStatementDependencies())
            {
                return _dependencies;
            }

            courseList = CourseExtractHelper.ExtractCourses(_text);

            this.GetOfDependencies();
            this.GetOrDependencies();
            this.GetAbsoluteDependencies();

            return _dependencies;
        }

        /* SPECIFIC PROCESSING HELPER METHODS */
        private bool GetConditionStatementDependencies()
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

            if (foundDependency)
            {
                _dependencies.Add(
                    new DependencyAbsoluteText
                    { ConditionStatement = _text });
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

                var numberOfCoursesPreReq = new DependencyNumberOfCourses
                {
                    courseIds = new HashSet<string>(courseList),
                    NumberOfCourses = 1
                };

                _dependencies.Add(numberOfCoursesPreReq);
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

                AppendToDependencyList(new DependencyNumberOfUnits());
                foundDependency = true;
            }
            else if (_textLower.IndexOf("of") > -1)
            {
                // N courses of subset of courses
                _logMessage("N courses of a list of courses");
                AppendToDependencyList(new DependencyNumberOfCourses());
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

                AppendToDependencyList(new DependencyAbsolute());

                foundDependency = true;
            }

            if (courseList.Length == 1)
            {
                // Single Course
                _logMessage("Only One Course");

                AppendToDependencyList(new DependencyAbsolute());
                foundDependency = true;
            }
            
            return foundDependency;
        }

        private void AppendToDependencyList(DependencyWithIds dependency)
        {
            foreach (var absCourse in courseList)
            {
                dependency.courseIds.Add(absCourse);
            }

            _dependencies.Add(dependency);
        }
    }
}
