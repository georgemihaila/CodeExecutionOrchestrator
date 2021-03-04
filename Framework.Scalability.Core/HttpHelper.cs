using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Scalability.Core
{
    public class HttpHelper
    {
        private readonly string _endpoint;
        public HttpHelper(string endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<string> GETRequestAsync(string path)
        {
            var request = (HttpWebRequest)WebRequest.Create(_endpoint + path);
            request.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
            request.Method = "GET";
            var response = (HttpWebResponse)await request.GetResponseAsync();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task POSTSimpleAsync(string path)
        {
            var request = (HttpWebRequest)WebRequest.Create(_endpoint + path);
            request.Method = "POST";
#if DEBUG
            request.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#endif
            var response = (HttpWebResponse)await request.GetResponseAsync();
        }

        public async Task<TOut> POSTRequestAsync<TOut>(string path, object content = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(_endpoint + path);
#if DEBUG
            request.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#endif
            request.Method = "POST";
            request.Headers[HttpRequestHeader.ContentType] = "application/json";
            if (content != null)
            {
                using (var stream = await request.GetRequestStreamAsync())
                {
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
            }
            var response = (HttpWebResponse)await request.GetResponseAsync();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return JsonConvert.DeserializeObject<TOut>(reader.ReadToEnd());
            }
        }

        public async Task POSTObjectAsync<T>(string path, T source)
        {
            var request = (HttpWebRequest)WebRequest.Create(_endpoint + path);
#if DEBUG
            request.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#endif
            request.Method = "POST";
            request.Headers[HttpRequestHeader.ContentType] = "application/json";
            if (source != null)
            {
                using (var stream = await request.GetRequestStreamAsync())
                {
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(source));
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
            }
            var response = (HttpWebResponse)await request.GetResponseAsync();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                reader.ReadToEnd();
            }
        }
    }
}