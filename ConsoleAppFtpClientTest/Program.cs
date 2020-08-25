using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFtpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var ftp = new FTPHelp("128.110.138.11", 990, "test", "011684");
            var ftp = new FTPHelp("128.110.5.134", "006788", "ftp006788");

            var conres = ftp.Connect();

            var reList = ftp.GetFileList("TEST");

            DateTime reDateTime = ftp.GetFileModifiedDate("", "chase_upload.txt");

            var reSize = ftp.GetFileSize("", "chase_upload.txt");

            //var reUpload = ftp.UploadFile("TEST", "123.txt", @"C:\Users\011714\Desktop\down", "TEST.txt");

            //var reDow = ftp.DownloadFile("", "TEST.txt", @"C:\Users\011714\Desktop\down", "123.txt");

            // var reCreateFolder = ftp.CreateFolder("", "TEST2");

            // var reDeleteFile = ftp.DeleteFile("TEST", "123.txt");

            // var reRemoveFolder = ftp.RemoveFolder("", "TEST2");

        }
    }
}
