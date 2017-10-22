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

namespace OSS.Core.WebSite.Controllers.Users
{
    /// <summary>
    ///   �û�ģ��
    /// </summary>
    [AllowAnonymous]
    public class PortalController : BaseController
    {
        #region �û���¼
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// �����¼
        /// </summary>
        /// <returns></returns>
    
        [HttpPost]
        public async Task<IActionResult> Login(UserRegLoginReq req)
        {
            var loginRes =await RegOrLogin(req, "portal/userlogin");

            return Json(loginRes);
        }

        private async Task<UserRegLoginResp> RegOrLogin(UserRegLoginReq req,string apiUrl)
        {
            var stateRes = CheckLoginModelState(req);
            if (!stateRes.IsSuccess())
                return stateRes.ConvertToResult<UserRegLoginResp>();

            var loginRes = await RestApiUtil.RestCoreApi<UserRegLoginResp>(apiUrl, req);
            if (!loginRes.IsSuccess()) return loginRes;

            Response.Cookies.Append(GlobalKeysUtil.UserCookieName, loginRes.token,
                new CookieOptions() {HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(30)});

            loginRes.return_url = Request.Cookies[GlobalKeysUtil.UserReturnUrlCookieName]??"/";
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

        #endregion
        
        #region  �û�ע��

        public IActionResult Registe()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registe(UserRegLoginReq req)
        {
            var regRes = await RegOrLogin(req, "portal/userregiste");

            return Json(regRes);
        }
        #endregion

    }


}