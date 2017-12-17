using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UvicCourseCalendar.Infrastructure;
using UvicCourseCalendar.Infrastructure.DataModel;

namespace UvicCourseCalendar.Infrastructure.CourseNodeRepo
{
    public interface ICourseNodeRepo
    {
        void AddCourse(CourseNode course);

        void RemoveCourse(string courseId);

        void SaveCoursesToDisk();

        void PublishCoursesRelationInfo();

        void Reload();

        IEnumerable<CourseNode> GetCourses();
    }

    public class CourseNodeRepo : ICourseNodeRepo
    {
        private Dictionary<string, CourseNode> _courses;
        private string _dataPath;
        private string _dataCachePath => _dataPath + "\\CachedCourses";
        private string _dataRelationsInfoPath => _dataPath + "\\RelationsInfo";

        private JsonSerializerSettings jset = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None };

        public CourseNodeRepo(string dataPath = null)
        {
            _dataPath = dataPath == null ? Constants.CoursesSavePath + "\\Data" : dataPath;
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
            Console.WriteLine("Start loading cached courses");

            if (!Directory.Exists(_dataCachePath))
            {
                return;
            }

            _courses = new Dictionary<string, CourseNode>();

            foreach (var folder in new DirectoryInfo(_dataCachePath).GetDirectories())
            {
                foreach (var file in folder.GetFiles("*.JSON"))
                {
                    if (!file.FullName.EndsWith("_def.JSON"))
                    {
                        continue;
                    }

                    var rawJson = File.ReadAllText(file.FullName);
                    CourseNode course = (CourseNode)JsonConvert.DeserializeObject<CourseNode>(rawJson, jset);

                    _courses.Add(course.CourseId, course);
                }
            }

            Console.WriteLine("End loading cached courses");
        }

        /// <summary>
        /// Write courses to disk for caching.
        /// </summary>
        public void SaveCoursesToDisk()
        {
            Console.WriteLine("Start caching courses.");

            if (!_courses.Any())
            {
                //Nothing to save

                return;
            }

            CreateDirIfNotFound(_dataCachePath);

            var coursesGroupByFeildOfStudy = _courses.Values.GroupBy(x => x.FieldOfStudy).OrderBy(x => x.Key);

            foreach (var fieldOfStudyGroup in coursesGroupByFeildOfStudy)
            {
                var fieldOfStudy = fieldOfStudyGroup.Key;
                var fieldOfStudyPath = _dataCachePath + "\\" + fieldOfStudy;
                CreateDirIfNotFound(fieldOfStudyPath);

                foreach (var course in fieldOfStudyGroup)
                {
                    var fileContent = JsonConvert.SerializeObject(course, jset);
                    var courseFileName = course.FieldOfStudy + course.CourseCode + "_def.JSON";
                    File.WriteAllText(fieldOfStudyPath + "\\" + courseFileName, fileContent);
                }
            }

            Console.WriteLine("End caching courses.");
        }

        /// <summary>
        /// Writes courses relationship info for front-end to disk
        /// </summary>
        public void PublishCoursesRelationInfo()
        {
            Console.WriteLine("Start publishing course relations information.");
            SaveCoursesMarkup();
            SaveCoursesRelations();
            SaveCoursesList();
            Console.WriteLine("End publishing course relations information.");
        }

        /// <summary>
        /// Write HTML docs to disk
        /// </summary>
        private void SaveCoursesMarkup()
        {
            Console.WriteLine("Start saving courses markup.");
            if (!_courses.Any())
            {
                //Nothing to save

                return;
            }

            CreateDirIfNotFound(_dataRelationsInfoPath);

            var coursesGroupByFeildOfStudy = _courses.Values.GroupBy(x => x.FieldOfStudy).OrderBy(x => x.Key);

            foreach (var fieldOfStudyGroup in coursesGroupByFeildOfStudy)
            {
                var fieldOfStudy = fieldOfStudyGroup.Key;
                var fieldOfStudyPath = _dataRelationsInfoPath + "\\" + fieldOfStudy;
                CreateDirIfNotFound(fieldOfStudyPath);

                foreach (var course in fieldOfStudyGroup)
                {
                    var courseFileName = course.FieldOfStudy + course.CourseCode + ".HTML";

                    File.WriteAllText(fieldOfStudyPath + "\\" + courseFileName, course.MarkUp);
                }
            }
            Console.WriteLine("End saving courses markup.");
        }

