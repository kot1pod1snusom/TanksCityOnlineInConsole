using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml;
class Bulet : Block
{

    public new Status status = Status.PLAYER;

    public enum Direction
    {
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


    public int lives;
    public string name;
    public string email;
    public bool NewPlayerOrNot;


    private string Password;
    public string password
    {
        set
        {
            var crypt = new SHA256Managed();
            string hash = "";
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(value));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            Password = hash;
        }
        get => Password;
    }

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