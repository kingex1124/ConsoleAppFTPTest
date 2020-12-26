using Common;
using Common.Interface;
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
            ISFTPHelper sftpHelpter = UnityContainer.Resolve<ISFTPHelper, SFTPHelper>(new object[] { "172.17.10.113", "APEC", "3DJ5s25h" });

            sftpHelpter.Connect();

            var sftpres = sftpHelpter.GetFileAndFolderList("upload","t");

            //SFTPHelper sftpHelp = new SFTPHelper("128.110.138.11", 22, "SSH011684", "011684");

            //var reConn = sftpHelp.Connect();

            //var reList = sftpHelp.GetFileList("/C:/Users/ssh011684/");

            //var reModDateTime = sftpHelp.GetFileModifiedDate("TEST", "TEST1.txt");

            //var reSize = sftpHelp.GetFileSize("TEST", "TEST2.txt");

            //var reUpload = sftpHelp.UploadFile("", "123TEST.txt", @"C:\Users\011714\Desktop\down", "TEST2.txt");

            //var reDownload = sftpHelp.DownloadFile("TEST", "TEST2.txt", @"C:\Users\011714\Desktop\down", "newdata.txt");

            //var reDowFolder = sftpHelp.DownloadFolder("TEST", @"C:\Users\011714\Desktop\down");

            //var reCompare = sftpHelp.CheckDownloadData("TEST", "TEST2.txt", @"C:\Users\011714\Desktop\down", "TEST2.txt");

            //var reCreateFolder = sftpHelp.CreateFolder("", "測試");

            //var reDelete = sftpHelp.DeleteFile("", "TEST.txt");

            //var reDeleteFolder = sftpHelp.RemoveFolder("", "測試");

            //sftpHelp.Move("123.txt", "TEST/123.txt");

            SFTPHelper sftpHelp2 = new SFTPHelper("128.110.5.135", 22, "APUSER", "2wsx#EDC");

            var reConn2 = sftpHelp2.Connect();
          
            //var reUpFolder = sftpHelp2.UploadFolder("測試", @"C:\Users\011714\Desktop\down");
            var reUpFolder = sftpHelp2.UploadFolder("測試", @"C:\Users\cam\Desktop\TestData");
        }
    }
}