        /// <summary>
        /// Write course relations to disk
        /// </summary>
        private void SaveCoursesRelations()
        {
            Console.WriteLine("Start saving relations information.");

            if (!_courses.Any())
            {
                // Nothing to save

                return;
            }

            CreateDirIfNotFound(_dataRelationsInfoPath);

            // Reduce duplicate process time.
            IDictionary<string, Relations> allFirstDegreeRelationsById = GenerateAllFirstDegreeRelations();

            var coursesGroupByFeildOfStudy = _courses.Values.GroupBy(x => x.FieldOfStudy).OrderBy(x => x.Key);

            foreach (var fieldOfStudyGroup in coursesGroupByFeildOfStudy)
            {
                var fieldOfStudy = fieldOfStudyGroup.Key;
                var fieldOfStudyPath = _dataRelationsInfoPath + "\\" + fieldOfStudy;
                CreateDirIfNotFound(fieldOfStudyPath);

                Console.Write("Field :" + fieldOfStudy);

                foreach (var course in fieldOfStudyGroup)
                {
                    Console.Write(" " + course.CourseCode);

                    Relations courseRelations = GetAllCourseRelations(course, allFirstDegreeRelationsById);

                    var fileContent = JsonConvert.SerializeObject(courseRelations, jset);
                    var courseFileName = course.FieldOfStudy + course.CourseCode + ".JSON";
                    File.WriteAllText(fieldOfStudyPath + "\\" + courseFileName, fileContent);
                }

                Console.WriteLine();
            }
            Console.WriteLine("End saving relations information.");
        }

        private Relations GetAllCourseRelations(CourseNode courseNode, IDictionary<string, Relations> allFirstDegreeRelationsById)
        {
            if (!allFirstDegreeRelationsById.TryGetValue(courseNode.CourseId, out Relations relations))
            {
                return new Relations();
            }

            Relations courseRelations = new Relations();
            courseRelations.RelationsList.UnionWith(relations.RelationsList.Select(x => x.Clone()));

            var processedCourseIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var relatedCourseIdsForCourse = GetRelatedCourseIds(courseNode, courseRelations);

            while (relatedCourseIdsForCourse.Any())
            {
                // Iterate on new collection so relatedCourseIdsForCourse collection can be modified.
                foreach (var courseId in relatedCourseIdsForCourse.ToArray())
                {
                    processedCourseIds.Add(courseId);
                    if (allFirstDegreeRelationsById.TryGetValue(courseId, out Relations subRelations))
                    {
                        // Merge nth degree relations
                        courseRelations.RelationsList.UnionWith(subRelations.RelationsList.Select(x => x.Clone()));

                        CourseNode relatedCourseNode = _courses[courseId];
                        var subCourseIds = GetRelatedCourseIds(relatedCourseNode, subRelations);

                        foreach (var subCourseId in subCourseIds)
                        {
                            relatedCourseIdsForCourse.Add(subCourseId);
                        }
                    }

                    relatedCourseIdsForCourse.Remove(courseId);
                }

                relatedCourseIdsForCourse.ExceptWith(processedCourseIds);
            }

            ISet<Relation> processedRelation = CaclulateRelationDepth(courseNode, courseRelations);
            courseRelations.RelationsList = processedRelation;

            courseRelations.CourseLevels = new HashSet<CourseLevelInfo>();

            foreach (var courseId in processedCourseIds)
            {
                int depthLevel = 1;
                Relation maxDepthRelation = processedRelation
                    .Where(x => x.Destination == courseId)
                    .OrderByDescending(x => x.Level)
                    .FirstOrDefault();

                if (maxDepthRelation != null)
                {
                    depthLevel = maxDepthRelation.Level;
                }

                courseRelations.CourseLevels.Add(new CourseLevelInfo { CourseId = courseId, Level = depthLevel });
            }

            int levelCount = 0;

            foreach (var courseLevelInfoGrp in courseRelations.CourseLevels.OrderBy(x => x.Level).GroupBy(x => x.Level))
            {
                levelCount += 1;
                if (courseLevelInfoGrp.Key == levelCount)
                {
                    continue;
                }

                foreach (var courseLevelInfo in courseLevelInfoGrp)
                {
                    courseLevelInfo.Level = levelCount;
                }                
            }

            // Add main node
            courseRelations.CourseLevels.Add(new CourseLevelInfo { CourseId = courseNode.CourseId, Level = 0 });

            return courseRelations;
        }

