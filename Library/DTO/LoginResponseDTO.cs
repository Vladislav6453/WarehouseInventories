namespace Library.DTO;

public class LoginResponseDTO
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
}