using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Core;


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
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(30)
                });

        }
    }
}
