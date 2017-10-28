using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class CourseExtracter
    {
        const string rootUrl = "https://web.uvic.ca/calendar2017-09/CDs/";
        const string slash = "/";
        const string urlEnd = ".html";
        public CourseExtracter(string fieldOfStudy, string courseNum)
        {
            string requestUrl = rootUrl +  fieldOfStudy  + slash + courseNum +  urlEnd;
            using (var wb = new WebClient())
            {
                var response = wb.DownloadString(requestUrl);
            }
        }
    }

}
