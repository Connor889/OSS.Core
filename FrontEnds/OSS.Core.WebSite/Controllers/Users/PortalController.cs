using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OSS.Common.ComModels;
using OSS.Common.ComModels.Enums;
using OSS.Core.Infrastructure.Enums;
using OSS.Core.Infrastructure.Utils;
using OSS.Core.WebSite.Controllers.Users.Mos;
using OSS.Http.Mos;

namespace OSS.Core.WebSite.Controllers.Users
{
    /// <summary>
    ///   �û�ģ��
    /// </summary>
    [AllowAnonymous]
    public class PortalController : BaseController
    {
        #region �û���¼
        /// <summary>
        /// �û���¼ҳ
        /// </summary>
        /// <param name="rurl"></param>
        /// <param name="state"> �û�״̬����������Ȩ��ʱʹ�� </param>
        /// <returns></returns>
        public IActionResult Login(string rurl,int state)
        {
            if (!string.IsNullOrEmpty(rurl))
            {
                Response.Cookies.Append(GlobalKeysUtil.UserReturnUrlCookieName, rurl);
            }
            return View();
        }

        /// <summary>
        /// �����¼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<UserRegLoginResp> Login(UserRegLoginReq req)
        {
            var loginRes =await RegOrLogin(req, "/portal/userlogin");

            return loginRes;
        }
        
        #endregion
        
        #region  �û�ע��

        public IActionResult Registe()
        {
            return View();
        }

        [HttpPost]
        public async Task<UserRegLoginResp> Registe(UserRegLoginReq req)
        {
            var regRes = await RegOrLogin(req, "/portal/userregiste");
            return regRes;
        }
        #endregion
        private async Task<UserRegLoginResp> RegOrLogin(UserRegLoginReq req, string apiUrl)
        {
            var stateRes = CheckLoginModelState(req);
            if (!stateRes.IsSuccess())
                return stateRes.ConvertToResult<UserRegLoginResp>();

            var loginRes = await RestApiUtil.RestCoreApi<UserRegLoginResp>(apiUrl, req);
            if (!loginRes.IsSuccess()) return loginRes;

            Response.Cookies.Append(GlobalKeysUtil.UserCookieName, loginRes.token,
                new CookieOptions() { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(30) });

            loginRes.return_url = Request.Cookies[GlobalKeysUtil.UserReturnUrlCookieName] ?? "/";
            return loginRes;
        }


        /// <summary>
        ///   ������¼ʱ����֤ʵ�����
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private ResultMo CheckLoginModelState(UserRegLoginReq req)
        {
            if (!ModelState.IsValid)
                return new ResultMo(ResultTypes.ParaError, GetVolidMessage());

            if (!Enum.IsDefined(typeof(RegLoginType), req.type))
                return new ResultMo(ResultTypes.ParaError, "δ֪���˺����ͣ�");

            if (string.IsNullOrEmpty(req.pass_code)
                && string.IsNullOrEmpty(req.pass_word))
                return new ResultMo(ResultTypes.ParaError, "����д���������֤�룡");

            var validator = new DataTypeAttribute(
                req.type == RegLoginType.Mobile
                    ? DataType.PhoneNumber
                    : DataType.EmailAddress);

            return !validator.IsValid(req.name)
                ? new ResultMo(ResultTypes.ParaError, "��������ȷ���ֻ������䣡")
                : new ResultMo();
        }
        #region �������û���Ȩ
        /// <summary>
        /// ��Ȩ
        /// </summary>
        /// <param name="plat">ƽ̨���μ�ThirdPaltforms�� 10-΢��  20-֧����  30-���� </param>
        /// <param name="state">�ص���������</param>
        /// <param name="type">��Ȩ�ͻ������� 1-pc��web�� 2-Ӧ������������繫�ںţ� 4-Ӧ���ھ�Ĭ��Ȩ</param>
        /// <returns></returns>
        public async Task<IActionResult> auth(int plat, string state, int type)
        {
            var redirectUrl = $"{m_CurrentDomain}/oauth/receive/{plat}";
            var authUrl = $"/oauth/getoauthurl?plat={plat}&redirectUrl={redirectUrl}&state={state}&type={type}";

            var urlRes = await RestApiUtil.RestSnsApi<ResultMo<string>>(authUrl, null, HttpMothed.GET);
            if (urlRes.IsSuccess())
                return Redirect(urlRes.data);

            return Content(urlRes.msg);
        }

        private static readonly string loginUrl = ConfigUtil.GetSection("Authorize:LoginUrl")?.Value;

        /// <summary>
        /// ��Ȩ�ص�����
        /// </summary>
        /// <param name="plat"></param>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<IActionResult> receive(int plat, string code, string state)
        {
            var url = string.Concat("/portal/socialauth?plat=", plat, "&code=", code, "&state=", state);
            var userRes = await RestApiUtil.RestCoreApi<UserRegLoginResp>(url);

            if (!userRes.IsSuccess())
                return Redirect(string.Concat("/un/error?ret=", userRes.ret, "&message=", userRes.msg));

            Response.Cookies.Append(GlobalKeysUtil.UserCookieName, userRes.token,
                new CookieOptions() { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(30) });

            if (userRes.user.status >=0 )
            {
                var returnUrl = Request.Cookies[GlobalKeysUtil.UserReturnUrlCookieName] ?? "/";
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect(string.Concat(loginUrl, "?state=", userRes.user.status));
            }

      
        }
        #endregion

    }


}