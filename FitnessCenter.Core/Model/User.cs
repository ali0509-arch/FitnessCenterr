public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int? MemberID { get; set; }
    public int? TrainerID { get; set; }
    public string Role { get; set; } = "User";
}