using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using static DevExpress.Mvvm.Native.TaskLinq;

namespace TCPChat.Server
{
    internal class sqlSeverClass
    {
        public static string connectionString;
        public static SqlDataAdapter adapter;

        public static DataTable usersTable;

        public static SqlConnection connection;
        public sqlSeverClass()
        {
            connectionString = "Data Source=94.241.175.205,1433;Initial Catalog=Messenger;Integrated Security=False;User Id=SA;Password=TheB3stPassw0rdF0rDB;";

            string sql = "SELECT * FROM Users";

            usersTable = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(usersTable);
            connection.Close();
        }
        public bool Autorization(string username, string password)
        {
            connectionString = "Data Source=94.241.175.205,1433;Initial Catalog=Messenger;Integrated Security=False;User Id=SA;Password=TheB3stPassw0rdF0rDB;";

            string sql = "SELECT * FROM Users";

            DataTable UsersTable = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(UsersTable);
            connection.Close();
            //App.server = new ClientServer();
            //Когда будет подрублена БД чекать всё
            var a = UsersTable.Select().AsEnumerable().Where(p => p["Login"].ToString() == username && p["Password"].ToString() == password).ToList();
            if (username != "" && password != "")
            {
                if (a.Count > 0)
                {
                    return true;
                    //App.chatPage = new ChatPage(a.First()["UserID"].ToString());
                    //mainWindow.frameMenu.Navigate(App.chatPage);
                    //App.server.Nick = a.First()["UserID"].ToString();

                    //App.server.ConnectCommand.ExecuteAsync(this);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public string AutoUserId(string username, string password)
        {
            string sql = "SELECT * FROM Users";

            DataTable UsersTable = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(UsersTable);
            connection.Close();
            var a = UsersTable.Select().AsEnumerable().Where(p => p["Login"].ToString() == username && p["Password"].ToString() == password).ToList();
            return a.First()["UserID"].ToString();
        }
        public bool Registration(string username, string password, string passwordConf)
        {
            connectionString = "Data Source=94.241.175.205,1433;Initial Catalog=Messenger;Integrated Security=False;User Id=SA;Password=TheB3stPassw0rdF0rDB;";

            string sql = "SELECT * FROM Users";

            DataTable UsersTable = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(UsersTable);
            connection.Close();
            Regex eng = new Regex(@"^[A-Za-z\d_-]+$");
            var logins = UsersTable.Select().AsEnumerable().Where(x => x["Login"].ToString() == username).ToList();

            if (username != "" && !username.Contains(' '))
            {
                if (logins.Count() == 0)
                {
                    if (password.Length < 6 | !(eng.Match(password).Success) | !(Regex.Match(password, @"\d").Success))
                    {
                        return false;
                        //MessageBox.Show("Пароль должен содержать 6 или более символов\nДолжен иметь хотя бы одну цифру\nДолжен содержать английские символы");
                    }
                    else
                    {
                        if (password == passwordConf)
                        {
                            return true;
                            //App.mainPage.Login.Background = new SolidColorBrush(Color.FromRgb(22, 49, 72));
                            //App.mainPage.Signin.Background = new SolidColorBrush(Color.FromRgb(31, 71, 104));
                            //App.mainPage.frameLogin.Navigate(App.loginPage);
                        }
                        else
                        {
                            return false;
                            //MessageBox.Show("Пароли не совпадают");
                        }

                    }

                }
                else
                {
                    return false;
                    //MessageBox.Show("Такой логин уже существует");
                }
            }
            else
            {
                return false;
            }
        }
        public string RegistrationConfirm(string username, string password, string passwordConf)
        {
            connectionString = "Data Source=94.241.175.205,1433;Initial Catalog=Messenger;Integrated Security=False;User Id=SA;Password=TheB3stPassw0rdF0rDB;";

            string sql = "SELECT * FROM Users";

            DataTable UsersTable = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(UsersTable);
            connection.Close();
            Regex eng = new Regex(@"^[A-Za-z\d_-]+$");
            var logins = UsersTable.Select().AsEnumerable().Where(x => x["Login"].ToString() == username).ToList();

            if (username != "" && !username.Contains(' '))
            {
                if (logins.Count() == 0)
                {
                    if (password.Length < 6 | !(eng.Match(password).Success) | !(Regex.Match(password, @"\d").Success))
                    {
                        return "Пароль должен содержать 6 или более символов.Должен иметь хотя бы одну цифру.Должен содержать английские символы";
                    }
                    else
                    {
                        if (password == passwordConf)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Пароли не совпадают";
                        }

                    }

                }
                else
                {
                    return "Такой логин уже существует";
                }
            }
            else
            {
                return "Логин не должен быть пустым и содержать пароли";
            }
        }
        public bool IsAvatar(string UserLogin)
        {
            string sql = $"SELECT AvatarCode FROM Users u WHERE u.Login='{UserLogin}'";
            var AvatarCode = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);

            connection.Open();
            adapter.Fill(AvatarCode);
            connection.Close();
            return AvatarCode.AsEnumerable().Select(x => x["AvatarCode"]).FirstOrDefault() != null;
        }
        public string GetAvatar(string UserLogin)
        {
            string sql = $"SELECT AvatarCode FROM Users u WHERE u.Login='{UserLogin}'";
            var AvatarCode = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(AvatarCode);
            connection.Close();
            return AvatarCode.AsEnumerable().Select(x => x["AvatarCode"]).FirstOrDefault().ToString();
        }
        public int GetUserID(string UserLogin)
        {
            string sql = $"SELECT UserID FROM Users u WHERE u.Login='{UserLogin}'";
            var AvatarCode = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(AvatarCode);
            connection.Close();
            return Convert.ToInt32(AvatarCode.AsEnumerable().Select(x => x["UserID"]).FirstOrDefault());
        }
        public string GetChatsJSON(string UserLogin)
        {
            string sql = $"SELECT ChatName, Chat.ChatID as [chatID], SUM(CAST(msg.Status AS INT)) as [unrMessage] FROM Chat chat LEFT JOIN userNchat unc ON unc.ChatID = chat.ChatID LEFT JOIN Users userus ON unc.UserID = userus.UserID  LEFT JOIN Message msg ON msg.ChatID = chat.ChatID AND msg.SenderID!=userus.UserID WHERE userus.Login='{UserLogin}' GROUP BY ChatName, Chat.ChatID";
            var UserChatsNames = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(UserChatsNames);
            connection.Close();
            return JsonConvert.SerializeObject(UserChatsNames.AsEnumerable().Select(x =>new { ChatName=x["ChatName"], ChatID = x["chatID"], UnReMessage = x["unrMessage"] }).ToList());
        }
        public string GetMessagesJSON(int ChatID)
        {
            string sql = $"SELECT chat.ChatID as [chatID], msg.MessageContent as [msgCont], usr.Login as [login], msg.DateTime as [datetime] FROM Chat chat LEFT JOIN Message msg ON msg.ChatID = chat.ChatID LEFT JOIN Users usr ON msg.SenderID=usr.UserID WHERE chat.ChatID={ChatID}";
            var ChatMessages = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(ChatMessages);
            connection.Close();
            return JsonConvert.SerializeObject(ChatMessages.AsEnumerable().Select(x => new { chatID = x["chatID"], msgCont = x["msgCont"], login = x["login"], datetime = x["datetime"] }).ToList());
        }
        public string GetLogin(int CHAT_ID, int USER_ID)
        {
            string sql = $"SELECT us.Login as log FROM Users us LEFT JOIN userNchat uc ON us.UserID = uc.UserID WHERE uc.ChatID = {CHAT_ID} and us.UserID != {USER_ID}";
            var Login = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(Login);
            connection.Close();
            return Login.AsEnumerable().First()["log"].ToString();
        }
        public string GetBackgroundCode(int CHAT_ID)
        {
            string sql = $"SELECT backgroundCode as bgc FROM Chat  WHERE ChatID = {CHAT_ID}";
            var Login = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(Login);
            connection.Close();
            return Login.AsEnumerable().First()["bgc"].ToString();
        }
        public string GetUsersAndChatsIDsWithUser(int UserID)
        {
            string sql = $"SELECT us.UserID as usID, us.Login as log, usch.ChatID as chatID FROM Users us LEFT JOIN userNchat usch on us.UserID=usch.UserID AND usch.ChatID IN (SELECT ChatID FROM userNchat WHERE UserID={UserID}) WHERE us.UserID!={UserID}";
            var UsersChats = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(UsersChats);
            connection.Close();
            return JsonConvert.SerializeObject(UsersChats.AsEnumerable().Select(x => new { userID = x["usID"], Login = x["log"], ChatID = x["chatID"] }).ToList());
        }
        public int GetChatByName(string chatName)
        {
            string sql = $"SELECT TOP 1 ChatID FROM Chat WHERE ChatName='{chatName}' ORDER BY ChatID DESC";
            var chat = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(chat);
            connection.Close();
            return Convert.ToInt32(chat.AsEnumerable().First()["ChatID"]);
        }
        public List<string> DeleteChat(int ChatID)
        {
            string sql = $"SELECT u.Login FROM Users u LEFT JOIN userNchat usch ON u.UserID = usch.UserID WHERE usch.ChatID = {ChatID}";
            var chat = new DataTable();

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);

            adapter = new SqlDataAdapter(command);


            connection.Open();
            adapter.Fill(chat);
            connection.Close();

            DeleteChatMsg(ChatID);
            DeleteUserNChat(ChatID);
            DeleteChatUs(ChatID);
            
            return chat.AsEnumerable().Select(x => x["Login"].ToString()).ToList();
        }
        public void DeleteChatMsg(int ChatID)
        {
            string sql = $"DELETE FROM Message WHERE Message.ChatID = @param1";
            connection.Open();
            SqlCommand command = sqlSeverClass.connection.CreateCommand();
            command.CommandText = sql;

            command.Parameters.AddWithValue("@param1", ChatID);
            var num = command.ExecuteNonQuery();

            connection.Close();
        }
        public void DeleteUserNChat(int ChatID) 
        {
            string sql = $"DELETE FROM userNchat WHERE ChatID = @param1";
            connection.Open();
            SqlCommand command = sqlSeverClass.connection.CreateCommand();
            command.CommandText = sql;

            command.Parameters.AddWithValue("@param1", ChatID);
            var num = command.ExecuteNonQuery();

            connection.Close();
        }
        public void DeleteChatUs(int ChatID)
        {
            string sql = $"DELETE FROM Chat WHERE ChatID = @param1";
            connection.Open();
            SqlCommand command = sqlSeverClass.connection.CreateCommand();
            command.CommandText = sql;

            command.Parameters.AddWithValue("@param1", ChatID);
            var num = command.ExecuteNonQuery();

            connection.Close();
        }
        public void AddChatByName(string ChatName)
        {
            string sql = "INSERT INTO Chat (ChatName) VALUES (@param1)";

            sqlSeverClass.connection.Open();
            SqlCommand command = sqlSeverClass.connection.CreateCommand();
            command.CommandText = sql;

            command.Parameters.AddWithValue("@param1", ChatName);
            var num = command.ExecuteNonQuery();
            sqlSeverClass.connection.Close();
        }
        public void AddChatLinks(int chatId, int userID, int secondChatter)
        {
            string sql = "INSERT INTO userNchat (ChatID, UserID) VALUES (@param1, @param2),(@param3,@param4)";
            sqlSeverClass.connection.Open();
            SqlCommand command = sqlSeverClass.connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("param1", chatId);
            command.Parameters.AddWithValue("param2", userID);
            command.Parameters.AddWithValue("param3", chatId);
            command.Parameters.AddWithValue("param4", secondChatter);

            var num = command.ExecuteNonQuery();
            sqlSeverClass.connection.Close();
        }
    }
}
