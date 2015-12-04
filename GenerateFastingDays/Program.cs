using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GenerateFastingDays
{
    class Program
    {
        static void Main(string[] args)
        {
            var isNumeric = new Func<string, bool>(s =>
            {
                int tmp;
                return int.TryParse(s, out tmp);
            });
            if (args.Length != 3 || (args[0] != "h" && args[0] != "g") || !args.Skip(1).All(k => isNumeric(k)))
            {
                Console.WriteLine("Usage: " + Assembly.GetExecutingAssembly().GetName().Name + "<h|g> <startYear> <endYear>");
                return;
            }
            int startYear = int.Parse(args[1]);
            int endYear = int.Parse(args[2]);
            var calendar = new UmAlQuraCalendar();
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            var root = (XmlElement)doc.AppendChild(doc.CreateElement("FastingDays"));
            bool formatHijri = args[0] == "h";
            if (formatHijri)
            {
                while (startYear <= endYear)
                {
                    var year = (XmlElement)root.AppendChild(doc.CreateElement("Year"));
                    year.SetAttribute("value", startYear.ToString());
                    for (int m = 1; m <= 12; ++m)
                    {
                        var month = (XmlElement)year.AppendChild(doc.CreateElement("Month"));
                        month.SetAttribute("value", m.ToString());
                        for (int d = 13; d <= 15; ++d)
                        {
                            var date = new DateTime(startYear, m, d, calendar);
                            var day = (XmlElement)month.AppendChild(doc.CreateElement("Day"));
                            day.SetAttribute("value", d.ToString());
                            day.SetAttribute("date", date.ToString("yyyy-MM-dd"));
                        }
                    }
                    ++startYear;
                }
            }
            else
            {
                var arr = new DateTime[(endYear - startYear + 1) * 12 * 3];
                int idx = 0;
                while (startYear <= endYear)
                {
                    for (int m = 1; m <= 12; ++m)
                    {
                        for (int d = 13; d <= 15; ++d)
                        {
                            arr[idx++] = new DateTime(startYear, m, d, calendar);
                        }
                    }
                    ++startYear;
                }
                var arSA = new CultureInfo("ar-SA");
                arSA.DateTimeFormat.Calendar = calendar;
                var years = arr.GroupBy(k => k.Year);
                foreach (var y in years)
                {
                    var year = (XmlElement)root.AppendChild(doc.CreateElement("Year"));
                    year.SetAttribute("value", y.Key.ToString());
                    var months = y.GroupBy(k => k.Month);
                    foreach (var m in months)
                    {
                        var month = (XmlElement)year.AppendChild(doc.CreateElement("Month"));
                        month.SetAttribute("value", m.Key.ToString());
                        foreach (var d in m)
                        {
                            var day = (XmlElement)month.AppendChild(doc.CreateElement("Day"));
                            day.SetAttribute("value", d.Day.ToString());
                            day.SetAttribute("date", d.ToString("yyyy-MM-dd", arSA.DateTimeFormat));
                        }
                    }
                }
            }
            doc.Save("output.xml");
        }
    }
}