        private ISet<Relation> CaclulateRelationDepth(CourseNode headCourse, Relations relations)
        {
            if (headCourse == null)
            {
                return new HashSet<Relation>();
            }

            ISet<Relation> processedRelation = new HashSet<Relation>();

            int currentLevel = 0;

            HashSet<CourseNode> remainingSourceCourses = new HashSet<CourseNode>();
            remainingSourceCourses.Add(headCourse);

            do
            {
                currentLevel += 1;
                foreach (var sourceCourse in remainingSourceCourses.ToArray())
                {
                    foreach (var childCourseInfo in GetChildCourses(sourceCourse))
                    {
                        if (_courses.TryGetValue(childCourseInfo.courseId, out CourseNode relatedCourseNode))
                        {
                            var relation = relations.RelationsList
                                .Where(x => x.Source.Equals(sourceCourse.CourseId, StringComparison.OrdinalIgnoreCase) &&
                            x.Destination.Equals(childCourseInfo.courseId, StringComparison.OrdinalIgnoreCase) &&
                            x.Type == childCourseInfo.courseType).First();
                            relation.Level = currentLevel;

                            if (processedRelation.Add(relation))
                            {
                                remainingSourceCourses.Add(relatedCourseNode);
                            }
                        }
                    }

                    remainingSourceCourses.Remove(sourceCourse);
                }
            } while (remainingSourceCourses.Any());

            return processedRelation;
        }

        private IEnumerable<(string courseId, string courseType)> GetChildCourses(CourseNode courseNode)
        {
            foreach (var courseId in courseNode.PreReqs)
            {
                yield return (courseId, Relation.RelationTypePreReq);
            }

            foreach (var courseId in courseNode.PreOrCoReqs)
            {
                yield return (courseId, Relation.RelationTypePreCoReq);
            }

            foreach (var courseId in courseNode.CoReqs)
            {
                yield return (courseId, Relation.RelationTypeCoReq);
            }
        }

        private ISet<string> GetRelatedCourseIds(CourseNode courseNode, Relations courseRelations)
        {
            var courseIds = new HashSet<string>(courseRelations.RelationsList
                                          .Where(x => x.Source == courseNode.CourseId)
                                          .Select(x => x.Destination)
                                          .ToArray(), StringComparer.OrdinalIgnoreCase);
            return courseIds;
        }

        private IDictionary<string, Relations> GenerateAllFirstDegreeRelations()
        {
            Dictionary<string, Relations> allRelations = new Dictionary<string, Relations>(StringComparer.OrdinalIgnoreCase);

            foreach (var course in _courses.Values)
            {
                var courseRelations = GetFirstDegreeRelations(course);
                allRelations.Add(course.CourseId, courseRelations);
            }

            return allRelations;
        }

