using System;
using System.Data.SqlClient;
using Core;
using Core.Model;
using Dapper;
using Dapper.NodaTime;
using Polly;

namespace TfvcToGitTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DapperNodaTimeSetup.Register();
            var connectionString = args[0];

            // A retry policy that hopefully works if the database is stopped  (it make take 1 minute to autostart)
            var policy = Policy
                .Handle<SqlException>(ex => ex.Message.Contains("not currently available"))
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(100),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(30)
                });

          
            var connection = new SqlConnection(connectionString);

            policy.Execute(() => connection.Open());

            var syncpaths = connection.Query<SyncPath>(
                "SELECT [TfsUrl], [TfsPath], [LastChangeSetId] FROM [TfsPathsToSync]");

            foreach (var syncpath in syncpaths)
            {
                Console.WriteLine($"Processing {syncpath.TfsPath}");
                var tfs = new TfsUtil(syncpath.TfsUrl);
                var history = tfs.GetHistory(syncpath.TfsPath);

                int newLastChangesetId = 0;

                foreach (var changeset in history)
                {
                    newLastChangesetId = Math.Max(newLastChangesetId, changeset.Id);

                    if (changeset.Id > syncpath.LastChangeSetId)
                    {
                        var count = connection.QuerySingle<int>(
                            "select count(*) from Changesets where TfsPath = @TfsPath and ChangeSetId = @ChangesetId",
                            new
                            {
                                // ReSharper disable once RedundantAnonymousTypePropertyName
                                TfsPath = syncpath.TfsPath,
                                ChangesetId = changeset.Id
                            });

                        if (count == 0)
                        {
                            connection.Execute(
                                @"insert into Changesets (TfsPath, ChangesetId, Comment, Committer, CommitterEmail, CommitterDate) values 
                            (@TfsPath, @ChangesetId, @Comment, @Committer, @CommitterEmail, @CommitterDate) ",
                                new[]
                                {
                                    new
                                    {
                                        syncpath.TfsPath,
                                        ChangesetId = changeset.Id,
                                        Comment = changeset.comment,
                                        Committer = changeset.committerName,
                                        CommitterEmail = changeset.committerEmail,
                                        CommitterDate = changeset.date
                                    }
                                });
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (syncpath.LastChangeSetId != newLastChangesetId)
                {
                    int rows = connection.Execute(
                        @"update TfsPathsToSync set LastChangeSetId = @newMaxId where TfsUrl =  @tfsUrl and TfsPath = @tfsPath",
                        new
                        {
                            newMaxId = newLastChangesetId,
                            tfsUrl = syncpath.TfsUrl,
                            tfsPath = syncpath.TfsPath
                        });

                    if (rows != 1)
                    {
                        Console.WriteLine($"Error updateing LastChangeSetId for {syncpath.TfsPath}");
                    }

                    Console.WriteLine($"Updated {syncpath.TfsPath} up to commit {newLastChangesetId}");
                }
            }
        }
    }
}