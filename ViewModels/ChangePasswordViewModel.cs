using System.ComponentModel.DataAnnotations;

namespace ViewMyAssetDev.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "전화번호를 입력해주세요")]
        [Phone(ErrorMessage = "전화번호 형식으로 입력해주세요")]
        public string PhoneNum { get; set; }

        [Required(ErrorMessage = "비밀번호를 입력해주세요")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "비밀번호는 8자 이상 15자 이하로 입력해주세요")]
        [DataType(DataType.Password)]
        [Display(Name = "새로운 비밀번호")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "비밀번호를 확인해주세요")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "비밀번호가 일치하지 않습니다")]
        [Display(Name = "새로운 비밀번호 확인")]
        public string ConfirmNewPassword { get; set; }
    }
}
