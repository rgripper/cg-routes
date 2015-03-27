using Cleangorod.Data;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Cleangorod.Web.Controllers
{
    public class IdentityAwareApiController : ApiController
    {
        protected const string LocalLoginProvider = "Local";

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        public IdentityAwareApiController()
        {
            HttpContext.Current.Response.SuppressFormsAuthenticationRedirect = true;
        }

        public IdentityAwareApiController(ApplicationUserManager userManager, ApplicationRoleManager roleManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;

            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? Request.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }

                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }

            base.Dispose(disposing);
        }

    }
}