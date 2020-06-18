using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography
{

    /// <summary>
    /// Хелпер для работы с CryptoPro
    /// </summary>
    public class CryptoPro
    {

        #region Методы
        /// <summary>
        /// Подписывает сообщение
        /// </summary>
        /// <param name="msg">Сообщение</param>
        /// <param name="signerCert">Сертификат</param>
        /// <returns>Результат подписания</returns>
        public static byte[] SignMsg(Byte[] msg, X509Certificate2 signerCert)
        {
            ContentInfo contentInfo = new ContentInfo(msg);
            SignedCms signedCms = new SignedCms(contentInfo);
            CmsSigner cmsSigner = new CmsSigner(signerCert);
            cmsSigner.IncludeOption = X509IncludeOption.EndCertOnly;
            signedCms.ComputeSignature(cmsSigner);
            return signedCms.Encode();
        }

        /// <summary>
        /// Проверяет подпись
        /// </summary>
        /// <param name="encodedSignedCms">Сообщение</param>
        /// <param name="origMsg">Сообщение без подписи</param>
        /// <returns>Результат проверки</returns>
        public static bool VerifyMsg(byte[] encodedSignedCms, out byte[] origMsg)
        {
            SignedCms signedCms = new SignedCms();
            signedCms.Decode(encodedSignedCms);
            try
            {
                signedCms.CheckSignature(true);
            }
            catch (System.Security.Cryptography.CryptographicException e)
            {
                origMsg = null;
                return false;
            }
            origMsg = signedCms.ContentInfo.Content;
            return true;
        }

        /// <summary>
        /// Шифрует сообщение
        /// </summary>
        /// <param name="msg">Сообщение</param>
        /// <param name="recipientCerts">Сертификат</param>
        /// <returns>Результат шифровки</returns>
        public static byte[] EncryptMsg(Byte[] msg, X509Certificate2Collection recipientCerts)
        {
            ContentInfo contentInfo = new ContentInfo(msg);
            EnvelopedCms envelopedCms = new EnvelopedCms(contentInfo);
            CmsRecipientCollection recips = new CmsRecipientCollection(SubjectIdentifierType.IssuerAndSerialNumber, recipientCerts);

            envelopedCms.Encrypt(recips);
            return envelopedCms.Encode();
        }

        /// <summary>
        /// Расшифровывает сообщение
        /// </summary>
        /// <param name="encodedEnvelopedCms">Сообщение</param>
        /// <returns>Результат расшифровки</returns>
        public static Byte[] DecryptMsg(byte[] encodedEnvelopedCms)
        {
            EnvelopedCms envelopedCms = new EnvelopedCms();
            envelopedCms.Decode(encodedEnvelopedCms);
            DisplayEnvelopedCms(envelopedCms, false);

            envelopedCms.Decrypt();

            return envelopedCms.ContentInfo.Content;
        }

        /// <summary>
        /// Отображает содержимое CMS
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="envelopedCms"></param>
        private static void DisplayEnvelopedCmsContent(String desc, EnvelopedCms envelopedCms)
        {
            /*Console.WriteLine(desc + " (length {0}):  ", envelopedCms.ContentInfo.Content.Length);
            foreach (byte b in envelopedCms.ContentInfo.Content)
            {
                Console.Write(b.ToString() + " ");
            }
            Console.WriteLine();*/
        }

        /// <summary>
        /// Отображает содержимое CMS
        /// </summary>
        /// <param name="e"></param>
        /// <param name="displayContent"></param>
        private static void DisplayEnvelopedCms(EnvelopedCms e, Boolean displayContent)
        {
            //Console.WriteLine("\nEnveloped PKCS #7 Message Information:");
            //Console.WriteLine("\tThe number of recipients for the Enveloped PKCS #7 " + "is:  {0}", e.RecipientInfos.Count);
            /*for (int i = 0; i < e.RecipientInfos.Count; i++)
            {
                Console.WriteLine("\tRecipient #{0} has type {1}.", i + 1, e.RecipientInfos[i].RecipientIdentifier.Type);
            }*/
            if (displayContent)
            {
                DisplayEnvelopedCmsContent("Enveloped PKCS #7 Content", e);
            }
            //Console.WriteLine();
        }

        /// <summary>
        /// Получает сертификат по отпечатку
        /// </summary>
        /// <param name="thumbPrint">Отпечаток</param>
        /// <returns>Сертификат</returns>
        public static X509Certificate2 GetSignerCert(string thumbPrint)
        {
            thumbPrint = thumbPrint.Replace(" ", "").ToUpper();

            X509Store storeMy = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            storeMy.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certColl = storeMy.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

            if (certColl.Count == 0)
            {
                Console.WriteLine("A suggested certificate to use for this example " + "is not in the certificate store. Select " +
                    "an alternate certificate to use for " + "signing the message.");
            }

            storeMy.Close();

            return certColl[0];
        }

        /// <summary>
        /// Получает массив сертификатов
        /// </summary>
        /// <returns>Массив сертификатов</returns>
        public static List<X509Certificate2> GetCertificates()
        {
            List<X509Certificate2> ret = new List<X509Certificate2>();

            X509Store storeMy = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            storeMy.Open(OpenFlags.ReadOnly);

            foreach (X509Certificate2 cert in storeMy.Certificates)
            {
                ret.Add(cert);
            }

            return ret;
        }
        #endregion

    }
}