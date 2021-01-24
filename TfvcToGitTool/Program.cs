﻿using System;
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

            var tfs = new TfsUtil(tfsUrl);


            var z = tfs.GetHistory(path);

            foreach (var qE in z)
            {
                Console.WriteLine(qE.Id);
                Console.WriteLine(qE.comment);
                Console.WriteLine(qE.committerEmail);
                Console.WriteLine(qE.commiterName);

            }

        }
    }
}
