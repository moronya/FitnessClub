using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Auth
{
    public class LoginModel
    {
        [EmailAddress]
        public string? email { get; set; }

        public string? password { get; set; }
    }
}
