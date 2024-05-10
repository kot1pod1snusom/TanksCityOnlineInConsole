using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System;
using Newtonsoft;
using Newtonsoft.Json.Converters;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Claims;
using System.Diagnostics;
using System.Net.WebSockets;

class UserClient
{
    public User user;
    public TcpClient tcpClient;

}

class Bulet : Block
{

    public new Status status = Status.PLAYER;

    public enum Direction
    {
        //Int key value --- up - 1, down = 2, left = 3, right = 4
        UP, DOWN, LEFT, RIGHT, NONE
    }

    public Direction direction = Direction.NONE;
}

class Block
{
    public virtual char Skin { get; set; }
    public ConsoleColor Color { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public Status status = Status.BORDER;
    public enum Status
    {
        BORDER, VOID, PLAYER
    }
}
class User : Block
{
    public Bulet bulet = new Bulet() { X = 0, Y = 0, Skin = '#' };

    public int GameId = 0;
    public enum MatchMakingStatus
    {
        Registration, InMenu, SearchingGame, InGame
    }

    public MatchMakingStatus matchMakingStatus = MatchMakingStatus.Registration;

    public enum OflineOnlineStatus
    {

        Offline,
        OnServer,
    }
    public OflineOnlineStatus OnServerStatus;

    public enum LoginStatus
    {
        LogIn,
        WrongPasswordOrEmail,
        NameIsOccupied,
        EmailIsOccupied,
        ThisUserOnServer
    }
    public LoginStatus statusLogin;


    public string name;
    public string email;
    public bool NewPlayerOrNot;
    public int lives;


    private string Password;
    public string password { get; set; }

    public override char Skin
    {
        get => base.Skin;
        set
        {
            if (base.Skin == ' ') base.Skin = '0';
            else base.Skin = value;
        }
    }
    public int Id { get; set; }
}


class Server
{
    public static List<UserClient> Clients = new List<UserClient>();
    public static List<User> AllUsers = new List<User>();
    public static List<User> SearchingGameUsers = new List<User>();
    private static int OnlinePlayersCount = 0;

    public static List<bool> GamesSatus = new List<bool>() { false, false, false };

    public static async Task MatchMaker()
    {
        List<UserClient> GoInGameUsers = new List<UserClient>();

        while (true)
        {
            if (SearchingGameUsers.Count == 2)
            {
                int index = GamesSatus.FindIndex(x => x == false) + 1;

                if (index != 1 && index != 2 && index != 3)
                {
                    continue;
                }


                for (int i = 0; i < SearchingGameUsers.Count; i++)
                {
                    SearchingGameUsers[i].matchMakingStatus = User.MatchMakingStatus.InGame;
                    SearchingGameUsers[i].GameId = index;
                    GoInGameUsers.Add(Clients.First(x => x.user.Id == SearchingGameUsers[i].Id));
                }

                Task.Delay(1000);
                List<User> newUsers = new List<User>();
                List<UserClient> newUserClient = new List<UserClient>();

                switch (index)
                {
                    case 1:

                        for (int i = 0; i < GoInGameUsers.Count; i++)
                        {
                            UserClient temp = GoInGameUsers[i];
                            Game1.Clients.Add(temp);
                        }
                        GamesSatus[0] = true;
                        break;
                    case 2:
                        for (int i = 0; i < GoInGameUsers.Count; i++)
                        {
                            UserClient temp = GoInGameUsers[i];
                            Game2.Clients.Add(temp);
                        }
                        GamesSatus[1] = true;
                        break;
                    default:
                        break;
                }


                if (SearchingGameUsers.Count == 2)
                {
                    SearchingGameUsers.Clear();
                    GoInGameUsers.Clear();
                }
                else
                {
                    for (int i = 4; i < SearchingGameUsers.Count; i++)
                    {
                        newUsers.Add(SearchingGameUsers[i]);
                        newUserClient.Add(Clients.First(x => x.user.Id == SearchingGameUsers[i].Id));
                    }

                    SearchingGameUsers.Clear();
                    GoInGameUsers.Clear();
                    GoInGameUsers = newUserClient;
                    SearchingGameUsers = newUsers;
                }
            }
            else
            {

            }

        }
    }


