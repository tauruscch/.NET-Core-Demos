using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GeekTime.Mobile.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BankAccountController : Controller
    {
        // 表示使用默认的认证方案，我们注册时默认设置的cookie，该接口支持使用cookie认证
        [Authorize]
        public IActionResult Cookie()
        {
            return Content("bank account");
        }

        // 该接口只支持JWT认证
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Jwt()
        {
            return Content(User.FindFirst("Name").Value);
        }

        // 通过逗号分隔，表示该接口可以同时支持JWT和Cookie认证
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult AnyOne()
        {
            return Content(User.FindFirst("Name").Value);
        }
    }
}