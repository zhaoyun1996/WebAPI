using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public partial class BaseModel : BaseEntity
    {
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("xmin", TypeName = "xid")]
        public long? edit_version { get; set; }

        [NotMapped]
        public ModelState state { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? modified_date { get; set; }

        public string? created_by { get; set; }

        public string? modified_by { get; set; }

        public object SetAutoPrimaryKey()
        {
            PropertyInfo[] props = MemoryCacheService.GetPropertyInfo(this.GetType());
            PropertyInfo propertyInfoKey = null;
            object key = "";
            if(props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                if(propertyInfoKey != null)
                {
                    key = this.SetAutoPrimaryKey(propertyInfoKey);
                }
            }
            return key;
        }

        public virtual object SetAutoPrimaryKey(PropertyInfo propertyInfoKey)
        {
            object key = "";
            if(propertyInfoKey.PropertyType == typeof(Guid)) {
                key = Guid.NewGuid();
                propertyInfoKey.SetValue(this, key);
            }
            return key;
        }
    }
}
