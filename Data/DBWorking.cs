
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using System.Data.Common;
using System.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.Swagger;
using System.Text;

namespace Tatneft.Data
{
    public class DBWorking
    {
        public User Get(int id)
        {
            return new User();
        }

        public MySqlConnection GetDBConnection()
        {
            string host = "localhost";
            int port = 3306;
            string database = "users";
            string username = "root";
            string password = "Ruslan2411";
            // Connection String.
            String connString = "Server=" + host + ";Database=" + database
                + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }
        /// <summary>
        /// Добавление пользователя в БД
        /// </summary>

        public void NewUserPush(User user, string salt)
        {
            MySqlConnection connection = GetDBConnection();
            MySqlCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "INSERT INTO data (login,password,salt) VALUES(@login, @password, @salt)";
            comm.Parameters.AddWithValue("@login", user.Login);
            comm.Parameters.AddWithValue("@password", user.Password);
            comm.Parameters.AddWithValue("@salt", salt);
            comm.ExecuteNonQuery();
            connection.Close();

        }

        private string SaltGenerator()
        {
            //Случайная генерация соли
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            var salt_bytes = new byte[32];
            rngCsp.GetBytes(salt_bytes);
            string salt = Convert.ToBase64String(salt_bytes);

            return salt;
        }

        //Регистрация: пользователь вводит логин и пароль, создается соль 
        public void NewUser(User user)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            DBWorking dbw = new DBWorking();

            //генерируем соль
            string salt = dbw.SaltGenerator();
            //шифрование пароля и запись в базу
            user.Password = dbw.GetPassword(salt, user.Password);
            dbw.NewUserPush(user, salt);


        }

        //Авторизация пользователя
        public User Auth(User user)
        {
            string login = "";
            string password = "";
            string salt = "";
            string role = "";

            //Подключение к sql
            MySqlConnection connection = GetDBConnection();
            MySqlCommand comm = connection.CreateCommand();
            connection.Open();

            comm.CommandText = "select * from users.data where login=@login";
            comm.Parameters.AddWithValue("@login", "ruslan2");
            // Возвращает одну строку, найденную по уникальному логину. Формат - массив из {login, password, salt, role}. Если пользователь не будет найден
            // возвращается пустой класс пользовтеля
            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    password = reader["password"].ToString();
                    salt = reader["salt"].ToString();
                    
                    //Проверяем пароль, взяв соль из базы. В случае успеха выдаем роль и токен. Если пароль не правильный возвращается пустой класс пользователя
                    if (password == GetPassword(salt, user.Password))
                    {
                        user.Role = reader["role"].ToString();
                    }
                    else user = new User();
                }
                else user = new User();
            }
            connection.Close();
            return user;
        }

        public string GetPassword(string salt, string password)
        {
            byte[] encoding = ASCIIEncoding.ASCII.GetBytes(salt + password);
            byte[] hmscsha256 = new HMACSHA256(encoding).ComputeHash(encoding);
            password = Convert.ToBase64String(hmscsha256);
            return password;
        }
    }
}

