using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using project.Models.ViewModels;
using project.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Humanizer;
using project.Models.Extension;


namespace project.Controllers
{
	public class AccountController : Controller
	{
        #region Ctor
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;
     

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            
        }


        #endregion

        #region Logout
        [HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
        #endregion

        #region Login
        public IActionResult Login(string? returnUrl = null)
        {
            // Pass the returnUrl to the view via ViewBag
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Redirect to the returnUrl if it's valid, otherwise to the default page
                    var redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
                    return Redirect(redirectUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            // If the model state is not valid, return to the login view
            // Pass the returnUrl back to the view in case of error
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        #endregion

        #region Registration
        public IActionResult Registration(string? returnUrl = null)
        {
            // Pass the returnUrl to the view via ViewBag
            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegisterViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName // Set the FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to the returnUrl if it's valid, otherwise to the default page
                    var redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
                    return Redirect(redirectUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Pass the returnUrl back to the view in case of errors
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        #endregion

        #region EditProfile

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var model = new EditProfileViewModel
                {
                    FullName = user.FullName,
                    BirthDate = user.BirthDate ?? DateTime.MinValue,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    PhoneNumber = user.PhoneNumber,
                    ExistingPhotoPath = user.Photo != null ? Convert.ToBase64String(user.Photo) : null
                };

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_EditProfilePartial", model);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "Error loading profile data.");
                return StatusCode(500, "An error occurred while loading the profile data.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return PartialView("_EditProfilePartial", model);
            //}

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.BirthDate = model.BirthDate;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.PostalCode = model.PostalCode;
            user.Country = model.Country;
            user.PhoneNumber = model.PhoneNumber;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await model.Photo.CopyToAsync(stream);
                    user.Photo = stream.ToArray();
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return PartialView("_EditProfilePartial", model);
        }



        #endregion


        #region ForgetPassword
        //public IActionResult ForgotPassword()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByEmailAsync(model.Email);
        //        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //        {
        //            // You can use TempData to pass messages between actions/views
        //            TempData["ErrorMessage"] = "If an account with that email exists, a password reset link will be sent to it.";
        //            return RedirectToAction("ForgotPasswordConfirmation");
        //        }

        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, protocol: HttpContext.Request.Scheme);

        //        try
        //        {
        //            await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log the exception or handle it accordingly
        //            ModelState.AddModelError("", "Unable to send email. Please try again later.");
        //            return View(model);
        //        }

        //        return RedirectToAction("ForgotPasswordConfirmation");
        //    }

        //    return View(model);
        //}


        //public IActionResult ForgotPasswordConfirmation()
        //{
        //    return View();
        //}


        //#endregion

        //#region ResetPassword
        //public IActionResult ResetPassword(string token = null)
        //{
        //    if (token == null)
        //    {
        //        throw new ApplicationException("A code must be supplied for password reset.");
        //    }
        //    var model = new ResetPasswordViewModel { Token = token };
        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user == null)
        //    {
        //        // Don't reveal that the user does not exist
        //        return RedirectToAction("ResetPasswordConfirmation");
        //    }

        //    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("ResetPasswordConfirmation");
        //    }

        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }
        //    return View(model);
        //}

        //public IActionResult ResetPasswordConfirmation()
        //{
        //    return View();
        //}

        #endregion

        #region AssignRole
        [HttpGet]
		public IActionResult AssignRole()
		{
			var users = _userManager.Users.ToList();
			return View(users);
		}

		[HttpPost]
		public async Task<IActionResult> AssignRole(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
				return View("NotFound");
			}

			var roleExists = await _roleManager.RoleExistsAsync(roleName);

			if (!roleExists)
			{
				ViewBag.ErrorMessage = $"Role with Name = {roleName} cannot be found";
				return View("NotFound");
			}

			var isInRole = await _userManager.IsInRoleAsync(user, roleName);

			if (!isInRole)
			{
				var result = await _userManager.AddToRoleAsync(user, roleName);

				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return RedirectToAction("Index", "Home");
		}
        #endregion

        #region userWithRole

        public async Task<IActionResult> UsersWithRoles()
		{
			var usersWithRoles = new List<UserWithRolesViewModel>();

			var users = _userManager.Users.ToList();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);

				var userWithRoles = new UserWithRolesViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					Roles = roles.ToList()
				};

				usersWithRoles.Add(userWithRoles);
			}

			return View(usersWithRoles);
		}
        #endregion

        #region AccessDenied
        public IActionResult AccessDenied()
		{
			return View();
		}
        #endregion
    }
}
