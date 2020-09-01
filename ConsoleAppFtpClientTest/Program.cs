using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppFtpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var ftp = new FTPHelp("128.110.138.11", 990, "test", "011684");

            //var ftp = new FTPHelp("128.110.5.134", "006788", "ftp006788");

            //var conres = ftp.Connect();

            //var reList = ftp.GetFileList("TEST");

            //DateTime reDateTime = ftp.GetFileModifiedDate("", "chase_upload.txt");

            //var reSize = ftp.GetFileSize("", "chase_upload.txt");

            //var reUpload = ftp.UploadFile("TEST", "123.txt", @"C:\Users\011714\Desktop\down", "TEST.txt");

            //var reDow = ftp.DownloadFile("", "TEST.txt", @"C:\Users\011714\Desktop\down", "123.txt");

            // var reCreateFolder = ftp.CreateFolder("", "TEST2");

            // var reDeleteFile = ftp.DeleteFile("TEST", "123.txt");

            // var reRemoveFolder = ftp.RemoveFolder("", "TEST2");

            //var reFolderExist = ftp.IsFolderExists("TEST", "123");

            //var reDpwFolder = ftp.DownloadFolder("TEST", @"C:\Users\011714\Desktop\down");

            //var reUpFolder = ftp.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

            //var reUpFolder = ftp.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

            //var ftp = new FTPHelp("128.110.5.134", 990, "006788", "ftp006788");

            //var reCon = ftp.Connect();

            //var reSize = ftp.GetFileSize("", "chase_upload.txt");

            //var reList = ftp.GetFileList("");

            

            //var reDow = ftp.DownloadFile("", "TEST.txt", @"C:\Users\011714\Desktop\down", "123.txt");

            //var reUpFile = ftp.UploadFile("TEST", "123.txt", @"C:\Users\011714\Desktop\down", "TEST1.txt");

            //var reUpFolder = ftp.UploadFolder("測試", @"C:\Users\011714\Desktop\down");

            var ftp2 = new FTPSHelp("128.110.5.135", 990, "006788", "ftp006788");

            var recon2 = ftp2.Connect();
            var reList = ftp2.GetFileAndFolderList("");

            var reFileList = ftp2.GetFileList("");

            var reFolderList = ftp2.GetFolderList("");

            //var reUpFolder3 = ftp2.UploadFile("測試", "TEST1.txt", @"C:\Users\011714\Desktop\down", "TEST1.txt");

            var reUpFolder2 = ftp2.UploadFolder("測試", @"C:\Users\011714\Desktop\down");
        }
    }
}
