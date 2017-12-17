using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public static class Log
    {
        /* LOGGING */
        public static void LogMessage(string message)
        {
            String timeStamp = DateTime.Now.ToLongTimeString();
            Console.WriteLine(timeStamp + ": " + message);
        }

        public static void LogError(string error)
        {
            LogMessage(" ERROR " + error);
        }
    }
}
