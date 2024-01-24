using DemoWebAPI.Base.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DemoWebAPI.Base.Model.ModelAttribute;

namespace DemoWebAPI.Model
{
    [Table("account", Schema = "public")]
    public partial class account : BaseModel
    {
        [Key]
        public Guid account_id { get; set; }

        [UniqueField]
        public string user_name { get; set; }

        public string password { get; set; }
    }
}
