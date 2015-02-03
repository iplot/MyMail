using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using NetWork.MailReciever;

namespace MyMail.Infrastructure
{
    public class MailsComparator:IComparer<IMailMess>
    {
        public int Compare(IMailMess x, IMailMess y)
        {
            DateTime xDate;
            DateTime yDate;

            DateTime.TryParseExact(x.Date, "ddd, dd MMM yyyy h:mm:ss zzz (PST)",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out xDate);
            DateTime.TryParseExact(x.Date, "ddd, dd MMM yyyy h:mm:ss zzz",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out xDate);

            DateTime.TryParseExact(y.Date, "ddd, dd MMM yyyy h:mm:ss zzz (PST)",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out yDate);
            DateTime.TryParseExact(y.Date, "ddd, dd MMM yyyy h:mm:ss zzz",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out yDate);

            return xDate.CompareTo(yDate) * -1;
        }
    }
}