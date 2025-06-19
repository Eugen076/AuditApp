namespace WebApplication1.Models
{
    public class UserSettingsViewModel
    {
        public string UserName { get; set; }     
        public string FirstName { get; set; }      
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
