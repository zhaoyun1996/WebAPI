using System.ComponentModel.DataAnnotations;

namespace DemoWebAPI.Model
{
    public class account
    {
        [Key]
        public Guid account_id { get; set; }

        public string user_name { get; set; }

        public string password { get; set; }
    }
}
