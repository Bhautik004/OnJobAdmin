using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminEntity.AdminModel
{
    class Company
    {
        //Company Id
        public int Cmp_id { get; set; }

        //Email
        [Required(ErrorMessage = "Email Is Required.")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+))@([A-Za-z0-9]+)(([\.\-‌​]?[a-zA-Z0-9]+))\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        //Password
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        //Company Name
        [Required(ErrorMessage = "Company Name Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string Company_name { get; set; }

        //Category Id
        [Required(ErrorMessage = "Please Select Category!")]
        public int Category_id { get; set; }

        //No Of Employee
        [Required(ErrorMessage = "Please Select Number Employee!")]
        public int No_Emp { get; set; }

        //Orgnization Type
        [Required(ErrorMessage = "Please Select Number Employee!")]
        public String Org_type { get; set; }

        //State Id
        [Required(ErrorMessage = "Please Select State!")]
        public int State_id { get; set; }

        //City Id
        [Required(ErrorMessage = "Please Select City!")]
        public int City_id { get; set; }

        //Country
        public string Country { get; set; }

        //Address
        [Required(ErrorMessage = "Address Is Required.")]
        public string Address { get; set; }

        //Mobile No
        [Required(ErrorMessage = "Mobile Number Is Required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public long Mobile_no { get; set; }

        //Web Site Name
        [Required(ErrorMessage = "Web Site Name Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string WebSite { get; set; }

        //Company Description
        [Required(ErrorMessage = "Company Description Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public String Compnay_Desc { get; set; }

        //Is_active (Show Company's States)
        public int Is_active { get; set; } //1 (By Default Status)
    }
}
