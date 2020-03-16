﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace IT.WebServices.MISC
{
    public class HashPassword
    {
        public static string GetHashCode(string input)
        {
            try
            {

                if (input != null)
                {

                    StringBuilder hash = new StringBuilder();
                    MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
                    byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        hash.Append(bytes[i].ToString("x2"));
                    }
                    return hash.ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}