namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public partial class DD_MISSIONTYPE
    {
        public int id { get; set; }

        [StringLength(10)]
        public string code { get; set; }

        [StringLength(50)]
        public string cvalue { get; set; }

        [StringLength(50)]
        public string evalue { get; set; }

        static public List<DD_MISSIONTYPE> GETALL()
        {
            Model4 db = new Model4();
            return db.DD_MISSIONTYPE.ToList();
        }
    }
}
