using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tatneft.Data;

namespace Tatneft.Servises
{
    public class UserService : IUserService
    {
        public void UserDelete(User user)
        {
            throw new NotImplementedException();
        }

        public string UserTokenGetById(string id)
        {
            return new DBWorkingSQLite().UserTokenGetById(id);
        }

        public void UserNew(User user)
        {
            new DBWorkingSQLite().UserAddNew(user);
        }

        public void UserTokenSet(User user, string token)
        {
            new DBWorkingSQLite().UserTokenSet(user, token);

        }

        public void UserTokenDelete(User user)
        {
            throw new NotImplementedException();
        }

        public void UserUpdate(User user, string param)
        {
            throw new NotImplementedException();
        }

        public User UserGet(string email, string password)
        {
            return new DBWorkingSQLite().UserAuth(new User { Email = email, Password = password });
        }
    }
}
