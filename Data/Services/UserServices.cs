using Backend.Data.DTOs;
using Backend.Data.IServices;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Backend.Data.Services
{
    public class UserServices:IUserServices
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserServices(AppDbContext context,IConfiguration configuration)
        {
                _context=context;
                _configuration=configuration;   
        }

        public async Task<bool> ConfirmEmail(string token)
        {
           bool tokenAvailable = await _context.Confirmations.AnyAsync(x => x.ConfirmationToken == token);
            if (!tokenAvailable)
            {
                throw new Exception("Invalid token");
            }

            var confirmation = await _context.Confirmations.FirstOrDefaultAsync(x => x.ConfirmationToken == token);

            if (DateTime.Parse(confirmation.ExpirationDate) < DateTime.Now)
            {
                throw new Exception("Token expired");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == confirmation.userId);

            user.EmailConfirmed = true;

            await _context.SaveChangesAsync();

            return true;


        }

        public async Task<List<User>> GetUsers()
        {
            var users = await _context.Users.Where(u=> u.EmailConfirmed==true).ToListAsync();
            if (users == null)
            {
                throw new Exception("There are no users");
            }
            return users;
        }

        public async Task<string> Login(Login login)
        {
           var user = await _context.Users.FirstOrDefaultAsync(u=> u.Email==login.EmailOrUsername || u.Username==login.EmailOrUsername);

            if(user==null)
            {
                throw new Exception("User not found");

            }else if (!user.EmailConfirmed)
            {
                throw new Exception("Please confirm your email");
            }

            var passwordHash = new PasswordHasher<User>();
            var result = passwordHash.VerifyHashedPassword(user, user.Password, login.Password);

            if(result==PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid password");
            }

            return CreateToken(user);
        }

        public async Task Register(UserDTO user)
        {
            bool emailExist =await _context.Users.AnyAsync(x => x.Email == user.Email);
            bool usernameExist =await _context.Users.AnyAsync(x => x.Username == user.Username);

            if(emailExist || usernameExist)
            {
                throw new Exception("Email or Username already exist");
            }

            var _user = new User()
            {
                Username = user.Username,
                Email = user.Email,
            };

            var passwordHash = new PasswordHasher<User>();
            _user.Password = passwordHash.HashPassword(_user, user.Password);

            await _context.Users.AddAsync(_user);
            await _context.SaveChangesAsync();


            var confirmation = new Confirmations()
            {
              userId = _user.Id,
              ConfirmationToken = CreateToken(_user),
              ExpirationDate = DateTime.Now.AddDays(1).ToString()
            };

            await _context.Confirmations.AddAsync(confirmation);
            await _context.SaveChangesAsync();

            await SendConfirmationEmail(_user.Email,confirmation.ConfirmationToken);

        }

        private async Task SendConfirmationEmail(string email, string confirmationToken)
        {
            var message = new MailMessage
            {
                From = new MailAddress("aldrinislami06@gmail.com", "aldrinislami06@gmail.com"),
                Subject = "Confirmation Email",
                Body = $@"
                    <html>
                      <body>
                       <p>Thank you for registering.</p>
                        <p>Please confirm your registration by clicking the following link:</p>
                        <a href=""http://127.0.0.1:5500/pages/confirm.html?token={confirmationToken}"">Click Here</a>
                        <br>
                      </body>
                    </html>",
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(email));


            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("aldrinislami06@gmail.com", "nmahmsinljiltuxr");

                try
                {
                    // Send the email
                    await client.SendMailAsync(message);
                }
                catch (SmtpException ex)
                {

                    throw new Exception("Failed to send confirmation email", ex);
                }
            }
        }

        private string CreateToken(User _user)
        {
            if (_user == null)
            {
                throw new ArgumentNullException(nameof(_user));
            }

            List<Claim> claims = new List<Claim> {
                 new Claim(ClaimTypes.Email, _user.Email),
            new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, _user.Username)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            notBefore: DateTime.UtcNow,
            signingCredentials: creds);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;

        }
    }
}
