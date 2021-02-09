using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeelabhCoreTools.Sets
{
    [Owned]
    public class DurationSet
    {
        public static readonly string NONE = string.Empty;
        public static readonly string UNLIMITED = "Unlimited";
        public static readonly string DAY = "Day";
        public static readonly string MONTH = "Month";
        public static readonly string YEAR = "Year";

        private string _period;
        public int Length { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Period
        {
            get
            {
                return _period;
            }
            set 
            {
                switch (value.ToUpper())
                {
                    case "":
                        Length = 0;
                        _period = NONE;
                        break;
                    case "UNLIMITED":
                        Length = 0;
                        _period = UNLIMITED;
                        break;
                    case "DAY":
                    case "DAYS":
                        _period = DAY;
                        break;
                    case "MONTH":
                    case "MONTHS":
                        _period = MONTH;
                        break;
                    case "YEAR":
                    case "YEARS":
                        _period = YEAR;
                        break;
                    default:
                        Length = 0;
                        _period = NONE;
                        throw new System.Exception("Not a valid period");
                }
            }
        }

        public DurationSet()
        {
            Length = 0;
            Period = null;
        }

        public DurationSet(int length, string period)
        {
            Length = length;
            Period = period;
        }

        [NotMapped]
        public string Value
        {
            get
            {
                // returns eg. : 1 month, 3 months etc
                if (Period == null || Period == NONE) return DurationSet.NONE;
                else if (Period == UNLIMITED) return DurationSet.UNLIMITED;
                return Length + " " + Period + (Length > 1 ? "s" : "");
            }
            set
            {
                // eg.: 24 Days, 2 Years, 6 Months, 3 Day
                value = value.Trim();

                int len = value.Length;
                // fetch Length --
                int idx = 0;
                while (idx <= len)
                {
                    var chkChar = value.Substring(idx, 1);
                    if (!chkChar.IsInt()) break;
                    idx++;
                }

                if (idx > 0) Length = value.Substring(0, idx).ToInt();

                // fetch Period --
                Period = value[idx..].Trim();
            }
        }
    }
}
