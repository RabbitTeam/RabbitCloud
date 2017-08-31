namespace Web.Server.Controller
{
    public class UserController
    {
        public object GetUser(long id)
        {
            return new
            {
                Name = "ben",
                Age = 20
            };
        }
    }
}