using FitnessClub.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitnessClub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "error", message = "The email has already been taken. Use another email and try again!" });
            }
            IdentityUser user = new(){
                Email = model.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.username                
            };
            var result = await _userManager.CreateAsync(user, model.password);
            if (!result.Succeeded) {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "Error", message = "User creation failed! Please check user details and try again." });
            }
            return Ok(new Response { status = "Success", message = "User created successfully!" });

        }
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.email);
            if (userExists != null) {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "error", message = "Sorry, the email seems to be taken already. Kindly use a different email address" });
            }
            IdentityUser user = new()
            {
                Email = model.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.username
            };
            var result = await _userManager.CreateAsync(user, model.password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "error", message = "Account creation was not successfull. Kindly check your credentials and try again" });
            }
            if (!await _roleManager.RoleExistsAsync(UserRoles.admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.trainer))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.trainer));

            if (await _roleManager.RoleExistsAsync(UserRoles.admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.trainer);
            }
            return Ok(new Response { status = "Success", message="User created successfully"});
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.email);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                // add all user roles
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();

        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(24),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            return token;
        }
    }

}
