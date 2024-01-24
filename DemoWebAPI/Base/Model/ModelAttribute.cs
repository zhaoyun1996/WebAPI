using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ModelAttribute
    {
        public class UpdateIgnoreAttribute : Attribute { }

        public class DeleteIgnoreAttribute : Attribute { }

        public class ColDataTypeAttribute : Attribute
        {
            public ColDataTypeAttribute(EnumColDataType cType)
            {
                this.cType = cType;
            }

            public EnumColDataType cType;
        }

        public class ViewAttribute : Attribute
        {
            public string ViewName { get; set; }

            public ViewAttribute(string viewName)
            {
                this.ViewName = viewName;
            }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class UniqueFieldAttribute : Attribute
        {
        }
    }
}
