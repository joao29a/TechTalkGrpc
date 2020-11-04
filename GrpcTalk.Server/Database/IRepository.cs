using System.Collections.Generic;

namespace GrpcTalk.Server.Database
{
    public interface IRepository
    {
        List<UserData> GetAll();
        UserData GetById(int id);
        void Add(UserData userData);
    }
}
