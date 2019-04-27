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
            return EncryptDecrypt(text, GetAppSetting("EL_ENCRYP_DECRYPT_KEY"), true);
        }

        public string Decrypt(string encryptedText)
        {
            return EncryptDecrypt(encryptedText, GetAppSetting("EL_ENCRYP_DECRYPT_KEY"), false);
        }

        public string EncryptDecrypt(string text, string key, bool doEncrypt)
        {
            try
            {
                using (TripleDESCryptoServiceProvider tripleDESCrServ = new TripleDESCryptoServiceProvider())
                {
                    using (MD5CryptoServiceProvider md5CrServ = new MD5CryptoServiceProvider())
                    {
                        byte[] keyArray = md5CrServ.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                        md5CrServ.Clear();
                        tripleDESCrServ.Key = keyArray;
                        tripleDESCrServ.Mode = CipherMode.ECB;
                        tripleDESCrServ.Padding = PaddingMode.PKCS7;

                        ICryptoTransform cryptoTransform;

                        byte[] inputArray;
                        byte[] resultArray;

                        if (doEncrypt)
                        {
                            cryptoTransform = tripleDESCrServ.CreateEncryptor();
                            inputArray = UTF8Encoding.UTF8.GetBytes(text);
                            resultArray = cryptoTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                            tripleDESCrServ.Clear();
                            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
                        }
                        else
                        {
                            cryptoTransform = tripleDESCrServ.CreateDecryptor();
                            inputArray = Convert.FromBase64String(text);
                            resultArray = cryptoTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                            tripleDESCrServ.Clear();
                            return UTF8Encoding.UTF8.GetString(resultArray);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Encrypt/Decrypt failed. {ex.GetBaseException().Message}");
            }
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
