using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instructions.Models
{
    public class Image
    {
        public int ImageID { get; set; }
       public string Link { get; set; }
        public Record RecordID { get; set; }
        public Step StepID { get; set; }
    }
}
