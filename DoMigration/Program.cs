using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Dapper;
using Dapper.NodaTime;

namespace DoMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            DapperNodaTimeSetup.Register();
            var connectionString = args[0];

            var connection = new SqlConnection(connectionString);
            connection.Open();


            var areas = connection.Query<dynamic>("select [gitlocalpath], lastchangeset from MigStatus");

            foreach (var area in areas)
            {
                string gitlocal = (string)area.gitlocalpath;

                if (!Directory.Exists(gitlocal))
                    continue;

                int startchangeset = (int) area.lastchangeset;

                var paths = connection.Query<syncpath>(
                        $"select gitrepolocalpath, tfslocalpath, tfspath from MigrationPaths where gitrepolocalpath like '{gitlocal}%'")
                    .ToArray();


                string sqlpaths = "(\'" + string.Join("', '", paths.Select(p => p.tfspath)) + "\')";

                do
                {
                    int noCommitsToBeMerged = connection.QueryFirst<int>($"select count(*) from changesets where changesetid > {startchangeset} and TfsPath in {sqlpaths}");
                    if (noCommitsToBeMerged == 0)
                        break;


                    var commitid = connection.QueryFirst<int>(
                        $"select min(changesetid) from changesets where changesetid > {startchangeset} and TfsPath in {sqlpaths}");


                    var changeset = connection.QueryFirst<CheckIn>(
                        $"select ChangesetId as Id, comment, trim(committer) as committerName, trim(committerEmail),  [CommitterDate] as date from changesets where changesetid  = {commitid}");


                    var tfsPaths = paths.Select(p => p.tfslocalpath).ToArray();
                    var gitPaths = paths.Select(p => p.gitrepolocalpath).ToArray();

                    var tfsRootPath =
                        new string(
                            tfsPaths
                                .First()
                                .Substring(0, tfsPaths.Min(s => s.Length))
                                .TakeWhile(((c, i) => tfsPaths.All(s => s[i] == c)))
                                .ToArray());

                    var gitRootPath =
                        new string(
                            gitPaths
                                .First()
                                .Substring(0, gitPaths.Min(s => s.Length))
                                .TakeWhile(((c, i) => gitPaths.All(s => s[i] == c)))
                                .ToArray());


                    foreach (var path in paths)
                    {
                        UpdateTfsPath(path.tfslocalpath, commitid);

                    }


                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo("cmd",
                            $"/c Robocopy {tfsRootPath} . /MIR /XD .git /XF .gitattributes /XF .gitignore")
                        {
                            UseShellExecute = false,
                            WorkingDirectory = gitRootPath
                        }
                    };
                    proc.Start();
                    proc.WaitForExit();

                    proc = new Process
                    {
                        StartInfo = new ProcessStartInfo("cmd", $"/c git add .")
                        {
                            UseShellExecute = false,
                            WorkingDirectory = gitRootPath
                        }
                    };
                    proc.Start();
                    proc.WaitForExit();

                    if (changeset.committerEmail == null)
                    {
                        changeset.committerEmail = changeset.committerName.ToLower().Replace(' ', '.') + "@unit4.com";
                    }

                    var d = changeset.date.ToString();

                    string commitcli =
                        $"git commit -m '{changeset.comment}' -m 'https://team47system1.corp.u4agr.com/tfs/DefaultCollection/Platform/_versionControl/changeset/{changeset.Id}' --author '{changeset.committerName} <{changeset.committerEmail}>' --date '{d}'";
                    proc = new Process
                    {
                        StartInfo = new ProcessStartInfo(@"pwsh", $"-c {commitcli}")
                        {
                            UseShellExecute = false,
                            WorkingDirectory = gitRootPath
                        }
                    };
                    proc.Start();
                    proc.WaitForExit();
                    startchangeset = commitid;


                    int count = connection.Execute("update MigStatus set lastchangeset = {startchangeset} where [gitlocalpath] = {gitlocal}");



                } while (true);
            }
        }

        private static void UpdateTfsPath(string localtfspath, int commitid)
        {
            const string tfexe =
                @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\tf.exe";

            if (!Directory.Exists(localtfspath))
            {
                Directory.CreateDirectory(localtfspath);
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = tfexe,
                    Arguments = $"vc get . /version:C{commitid} /recursive",
                    UseShellExecute = false,
                    WorkingDirectory = localtfspath
                }
            };
            proc.Start();
            proc.WaitForExit();

            



            
        }
    }
}
