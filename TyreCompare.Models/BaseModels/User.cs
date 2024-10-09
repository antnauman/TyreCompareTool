namespace TyreCompare.Models;

public class User
{
    public User(int id)
    {
        Id = id;
        Name = "name" + id;
        Username = "username" + id;
        Password = "password" + id;
        UserRoleId = 1;
    }

    public User() { }

    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public int UserRoleId { get; set; }

    public string UserRoleName { get; set; }
}
