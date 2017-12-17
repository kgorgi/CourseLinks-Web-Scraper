namespace UvicCourseCalendar.Infrastructure
{
    public static class Constants
    {
        public const int HashComputePrime = 397;
        public static string CoursesSavePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string UvicRootUrl = "https://web.uvic.ca";
    }
}
