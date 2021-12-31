using Microsoft.AspNetCore.Http;
using NCU_SE.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NCU_SE.SharedModule
{
    public class sharedModule
    {
        public string getAPIdata(string url)
        {
            //申請的APPID
            //（FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF 為 Guest 帳號，以IP作為API呼叫限制，請替換為註冊的APPID & APPKey）
            const string APPID = "8a8ffddac5af4e42a3fb1ed014472ece";
            //申請的APPKey
            const string APPKey = "cRraTKB4MRKUsfMseiq0UxislXA";

            //取得當下UTC時間
            string xdate = DateTime.Now.ToUniversalTime().ToString("r");
            string SignDate = "x-date: " + xdate;

            //加密簽章產生
            Encoding _encode = Encoding.GetEncoding("utf-8");
            byte[] _byteData = Encoding.GetEncoding("utf-8").GetBytes(SignDate);
            HMACSHA1 _hmac = new(_encode.GetBytes(APPKey));
            using (CryptoStream _cs = new(Stream.Null, _hmac, CryptoStreamMode.Write))
            {
                _cs.Write(_byteData, 0, _byteData.Length);
            }
            //取得加密簽章
            string Signature = Convert.ToBase64String(_hmac.Hash);
            string sAuth = "hmac username=\"" + APPID + "\", algorithm=\"hmac-sha1\", headers=\"x-date\", signature=\"" + Signature + "\"";

            //取得API資料(官方提供方法)
            string json = null;
            using (HttpClient Client = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }))
            {
                Client.DefaultRequestHeaders.Add("Authorization", sAuth);
                Client.DefaultRequestHeaders.Add("x-date", xdate);
                json = Client.GetStringAsync(url).Result;
            }
            return json;
        }
    }
}
