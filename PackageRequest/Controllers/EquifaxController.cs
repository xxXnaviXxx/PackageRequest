using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.IO.Compression;
using PackageRequest.Models;

namespace PackageRequest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EquifaxController : ControllerBase
    {
        public static IConfiguration AppConfiguration;
        public EquifaxController(IConfiguration configuration)
        {
            AppConfiguration = configuration;
        }

        private static X509Certificate2 _certThumbprint = null;
        private static X509Certificate2 CertThumbprint
        {
            get
            {
                if (_certThumbprint == null) _certThumbprint = Cryptography.CryptoPro.GetSignerCert(AppConfiguration["Thumprint"]);
                return _certThumbprint;
            }
        }

        [HttpGet]
        public void EquifaxGet ()
        {
            string path = AppConfiguration["LogingPath"];
            //string ftpFileList = "";
            //string fileName = "";
            //string downloadFileName = "";
            byte[] resp = new byte[100000000];

            while (true)
            {
                try
                {
                    string[] filesOnFtpInbox = Directory.GetFiles(AppConfiguration["FtpDirectoryIn"]);
                    string[] filesOnResponsDir = Directory.GetFiles(AppConfiguration["RKK_EquifaxResponcePath"]);

                    foreach (string filename in filesOnFtpInbox)
                    {
                        foreach (string filenameResponce in filesOnResponsDir)
                        {
                            if (filename.Length > 0 && filename == filenameResponce)
                            {
                                // получить коллекцию сертов для шифрования
                                X509Store storeMy = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                                storeMy.Open(OpenFlags.ReadOnly);
                                X509Certificate2Collection certColl = storeMy.Certificates.Find(X509FindType.FindByThumbprint,CertThumbprint, false);

                                // подпись архива
                                using (FileStream byteFileStream = System.IO.File.OpenRead(AppConfiguration["FtpDirectoryIn"] + filename))
                                {
                                    byte[] clearRequest = new byte[byteFileStream.Length];
                                    resp = Cryptography.CryptoPro.SignMsg(clearRequest, CertThumbprint);
                                }

                                // шифрование архива
                                resp = Cryptography.CryptoPro.EncryptMsg(resp, certColl);
                                
                                // запись результата в папку Outbox
                                using (var fileStream = new FileStream(AppConfiguration["FtpDirectoryOut"] + filenameResponce, FileMode.Create))
                                {
                                    fileStream.Write(resp, 0, resp.Length);
                                }

                                // удаляем файл из Inbox
                                FileInfo fileInf = new FileInfo(AppConfiguration["FtpDirectoryIn"] + filename);
                                if (fileInf.Exists)
                                {
                                    fileInf.Delete();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }

                System.Threading.Thread.Sleep(Convert.ToInt32(AppConfiguration["SleepEquifax"]));
            }
/*
            while (true)
            {
                try{
                    ftpFileList = FtpWorker.FtpFileList(AppConfiguration["FtpAddressIn"], AppConfiguration["FtpUser"], AppConfiguration["FtpPassword"]);
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }

                if (ftpFileList.Length > 0)
                {
                    // получаем список файлов на FTP
                    fileName = ftpFileList;
                    try
                    {
                        // загружаем архив с ответом на FTP
                        FtpWorker.FtpUploadFile(AppConfiguration["FtpAddressOut"], path + "\\Equifax\\" + targetFile, AppConfiguratio["FtpUser"], AppConfiguration["FtpPassword"]);
                    }
                    catch (Exception ex)
                    {
                        if (AppConfiguration["Logging"] == "true")
                        {
                            System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message +"\n");
                        }
                    }  
                    System.Threading.Thread.Sleep(60000);
                }
                else
                {
                    System.Threading.Thread.Sleep(60000);
                }
            }
                
                try
                {
                    // скачаиваем файл с FTP
                    downloadFileName = FtpWorker.FtpDownloadFile(AppConfiguration["FtpAddressIn"], path + "\\Equifax\\" + fileName, AppConfiguration["FtpUser"], AppConfiguration["FtpPassword"]);
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }
            
                try
                {
                    // удаляем скачанный файл из FTP
                    FtpWorker.FtpDeleteFile(AppConfiguration["FtpAddressIn"], AppConfiguration["FtpUser"], AppConfiguration["FtpPassword"]);
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }
            
                try
                {
                    // расшифровываем скачанный архив
                    using (FileStream byteFileStream = System.IO.File.OpenRead(path + "\\Equifax\\" + fileName))
                    {
                        byte[] array = new byte[byteFileStream.Length];
                        cleanRequest = Cryptography.CryptoPro.DecryptMsg(array);
                    }
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }

                try
                {
                    // сохраняем расшифрованный файл
                    using (FileStream cleanFileStream = new FileStream(path + "\\Equifax\\clean_" + fileName, FileMode.Create))
                    {
                        cleanFileStream.Write(cleanRequest, 0, cleanRequest.Length);
                    }
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }

                try
                {
                    // распаковываем расшифрованный файл
                    string targetFile = path + "PackageEmulatorLogs\\clean_" + fileName;
                    ZipFile.ExtractToDirectory(targetFile, path);
                }
                catch (Exception ex)
                {
                    if (AppConfiguration["Logging"] == "true")
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(path, "PackageEmulatorLogs", "log.txt"), ex.Message + "\n");
                    }
                }

                    // архивируем файл ответа
                    PackageRequest.ZipUnZip.Compress(targetFile, path + downloadFileName.Substring(0, downloadFileName.IndexOf(".")) + ".zip");
                */
        }
    }
}