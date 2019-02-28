using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Instructions.Models
{
    public class Step
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public string StepName { get; set; }
        public int StepID { get; set; }
        public Record RecordID { get; set; }
    }
}
