using System.Collections.Generic;
using System.Dynamic;

namespace SSS.CommonLib
{
    public class DapperHelpers
    {
        public static dynamic ToExpandoObject(object value)
        {
            var dapperRowProperties = value as IDictionary<string, object>;

            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var (key, o) in dapperRowProperties)
                expando.Add(key, o);

            return expando;
        }
    }
}