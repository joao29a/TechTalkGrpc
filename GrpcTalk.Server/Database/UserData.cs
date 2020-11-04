using System.Collections.Generic;

namespace GrpcTalk.Server.Database
{
    public class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Emails { get; set; }
    }
}
