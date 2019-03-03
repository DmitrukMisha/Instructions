using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Instructions.Models
{
    public class Record
    {
       
        public int RecordID { get; set; }
        public string USerID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string ThemeName { get; set; }
        public string ImageLink { get; set; }
    }
}
