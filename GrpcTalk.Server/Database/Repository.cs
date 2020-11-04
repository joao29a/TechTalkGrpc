using System.Collections.Generic;
using System.Linq;

namespace GrpcTalk.Server.Database
{
    public class Repository : IRepository
    {
        private readonly IList<UserData> _users;

        public Repository(IList<UserData> users)
        {
            _users = users;
        }

        public void Add(UserData userData)
        {
            _users.Add(userData);
        }

        public List<UserData> GetAll()
        {
            return _users.ToList();
        }

        public UserData GetById(int id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }
    }
}
