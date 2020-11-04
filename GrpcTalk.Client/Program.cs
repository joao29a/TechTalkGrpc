using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcTalk.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new Talk.TalkClient(channel);

            var line = Console.ReadLine();
            var words = line.Split(';');

            if (words[0] == "0")
            {
                GetUser(client, Convert.ToInt32(words[1]));
            }
            else if (words[0] == "1")
            {
                GetAllUsers(client).Wait();
            }
            else if (words[0] == "2")
            {
                AddUsersWithSummary(client, words[1]).Wait();
            }
            else if (words[0] == "3")
            {
                AddUsersWithResponseForEach(client, words[1]).Wait();
            }

            channel.ShutdownAsync().Wait();
        }

        static void GetUser(Talk.TalkClient client, int id)
        {
            var response = client.GetUser(new UserRequest
            {
                User = new User
                {
                    Id = id
                }
            });

            Console.WriteLine(response);
        }

        static async Task GetAllUsers(Talk.TalkClient client)
        {
            using (var stream = client.GetUsers(new UserRequest()))
            {
                while (await stream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var response = stream.ResponseStream.Current;
                    Console.WriteLine(response);
                }
            }
        }

        static async Task AddUsersWithSummary(Talk.TalkClient client, string words)
        {
            var users = words.Split('|');

            using (var stream = client.AddUsersWithSummary())
            {
                foreach (var user in users)
                {
                    var data = user.Split('-');
                    var userGrpc = new User
                    {
                        Id = Convert.ToInt32(data[0]),
                        Name = data[1]
                    };
                    var request = new UserRequest
                    {
                        User = userGrpc
                    };

                    await stream.RequestStream.WriteAsync(request);
                }
                await stream.RequestStream.CompleteAsync();
                var response = await stream.ResponseAsync;
                Console.WriteLine(response);
            }
        }

        static async Task AddUsersWithResponseForEach(Talk.TalkClient client, string words)
        {
            var users = words.Split('|');

            using (var stream = client.AddUsersWithResponseForEach())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await stream.ResponseStream.MoveNext(CancellationToken.None)) 
                    {
                        var response = stream.ResponseStream.Current;
                        Console.WriteLine(response);
                    }
                });

                foreach (var user in users)
                {
                    var data = user.Split('-');
                    var userGrpc = new User
                    {
                        Id = Convert.ToInt32(data[0]),
                        Name = data[1]
                    };
                    var request = new UserRequest
                    {
                        User = userGrpc
                    };

                    await stream.RequestStream.WriteAsync(request);
                    Thread.Sleep(1000);
                }
                await stream.RequestStream.CompleteAsync();
                await responseReaderTask;
            }
        }
    }
}
