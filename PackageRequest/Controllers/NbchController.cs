using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace BkiScoring.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NbchController : ControllerBase
    {
        /*private readonly ILogger<NbchController> _logger;

        public NbchController(ILogger<NbchController> logger)
        {
            _logger = logger;
        }*/

        public static IConfiguration AppConfiguration;
        public NbchController(IConfiguration configuration)
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

        [HttpPost, DisableRequestSizeLimit]
        public FileResult NbchPost()
        {
            string id = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString();
            var file = Request.Form.Files[0];
            string path = @"C:\\Users\\Ivan\\Desktop\\log\\";
            byte[] clearRequest;

            // записываем пришедший файл на винт
            using (var fileStream = new FileStream(path + "Nbch\\" + file.Name, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            // расшифровываем архив
            /* using (FileStream byteFileStream = System.IO.File.OpenRead(path + "Nbch\\" + file.Name))
            {
                byte[] array = new byte[byteFileStream.Length];
                clearRequest = Cryptography.CryptoPro.DecryptMsg(array);
            }

            // сохраняем расшифрованный файл
            using (FileStream clearFileStream = new FileStream(path + "Nbch\\clear_" + file.Name, FileMode.Create))
            {
                clearFileStream.Write(clearRequest, 0, clearRequest.Length);
            } */

            // распаковываем расшифрованный файл
            string targetFile = path + file.Name.Substring(0, file.Name.IndexOf(".")) + ".xml";
            //PackageRequest.ZipUnZip.Decompress(path + "Nbch\\" + file.Name, targetFile);
            ZipFile.ExtractToDirectory(path + "Nbch\\" + file.Name, path);

            // архивируем ответный файл
            PackageRequest.ZipUnZip.Compress(targetFile, path + file.Name.Substring(0, file.Name.IndexOf(".")) + ".zip");
            
            // получить коллекцию сертов для шифрования
            /* X509Store storeMy = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            storeMy.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certColl = storeMy.Certificates.Find(X509FindType.FindByThumbprint, CertThumbprint, false);  
            
            // подпись архива
            byte[] resp = Cryptography.CryptoPro.SignMsg(clearRequest, CertThumbprint);
            // шифрование архива
            resp = Cryptography.CryptoPro.EncryptMsg(resp, certColl); */

            byte[] resp;
            using (FileStream fs = System.IO.File.OpenRead(path + file.Name.Substring(0, file.Name.IndexOf(".")) + ".zip"))
            {
                resp = new byte[fs.Length];
            }
            FileContentResult respRes = new FileContentResult(resp, "multipart/form-data");

            System.Threading.Thread.Sleep(Convert.ToInt32(AppConfiguration["SleepNbch"]));
            return respRes;            
        }
    }
}