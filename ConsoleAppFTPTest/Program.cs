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

            // 測試取得資料大小
            //var data = new FileInfo(@"C:\Users\011714\Desktop\down\TEST2.txt").Length;

            FTPParameter param = new FTPParameter("128.110.5.134", "006788", "ftp006788");

            FTPHelp ftp = new FTPHelp(param);

            //FTPParameter param2 = new FTPParameter("128.110.5.135", "006788", "ftp006788");

            //FTPHelp ftp2 = new FTPHelp(param2);

            //var reUpFolder = ftp.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

            //var reFileList = ftp.GetFileList("TEST");

            //bool reDowFolder = ftp.DownloadFolder("TEST", @"C:\Users\011714\Desktop\down");

            //DateTime reDateTime = ftp.GetFileModifiedDate("", "chase_upload.txt");

            //bool reDowFile = ftp.DownloadFile("", "TEST", @"C:\Users\011714\Desktop\down", @"C:\Users\011714\Desktop\down");

            //var reUpFile = ftp.UploadFile("", "TEST.txt", @"C:\Users\011714\Desktop\down", "TEST.txt");

            //var reFileList2 = ftp.GetFileList("");
        }

    }
}
