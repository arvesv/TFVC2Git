using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using NodaTime;

namespace Core
{
    public class CheckIn
    {
        public int Id;
        public string commiterName;
        public string committerEmail;
        public string comment;
        public LocalDateTime date;
    }
}
