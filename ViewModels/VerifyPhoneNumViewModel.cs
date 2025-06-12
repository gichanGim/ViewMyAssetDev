using System.ComponentModel.DataAnnotations;

namespace ViewMyAssetDev.ViewModels
{
    public class VerifyPhoneNumViewModel
    {
        [Required(ErrorMessage = "전화번호를 입력해주세요")]
        [Phone(ErrorMessage = "전화번호 형식으로 입력해주세요")]
        public string PhoneNum { get; set; }
    }
}
