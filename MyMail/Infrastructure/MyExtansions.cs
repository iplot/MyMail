using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMail.Infrastructure
{
    public static class MyExtansions
    {
        public static string CleanString(this string target)
        {
            target = target.Replace('/', '.');
            target = target.Replace('?', '1');
            target = target.Replace('+', '2');
            target = target.Replace('\\', '3');

            return target;
        }
    }
}