    public static async Task InMenu(TcpClient client)
    {

        while (true)
        {

            var stream = client.GetStream();
            List<byte> bytes = new List<byte>();
            int bytesRead = 0;

            while ((bytesRead = stream.ReadByte()) != '\0')
            {
                bytes.Add((byte)bytesRead);
            }
            bytes.Add((byte)'\0');

            User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(Encoding.UTF8.GetString(bytes.ToArray()));

            if (user.matchMakingStatus == User.MatchMakingStatus.SearchingGame)
            {
                SearchingGameUsers.Add(user);
                bool GameStart = false;
                while (GameStart == false)
                {
                    if (SearchingGameUsers.Count != 1)
                    {

                        for (int i = 0; i < SearchingGameUsers.Count; i++)
                        {
                            User temp = SearchingGameUsers[i];
                            if (user.Id == temp.Id)
                            {
                                user = temp;
                                if (user.matchMakingStatus == User.MatchMakingStatus.InGame)
                                {
                                    GameStart = true;
                                }

                                switch (i)
                                {
                                    case 0:
                                        user.X = 1;
                                        user.Y = 1;
                                        break;
                                    case 1:
                                        user.X = 18;
                                        user.Y = 18;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                }
            }

            string jsonUser = Newtonsoft.Json.JsonConvert.SerializeObject(user);

            if (jsonUser != null)
            {
                jsonUser += '\0';
                byte[] bytes1 = Encoding.UTF8.GetBytes(jsonUser);
                await stream.WriteAsync(bytes1);
            }

            if (user.matchMakingStatus == User.MatchMakingStatus.InGame)
            {


                switch (user.GameId)
                {
                    case 1:
                        Game1.ProcessClient(client);
                        break;
                    case 2:
                        Game2.ProcessClient(client);
                        break;
                    default:
                        break;
                }


            }
        }
    }


    public static void PutUsersInFile()
    {
        string InFile = Newtonsoft.Json.JsonConvert.SerializeObject(AllUsers);
        File.WriteAllText("FileWithUsers.json", InFile);
    }

    public static void GetUsersFromFile()
    {
        string fileRead = File.ReadAllText("FileWithUsers.json");
        AllUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(fileRead);
    }


    public static void SendClientsListToUser(TcpClient tcpClient)
    {
        List<User> temp = new List<User>();
        for (int i = 0; i < Clients.Count; i++)
        {
            temp.Add(Clients[i].user);
        }
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(temp);
        ServerUser.SendString(tcpClient, str);
    }

    public static async Task Main(string[] args)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("26.64.111.48"), 9010);
        tcpListener.Start();
        _ = Task.Run(async () => await MatchMaker());
        Console.WriteLine("Server started..");
        GetUsersFromFile();

        User us = new User();

        while (true)
        {
            try
            {

                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                await Console.Out.WriteLineAsync("Client connected..");

                Registr(tcpClient);
                us.OnServerStatus = User.OflineOnlineStatus.OnServer;



                PutUsersInFile();
                Clients.Add(new UserClient() { tcpClient = tcpClient, user = us, });
                Task.Run(async () => await InMenu(Clients[Clients.Count - 1].tcpClient));

            }
            catch (Exception)
            {

                throw;
            }

        }




        async Task Registr(TcpClient client)
        {

            User user = new User();
            bool logIn = false;
            while (logIn != true)
            {
                string temp = ServerUser.GetString(client);
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(temp);


                int count = 0;
                for (int i = 0; i < AllUsers.Count; i++)
                {

                    if (AllUsers[i].email == user.email && AllUsers[i].password == user.password)
                    {
                        if (user.NewPlayerOrNot == false)
                        {
                            if (Clients.Find(x => x.user.Id == AllUsers[i].Id) == null)
                            {
                                user.statusLogin = User.LoginStatus.LogIn;
                                logIn = true;
                                user = AllUsers[i];
                                us = user;
                                Console.WriteLine($"User LogIn  {user.name}   {user.Id}");
                            }
                            else
                            {
                                user.statusLogin = User.LoginStatus.ThisUserOnServer;
                            }
                        }
                        else
                        {
                            count++;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
                Console.WriteLine($"{user.email}     {user.password}");
                Console.WriteLine($" 13123 -       {us.email}     {user.password}   ");
                if (count == AllUsers.Count)
                {
                    bool key = false;
                    if (user.NewPlayerOrNot == true)
                    {

                        for (int i = 0; i < AllUsers.Count; i++)
                        {
                            if (user.email == AllUsers[i].email)
                            {
                                user.statusLogin = User.LoginStatus.EmailIsOccupied;
                                key = true;
                                break;
                            }
                            else if (user.name == AllUsers[i].name)
                            {
                                user.statusLogin = User.LoginStatus.NameIsOccupied;
                                key = true;
                                break;
                            }
                        }

                        if (key == false)
                        {
                            user.Id = AllUsers.Count + 1;
                            AllUsers.Add(user);
                            user.NewPlayerOrNot = false;
                            us = user;
                            logIn = true;
                            user.statusLogin = User.LoginStatus.LogIn;
                            Console.WriteLine($"User LogIn {user.name} {user.Id}");

                        }

                    }
                    else
                    {
                        user.statusLogin = User.LoginStatus.WrongPasswordOrEmail;
                    }
                }

                ServerUser.SendClass<User>(client, user);
            }
            PutUsersInFile();
        }
    }
}