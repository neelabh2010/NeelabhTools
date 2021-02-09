using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeelabhCoreTools.Sets
{
    [Owned]
    public class SizeSet
    {
        // Note: only one should be used between the below two fields
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitValue { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Unit { get; set; }

        [NotMapped]
        public string Value
        {
            get
            {
                return UnitValue.IsNull() ? Unit.Trim()
                    : ((UnitValue.Value % 1 == 0 ? ((int)UnitValue).ToString() : UnitValue.ToString()) + " " + Unit).Trim();
            }
            set
            {
                // value should like: 24 Qty, 15 pcs, 23.32 Ltr, etc
                // also allowed: 24Qty, 24.22Ltr, 24Pcs, 24Pcs
                // also allowed: someunit, one pc, pair, etc.

                int len = value.Length;
                bool checkDecimal = true;
                // fetch UnitValue --
                int idx = 0;
                while (idx <= len)
                {
                    var chkChar = value.Substring(idx, 1);
                    if (chkChar == "." && checkDecimal)
                    {
                        if (!value.Substring(idx + 1, 1).IsInt()) break;
                        checkDecimal = false;
                    }
                    else if (!chkChar.IsInt()) break;
                    idx++;
                }

                if (idx > 0) UnitValue = value.Substring(0, idx).ToDecimal();

                // fetch Unit --
                Unit = value[idx..].Trim();
            }
        }
    }
}
