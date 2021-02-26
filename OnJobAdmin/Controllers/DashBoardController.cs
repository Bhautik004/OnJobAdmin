using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin_Entity.Entity;

namespace OnJObAdmin.Controllers
{
    public class DashBoardController : Controller
    {
        Admin_Bal.Bal.AdminBal _adminBal = new Admin_Bal.Bal.AdminBal();
        Admin_Entity.Entity.AdminModel _adminModel = new Admin_Entity.Entity.AdminModel();
        // GET: DashBoard
        [Authorize]
        public ActionResult Index()
        {

            _adminModel = _adminBal.GetAllCount();
            _adminModel.lstCompany = _adminBal.GetCompany();
            _adminModel.lstUser = _adminBal.GetLatestUser();
        
            return View(_adminModel);
        }
    }
}