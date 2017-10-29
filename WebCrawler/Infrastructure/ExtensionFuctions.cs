using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UvicCourseCalendar.Infrastructure
{
    public static class ExtensionFuctions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            object[] attribs = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attribs.Length > 0)
            {
                string description = ((DescriptionAttribute)attribs[0]).Description;

                return description;
            }

            return string.Empty;
        }
    }
}
