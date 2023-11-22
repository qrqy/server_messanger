using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Newtonsoft.Json;
using static DevExpress.Mvvm.Native.TaskLinq;
using DevExpress.Mvvm.Native;

namespace TCPChat.Server
{
    internal class Program
    {
        static sqlSeverClass sqlSeverClass = new sqlSeverClass();
        static TcpListener listener = new TcpListener(IPAddress.Any, 5050);
        static List<ConnectedClient> clients = new List<ConnectedClient>();
        static void Main(string[] args)
        {
            Console.WriteLine("SERVER START");
            listener.Start();
            CheckOnline();
            while (true)
            {
                var client = listener.AcceptTcpClient();

                Task.Factory.StartNew(() =>
                {
                    int userID = -1;
                    string userLogin="";
                    var sr = new StreamReader(client.GetStream());
                    while (client.Connected)
                    {
                        try
                        {
                            var line = sr.ReadLine();
                            if (line == "auth")
                            {
                                userLogin = sr.ReadLine();
                                line = userLogin;
                                userLogin = userLogin.Replace("Login: ", "");
                                var userPass = sr.ReadLine();
                                userPass = userPass.Replace("Password: ", "");
                                //Console.WriteLine(userLogin + " " + userPass);
                                var sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                if (sqlSeverClass.Autorization(userLogin, userPass))
                                {
                                    if (clients.FirstOrDefault(x => x.Name == userLogin) is null)
                                    {
                                        clients.Add(new ConnectedClient(client, userLogin));
                                        sw.WriteLine("OK");
                                        sw.WriteLine(sqlSeverClass.AutoUserId(userLogin, userPass));
                                        Console.WriteLine($"Новое подключение: {userLogin}");
                                        if (sqlSeverClass.IsAvatar(userLogin)){
                                            Console.WriteLine("IsAvatar");
                                            sw.WriteLine("true");
                                            sw.WriteLine(sqlSeverClass.GetAvatar(userLogin));
                                            userID = sqlSeverClass.GetUserID(userLogin);
                                        }
                                        else{
                                            Console.WriteLine("IsNotAvatar");
                                            sw.WriteLine("false");
                                        }
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("NO");
                                    Console.WriteLine("NO");
                                }
                            }
                            else if (line == "ChangeAvatar")
                            {
                                Console.WriteLine("ChangeAvatarWorking");
                                var avatar = sr.ReadLine();
                                try
                                {
                                    string sql = $"UPDATE Users SET AvatarCode = '{avatar}' WHERE Login='{clients.Where(x => x.Client == client).FirstOrDefault().Name}'";
                                    sqlSeverClass.connection.Open();
                                    SqlCommand command = new SqlCommand(sql, sqlSeverClass.connection);
                                    var a = command.ExecuteNonQuery();
                                    sqlSeverClass.connection.Close();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }else if (line=="GetChats") 
                            {
                                if (!String.IsNullOrEmpty(userLogin))
                                {
                                    var sw = new StreamWriter(client.GetStream());
                                    sw.AutoFlush = true;
                                    sw.WriteLine(sqlSeverClass.GetChatsJSON(userLogin));
                                }
                            }else if (line == "GetMessagesFromChat")
                            {
                                if (!String.IsNullOrEmpty(userLogin))
                                {
                                    var sw = new StreamWriter(client.GetStream());
                                    sw.AutoFlush = true;
                                    var ChatID = sr.ReadLine();
                                    if (!String.IsNullOrEmpty(ChatID))
                                    {
                                        sw.WriteLine("MessagesFromChat");
                                        sw.WriteLine(sqlSeverClass.GetMessagesJSON(Convert.ToInt32(ChatID)));
                                    }
                                }
                            }else if (line == "SendMessage")
                            {
                                var sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                var a = JsonConvert.DeserializeObject<dynamic>(sr.ReadLine());
                                if (userID < 0)
                                {
                                    userID = sqlSeverClass.GetUserID(userLogin);
                                }
                                string sql = "INSERT INTO Message(ChatID, MessageContent, Status, DateTime, SenderID) VALUES (@param1,@param2, @param3,@param4,@param5)";
                                
                                sqlSeverClass.connection.Open();
                                SqlCommand command = sqlSeverClass.connection.CreateCommand();
                                command.CommandText = sql;

                                int chatID = a.SelectedChat;
                                string messageText = a.MessageText;
                                bool IsRead = true;
                                DateTime tumis = a.Time;

                                command.Parameters.AddWithValue("@param1", chatID);
                                command.Parameters.AddWithValue("@param2", messageText);
                                command.Parameters.AddWithValue("@param3", IsRead);
                                command.Parameters.AddWithValue("@param4", tumis);
                                command.Parameters.AddWithValue("@param5", userID);
                                var num = command.ExecuteNonQuery();

                                sqlSeverClass.connection.Close();

                                var lists = clients.Find(x => x.Name == sqlSeverClass.GetLogin(chatID, userID));
                                try
                                {
                                    if (lists.Client.Connected)
                                    {
                                        var localSW = new StreamWriter(lists.Client.GetStream());
                                        localSW.AutoFlush = true;
                                        localSW.WriteLine("Message");
                                        localSW.WriteLine(sqlSeverClass.GetMessagesJSON(Convert.ToInt32(chatID)));
                                    }
                                    else
                                    {
                                        Console.WriteLine($"{lists.Name} отключился");
                                        clients.Remove(lists);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }else if (line== "GetBackGround")
                            {
                                int ChatID = Convert.ToInt32(sr.ReadLine());
                                var sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                sw.WriteLine("BackGround");
                                sw.WriteLine(sqlSeverClass.GetBackgroundCode(ChatID));
                            }
                            else if (line== "ChangeBackground")
                            {
                                var photo = sr.ReadLine();
                                var chatID = sr.ReadLine();
                                try
                                {
                                    string sql = $"UPDATE Chat SET backgroundCode = '{photo}' WHERE ChatID = {chatID}";
                                    sqlSeverClass.connection.Open();
                                    SqlCommand command = new SqlCommand(sql, sqlSeverClass.connection);
                                    var a = command.ExecuteNonQuery();
                                    sqlSeverClass.connection.Close();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            else if (line == "ChangeNick")
                            {
                                var Nick = sr.ReadLine();
                                userLogin = Nick;
                                try
                                {
                                    string sql = $"UPDATE Users SET Login = '{Nick}' WHERE UserID = {userID}";
                                    sqlSeverClass.connection.Open();
                                    SqlCommand command = new SqlCommand(sql, sqlSeverClass.connection);
                                    var a = command.ExecuteNonQuery();
                                    sqlSeverClass.connection.Close();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }else if (line=="Registration")
                            {
                                var sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                var a = JsonConvert.DeserializeObject<dynamic>(sr.ReadLine());
                                string RegistrationLogin = a.Login;
                                string RegistrationPassword = a.Password;
                                string RegistrationConfirmPassword = a.ConfirmPassword;

                                if (sqlSeverClass.Registration(RegistrationLogin, RegistrationPassword, RegistrationConfirmPassword))
                                {
                                    string sql = "INSERT INTO Users (Password, Login, AvatarCode, StatusID) VALUES (@param1, @param2, NULL, 2)";

                                    sqlSeverClass.connection.Open();
                                    SqlCommand command = sqlSeverClass.connection.CreateCommand();
                                    command.CommandText = sql;

                                    command.Parameters.AddWithValue("@param1", RegistrationPassword);
                                    command.Parameters.AddWithValue("@param2", RegistrationLogin);
                                    var num = command.ExecuteNonQuery();

                                    sqlSeverClass.connection.Close();
                                    clients.Add(new ConnectedClient(client, RegistrationLogin));
                                    sw.WriteLine("OK");
                                    sw.WriteLine(sqlSeverClass.AutoUserId(RegistrationLogin, RegistrationPassword));
                                    Console.WriteLine($"Новое подключение: {RegistrationLogin}");
                                    Console.WriteLine("OK");
                                    userLogin = RegistrationLogin;
                                }
                                else
                                {
                                    sw.WriteLine(sqlSeverClass.RegistrationConfirm(RegistrationLogin, RegistrationPassword, RegistrationConfirmPassword));
                                }
                            }else if (line== "GetUsersAndChatsIDsWithUser")
                            {
                                var sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                sw.WriteLine("GetUsersAndChatsIDsWithUser");
                                sw.WriteLine(sqlSeverClass.GetUsersAndChatsIDsWithUser(userID));
                            }else if (line=="AddChat")
                            {
                                
                                var ChatName = sr.ReadLine();
                                var secondChatter = Convert.ToInt32(sr.ReadLine());
                                var secondChatterLogin = sr.ReadLine();
                                var sw = new StreamWriter(client.GetStream());
                                sw.AutoFlush = true;
                                //Add chat
                                sqlSeverClass.AddChatByName(ChatName);
                                
                                int chatId = sqlSeverClass.GetChatByName(ChatName);
                                
                                sqlSeverClass.AddChatLinks(chatId, userID, secondChatter);
                                sw.WriteLine("NewChat");
                                sw.WriteLine(JsonConvert.SerializeObject(new {ChatName, chatId}));
                                clients.Where(x => x.Name == secondChatterLogin).ForEach(x =>
                                {
                                    var chach = new StreamWriter(x.Client.GetStream());
                                    chach.AutoFlush = true;
                                    chach.WriteLine("NewChat");
                                    chach.WriteLine(JsonConvert.SerializeObject(new { ChatName, chatId }));
                                });
                            }else if (line== "DeleteChat")
                            {
                                int DeletingChatID = Convert.ToInt32(sr.ReadLine());
                                List<string> users =  sqlSeverClass.DeleteChat(DeletingChatID);
                                clients.Where(x=>users.Contains(x.Name)).ForEach(x =>
                                {
                                        var LocalSW = new StreamWriter(x.Client.GetStream());
                                        LocalSW.AutoFlush = true;
                                        LocalSW.WriteLine("DeleteChat");
                                        LocalSW.WriteLine(DeletingChatID);
                                    Console.WriteLine("Send delete chat "+DeletingChatID+" to "+x.Name);
                                });
                                Console.WriteLine(clients.Where(x => users.Contains(x.Name)).Count().ToString()) ;
                                Console.WriteLine(String.Join(",",users.ToArray()));
                            }
                            else
                            {
                                SendToAllClients(line);

                                Console.WriteLine(line);
                            }
                            Task.Delay(10).Wait();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                });
            }
        }
        private static void CheckOnline()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        try
                        {
                            if (!clients[i].Client.Connected)
                            {
                                Console.WriteLine($"{clients[i].Name} отключился");
                                clients.RemoveAt(i);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    await Task.Delay(1000);
                }
            });
        }
        private static void SendToAllClients(string message)
        {
            Task.Run(() =>
            {
                for(int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        if (clients[i].Client.Connected)
                        {
                            var sw = new StreamWriter(clients[i].Client.GetStream());
                            sw.AutoFlush = true;
                            sw.WriteLine(message);
                        }
                        else
                        {
                            Console.WriteLine($"{clients[i].Name} отключился");
                            clients.RemoveAt(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }
        private static void SendToClien(string message)
        {
            Task.Run(() =>
            {
                var lists = clients.Find(x => x.Name == "amogus1");
                    try
                    {
                        if (lists.Client.Connected)
                        {
                            var sw = new StreamWriter(lists.Client.GetStream());
                            sw.AutoFlush = true;
                            sw.WriteLine(message);
                        }
                        else
                        {
                            Console.WriteLine($"{lists.Name} отключился");
                            clients.Remove(lists);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
            });
        }
        
    }
}
