using System;

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

        public class TinyCommitInfo
        {
            public string Version { get; set; }
        }
    }
}
