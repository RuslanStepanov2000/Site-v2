using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tatneft.Data;

namespace Tatneft.Servises
{
    //Класс взаимодейсвия с пользовтелем
    public class UserService : IUserService
    {
        //Удаление пользователя из базы данных
        public void UserDelete(User user)
        {
            throw new NotImplementedException();
        }
        //Получение токена пользователя по его Id
        public string UserTokenGetById(string id)
        {
            return new DBWorkingSQLite().UserTokenGetById(id);
        }
        //Создание нового пользовтеля
        public void UserNew(User user)
        {
            new DBWorkingSQLite().UserAddNew(user);
        }
        //Добавление токена пользовтеля
        public void UserTokenSet(User user, string token)
        {
            new DBWorkingSQLite().UserTokenSet(user, token);

        }
        //Удаление токена пользователя
        public void UserTokenDelete(User user)
        {
            throw new NotImplementedException();
        }
        //Обновление данных учетной записи пользователя
        public void UserUpdate(User user, string param)
        {
            throw new NotImplementedException();
        }
        //Авторизация и получение класса пользовтеля
        public User UserGet(string email, string password)
        {
            return new DBWorkingSQLite().UserAuth(new User { Email = email, Password = password });
        }
    }
}
