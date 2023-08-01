using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailConfirmed { get; set; }=false;





        //Navigation Properties

        [JsonIgnore]
        public Confirmations Confirmations { get; set; }
    }
}
