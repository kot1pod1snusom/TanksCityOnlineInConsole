using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using static Bulet;
using System.Diagnostics.Metrics;


//List players near map
interface IPlayers
{
    public void PlayersOut(User player, int count)
    {
        Console.SetCursorPosition(Map.width + 1, count);
        if (count == 1) Console.Write("(you) ");
        string str = "";
        if (player.Skin != '#') Console.WriteLine($"{player.name} - Skin: {player.Skin}, Color - {player.Color}, X - {player.X} Y - {player.Y}");
        else
        {
            for (int i = 0; i < player.name.Length + 23; i++) str += " ";
            Console.WriteLine(str);
        }
    }
}



class Client
{
    private static bool OpenConsole = false;
    private static bool CloseGameOrNot = false;
    private static bool GameIsPlaying = false;
    public static List<User> Users = new List<User>();

    public static void PlayersOut(User player, int count)
    {
        Console.SetCursorPosition(Map.width + 1, count);

        if (count == 1) Console.Write("(you) ");

        string str = "";

        if (player.Skin != '#') Console.WriteLine($"" +
            $"{player.name}," +
            $" Skin: {player.Skin}," +
            $"Lives {player.lives} ," +
            $" Color - {player.Color}, " +
            $"X - {player.X}, " +
            $"Y - {player.Y}  ");
        else
        {
            for (int i = 0; i < player.name.Length + 1000; i++) str += " ";
            Console.WriteLine(str);
        }
    }

    public static void ClearPreviousPosition(Bulet bulet)
    {
        Console.SetCursorPosition(bulet.X, bulet.Y);
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(' ');
    }

    public static void ClearPreviousPosition(User user)
    {

        Console.SetCursorPosition(user.X, user.Y);
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(' ');
    }
    public static void WriteCurrentPosition(User user)
    {
        Console.SetCursorPosition(user.X, user.Y);
        Console.ForegroundColor = user.Color;
        Console.Write(user.Skin);
    }

    public static void WriteCurrentPosition(Bulet bulet)
    {
        Console.SetCursorPosition(bulet.X, bulet.Y);
        Console.ForegroundColor = bulet.Color;
        Console.Write(bulet.Skin);
    }

