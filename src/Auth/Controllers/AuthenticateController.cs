
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Auth.IdentityAuth;
using Auth.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Controllers;

// TODO: Logout
// TODO: Cookie instead of Bearer. https://docs.microsoft.com/es-es/aspnet/core/security/authentication/cookie?view=aspnetcore-6.0

[Route("api/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticateController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterModel model
    )
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);

        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new Response { Status = "Error", Message = "User already exists" }
            );
        }

        ApplicationUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                }
            );
        }

        return Ok(new Response { Status = "Success", Message = "User create successfully!" });
    }

    [HttpPost]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin(
        [FromBody] RegisterModel model
    )
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);

        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new Response { Status = "Error", Message = "User already exists" }
            );
        }

        ApplicationUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                }
            );
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

        if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);

        return Ok(new Response { Status = "Success", Message = "User create successfully!" });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginModel model
        )
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user is null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);
        var authClaims = new List<Claim>
        {
          new Claim(ClaimTypes.Name, user.UserName),
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            signingCredentials: new SigningCredentials(
                authSigningKey,
                SecurityAlgorithms.HmacSha256
            )
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }
}