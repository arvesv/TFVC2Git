using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Core;

namespace TfvcToGitTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string tfsUrl = args[0];
            string path = args[1];

            var tfs = new TFSUtil(tfsUrl);


            var z = tfs.GetHistory(path);

        }
    }
}
