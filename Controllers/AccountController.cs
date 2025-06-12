using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using ViewMyAssetDev.Models;
using ViewMyAssetDev.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ViewMyAssetDev.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private bool isPhoneVerified;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)  
            {
                var user = await userManager.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null) // 아이디에 해당하는 유저가 있는 경우
                {
                    if (await userManager.CheckPasswordAsync(user, model.Password)) // 비밀번호가 일치하는 경우
                        await signInManager.SignInAsync(user, model.RememberMe);
                    else
                    {
                        ModelState.AddModelError("", "올바르지 않은 비밀번호입니다");
                        return View(model);
                    }
                }

                else
                {
                    ModelState.AddModelError("", "가입되지 않은 아이디입니다");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            isPhoneVerified = false;

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.PhoneNum);

                if (user != null) // 가입된 휴대전화번호 존재하는 경우
                {
                    ModelState.AddModelError("", "이미 가입된 번호입니다");
                    return View(model); 
                }

                if (!isPhoneVerified) // 번호 인증 안된 경우
                {
                    ModelState.AddModelError("", "번호 인증이 필요합니다");
                    return View(model);
                }

                Users users = new Users()
                {
                    UserId = model.UserId,
                    UserName = model.PhoneNum
                };

                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded) // 회원가입 성공했을 경우
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var errors in result.Errors)
                        ModelState.AddModelError("", errors.Description);

                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPhoneNum(VerifyPhoneNumViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.PhoneNum);

                if (user == null) // 가입된 휴대번호가 없을 경우
                {
                    ModelState.AddModelError("", "가입되지 않은 전화번호입니다");
                    return View(model);
                }
                else
                {
                    if (await IsVerifiedPhone(model.PhoneNum)) // 휴대폰 인증 성공 한 경우
                        return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                    else
                    {
                        ModelState.AddModelError("", "번호 인증에 실패했습니다");
                        return View(model);
                    }
                }
            }
            return View(model);
        }

        public IActionResult VerifyPhoneNum()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.PhoneNum);

                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);

                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var errors in result.Errors)
                            ModelState.AddModelError("", errors.Description);

                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "가입되지 않은 이메일입니다");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "입력 형식을 확인해주시고 다시 시도해주세요");
                return View(model);
            }
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("VerifyPhoneNum", "Account");
            
            return View(new ChangePasswordViewModel { PhoneNum = username });
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("UnAuthenticatedMain", "UnAuthenticated");
        }

        public async Task<bool> IsVerifiedPhone(string phoneNum)
        {
            return true;
        }
    }
}
