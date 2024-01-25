using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ModelAttribute
    {
        public class UpdateIgnoreAttribute : Attribute { }

        public class DeleteIgnoreAttribute : Attribute { }

        public class ColDataTypeAttribute : Attribute
        {
            public EnumColDataType cType;

            public ColDataTypeAttribute(EnumColDataType colType)
            {
                cType = colType;
            }
        }

        public class ViewAttribute : Attribute
        {
            public string ViewName { get; set; }

            public ViewAttribute(string viewName)
            {
                ViewName = viewName;
            }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class UniqueFieldAttribute : Attribute
        {
        }
    }
}
