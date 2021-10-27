﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SpeedTyping.Domain;
using SpeedTyping.Model.ViewModel;
using SpeedTyping.Service;
using SpeedTyping.Helper;

namespace SpeedTyping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountManager _accountManager;

        public AccountController(AccountManager accountManager)
        {
            _accountManager = accountManager;
        }
        [HttpGet]
        [Route("Get")]
        public void Get()
        {
            var xz = 2;
            xz++;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<object> RegistrateUser(RegistrationViewModel model)
        {
            var user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
            };
            var result = await _accountManager.UserManager.CreateAsync(user, model.Password);
            await _accountManager.UserManager.AddToRoleAsync(user, "user");
            return Ok(result);
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LoginUser(LoginViewModel model)
        {
            var isModelInvalid = model == null || model.UserName.IsNullOrEmpty() || model.Password.IsNullOrEmpty();
            if (isModelInvalid) return BadRequest(new { message = "Ошибка при передаче параметров" });

            var user = await _accountManager.UserManager.FindByNameAsync(model.UserName);
            if (user == null) return BadRequest(new { message = "Пользователь не найден" });

            var isPasswordCorrect = await _accountManager.UserManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordCorrect) return BadRequest(new { message = "Неверный пароль" });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.JWT_Secret));
            var options = new IdentityOptions();
            var role = await _accountManager.UserManager.GetRolesAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new ("UserID", user.Id),
                    new (options.ClaimsIdentity.RoleClaimType, role.FirstOrDefault() ?? string.Empty)
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            return Ok(new { token });

        }
        [HttpGet]
        [Authorize]
        [Route("UserInfo")]
        public async Task<object> GetUserInfo()
        {
            var userId = User.Claims.First(x => x.Type == "UserID").Value;
            var user = await _accountManager.UserManager.FindByIdAsync(userId);
            return new
            {
                user.UserName
            };
        }
    }
}