using DemoWebAPI.Base.Interface;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ServiceResponse : IServiceReulst
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
                return DateTime.Now - ServerTime;
            }
        }

        public double TotalTime { get; set; }

        public ServiceResponse() { }

        /// <summary>
        /// Gán giá trị trả về cho trường hợp success
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ServiceResponse OnSuccess(object data = null)
        {
            Data = data;

            return this;
        }

        /// <summary>
        /// Gán giá trị trả về trong trường hợp không có quyền
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ServiceResponse OnForbidden(object data = null)
        {
            Code = ServiceResponseCode.Forbidden;
            Data = data;
            SystemMessage = "Forbidden";
            return this;
        }

        /// <summary>
        /// Gián giá trị cho api response khi gặp Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public ServiceResponse OnException(Exception ex)
        {
            if(ex != null)
            {
                Success = false;
                Code = ServiceResponseCode.Error;
                SystemMessage = ex.ToString();
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
        public ServiceResponse OnError(ServiceResponseCode code, int subCode = 0, object data = null, string userMessage = "Error while process request.", string systemMessage = "")
        {
            Success = false;
            Code = code;
            
            if(subCode != 0)
            {
                SubCode = subCode;
            }

            if(data != null)
            {
                Data = data;
            }

            if(!string.IsNullOrEmpty(userMessage))
            {
                UserMessage = userMessage;
            }

            if(string.IsNullOrEmpty(systemMessage))
            {
                SystemMessage = code.ToString();
            }
            else
            {
                SystemMessage = systemMessage;
            }

            return this;
        }
    }
}
