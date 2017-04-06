namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("enterpine.article")]
    public partial class article
    {
        public int ArticleID { get; set; }

        [StringLength(50)]
        public string Title { get; set; }

        [DataType (DataType.MultilineText)]
        public string Content { get; set; }

        [StringLength(20)]
        public string AuthorName { get; set; }

        public DateTime? PostDate { get; set; }
    }
}
