using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppSFTPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SFTPHelper sftpHelp = new SFTPHelper("128.110.138.11", 22, "SSH011684", "011684");

            var re = sftpHelp.Connect();
        }
    }
}
