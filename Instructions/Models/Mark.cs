using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instructions.Models
{
    public class Mark
    {
        public int MarkID { get; set; }
        public double MarkValue { get; set; }
        public Record RecordID { get; set; }
        public User UserID { get; set; }
    }
}
