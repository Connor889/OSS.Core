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
using OSS.Core.WebSite.AppCodes;
using OSS.Core.WebSite.AppCodes.Tools;
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
            var loginRes =await RegOrLogin(req, false, "portal/userlogin");

            return Json(loginRes);
        }

        private async Task<UserRegLoginResp> RegOrLogin(UserRegLoginReq req, bool isReg,string apiUrl)
        {
            var stateRes = CheckLoginModelState(req, isReg);
            if (!stateRes.IsSuccess())
                return stateRes.ConvertToResult<UserRegLoginResp>();

            var loginRes = await ApiUtil.PostCoreApi<UserRegLoginResp>(apiUrl, req);
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
        /// <param name="isReg">�Ƿ���ע��</param>
        /// <returns></returns>
        private ResultMo CheckLoginModelState(UserRegLoginReq req,bool isReg)
        {
            if (!ModelState.IsValid)
                return new ResultMo(ResultTypes.ParaError, GetVolidMessage());

            if (!Enum.IsDefined(typeof(RegLoginType), req.type))
                return new ResultMo(ResultTypes.ParaError, "δ֪���˺����ͣ�");
            
            if (req.type == RegLoginType.MobileCode || (req.type == RegLoginType.Mobile && isReg))
            {
                if (string.IsNullOrEmpty(req.pass_code))
                    return new ResultMo(ResultTypes.ParaError, "��֤�벻��Ϊ�գ�");
            }

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
            var regRes = await RegOrLogin(req, false, "portal/userregiste");

            return Json(regRes);
        }
        #endregion

    }


}