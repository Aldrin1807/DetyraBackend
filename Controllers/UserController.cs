using Azure;
using Backend.Data.DTOs;
using Backend.Data.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _service;
        public UserController(IUserServices service)
        {
            _service = service;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register ([FromBody]UserDTO user)
        {
            try
            {
                await _service.Register(user);
                return Ok(new Response
                { Status = "Success", Message = "User registered succesfully." });
            }
            catch (Exception e)
            {
                return Ok(new Response
                { Status = "Error", Message = e.Message });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            try
            {
               var token =  await _service.Login(login);
                return Ok(new Response
                { Status = "Success", Message = token });
            }
            catch (Exception e)
            {
                return Ok(new Response
                { Status = "Error", Message = e.Message });
            }
        }

        [Authorize]
        [HttpGet("getusers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _service.GetUsers();
                return Ok(users);
            }
            catch (Exception e)
            {
                return Ok(new Response
                { Status = "Error", Message = e.Message });
            }
        }

        [HttpPost("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            try
            {
                bool result = await _service.ConfirmEmail(token);
                return Ok(new Response
                { Status = "Success", Message = result.ToString() });
            }
            catch (Exception e)
            {
                return Ok(new Response
                { Status = "Error", Message = e.Message });
            }
        }


    }
}
