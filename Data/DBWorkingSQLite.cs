using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;


namespace Tatneft.Data
{
    class DBWorkingSQLite
    {

        //Создание подключения к БД
        private static SqliteConnection connection=new SqliteConnection("Data Source=UsersDb.db");

        //Получение пароля из хэша и соли из БД
        private string GetPassword(string salt, string password)
        {
            byte[] encoding = ASCIIEncoding.ASCII.GetBytes(salt + password);
            byte[] hmscsha256 = new HMACSHA256(encoding).ComputeHash(encoding);
            password = Convert.ToBase64String(hmscsha256);
            return password;
        }

        //Генерация соли при создании нового пользовтеля
        private string SaltGenerator()
        {
            //Случайная генерация соли
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            var salt_bytes = new byte[32];
            rngCsp.GetBytes(salt_bytes);
            string salt = Convert.ToBase64String(salt_bytes);

            return salt;
        }

        //Добавление нового пользовтеля в БД
        public void UserAddNew(User user)
        {
            SqliteCommand comm = connection.CreateCommand();
        
            string salt = SaltGenerator();
            user.Password = GetPassword(salt, user.Password);

            try
            { 
                connection.Open();
                //заполнение таблицы данных пользовтеля
                comm.CommandText = "INSERT INTO userData (email,password,salt) VALUES(@email, @password, @salt)";
                comm.Parameters.AddWithValue("@email", user.Email);
                comm.Parameters.AddWithValue("@password", user.Password);
                comm.Parameters.AddWithValue("@salt", salt);
                comm.ExecuteNonQuery();

                //Получение Id нового пользовтеля и создание записи в таблице токенов 
                comm.CommandText = "select * from userData where email=@email";
                comm.Parameters.AddWithValue("@email", user.Email);

                using (var reader = comm.ExecuteReader())
                {
                   reader.Read();
                   user.Id = reader["id"].ToString();
                }
                comm.CommandText = "INSERT into userTokens (id=@id)";
                comm.Parameters.AddWithValue("@id", user.Id);
                comm.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        //Авторизация пользователя и получение его параметров, запись его нового токена в базу.
        public User UserAuth(User user)
        {
            SqliteCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "select * from userData where email=@email";
            comm.Parameters.AddWithValue("@email", user.Email);

            // Возвращает одну строку, найденную по уникальному логину. Формат - массив из {id, email, password, role, salt}. Если пользователь не будет найден
            // возвращается пустой класс пользовтеля

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    user.Salt = reader["salt"].ToString();
                    //Проверяем пароль. В случае успеха выдаем токен. Если пароль не правильный возвращается пустой класс пользователя
                    
                    //string str1 = GetPassword(user.Salt, user.Password);
                    //string str2 = reader["password"].ToString();
                    if ( GetPassword(user.Salt, user.Password) == reader["password"].ToString())
                    {
                        user.Id = reader["id"].ToString();
                        user.Password = "";
                        user.Role = reader["role"].ToString();

                        var token = GenerateJSONWebToken(user);
                        user.Token = token;
                        UserTokenSet(user, token);
                        
                    }
                    else user = new User();
                }
                else user = new User();
            }
            connection.Close();
            return user;
        }
        
        //Добавление токена пользователя
        public void UserTokenSet(User user, string token)
        {
            SqliteCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "select * from userData where email=@email";
            comm.Parameters.AddWithValue("@email", user.Email);

            using (var reader = comm.ExecuteReader())
            {
                reader.Read();
                user.Id = reader["id"].ToString();
            }
            comm.CommandText= "UPDATE userTokens SET token=@token WHERE id=@id";
            //comm.CommandText = "insert into userToken (token) values(@token) where id=@id";
            comm.Parameters.AddWithValue("@token", token);
            comm.Parameters.AddWithValue("@id", user.Id);
            comm.ExecuteNonQuery();
            connection.Close();
        }

        //Получение токена пользовтеля по его Id
        public string UserTokenGetById(string id)
        {
            string token;
            SqliteCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "select * from userTokens where id=@id";
            comm.Parameters.AddWithValue("@id", id);

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                     token = reader["token"].ToString();
                }
                else return "token";
            }
            connection.Close();
            return token;
        }
        public User UserGetByToken(string token)
        {
            User user = new User();
            string id;

            SqliteCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "select * from userTokens where token=@token";
            comm.Parameters.AddWithValue("@token", token);

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    id = reader["id"].ToString();
                }
                else return null;              
            }

            comm.CommandText = "select * from userData where id=@id";
            comm.Parameters.AddWithValue("@id", id);
            using (var reader = comm.ExecuteReader())
            {
                reader.Read();
                user.Email = reader["email"].ToString();
                user.Role = reader["role"].ToString();
                user.Token = token;    
            }
           
            connection.Close();
            return user;
        }
        //Создание токена
        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Jwt:Keyqwertyuytrewertyuiqwe"));
            var credintalis = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            if (user.Email != null)
            {
                var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
                 };
                var token = new JwtSecurityToken(
                    issuer: "Jwt:Issuer",
                    audience: "Jwt:Issuer",
                    claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: credintalis);
                var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
                return encodetoken;
            }
            else
            {
                return null;
            }


        }
        
    }
}
