namespace UMC_Email.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UMC_EMAIL
    {
        public int ID { get; set; }

        [StringLength(100)]
        public string NAME { get; set; }

        [StringLength(50)]
        public string DEPARTMENT { get; set; }

        [StringLength(200)]
        public string EMAIL { get; set; }
    }
}
