using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Confirmations
    {
        [Key]
        public int ID { get; set; }

        public int userId { get; set; }
        public User User { get; set; }
        public string ConfirmationToken { get; set; }
        public string ExpirationDate { get; set; }

    }
}
