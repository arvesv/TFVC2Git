using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Core;
using System.IO;

namespace Core
{
    public static class Stuff
    {
        public static Policy GetDbRetryPolicy()
        {

            // A retry policy that hopefully works if the database is stopped  (I am using an Azure SQL server stateless to save cost. It make take 1 minute to auto start)
            return Policy
                .Handle<SqlException>(ex =>
                {
                    return ex.Message.Contains("not currently available");
                })
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(100),
                    TimeSpan.FromSeconds(100),
                    TimeSpan.FromSeconds(60)
                });
        }

        private static string _tfpath;

        public static string GetTFEXEPath()
        {
            if (_tfpath == null) {

                var dirs = Directory.GetDirectories(@"C:\Program Files (x86)\Microsoft Visual Studio\2019");

                _tfpath = Path.Combine(
                    @"C:\Program Files (x86)\Microsoft Visual Studio\2019",
                    dirs[0],
                    @"Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\tf.exe");

                if (!File.Exists(_tfpath))
                {
                    Console.WriteLine("TF.exe from VS 2019 not found\n");
                    throw new Exception();
                }
            }
            return _tfpath;
        }
    }
}
