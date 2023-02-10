using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SSS.AspNetCore.Extensions
{
    public static class HttpExtension
    {
        public static StringContent ObjToHttpContent(this object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        public static async Task<TValue> ReadAsAsync<TValue>(this HttpResponseMessage httpResponse)
        {
            var json = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TValue>(json);
        }

        public static string BuildRelativeUri(this string root, NameValueCollection query)
        {
            var collection = HttpUtility.ParseQueryString(string.Empty);

            foreach (var key in query.Cast<string>().Where(key => !string.IsNullOrEmpty(query[key])))
            {
                collection[key] = query[key];
            }

            if (root.Contains('?'))
            {
                if (root.EndsWith("&"))
                {
                    root += collection.ToString();
                }
                else
                {
                    root = $"{root}&{collection}";
                }
            }
            else
            {
                root = $"{root}?{collection}";
            }

            return root;
        }

        public static bool TransientHttpStatusCodePredicate(this HttpStatusCode statusCode)
            => statusCode >= HttpStatusCode.NotImplemented || statusCode == HttpStatusCode.RequestTimeout || statusCode == HttpStatusCode.InternalServerError;
    }
}
