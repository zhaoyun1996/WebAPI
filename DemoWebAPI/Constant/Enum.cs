namespace DemoWebAPI.Constant
{
    public class Enum
    {
        public enum ServiceResponseCode : int
        {
            /// <summary>
            /// Thành công
            /// </summary>
            Success = 0,

            /// <summary>
            /// Dữ liệu truyền lên không hợp lệ
            /// </summary>
            InvalidData = 1,

            /// <summary>
            /// Dữ liệu không tồn tại
            /// </summary>
            NotFound = 2,

            /// <summary>
            /// Trùng dữ liệu
            /// </summary>
            Duplicate = 3,

            /// <summary>
            /// Cần người dùng xác nhận
            /// </summary>
            RequireConfirm = 4,

            /// <summary>
            /// Dữ liệu đã bị thay đổi
            /// </summary>
            ObsoleteVersion = 5,

            /// <summary>
            /// Gặp lỗi
            /// </summary>
            Error = 99,

            /// <summary>
            /// Không có quyền thực hiện hành động
            /// </summary>
            Forbidden = 403,

            /// <summary>
            /// Lỗi hệ thống
            /// </summary>
            Exception = 999
        }

        public enum ModelState : int
        {
            None = 0,
            Insert = 1,
            Update = 2,
            Delete = 3,
            Duplicate = 4
        }

        public enum DBDataType : int
        {
            none = 0,
            uuid = 1,
            numeric = 2,
            boolean = 3,
            timestamp = 4,
            timestamp_with_time_zone = 5,
            integer = 6,
            bigint = 7,
            date = 8,
            text = 9,
            uuid_arr = 10,
            numeric_arr = 11,
            boolean_arr = 12,
            timestamp_arr = 13,
            integer_arr = 14,
            bigint_arr = 15,
            date_arr = 16,
            text_arr = 17,
            smallint = 18,
            smallint_arr = 19
        }

        public enum EnumColDataType
        {
            JSON = 1
        }

        public enum EnumDataType : int
        {
            None = 0,
            String = 1,
            Boolean = 2,
            DateTime = 3,
            Number = 4,
            Decimal = 5,
            DateTimeFull = 6,
            Guid = 10
        }
    }
}
