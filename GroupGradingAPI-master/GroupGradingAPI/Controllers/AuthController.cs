﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GroupGradingAPI.Data;
using GroupGradingAPI.Models;
using GroupGradingAPI.ViewModel;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GroupGradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly GradingContext _context;

        public AuthController(GradingContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [EnableCors("AllAccessCors")]
        [HttpPost]
        public async Task<ActionResult> InsertUser([FromBody] IdentityUser model)
        {
            try
            {
                Guid newGuid = Guid.NewGuid();
                var user = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    PasswordHash = model.PasswordHash,
                    SecurityStamp = newGuid.ToString(),
                };
                var result = await _userManager.CreateAsync(user, model.PasswordHash);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");
                }

                _context.Add(user);

               
                await _context.SaveChangesAsync();
                return Ok(new { Username = user.UserName, response = true });
            }
            catch (Exception e)
            {
                return Ok(new { Error = e.Message, response = false });
            }
        }

        [EnableCors("AllAccessCors")]
        [HttpPost("login")]
        public async Task<ActionResult> Login(IdentityUser model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.PasswordHash))
            {
                var claim = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claim, "Token");
                var userRoles = await _userManager.GetRolesAsync(user);

                foreach (var role in userRoles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                var signinKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));

                int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);

                var token = new JwtSecurityToken(
                  issuer: _configuration["Jwt:Site"],
                  audience: _configuration["Jwt:Site"],
                  claims: claimsIdentity.Claims,
                  expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                  signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(
                  new
                  {
                      token = new JwtSecurityTokenHandler().WriteToken(token),
                      expiration = token.ValidTo
                  });
            }
            return Unauthorized();
        }


    }
}