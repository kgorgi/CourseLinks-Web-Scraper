using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace UvicCourseCalendar.Infrastructure.CourseNodeRepo
{
    public interface ICourseNodeRepo
    {
        void AddCourse(CourseNode course);

        void RemoveCourse(string courseId);

        void Save();

        void Reload();

        IEnumerable<CourseNode> GetCourses();
    }

    public class CourseNodeRepo : ICourseNodeRepo
    {
        private Dictionary<string, CourseNode> _courses;
        private string _dataPath;
        private JsonSerializerSettings jset = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };

        public CourseNodeRepo(string dataPath = null)
        {
            _dataPath = dataPath == null ? Constants.CoursesSavePath : dataPath;
            _courses = new Dictionary<string, CourseNode>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<CourseNode> GetCourses()
        {
            return _courses.Values;
        }

        public void AddCourse(CourseNode course)
        {
            if (string.IsNullOrWhiteSpace(course.CourseId))
            {
                throw new InvalidDataException(nameof(course.CourseId));
            }

            if (_courses.ContainsKey(course.CourseId))
            {
                throw new DuplicateNameException(nameof(course.CourseId));
            }

            _courses.Add(course.CourseId, course);
        }

        public void RemoveCourse(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            _courses.Remove(courseId);
        }

        public void Reload()
        {
            if(!Directory.Exists(_dataPath))
            {
                return;
            }

            _courses = new Dictionary<string, CourseNode>();
            
            foreach (var folder in new DirectoryInfo(_dataPath).GetDirectories())
            {
                foreach(var file in folder.GetFiles("*.JSON"))
                {
                    var rawJson = File.ReadAllText(file.FullName);
                    CourseNode course = (CourseNode)JsonConvert.DeserializeObject<CourseNode>(rawJson, jset);

                    _courses.Add(course.CourseId, course);
                }
            }
        }

        public void Save()
        {
            if (!_courses.Any())
            {
                //Nothing to save

                return;
            }
            
            CreateDirIfNotFound(_dataPath);

            var coursesGroupByFeildOfStudy = _courses.Values.GroupBy(x => x.FieldOfStudy).OrderBy(x => x.Key);
            
            foreach (var fieldOfStudyGroup in coursesGroupByFeildOfStudy)
            {
                var fieldOfStudy = fieldOfStudyGroup.Key;
                var fieldOfStudyPath = _dataPath + "\\" + fieldOfStudy;
                CreateDirIfNotFound(fieldOfStudyPath);

                foreach (var course in fieldOfStudyGroup)
                {
                    var courseFileName = course.CourseId + ".JSON";
                    var fileContent = JsonConvert.SerializeObject(course, jset);

                    File.WriteAllText(fieldOfStudyPath + "\\" + courseFileName, fileContent);
                }
            }
        }

        private void CreateDirIfNotFound(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
