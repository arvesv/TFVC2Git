
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Core
{
    public class TFSUtil
    {
        private TfsTeamProjectCollection tfs;
        private VersionControlServer vcServer;

        public TFSUtil(string url)
        {
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(url), new TfsClientCredentials());

            vcServer = tfs.GetService<VersionControlServer>();
        }

        //VssConnection vss 
        public int[] GetHistory(string path)
        {
            var z = vcServer.QueryHistory(path, RecursionType.Full).ToArray();
            


            return z.Select(c => c.ChangesetId).ToArray();
        }

    }
}
