using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace WebApi.Controllers
{

    public class jsonCommand
    {
        public string command;
    }

    public class UsersController : ApiController
    {
        MySqlConnection conn;
        public async Task<string> SendCommand(string command)
        {
            jsonCommand comm = new jsonCommand();
            comm.command = command;

            string json = new JavaScriptSerializer().Serialize(comm);
            //comm = JsonConvert.DeserializeObject<jsonCommand>(json);
            TcpClient client = new TcpClient();
            try
            {
                await Task.Run(() =>
                {
                    client.StartClient(json);
                });
            }
            catch { }
            return client.responseServer;
        }

        private void connection()
        {
            string conString = ConfigurationManager.ConnectionStrings["conn"].ToString();
            conn = new MySqlConnection(conString);
        }

        public async Task<ActionResult<User>> GetUser(int id)
        {
            User user = new User(); ;
            connection();
            conn.Open();
            string sqlSelect = "Select * from test.users where users.Id='" + id + "';";
            MySqlCommand select = new MySqlCommand(sqlSelect, conn);
            var reader = await select.ExecuteReaderAsync();
            reader.Read();
            while (reader.HasRows)
            {

                user.Name = reader[1].ToString();
                user.Password = reader[2].ToString();
                user.ConfirmPassword = reader[3].ToString();
            }
            conn.Close();
            return user;
        }

        public async Task<ActionResult<Response>> RegisterUser(User user)
        {
            Response response = new Response();
            try
            {
                if (string.IsNullOrEmpty(user.Name))
                {
                    response.Message = "Имя не заполнено";
                    response.Status = 0;
                }
                else if (string.IsNullOrEmpty(user.Password))
                {
                    response.Message = "Пароль не заполнен";
                    response.Status = 0;
                }
                else if (string.IsNullOrEmpty(user.ConfirmPassword))
                {
                    response.Message = "Повторный пароль не заполнен";
                    response.Status = 0;
                }
                else if (user.Password != user.ConfirmPassword)
                {
                    response.Message = "Пароли не совпадают";
                    response.Status = 0;
                }
                else
                {
                    //connection();
                    string s = user.Name + "," + user.Username + "," + user.Password + '$' + "66";

                    string responseData = await SendCommand(s);
                    //conn.Open();
                    //string sqlInsert = "Insert into test.users(users.Name, users.Username, users.Password,users.ConfirmPassword) values('" + user.Name + "', '" + user.Username + "', '" + user.Password + "', '" + user.ConfirmPassword + "');";
                    //MySqlCommand insert = new MySqlCommand(sqlInsert, conn);
                    //int i = await insert.ExecuteNonQueryAsync();
                    //conn.Close();

                    if (responseData != null)
                    {
                        response.Message = "Вы успешно зарегестрировались";
                        response.Status = 1;
                    }
                    else
                    {
                        response.Message = "Ошибка в создании";
                        response.Status = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Status = 0;
            }
            return response;
        }
    }
}