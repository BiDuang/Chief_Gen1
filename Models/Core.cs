using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chief.Models
{
    public class Core
    {
        public class CommitInfo
        {
            public string Version { get; set; }
            public string ID { get; set; }
            public string RelaeseMessage { get; set; }
            public DateTime ReleaseDateTime { get; set; }
            public string Author { get; set; }
        }
    }
}