        private Relations GetFirstDegreeRelations(CourseNode course)
        {
            Relations courseRelations = new Relations();

            var prereqRelations = GetRelations(course.PreReqs, course.CourseId, Relation.RelationTypePreReq);
            var coreqRelations = GetRelations(course.CoReqs, course.CourseId, Relation.RelationTypeCoReq);
            var precoreqRelations = GetRelations(course.PreOrCoReqs, course.CourseId, Relation.RelationTypePreCoReq);

            courseRelations.RelationsList.UnionWith(prereqRelations);
            courseRelations.RelationsList.UnionWith(coreqRelations);
            courseRelations.RelationsList.UnionWith(precoreqRelations);

            return courseRelations;
        }

        private IList<Relation> GetRelations(ISet<string> DestCourses, string sourceCourse, string relationType)
        {
            List<Relation> relations = new List<Relation>();

            foreach (var prereqCourse in DestCourses)
            {
                Relation relation = new Relation
                {
                    Source = sourceCourse,
                    Destination = prereqCourse,
                    Type = relationType
                };

                relations.Add(relation);
            }

            return relations;
        }

        /// <summary>
        /// Write course relations to disk
        /// </summary>
        public void SaveCoursesList()
        {
            Console.WriteLine("Start saving courses list.");
            if (!_courses.Any())
            {
                //Nothing to save

                return;
            }

            CreateDirIfNotFound(_dataRelationsInfoPath);
            var coursesListFileName = _dataRelationsInfoPath + "\\Courses.JSON";
            var CourseList = new CourseList() { Courses = new HashSet<string>(_courses.Keys) };
            var fileContent = JsonConvert.SerializeObject(CourseList, jset);
            File.WriteAllText(coursesListFileName, fileContent);
            Console.WriteLine("End saving courses list.");
        }

        private void CreateDirIfNotFound(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void PrintAllCourses()
        {
            Console.WriteLine("Start printing all courses in repository.");
            foreach (var course in _courses.Values)
            {
                course.PrintCourse();
            }
            Console.WriteLine("End printing all courses in repository.");
        }
    }
}

public class CourseList
{
    [JsonProperty]
    public ISet<string> Courses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

public class Relations
{
    [JsonProperty]
    public ISet<Relation> RelationsList = new HashSet<Relation>();

    [JsonIgnore]
    public ISet<CourseLevelInfo> CourseLevels = new HashSet<CourseLevelInfo>();

    [JsonProperty]
    public IDictionary<string, int> CourseLevelsInfo => CourseLevels
        .OrderBy(x => x.Level)
        .ToDictionary(x => x.CourseId, x => x.Level, StringComparer.OrdinalIgnoreCase);
}

public class CourseLevelInfo : IEquatable<CourseLevelInfo>
{
    [JsonProperty]
    public string CourseId { get; set; }

    [JsonProperty]
    public int Level { get; set; }

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

        return Equals((CourseLevelInfo)obj);
    }

    public bool Equals(CourseLevelInfo other)
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

public class Relation : IEquatable<Relation>
{
    [JsonIgnore]
    public const string RelationTypePreReq = "prereq";
    [JsonIgnore]
    public const string RelationTypeCoReq = "coreq";
    [JsonIgnore]
    public const string RelationTypePreCoReq = "precoreq";

    [JsonProperty]
    public string Source { get; set; }

    [JsonProperty]
    public string Destination { get; set; }

    [JsonProperty]
    public string Type { get; set; }

    [JsonIgnore]
    public int Level { get; set; }

    public Relation Clone(int level = 0)
    {
        var newRelation = (Relation)this.MemberwiseClone();
        newRelation.Level = level;

        return newRelation;
    }

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

        return Equals((Relation)obj);
    }

    public bool Equals(Relation other)
    {
        if (other == null)
        {
            return false;
        }

        bool result = Source.Equals(other.Source, StringComparison.Ordinal) &&
                        Destination.Equals(other.Destination, StringComparison.Ordinal) &&
                        Type.Equals(other.Type, StringComparison.Ordinal);

        return result;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int result = (Constants.HashComputePrime) ^ Source.GetHashCode() ^ Destination.GetHashCode() ^ Type.GetHashCode();

            return result;
        }
    }

    #endregion Equality
}
