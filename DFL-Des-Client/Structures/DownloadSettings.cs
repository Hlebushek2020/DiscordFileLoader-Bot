using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFL_Des_Client.Structures
{
    public struct DownloadSettings
    {
        public string Folder { get; }
        public bool OpenInIc { get; }
        public bool SearchCollections { get; }

        public DownloadSettings(string folder, bool openInIc, bool searchCollections)
        {
            Folder = folder;
            OpenInIc = openInIc;
            SearchCollections = searchCollections;
        }
    }
}
