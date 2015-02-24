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

            parse(out xDate, x.Date);
            parse(out yDate, y.Date);

            return xDate.CompareTo(yDate) * -1;
        }

        private void parse(out DateTime date, string dateString)
        {
            date = new DateTime();

            string[] patterns =
            {
                "ddd, dd MMM yyyy HH:mm:ss GMT",
                "ddd, dd MMM yyyy HH:mm:ss K (PST)",
                "ddd, dd MMM yyyy HH:mm:ss K",
                "ddd, dd MMM yyyy HH:mm:ss K (PDT)",
                "ddd, dd MMM yyyy HH:mm:ss K (UTC)"
            };

            bool res = false;
            foreach (string pattern in patterns)
            {
                res = DateTime.TryParseExact(dateString, pattern,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                if (res)
                    break;
            }

            if (!res)
                DateTime.TryParse(dateString, out date);
        }
    }
}