namespace AuthService.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User(string name, string login, string password, string email)
        {
            Name = name;
            Active = true;
            Login = login;
            Email = email;
            Password = password;
        }

    }
}
