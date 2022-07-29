using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        // Registration end point
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDto) {
            
            // First, check if the user already exists
            if (await UserExists(registerDto.UserName)) return BadRequest("Username is taken");

            // If the user registration is unique, then continue...
            using var hmac = new HMACSHA512();

            var user = new AppUser {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key                                                               
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDTO loginDto) {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName.ToLower() == loginDto.UserName.ToLower());

            // If no user with the given username is found, then the username is invalid
            if (user == null) return Unauthorized("Invalid username");

            // If we do find a user
            // We need to reverse the Password hash, using the PasswordSalt
            // Using the overlaod of the HMACSHA512 class, we can pass in the PasswordSalt
            // to get back the PasswordHash that was first created
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // If the computedHash is the same as the original PasswordHash with the same PasswordSalt
            // Then we know that the password that the user entered is correct

            // Let's loop over each character of the byte array and make sure that they are the same
            for (int i = 0; i < computedHash.Length; ++i) {

                // If ANY character is not the same, then return "Unauthorized"
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            // else, return the user object
            return new UserDto {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        // Helper method to ensure that username is unique
        private async Task<bool> UserExists(string username) {
            // Return true or false depending on whether user is already in the database
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }
    }
}