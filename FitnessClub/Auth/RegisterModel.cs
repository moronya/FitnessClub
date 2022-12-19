using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Auth
{
    public class RegisterModel
    {
        public string? username { get; set; }

        [EmailAddress]
        public string? email { get; set; }

        public string? password { get; set; }
    }
}
