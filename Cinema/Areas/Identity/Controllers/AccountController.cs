using Cinema.ModelView;
using Cinema.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOtp> _applicationUserOtpRepository;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IRepository<ApplicationUserOtp> applicationUserOtpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
        }

        // Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Name = registerVM.Name,
                Address = registerVM.Address,
                UserName = registerVM.UserName,
                Email = registerVM.Email
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerVM);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", id = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(
                registerVM.Email,
                "Cinema Confirmation Mail ",
                $"<h1> please click <a href='{link}'> here</a> to confirm Your Mail</h1>"
                );
            TempData["Successful_Notification"] = "Registration Successfully";
            return RedirectToAction(nameof(Login));
        }

        // Confirm Email

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                TempData["Error_Notification"] = "Error happened while confirming email.";
                return RedirectToAction(nameof(Login));
            }

            TempData["Successful_Notification"] = "Email confirmed successfully.";

            return RedirectToAction(nameof(Login));
        }

        // Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail)
                ?? await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or email or password.");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                loginVM.Password,
                loginVM.RememberMe,
                true
            );

            if (result.Succeeded)
                return RedirectToAction("Index", "Home", new { area = "Client" });

            if (result.IsNotAllowed)
                ModelState.AddModelError("", "Please confirm your email first.");
            else if (result.IsLockedOut)
                ModelState.AddModelError("", "Your account is locked. Try again later.");
            else
                ModelState.AddModelError("", "Invalid username or password.");

            return View(loginVM);
        }

        // Resend Email Confirmation

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(
            ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOrEmail)
                ?? await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or email.");
                return View(resendEmailConfirmationVM);
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["Successful_Notification"] = "Email is already confirmed.";
                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(
                "ConfirmEmail",
                "Account",
                new
                {
                    area = "Identity",
                    id = user.Id,
                    token = token
                },
                Request.Scheme
            );

            await _emailSender.SendEmailAsync(
                user.Email,
                "Cinema Confirmation Mail",
                $"<h1>Please click <a href='{link}'>here</a> to confirm your email.</h1>"
            );

            TempData["Successful_Notification"] = "Confirmation email sent successfully.";

            return RedirectToAction(nameof(Login));
        }

        // Forget Password

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(forgetPasswordVM);
            }

            var user = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOrEmail)
                ?? await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user.");
                return View(forgetPasswordVM);
            }

            string otp = new Random().Next(1000, 10000).ToString();

            var applicationUserOtp = new ApplicationUserOtp(user.Id, otp);

            await _applicationUserOtpRepository.AddAsync(applicationUserOtp);
            await _applicationUserOtpRepository.CommitAsync();

            await _emailSender.SendEmailAsync(
                user.Email,
                "Cinema Forget Password",
                $"Use this OTP {otp} to reset your password."
            );

            TempData["Successful_Notification"] = "OTP sent successfully.";

            return RedirectToAction(nameof(VerifyOTP), new { userId = user.Id });
        }

        // Verify OTP

        [HttpGet]
        public IActionResult VerifyOTP(string userId)
        {
            return View(new VerifyOTPVM() { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(VerifyOTPVM verifyOTPVM)
        {
            if (!ModelState.IsValid)
            {
                return View(verifyOTPVM);
            }

            var user = await _userManager.FindByIdAsync(verifyOTPVM.UserId);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user.");
                return View(verifyOTPVM);
            }

            var applicationUserOtps = await _applicationUserOtpRepository.GetAllAsync(a =>
                a.ApplicationUserId == user.Id &&
                a.IsValid &&
                a.ValidTo >= DateTime.UtcNow
            );

            var applicationUserOtp = applicationUserOtps
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (applicationUserOtp == null || applicationUserOtp.OTP != verifyOTPVM.OTP)
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return View(verifyOTPVM);
            }

            applicationUserOtp.IsValid = false;

            await _applicationUserOtpRepository.CommitAsync();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return RedirectToAction(nameof(ResetPassword), new
            {
                userId = user.Id,
                token = token
            });
        }

        // Reset Password

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            return View(new ResetPasswordVM()
            {
                UserId = userId,
                Token = token
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordVM);
            }

            var user = await _userManager.FindByIdAsync(resetPasswordVM.UserId);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user.");
                return View(resetPasswordVM);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                resetPasswordVM.Token,
                resetPasswordVM.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(resetPasswordVM);
            }

            TempData["Successful_Notification"] = "Password reset successfully.";

            return RedirectToAction(nameof(Login));
        }
    }
}
