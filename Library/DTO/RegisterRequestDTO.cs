namespace Library.DTO;

public class RegisterRequestDTO
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int RoleId { get; set; }
}