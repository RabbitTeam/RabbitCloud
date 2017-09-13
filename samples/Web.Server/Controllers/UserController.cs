using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Server.Controllers
{
    [Route("api/[controller]")]
    public class UserController
    {
        private static readonly UserViewModel User = new UserViewModel
        {
            Name = "ben",
            Age = 20
        };

        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public UserViewModel Get(long id)
        {
            _logger.LogInformation($"GetUser {id}.");
            return User;
        }

        public class UserViewModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [HttpPut("{id}")]
        public object Put(long id, [FromBody]UserViewModel model)
        {
            if (model == null)
                return null;

            User.Name = model.Name;
            User.Age = model.Age;
            return new
            {
                Id = id,
                User.Name,
                User.Age
            };
        }
    }
}