public class UserRoleViewModel 
{ 
    public string Id { get; set; } = string.Empty; 
    public string? UserName { get; set; } 
    public string? Email { get; set; } 
    public bool EmailConfirmed { get; set; } 
    public List<string> Roles { get; set; } = new(); 
}