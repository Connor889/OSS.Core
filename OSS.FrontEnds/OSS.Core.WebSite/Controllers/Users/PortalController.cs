using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OSS.Common.ComModels;
using OSS.Common.ComModels.Enums;
using OSS.Core.Infrastructure.Enums;
using OSS.Core.Infrastructure.Utils;
using OSS.Core.WebSite.Controllers.Users.Mos;

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
            ViewBag.UserState = state;
            if (!string.IsNullOrEmpty(rurl))
            {
                Response.Cookies.Append(GlobalKeysUtil.UserReturnUrlCookieName, rurl);
            }
            return View();
        }

        
        [HttpPost]
        public async Task<UserRegLoginResp> CodeLogin(CodeLoginReq req)
        {
            var regRes = await RegOrLogin(req, "/member/portal/CodeLogin");
            return regRes;
        }

        private async Task<UserRegLoginResp> RegOrLogin(CodeLoginReq req, string apiUrl)
        {
            var stateRes = CheckLoginModelState(req);
            if (!stateRes.IsSuccess())
                return stateRes.ConvertToResultInherit<UserRegLoginResp>();

            var loginRes = await RestApiUtil.PostCoreApi<UserRegLoginResp>(apiUrl, req);
            if (!loginRes.IsSuccess()) return loginRes;

            Response.Cookies.Append(GlobalKeysUtil.UserCookieName, loginRes.token,
                new CookieOptions() { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(30) });

            loginRes.return_url = Request.Cookies[GlobalKeysUtil.UserReturnUrlCookieName] ?? "/";
            return loginRes;
        }


        #endregion




        /// <summary>
        ///   ������¼ʱ����֤ʵ�����
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private ResultMo CheckLoginModelState(CodeLoginReq req)
        {
            if (!ModelState.IsValid)
                return new ResultMo(ResultTypes.ParaError, GetVolidMessage());

            if (!Enum.IsDefined(typeof(RegLoginType), req.type))
                return new ResultMo(ResultTypes.ParaError, "δ֪���˺����ͣ�");
            
            if (string.IsNullOrEmpty(req.passcode)
                && string.IsNullOrEmpty(req.password))
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
            var authUrl = $"/sns/oauth/getoauthurl?plat={plat}&redirectUrl={redirectUrl}&state={state}&type={type}";

            var urlRes = await RestApiUtil.RestApi<ResultMo<string>>(authUrl, null, HttpMethod.Get);
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
            var url = string.Concat("/member/portal/socialauth?plat=", plat, "&code=", code, "&state=", state);
            var userRes = await RestApiUtil.PostCoreApi<UserRegLoginResp>(url);

            if (!userRes.IsSuccess())
                return Redirect(string.Concat("/un/error?ret=", userRes.ret, "&message=", userRes.msg));

            Response.Cookies.Append(GlobalKeysUtil.UserCookieName, userRes.token,
                new CookieOptions() { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(30) });

            if (userRes.user.status >=0 )
            {
                var returnUrl = Request.Cookies[GlobalKeysUtil.UserReturnUrlCookieName] ?? "/";
                return Redirect(returnUrl);
            }
            return Redirect(string.Concat(loginUrl, "?state=", userRes.user.status));
        }
        #endregion

    }


}