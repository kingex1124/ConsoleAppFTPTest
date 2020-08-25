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

            //var data = new FileInfo(@"C:\Users\011714\Desktop\down\TEST2.txt").Length;

            FTPParameter param = new FTPParameter("128.110.5.134", "006788", "ftp006788");

            FTPHelp ftp = new FTPHelp(param);

            //var da = ftp.GetFileList("TEST");

            //bool result = ftp.DownloadFolder("TEST", @"C:\Users\011714\Desktop\down");

            //DateTime re = ftp.GetFileModifiedDate("", "chase_upload.txt");

            // bool re = ftp.DownloadFile("", "TEST", @"C:\Users\011714\Desktop\down", @"C:\Users\011714\Desktop\down");

            FTPParameter param2 = new FTPParameter("128.110.5.135", "006788", "ftp006788");

            FTPHelp ftp2 = new FTPHelp(param2);

   

            var res = ftp2.UploadFile("", "TEST.txt", @"C:\Users\011714\Desktop\down", "TEST.txt");

            var data = ftp2.GetFileList("");
        }
      
    }
}
