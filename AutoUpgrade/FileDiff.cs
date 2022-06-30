using System.Collections.Generic;

namespace AutoUpgrade
{
    /// <summary>
    /// 文件差异
    /// </summary>
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
