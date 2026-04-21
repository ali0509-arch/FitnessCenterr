namespace FitnessCenterr.Core.DTOs.Members;

public class MemberDto
{
    public int MemberID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? TrainerID { get; set; }
    public string? TrainerName { get; set; }
}

public class CreateMemberDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? TrainerID { get; set; }
}

public class UpdateMemberDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? TrainerID { get; set; }
}
