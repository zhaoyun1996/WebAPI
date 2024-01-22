using DemoWebAPI.Base.Interface;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ServiceRespone : IServiceReulst
    {
        /// <summary>
        /// Kết quả thực thi
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Mã lỗi chính
        /// </summary>
        public ServiceResponseCode Code { get; set; } = ServiceResponseCode.Success;

        /// <summary>
        /// Mã lỗi phụ
        /// </summary>
        public int SubCode { get; set; }

        /// <summary>
        /// Lỗi trả về của người dùng
        /// </summary>
        public string UserMessage { get; set; }

        /// <summary>
        /// Lỗi trả về của hệ thống
        /// </summary>
        public string SystemMessage { get; set; }

        /// <summary>
        /// Dữ liệu trả về
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Thời gian hiện tại của server
        /// </summary>
        public DateTime ServerTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Thời gian xử lý
        /// </summary>
        public TimeSpan RequestTime
        {
            get
            {
                return DateTime.Now - this.ServerTime;
            }
        }

        public double TotalTime { get; set; }

        public ServiceRespone() { }

        /// <summary>
        /// Gán giá trị trả về cho trường hợp success
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ServiceRespone OnSuccess(object data = null)
        {
            this.Data = data;

            return this;
        }

        /// <summary>
        /// Gán giá trị trả về trong trường hợp không có quyền
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ServiceRespone OnForbidden(object data = null)
        {
            this.Code = ServiceResponseCode.Forbidden;
            this.Data = data;
            this.SystemMessage = "Forbidden";
            return this;
        }

        /// <summary>
        /// Gián giá trị cho api response khi gặp Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public ServiceRespone OnException(Exception ex)
        {
            if(ex != null)
            {
                this.Success = false;
                this.Code = ServiceResponseCode.Error;
                this.SystemMessage = ex.ToString();
            }

            return this;
        }

        /// <summary>
        /// Gán giá trị khi gặp lỗi
        /// </summary>
        /// <param name="code"></param>
        /// <param name="subCode"></param>
        /// <param name="data"></param>
        /// <param name="userMessage"></param>
        /// <param name="systemMessage"></param>
        /// <returns></returns>
        public ServiceRespone OnError(ServiceResponseCode code, int subCode = 0, object data = null, string userMessage = "Error while process request.", string systemMessage = "")
        {
            this.Success = false;
            this.Code = code;
            
            if(subCode != 0)
            {
                this.SubCode = subCode;
            }

            if(data != null)
            {
                this.Data = data;
            }

            if(!string.IsNullOrEmpty(userMessage))
            {
                this.UserMessage = userMessage;
            }

            if(string.IsNullOrEmpty(systemMessage))
            {
                this.SystemMessage = code.ToString();
            }
            else
            {
                this.SystemMessage = systemMessage;
            }

            return this;
        }
    }
}
