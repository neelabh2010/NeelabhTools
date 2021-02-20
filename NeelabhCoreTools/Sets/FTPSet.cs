using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeelabhCoreTools.Sets
{

    [Owned]
    public class FTPSet
    {
        /// <summary>
        /// e.g. ftp://ftpserver.com
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public string Host { get; set; }    

        //public int? Port { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string User { get; set; }

        [Column(TypeName = "varchar(400)")]
        public string Password { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Root { get; set; }
    }
}
