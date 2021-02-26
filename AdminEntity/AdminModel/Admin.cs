using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminEntity.AdminModel
{
    public class Admin
    {
        //Admin_id
        public int Admin_id { get; set; }

        //Email
        [Required(ErrorMessage = "Email Is Required.")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+))@([A-Za-z0-9]+)(([\.\-‌​]?[a-zA-Z0-9]+))\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        //Password
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        //Mobile Number
        [Required(ErrorMessage = "Mobile Number Is Required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public long Mobileno { get; set; }
    }
}