using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upgrade
{
    public class FileDiff
    {
        public FileDiff()
        {
            this.Changes = new List<string>();
            this.Deletedes = new List<string>();
        }
        public List<string> Changes { get; set; }

        public List<string> Deletedes { get; set; }
    }
}