    public static async Task GetMessage(TcpClient client)
    {
        try
        {
            while (CloseGameOrNot != true)
            {

                var stream = client.GetStream();
                List<byte> bytes = new List<byte>();
                int bytesRead = 0;

                while ((bytesRead = stream.ReadByte()) != '\0')
                {
                    bytes.Add((byte)bytesRead);
                }
                bytes.Add((byte)'\0');

                string str = Encoding.UTF8.GetString(bytes.ToArray());

                User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(str);
                bool isContain = false;



                for (int i = 0; i < Users.Count; i++)
                {

                    if (user.matchMakingStatus == User.MatchMakingStatus.InMenu)
                    {
                        GameIsPlaying = false;
                        return;
                    }

                    if (Users[i].Id == user.Id)
                    {
                        PlayersOut(user, i + 1);

                        if (OpenConsole == false) { ClearPreviousPosition(Users[i]); ClearPreviousPosition(Users[i].bulet); }
                        Users[i] = user;
                        if (OpenConsole == false) { WriteCurrentPosition(Users[i]); WriteCurrentPosition(Users[i].bulet); }
                        isContain = true;
                        break;
                    }


                }

                if (!isContain)
                {
                    Users.Add(user);
                    if (OpenConsole == false) WriteCurrentPosition(user);
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    static bool Collizion(Bulet bulet)
    {
        switch (bulet.direction)
        {
            case Bulet.Direction.UP:
                if (Map.field.First(cell => cell.X == bulet.X && cell.Y == bulet.Y - 1).status != Block.Status.VOID)
                {
                    return true;
                }
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].X == bulet.X && Users[i].Y == bulet.Y - 1)
                    {
                        return true;
                    }
                }
                return false;
            case Bulet.Direction.DOWN:
                if (Map.field.First(cell => cell.X == bulet.X && cell.Y == bulet.Y + 1).status != Block.Status.VOID)
                {
                    return true;
                }
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].X == bulet.X && Users[i].Y == bulet.Y + 1)
                    {
                        return true;
                    }
                }
                return false;
            case Bulet.Direction.LEFT:
                if (Map.field.First(cell => cell.X == bulet.X - 1 && cell.Y == bulet.Y).status != Block.Status.VOID)
                {
                    return true;
                }
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].X == bulet.X - 1 && Users[i].Y == bulet.Y)
                    {
                        return true;
                    }
                }
                return false;
            case Bulet.Direction.RIGHT:
                if (Map.field.First(cell => cell.X == bulet.X + 1 && cell.Y == bulet.Y).status != Block.Status.VOID)
                {
                    return true;
                }
                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].X == bulet.X + 1 && Users[i].Y == bulet.Y - 1)
                    {
                        return true;
                    }
                }
                return false;
        }
        return false;
    }

    static User FindWoundedUser(Bulet bulet)
    {

        switch (bulet.direction)
        {
            case Bulet.Direction.UP:
                if (bulet.Y == 1) return new User { Id = -1 };
                return Users.First(user => user.X == bulet.X && user.Y == bulet.Y - 1);
            case Bulet.Direction.DOWN:
                if (bulet.Y == 18) return new User { Id = -1 };
                return Users.First(user => user.X == bulet.X && user.Y == bulet.Y + 1);
            case Bulet.Direction.LEFT:
                if (bulet.X == 1) return new User { Id = -1 };
                return Users.First(user => user.X == bulet.X - 1 && user.Y == bulet.Y);
            case Bulet.Direction.RIGHT:
                if (bulet.X == 18) return new User { Id = -1 };
                return Users.First(user => user.X == bulet.X + 1 && user.Y == bulet.Y);
        }

        return new User { Id = -1 };
    }

    public static Bulet BuletMove(Bulet bulet)
    {
        switch (bulet.direction)
        {
            case Direction.UP:
                bulet.Y--;
                break;
            case Direction.DOWN:
                bulet.Y++;
                break;
            case Direction.LEFT:
                bulet.X--;
                break;
            case Direction.RIGHT:
                bulet.X++;
                break;
            default:
                break;
        }

        return bulet;
    }

    static async Task BuletFlyControl(User me, ConsoleKey consoleKey, TcpClient client)
    {
        switch (consoleKey)
        {
            case ConsoleKey.UpArrow:
                me.bulet.direction = Bulet.Direction.UP;
                me.bulet.Y = me.Y;
                me.bulet.X = me.X;
                me.bulet.Color = me.Color;
                me.bulet.Skin = '|';
                break;
            case ConsoleKey.DownArrow:
                me.bulet.direction = Bulet.Direction.DOWN;
                me.bulet.Y = me.Y;
                me.bulet.X = me.X;
                me.bulet.Color = me.Color;
                me.bulet.Skin = '|';
                break;
            case ConsoleKey.LeftArrow:
                me.bulet.direction = Bulet.Direction.LEFT;
                me.bulet.X = me.X;
                me.bulet.Y = me.Y;
                me.bulet.Color = me.Color;
                me.bulet.Skin = '-';
                break;
            case ConsoleKey.RightArrow:
                me.bulet.direction = Bulet.Direction.RIGHT;
                me.bulet.X = me.X;
                me.bulet.Y = me.Y;
                me.bulet.Color = me.Color;
                me.bulet.Skin = '-';
                break;
        }

        while (true)
        {
            me.bulet = BuletMove(me.bulet);

            Thread.Sleep(100);
            SendToServer(client, me);


            bool key = Collizion(me.bulet);
            if (key == true)
            {
                break;
            }
            else
            {
            }
        }

        User WoundedUser = FindWoundedUser(me.bulet);
        if (WoundedUser.Id != -1)
        {
            me.bulet.X = 0;
            me.bulet.Y = 0;
            me.bulet.Skin = '#';
            me.bulet.Color = ConsoleColor.Green;
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].Id == WoundedUser.Id)
                {
                    Users[i].lives -= 1;
                }
            }
            SendToServer(client, WoundedUser);
            SendToServer(client, me);
            return;
        }
        else
        {
            me.bulet.X = 0;
            me.bulet.Y = 0;
            me.bulet.Skin = '#';
            me.bulet.Color = ConsoleColor.Green;
            SendToServer(client, me);
            return;
        }
    }

    static async Task SendToServer(TcpClient client, User me)
    {
        var stream = client.GetStream();
        string jsonUser = Newtonsoft.Json.JsonConvert.SerializeObject(me);


        if (jsonUser != null)
        {
            jsonUser += '\0';
            byte[] bytes = Encoding.UTF8.GetBytes(jsonUser);
            await stream.WriteAsync(bytes);
        }
    }

    static void UserConsole(ref User me)
    {
        OpenConsole = true;
        UserConsole uc = new UserConsole(ref me);
        OpenConsole = false;
        Console.Clear();
        Map.OutputField();
    }

    static void GameExit(ref User me)
    {
        CloseGameOrNot = true;
        me.X = 0;
        me.Y = 0;
        me.Skin = '#';
        me.Color = ConsoleColor.Green;
        me.OnServerStatus = User.OflineOnlineStatus.Offline;
    }


    public static User GetChanges(User user)
    {

        User temp = new User();
        for (int i = 0; i < Users.Count; i++)
        {
            if (user.Id == Users[i].Id)
            {
                //user = Users[i];
                temp = Users[i];
            }
        }
        return temp;

    }


    public static async Task MovementAsync(TcpClient client, User me)
    {
        while (GameIsPlaying == true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            me = GetChanges(me);
            ClearPreviousPosition(me);

            if (me.lives <= 0)
            {
                me.matchMakingStatus = User.MatchMakingStatus.InMenu;
                GameIsPlaying = false;
            }
            else
            {
                switch (key.Key)
                {
                    case ConsoleKey.W:
                        me.Y -= 1;
                        if (!Availiable(me))
                        {
                            me.Y += 1;
                        }
                        break;
                    case ConsoleKey.A:
                        me.X -= 1;
                        if (!Availiable(me))
                        {
                            me.X += 1;
                        }
                        break;
                    case ConsoleKey.S:
                        me.Y += 1;
                        if (!Availiable(me))
                        {
                            me.Y -= 1;
                        }
                        break;
                    case ConsoleKey.D:
                        me.X += 1;
                        if (!Availiable(me))
                        {
                            me.X -= 1;
                        }
                        break;
                    /*case ConsoleKey.Enter:
                        UserConsole(ref me);
                        break;*/
                    case ConsoleKey.UpArrow:
                        _ = Task.Run(async () => await BuletFlyControl(me, key.Key, client));
                        break;
                    case ConsoleKey.LeftArrow:
                        _ = Task.Run(async () => await BuletFlyControl(me, key.Key, client));
                        break;
                    case ConsoleKey.DownArrow:
                        _ = Task.Run(async () => await BuletFlyControl(me, key.Key, client));
                        break;
                    case ConsoleKey.RightArrow:
                        _ = Task.Run(async () => await BuletFlyControl(me, key.Key, client));
                        break;
                    /*case ConsoleKey.Escape:
                        GameExit(ref me);
                        break;*/
                    default:
                        break;
                }
            }

            await SendToServer(client, me);
        }


        bool Availiable(User mee)
        {
            if (Map.field.First(cell => cell.X == me.X && cell.Y == me.Y).status != Block.Status.VOID)
            {
                return false;
            }
            for (int i = 0; i < Users.Count; i++)
            {
                if (mee.X == Users[i].X && mee.Y == Users[i].Y && mee.Id != Users[i].Id)
                {
                    return false;
                }
            }
            return true;
        }
    }


    public static async void Menu(TcpClient client, User user)
    {
        while (true)
        {

            Console.Clear();
            user.matchMakingStatus = User.MatchMakingStatus.InMenu;
            Console.Out.WriteLineAsync($"Name - {user.name}, skin - {user.Skin}");
            Console.Out.WriteLineAsync("Введите 1 чтобы начать поиск игры");
            Console.Out.WriteLineAsync("Введите 2 чтобы открыть настрйоки");
            Console.Out.WriteLineAsync("Введите 4 чтобы выйти из игры");


            while (true)
            {
                string str;
                str = Console.ReadLine();

                if (str[0] == '1')
                {
                    user.matchMakingStatus = User.MatchMakingStatus.SearchingGame;
                    break;
                }
                else if (str[0] == '2')
                {
                    UserConsole(ref user);
                    SendToServer(client, user);
                }

            }


            while (true)
            {
                SendToServer(client, user);


                var stream = client.GetStream();
                List<byte> bytes = new List<byte>();
                int bytesRead = 0;

                while ((bytesRead = stream.ReadByte()) != '\0')
                {
                    bytes.Add((byte)bytesRead);
                }
                bytes.Add((byte)'\0');

                string str = Encoding.UTF8.GetString(bytes.ToArray());
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(str);

                if (user.matchMakingStatus == User.MatchMakingStatus.InGame)
                {
                    Console.Clear();
                    Map.GenerateMap(20, 20);
                    Map.OutputField();
                    Console.WriteLine(' ');
                    Console.WriteLine("Нажмите Enter чтобы открыть консоль");
                    Console.WriteLine("Нажмите Esc чтобы выйти из игры");
                    GameIsPlaying = true;

                    _ = Task.Run(async () => await GetMessage(client));

                    await SendToServer(client, user);

                    MovementAsync(client, user);
                    break;
                }
            }
        }
    }

    public static async Task Main(string[] args)
    {
        TcpClient tcpClient = new TcpClient();

        await tcpClient.ConnectAsync(IPAddress.Parse("26.64.111.48"), 9010);

        await Console.Out.WriteLineAsync("Connected..");

        User user = new User();

        Registr(tcpClient);
        await Console.Out.WriteAsync("Input symbol ");

        char symb = Console.ReadLine()[0];

        await Console.Out.WriteLineAsync("Input color (1 - 15)");
        int color = Convert.ToInt32(Console.ReadLine());

        ConsoleColor cColor = (ConsoleColor)(color < 1 ? 1 : (color > 15 ? 15 : color));

        user.bulet.Color = ConsoleColor.Green;
        user.Color = cColor;
        user.Skin = symb;
        user.X = 10;
        user.Y = 10;
        user.lives = 5;
        user.OnServerStatus = User.OflineOnlineStatus.OnServer;

        Console.Clear();
        Users.Add(user);


        Menu(tcpClient, user);




        async Task Registr(TcpClient client)
        {
            var st = client.GetStream();
            bool logIn = false;
            while (logIn != true)
            {
                Console.Clear();
                Console.WriteLine("Введи 1 если создаешь новый акк и введи 2 если уже он у тебя есть");
                int te = 0;
                try { te = Convert.ToInt32(Console.ReadLine()); } catch { te = 0; }

                if (te == 1)
                {
                    Console.WriteLine("Введи новый email");
                    user.email = Console.ReadLine();
                    Console.WriteLine("Введи новый пароль");
                    user.password = Console.ReadLine();
                    Console.WriteLine("Введи имя аккаунта");
                    user.name = Console.ReadLine();
                    user.NewPlayerOrNot = true;
                }
                else if (te == 2)
                {
                    Console.WriteLine("Input Email");
                    user.email = Console.ReadLine();
                    Console.WriteLine("Input Password");
                    user.password = Console.ReadLine();
                    user.NewPlayerOrNot = false;
                }
                else
                {
                    continue;
                }

                string ToUs = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                ToUs += '\0';
                byte[] bytes1 = Encoding.UTF8.GetBytes(ToUs);
                await st.WriteAsync(bytes1);


                int bytes_read = 0;
                List<byte> bt = new List<byte>();
                while ((bytes_read = st.ReadByte()) != '\0')
                {
                    bt.Add((byte)bytes_read);
                }
                string temp = Encoding.UTF8.GetString(bt.ToArray());
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(temp);


                if (user.statusLogin == User.LoginStatus.LogIn)
                {
                    Console.WriteLine($"Wellcum {user.name}");
                    logIn = true;
                }
                else
                {
                    if (user.statusLogin == User.LoginStatus.ThisUserOnServer) Console.WriteLine("This user now on server. Maybe your hacked <3");
                    else if (user.statusLogin == User.LoginStatus.WrongPasswordOrEmail) Console.WriteLine("Wrong email or password");
                    else if (user.statusLogin == User.LoginStatus.EmailIsOccupied) Console.WriteLine("This email used on another account");
                    else if (user.statusLogin == User.LoginStatus.NameIsOccupied) Console.WriteLine("This name is used on another account");

                    Console.WriteLine("Press Enter to continue");
                    Console.ReadLine();
                }
            }
        }

    }
}   