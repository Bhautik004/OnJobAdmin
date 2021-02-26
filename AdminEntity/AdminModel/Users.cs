using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminEntity.AdminModel
{
    class Users
    {
        public int User_id { get; set; }

        //First Name
        [Required(ErrorMessage = "First Name Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string FirstName { get; set; }

        //Last Name
        [Required(ErrorMessage = "Last Name Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string LastName { get; set; }
        
        //Emaill 
        [Required(ErrorMessage = "Email Is Required.")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+))@([A-Za-z0-9]+)(([\.\-‌​]?[a-zA-Z0-9]+))\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        //Password
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Is Required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        //Date Of Birth
        [Required(ErrorMessage = "Please Select Date Of Birth")]
        public DateTime Dob { get; set; }

        //Age
        [Required(ErrorMessage = "Please Select Age")]
        public int Age { get; set; }

        //Profile Picture Path
        public string Profile_picture_path { get; set; }

        //Mobile No
        [Required(ErrorMessage = "Mobile Number Is Required.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public long MobileNo { get; set; }

        //Category
        [Required(ErrorMessage = "Please Select Category!")] 
        public int Category { get; set; }

        //Job Title
        [Required(ErrorMessage = "Job Title Is Required.")]
        public string Job_title { get; set; }

        //Country
        public string Country { get; set; }

        //State
        [Required(ErrorMessage = "Please Select State!")]
        public int State_id { get; set; }
        
        //City
        [Required(ErrorMessage = "Please Select City!")]
        public int City_id { get; set; }
        
        //Address
        [Required(ErrorMessage = "Address Is Required.")]
        public string Address { get; set; }

        //Experience
        [Required(ErrorMessage = "Please Select Experience!")]
        public string Experience { get; set; }

        //Skills
        [Required(ErrorMessage = "Skill Is Required")]
        public string Skills { get; set; }
        
        //Current Salary
        [Required(ErrorMessage = "Current Salary Is Required.")]
        public int Current_salary { get; set; }

        //Expected Salary
        [Required(ErrorMessage = "Expected Salary Is Required.")]
        public int Expected_salary { get; set; }

        //Resume
        public string Resume { get; set; }

        //Resume Path
        public string Resume_path { get; set; }

        //Is_active (Show User's States)
        public int Is_active { get; set; } //1 (By Default Status)
    }
}
