﻿
using HatliFood.Data;
using HatliFood.Models;
using HatliFood.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace HatliFood.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _Context;

        public AccountController(UserManager<IdentityUser> userManager , SignInManager<IdentityUser> signInManager , ApplicationDbContext Context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _Context = Context;
        }
        
        public IActionResult Login()
        {
            var response = new LoginVM();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }
            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);

            if (user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                
                if(passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                    if(result.Succeeded)
                    {
                        return RedirectToAction("Index", "Buyers");
                    }
                }
                TempData["Error"] = "Wrong crendentials ,  try again!";
                return View(loginVM);
            }
            TempData["Error"] = "Wrong crendentials ,  try again!";
            return View(loginVM);
        }
    
    
        public IActionResult Register()
        {
            var response = new RegisterVM();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This Email address is already in use";
                return View(registerVM);
            }

            var newUser = new IdentityUser()
            {
                Email = registerVM.EmailAddress,
                UserName = registerVM.EmailAddress
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if(newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);

                var newBuyer = new Buyer()
                {
                    UserId = newUser.Id,
                    FirstName = registerVM.FisrtName,
                    LastName = registerVM.LastName
                };
                _Context.Buyers.Add(newBuyer);
                _Context.SaveChanges();
            }

            return View("RegisterCompleted");
        }
    }
}
