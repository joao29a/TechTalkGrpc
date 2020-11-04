using Grpc.Core;
using GrpcTalk.Server.Database;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcTalk.Server.Services
{
    public class TalkServiceImpl : Talk.TalkBase
    {
        private readonly ILogger<TalkServiceImpl> _logger;
        private readonly IRepository _repository;

        public TalkServiceImpl(ILogger<TalkServiceImpl> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public override Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
        {
            var userData = _repository.GetById(request.User.Id);
            return Task.FromResult(GetUserResponse(userData));
        }

        public override async Task GetUsers(UserRequest request, IServerStreamWriter<UserResponse> responseStream, ServerCallContext context)
        {
            foreach(var userData in _repository.GetAll())
            {
                await responseStream.WriteAsync(GetUserResponse(userData));
            }
        }

        public override async Task<UserSummaryResponse> AddUsersWithSummary(IAsyncStreamReader<UserRequest> requestStream, ServerCallContext context)
        {
            var totalUsers = 0;
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                var request = requestStream.Current;
                var emails = new List<string>(request.User.Email);
                _repository.Add(new UserData
                {
                    Id = request.User.Id,
                    Name = request.User.Name,
                    Emails = emails
                });
                _logger.LogInformation(request.User.Name);
                totalUsers++;
            }

            return new UserSummaryResponse
            {
                TotalUsers = totalUsers
            };
        }

        public override async Task AddUsersWithResponseForEach(IAsyncStreamReader<UserRequest> requestStream, IServerStreamWriter<UserResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(CancellationToken.None))
            {
                var request = requestStream.Current;
                var emails = new List<string>(request.User.Email);
                _repository.Add(new UserData
                {
                    Id = request.User.Id,
                    Name = request.User.Name,
                    Emails = emails
                });
                _logger.LogInformation(request.User.Name);

                await responseStream.WriteAsync(new UserResponse
                {
                    User = request.User
                });
            }
        }

        private UserResponse GetUserResponse(UserData userData)
        {
            var user = new User
            {
                Id = userData.Id,
                Name = userData.Name
            };
            user.Email.AddRange(userData.Emails);
            return new UserResponse
            {
                User = user
            };
        }
    }
}
