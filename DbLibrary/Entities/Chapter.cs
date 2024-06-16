using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.Entities
{
    public class Chapter
    {
        public int Id { get; set; }
        public virtual Book Book { get; set; }
        public string Name { get; set; }
    }
}
