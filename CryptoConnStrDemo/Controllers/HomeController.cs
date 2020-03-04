using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Dapper;
using MySql.Data.MySqlClient;

namespace CryptoConnStrDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.ConnStr = GetConnStr();
            ViewBag.Encrypt = GetEncrypt();
            ViewBag.Version = GetVersion();
            return View();
        }

        private string GetEncrypt()
        {
            var hashKey = ConfigurationManager.AppSettings["hashKey"];
            var iv = ConfigurationManager.AppSettings["iv"];
            return Encryption(GetConnStr(), hashKey, iv);
        }

        private string GetConnStr()
        {
            const string connStr =
                "server=172.21.13.48;port=4006;user id=apadm;password=Aa123456;database=moneyin;charset=utf8;ConnectionTimeout=3;";
            return connStr;
        }

        private string GetVersion()
        {
            var connectionString = GetConnectionString("db");
            var commandText = "select @@version";
            using (var cnn = new MySqlConnection(connectionString))
            {
                return cnn.Query<string>(commandText).FirstOrDefault();
            }
        }

        public string GetConnectionString(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            var source = connectionString.ConnectionString;
            if (connectionString.LockItem)
            {
                var hashKey = ConfigurationManager.AppSettings["hashKey"];
                var iv = ConfigurationManager.AppSettings["iv"];
                source = Decryption(source, hashKey, iv);
            }
            return source;
        }

        public string Decryption(string CipherText,string hashKey,string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.Unicode.GetBytes(hashKey);
                aesAlg.IV = Encoding.Unicode.GetBytes(iv);
                ICryptoTransform decrypt = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] decrypted = decrypt.TransformFinalBlock(Convert.FromBase64String(CipherText), 0, Convert.FromBase64String(CipherText).Length);
                return Encoding.Unicode.GetString(decrypted);
            }
        }

        public string Encryption(string PlainText,string hashKey, string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.Unicode.GetBytes(hashKey);
                aesAlg.IV = Encoding.Unicode.GetBytes(iv);
                ICryptoTransform encrypt = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] encrypted = encrypt.TransformFinalBlock(Encoding.Unicode.GetBytes(PlainText), 0,
                    Encoding.Unicode.GetBytes(PlainText).Length);
                return Convert.ToBase64String(encrypted);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}