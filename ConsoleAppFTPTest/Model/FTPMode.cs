using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFTPTest
{
    public enum FTPMode
    {
        /// <summary>
        /// 一般FTP 未加密 port 21
        /// </summary>
        None,
        /// <summary>
        /// 外顯式TLS的FTP port 21
        /// </summary>
        Explicit
    }
}
