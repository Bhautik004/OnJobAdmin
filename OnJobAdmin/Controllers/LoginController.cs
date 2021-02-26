using AdminEntity.AdminModel;
using System.Web.Mvc;
using System.Web.Security;
using AdminBal.Bal;

namespace OnJObAdmin.Controllers
{
    public class LoginController : Controller
    {

        AdminBal.Bal.admin_bal _adminBal = new AdminBal.Bal.admin_bal();
        AdminEntity.AdminModel.Admin _adminModel = new AdminEntity.AdminModel.Admin();
        
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult Index(Admin ObjAdmin)
        //{
           
        //    _adminModel = _adminBal.GetLoginDetails(ObjAdmin);

        //    if (_adminModel.Email != null)
        //    {
        //        Session["Email"] = _adminModel.Email;
        //        Session["Password"] = _adminModel.Password;
        //        Session["Id"] = _adminModel.id;
        //        Session["Name"] = _adminModel.Username;
                
        //        TempData["Message"] = "2";
        //        FormsAuthentication.SetAuthCookie(_adminModel.Email, false);
        //        return RedirectToAction("Index", "DashBoard", _adminModel);//method,controller,object
        //    }
        //    else
        //    {
        //        TempData["Message"] = "1";
        //        ModelState.Clear();
        //        return View();
        //    }
        //}

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            TempData["Message"] = "Logout Successfully";
            return RedirectToAction("Index", "Login");
        }
    }
}