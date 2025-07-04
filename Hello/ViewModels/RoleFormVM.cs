using System.ComponentModel.DataAnnotations;

namespace Hello.ViewModels
{
    public class RoleFormVM
    {
       [Required, StringLength(256)]
       public string Name { get; set; }

    }
}

