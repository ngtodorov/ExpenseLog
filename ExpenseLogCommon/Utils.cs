using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ExpenseLogCommon
{
    public class Utils
    {
        public string Encrypt(string text)
        {
            string ecryptionKey = GetAppSetting("EL_ENCRYP_DECRYPT_KEY");
            byte[] SrctArray;
            byte[] EnctArray = UTF8Encoding.UTF8.GetBytes(text);
            SrctArray = UTF8Encoding.UTF8.GetBytes(ecryptionKey);
            TripleDESCryptoServiceProvider objt = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider objcrpt = new MD5CryptoServiceProvider();
            SrctArray = objcrpt.ComputeHash(UTF8Encoding.UTF8.GetBytes(ecryptionKey));
            objcrpt.Clear();
            objt.Key = SrctArray;
            objt.Mode = CipherMode.ECB;
            objt.Padding = PaddingMode.PKCS7;
            ICryptoTransform crptotrns = objt.CreateEncryptor();
            byte[] resArray = crptotrns.TransformFinalBlock(EnctArray, 0, EnctArray.Length);
            objt.Clear();
            return Convert.ToBase64String(resArray, 0, resArray.Length);
        }

        public string Decrypt(string encryptedText)
        {
            string decryptionKey = GetAppSetting("EL_ENCRYP_DECRYPT_KEY");
            byte[] SrctArray;
            byte[] DrctArray = Convert.FromBase64String(encryptedText);
            SrctArray = UTF8Encoding.UTF8.GetBytes(decryptionKey);
            TripleDESCryptoServiceProvider objt = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider objmdcript = new MD5CryptoServiceProvider();
            SrctArray = objmdcript.ComputeHash(UTF8Encoding.UTF8.GetBytes(decryptionKey));
            objmdcript.Clear();
            objt.Key = SrctArray;
            objt.Mode = CipherMode.ECB;
            objt.Padding = PaddingMode.PKCS7;
            ICryptoTransform crptotrns = objt.CreateDecryptor();
            byte[] resArray = crptotrns.TransformFinalBlock(DrctArray, 0, DrctArray.Length);
            objt.Clear();
            return UTF8Encoding.UTF8.GetString(resArray);
        }

        /// <summary>
        /// If the application is running on the local / developer machine, 
        /// then 1st it will try to take the parameter from the machine Environment Parameters/Settings
        /// Just open common prompt (cmd) and type SET
        /// 
        /// If the application is running on Azure, 
        /// then 1st it will try to take the parameter from the Azure Application Settings
        /// 
        /// If the parameter is still not found, then it will try to take it from the Web.config Application Settings
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetAppSetting(string settingName, string defaultValue="")
        {
            string result = String.Empty;
            
            result = System.Environment.GetEnvironmentVariable(settingName);

            if (String.IsNullOrEmpty(result))
            {
                if (System.Configuration.ConfigurationManager.AppSettings[settingName] != null)
                {
                    result = System.Configuration.ConfigurationManager.AppSettings[settingName].Trim();
                }
            }

            if (String.IsNullOrEmpty(result))
                result = defaultValue;

            if (result == String.Empty)
                throw new Exception($"Missing required Application Setting: '{settingName}'. Check Azure Application Settings, Environment Settings or Web.config.");

            return result;
        }
    }
}
