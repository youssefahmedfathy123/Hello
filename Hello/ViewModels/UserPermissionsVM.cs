namespace Hello.ViewModels
{
    public class UserPermissionsVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<CheckBoxViewModel> Permissions { get; set; } 
    }
}


