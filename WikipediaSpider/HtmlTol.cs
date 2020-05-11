using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WikipediaSpider
{
    public static class HtmlTol
    {
        /// <summary>
        /// 获取Html元素属性
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAttributes(this HtmlAttributeCollection attribute, string name)
        {
            return attribute.ToList().Where(m => m.Name == name).FirstOrDefault().Value;
        }

        /// <summary>
        /// Http下载
        /// </summary>
        /// <param name="HttpFileUrl"></param>
        /// <param name="FSavePath"></param>
        /// <returns></returns>
        public static string HttpDownLoadFile(string HttpFileUrl, string FSavePath, string SaveFileName)
        {
            HttpFileUrl = HttpFileUrl.Replace("\\", "/");
            var request = WebRequest.Create(HttpFileUrl);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/octet-stream";
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            var directPath = FSavePath;
            if (!Directory.Exists(directPath))
            {
                Directory.CreateDirectory(directPath);
            }

            string filePath = directPath + "/" + SaveFileName;
            var fileStream = new FileStream(filePath, FileMode.CreateNew);

            var bytes = new byte[2048];
            var count = stream.Read(bytes, 0, bytes.Length);
            while (count > 0)
            {
                fileStream.Write(bytes, 0, count);
                count = stream.Read(bytes, 0, bytes.Length);
            }
            stream.Close();
            fileStream.Close();

            return filePath;
        }
    }
}
