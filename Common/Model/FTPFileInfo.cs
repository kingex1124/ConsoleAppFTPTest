using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class FTPFileInfo
    {
        public FTPFileInfo()
        {
        }

        public string FullFileName { get; set; }
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
