public class ManageUserRolesViewModel 
{ 
    public string UserId { get; set; } = string.Empty; 
    public string? UserName { get; set; } 
    public string? Email { get; set; } 
    public List<string> CurrentRoles { get; set; } = new(); 
    public List<string> AvailableRoles { get; set; } = new(); 
}