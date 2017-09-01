using Microsoft.Extensions.Logging;

namespace Web.Server.Controller
{
    public class UserController
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public object GetUser(long id)
        {
            _logger.LogInformation($"GetUser {id}.");
            return new
            {
                Name = "ben",
                Age = 20
            };
        }
    }
}