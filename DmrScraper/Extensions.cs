using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dmr
{
    public static class RequestExtensions
    {
        public static string ToJson(this Request model)
        {
            var formatting = Formatting.Indented;
            var settings = new JsonSerializerSettings();
            var json = JsonConvert.SerializeObject(model, formatting, settings);
            return json;
        }
    }
}
