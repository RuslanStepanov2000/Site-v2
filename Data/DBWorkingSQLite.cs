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
                    if ( GetPassword(user.Salt, user.Password) == reader["password"].ToString())
                    {
                        user.Id = reader["id"].ToString();
                        user.Password = "";
                        user.Role = reader["role"].ToString();

                        //генерация временного токена на 10минут
                        var token = GenerateJSONWebToken(user);
                        user.Token = token;
                        var tokenExpTime = DateTime.Now.AddMinutes(10).ToString();

                        //генерация рефреш токена на 7дней
                        var tokenRefresh = GenerateJSONWebToken(user);
                        user.TokenRefresh = tokenRefresh;
                        var tokenRefreshExptime = DateTime.Now.AddDays(7).ToString();

                        //Запись токенов в БД
                        UserTokenSet(user, token, tokenExpTime, tokenRefresh, tokenRefreshExptime);
                        
                    }
                    else user = new User();
                }
                else user = new User();
            }
            connection.Close();
            return user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        /// <param name="tokentype">token, tokenRefresh, token_exp, tokenRefresh_exp</param>
        public void UserTokenSet(User user, string data, string tokentype)
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
            comm.CommandText= "UPDATE userTokens SET "+tokentype+"=@data WHERE id=@id";
            comm.Parameters.AddWithValue("@data", data);
            comm.Parameters.AddWithValue("@id", user.Id);
            comm.ExecuteNonQuery();
            connection.Close();
        }
        //Обновление всех данных для токена при регистрации
        private void UserTokenSet(User user, string token, string tokenExpTime, string tokenRefresh, string tokenRefreshExptime)
        {
            SqliteCommand comm = connection.CreateCommand();
            connection.Open();
            comm.CommandText = "UPDATE userTokens SET token = @token, " +
                "tokenRefresh = @tokenRefresh, " +
                "token_exp=@token_exp, " +
                "tokenRefresh_exp=@tokenRefresh_exp " +
                "WHERE id =@id";
            comm.Parameters.AddWithValue("@token", token);
            comm.Parameters.AddWithValue("@tokenRefresh", tokenRefresh);
            comm.Parameters.AddWithValue("@token_exp", tokenExpTime);
            comm.Parameters.AddWithValue("@tokenRefresh_exp", tokenRefreshExptime);
            comm.Parameters.AddWithValue("@id", user.Id);
            comm.ExecuteNonQuery();
            connection.Close();
        }

        //Получение токена пользовтеля по его Id
        public User UserGetById(User user)
        {
            SqliteCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "select * from userData where id=@id";
            comm.Parameters.AddWithValue("@id", user.Id);

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    user.Id = reader["id"].ToString();
                    user.Email = reader["email"].ToString();
                    user.Role = reader["role"].ToString();
                }
                else return new User();
            }
            connection.Close();
            return user;
        }
        public User UserGetByToken(string token)
        {
            User user = new User();
            string id;
            DateTime date_exp;

            SqliteCommand comm = connection.CreateCommand();

            connection.Open();
            comm.CommandText = "select * from userTokens where token=@token";
            comm.Parameters.AddWithValue("@token", token);

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    id = reader["id"].ToString();
                    date_exp =DateTime.Parse(reader["token_exp"].ToString());
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
                string encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
                string token_return = encodetoken.Substring(encodetoken.LastIndexOf('.')+1);
                return token_return;
            
        }
        public User TokenValidate(User user)
        {
            SqliteCommand comm = connection.CreateCommand();
            DBWorkingSQLite db = new DBWorkingSQLite();

            connection.Open();
            comm.CommandText = "select * from userTokens where token=@token";
            comm.Parameters.AddWithValue("@token", user.Token);
            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    //Прописываем айди пользовтеля для дальнейшей удобной работы
                    user.Id = reader["id"].ToString();

                    //Сверяем его срок действия с базой
                    if (DateTime.Parse(reader["token_exp"].ToString()) > DateTime.Now)
                    {
                        return user;
                    }
                    else
                    {
                        //Сверяем tokenRefresh и его срок дейсвтия с базой и в случае успеха выдаем новый token
                        if (user.TokenRefresh == reader["tokenRefresh"].ToString()
                        &&
                        DateTime.Parse(reader["tokenRefresh_exp"].ToString()) > DateTime.Now)
                        {
                            user = db.UserGetById(user);
                            user.Token = GenerateJSONWebToken(user);
                            db.UserTokenSet(user, user.Token, "token");
                            db.UserTokenSet(user, DateTime.Now.AddMinutes(10).ToString(), "token_exp");
                        }
                        return user;
                    }

                }
                else return new User();
            }
        }

    }
}
