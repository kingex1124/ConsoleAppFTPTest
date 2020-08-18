using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFTPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Path.Combine("FTP://", "123");


            FTPParameter param = new FTPParameter("128.110.5.134", "006788", "ftp006788");

            FTPHelp ftp = new FTPHelp(param);

            //DateTime re = ftp.GetFileModifiedDate("", "chase_upload.txt");

            bool re = ftp.RemoveFolder("", "TEST.txt");
         
        }
      
    }
}
