using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NeelabhCoreTools.Sets
{

    [Owned]
    public class FTPSet
    {
        [Column(TypeName = "varchar(50)")]
        public string Host { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string User { get; set; }

        [Column(TypeName = "varchar(400)")]
        public string Password { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Root { get; set; }
    }
}
