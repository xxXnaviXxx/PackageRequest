using System.IO;
using System.Net;

namespace PackageRequest.Models
{
    public class FtpWorker
    {
        public static string FtpFileList (string ftpAddress, string ftpUser, string ftpPassword)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            string ftpFileList = reader.ReadToEnd();

            reader.Close();
            responseStream.Close();
            response.Close();

            return ftpFileList;
        }

        public static void FtpUploadFile (string ftpAddress, string fileName, string ftpUser, string ftpPassword)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            FileStream fs = new FileStream(fileName, FileMode.Open);
            byte[] fileContents = new byte[fs.Length];
            fs.Read(fileContents, 0, fileContents.Length);
            fs.Close();

            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();
        }

        public static string FtpDownloadFile (string ftpAddress, string fileName, string ftpUser, string ftpPassword)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();

            FileStream fs = new FileStream(fileName, FileMode.Create);
            byte[] buffer = new byte[64];
            int size = 0;

            while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, size);
            }
            fs.Close();
            responseStream.Close();
            response.Close();

            return fs.Name;
        }

        public static void FtpDeleteFile (string ftpAddress, string ftpUser, string ftpPassword)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse) request.GetResponse();
            response.Close();
        }
    }
}