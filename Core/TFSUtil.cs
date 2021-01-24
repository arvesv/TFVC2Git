using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Common;
using NodaTime;

namespace Core
{
    public class TfsUtil
    {
        private readonly VersionControlServer _vcServer;
        private readonly IIdentityManagementService _managementService;

        public TfsUtil(string url)
        {
            var tfs = new TfsTeamProjectCollection(new Uri(url), new VssCredentials());
            _managementService = tfs.GetService<IIdentityManagementService>();
            _vcServer = tfs.GetService<VersionControlServer>();

            _emaillookup = new Dictionary<string, string>();
        }

        private readonly Dictionary<string, string> _emaillookup;

        string GetUserEmail(string userid, string backupName)
        {
            if (!_emaillookup.ContainsKey(userid))
            {
                var member = _managementService.ReadIdentity(IdentitySearchFactor.AccountName, userid,
                    MembershipQuery.Direct, ReadIdentityOptions.ExtendedProperties);


                string email;

                if (member == null)
                {
                    email = backupName.ToLower().Replace(' ', '.') + "@unit4.com";
                }
                else
                {
                    email = member.GetProperty("Mail").ToString().ToLower();
                }

                _emaillookup[userid] = email.ToString().ToLower();
            }

            return _emaillookup[userid];
                
        }


        //VssConnection vss 
        public IEnumerable<CheckIn> GetHistory(string path)
        {
            var z = _vcServer.QueryHistory(path, RecursionType.Full);

            foreach (var changeset  in z)
            {

                yield return  new CheckIn
                {
                    Id = changeset.ChangesetId, 
                    comment = changeset.Comment,
                    date = LocalDateTime.FromDateTime(changeset.CreationDate),
                    commiterName = changeset.CommitterDisplayName, 
                    committerEmail = GetUserEmail(changeset.Committer, changeset.CommitterDisplayName)
                };
            }
        }

    }
}
