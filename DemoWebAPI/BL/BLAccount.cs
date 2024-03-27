using DemoWebAPI.Base.BL;
using DemoWebAPI.Base.Model;
using DemoWebAPI.Core.Model;
using DemoWebAPI.DL;
using DemoWebAPI.Model;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoWebAPI.BL
{
    public class BLAccount : BLBase<account>
    {
        protected DLAccount _dLAccount = new DLAccount();

        private const string AccessToken = "AccessToken";

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ServiceResponse> Login(account model)
        {
            ServiceResponse res = new ServiceResponse();

            try
            {
                res = await _dLAccount.Login(model);
            }
            catch (Exception)
            {
                throw new Exception();
            }

            return res;
        }

        /// <summary>
        /// Lưu thông tin đăng nhập vô cache
        /// </summary>
        /// <param name="response"></param>
        /// <param name="account"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual async Task<ServiceResponse> SetCacheRedis(ServiceResponse response, account account, CenterConfig config)
        {
            var claimIdentity = new ClaimsIdentity();
            claimIdentity.AddClaim(new Claim("aid", account != null ? "Auth-" + account.account_id.ToString() : ""));
            claimIdentity.AddClaim(new Claim("una", account != null ? account.user_name : ""));

            DateTime now = DateTimeUtility.GetNow();
            var tokenExprired = now.AddSeconds(config.AppSettings.AccessTokenExpiredTime);
            var key = Encoding.ASCII.GetBytes(config.AppSettings.JwtSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = config.AppSettings.JwtIssuer,
                Subject = claimIdentity,
                Expires = tokenExprired,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken kTokent = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(kTokent);
            object value = new
            {
                Token = token,
                TokenExprired = tokenExprired
            };

            response.OnSuccess(new Dictionary<string, object>()
            {
                {
                    AccessToken, value
                }
            });

            // Lưu thông tin đăng nhập vào cache
            ConnectionMultiplexer connectionCacheRedis = null;

            try
            {
                connectionCacheRedis = await _dLBase.GetConnectionCacheRedis();

                IDatabase db = connectionCacheRedis.GetDatabase();
                db.StringSet(AccessToken, JsonConvert.SerializeObject(value));

                TimeSpan expiry = TimeSpan.FromSeconds(config.AppSettings.AccessTokenExpiredTime);
                bool keyExpirySet = db.KeyExpire(AccessToken, expiry);
            }
            catch (Exception)
            {
                throw new Exception();
            }
            finally
            {
                _dLBase.CloseConnectionCacheRedis(connectionCacheRedis);
            }

            return response;
        }
    }
}
