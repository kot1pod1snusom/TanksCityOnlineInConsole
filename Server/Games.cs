using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Game1
{

    public static List<UserClient> Clients = new List<UserClient>();

    public static void ProcessClient(TcpClient client)
    {

        bool UserOnServer = true;
        while (UserOnServer)
        {
            try
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
                Console.WriteLine($"{Encoding.UTF8.GetString(bytes.ToArray())}");


                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].user.Id == user.Id)
                    {
                        Clients[i].user = user;
                    }

                    if (client.Connected)
                    {

                        var sendMessageStream = Clients[i].tcpClient.GetStream();
                        _ = sendMessageStream.WriteAsync(bytes.ToArray());
                    }
                }

                if (user.matchMakingStatus == User.MatchMakingStatus.InMenu)
                {
                    return;
                }

                bytes.Clear();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred.");
                return;
            }
        }
    }
}

class Game2
{

    public static List<UserClient> Clients = new List<UserClient>();

    public static void ProcessClient(TcpClient client)
    {

        bool UserOnServer = true;
        while (UserOnServer)
        {
            try
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
                Console.WriteLine($"{Encoding.UTF8.GetString(bytes.ToArray())}");


                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].user.Id == user.Id)
                    {
                        Clients[i].user = user;
                    }

                    if (client.Connected)
                    {

                        var sendMessageStream = Clients[i].tcpClient.GetStream();
                        _ = sendMessageStream.WriteAsync(bytes.ToArray());
                    }
                }

                bytes.Clear();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred.");
                return;
            }
        }
    }
}