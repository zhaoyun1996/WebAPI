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
    }
}
