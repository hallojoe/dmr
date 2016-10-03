using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmr
{
    public class Request
    {
        public bool Success { get; set; }
        public int Count { get { return Result == null ? 0 : Result.Count; } }
        public string Token { get; set; }
        public string Message { get; set; } 
        public List<Entity> Result { get; set; }
        
    }

    public class Entity
    {
        public string Path { get; set; }
        public string Slug { get; set; }
        public string Category { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }

}
