using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoWebAPI.Model
{
    [Table("account", Schema = "public")]
    public class account
    {
        [Key]
        public Guid account_id { get; set; }

        public string user_name { get; set; }

        public string password { get; set; }
    }
}
