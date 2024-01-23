using System.ComponentModel.DataAnnotations.Schema;

namespace DemoWebAPI.Base.Interface
{
    public interface ITrackingModel<T>
    {
        [NotMapped]
        T old_data { get; set; }
    }
}
