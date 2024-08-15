using AdminMVC.Data;
using AdminMVC.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using WebAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using Super_Cartes_Infinies.Models.Dtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO loginDTO)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDTO.UserName, loginDTO.Password, true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // À la place, on pourrait simplement obtenir le NameIdentifier de la propriété User (comme on a simplement de son Id)
                IdentityUser identityUser = _dbContext.Users.Single(u => u.UserName == loginDTO.UserName);
                

                // Note: On ajoute simplement le NameIdentifier dans les claims. Le login pour l'admin ne se fait pas par cette methode de toute facon
                List<Claim> authClaims = new List<Claim>();
                authClaims.Add(new Claim(ClaimTypes.NameIdentifier, identityUser.Id));

                SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("C'est tellement la meilleure cle qui a jamais ete cree dans l'histoire de l'humanite (doit etre longue)"));

                string issuer = this.Request.Scheme + "://" + this.Request.Host;

                // TODO: Modifier issuer en production
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: null,
                    claims: authClaims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
                );

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(tokenString);
            }

            return NotFound(new { Error = "L'utilisateur est introuvable ou le mot de passe ne concorde pas" });
        }


        [HttpPost]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {

            if (registerDTO.Password != registerDTO.PasswordConfirm)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Le mot de passe et la confirmation ne sont pas identique" });
            }

            IdentityUser user = new IdentityUser()
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email
            };
            IdentityResult identityResult = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!identityResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = identityResult.Errors });
            }

            return await Login(new LoginDTO() { UserName = registerDTO.UserName, Password = registerDTO.Password });
        }
    }
}
