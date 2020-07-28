using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tatneft.Data;

namespace Tatneft.Servises
{
    public interface IUserService
    {
        void UserNew(User user);
        void UserDelete(User user);
        void UserUpdate(User user, string param);
        void UserTokenSet(User user, string token);
        void UserTokenDelete(User user);
        string UserTokenGetById(string id);
        User UserGet(string email, string password);
    }
}
