using System;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Common;

namespace Core
{
    public class TfsUtil
    {
        private readonly VersionControlServer _vcServer;

        public TfsUtil(string url)
        {
            var tfs = new TfsTeamProjectCollection(new Uri(url), new VssCredentials());
            _vcServer = tfs.GetService<VersionControlServer>();
        }

        //VssConnection vss 
        public int[] GetHistory(string path)
        {
            var z = _vcServer.QueryHistory(path, RecursionType.Full).ToArray();
            


            return z.Select(c => c.ChangesetId).ToArray();
        }

    }
}
