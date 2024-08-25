using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using WebAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Super_Cartes_Infinies.Models.Dtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public ActionResult PublicTest()
        {
            return Ok(new string[] { "Pomme", "Poire", "Banane" });
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrivateTest()
        {
            return Ok(new string[] { "PrivatePomme", "PrivatePoire", "PrivateBanane" });
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDTO loginDTO)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDTO.Username, loginDTO.Password, true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Claim? nameIdentifierClaim = User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                // Note: On ajoute simplement le NameIdentifier dans les claims. Il n'y aura pas de rôle pour les utilisateurs du WebAPI.
                List<Claim> authClaims = new List<Claim>();
                authClaims.Add(nameIdentifierClaim);

                SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("C'est tellement la meilleure cle qui a jamais ete cree dans l'histoire de l'humanite (doit etre longue)"));

                string issuer = this.Request.Scheme + "://" + this.Request.Host;

                DateTime expirationTime = DateTime.Now.AddMinutes(30);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: null,
                    claims: authClaims,
                    expires: expirationTime,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
                );

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // On ne veut JAMAIS retouner une string directement lorsque l'on utilise Angular.
                // Angular assume que l'on retourne un objet et donne une erreur lorsque le résultat obtenu est une simple string!
                return Ok(new LoginSuccessDTO() { Token = tokenString });
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
                UserName = registerDTO.Username,
                Email = registerDTO.Email
            };
            IdentityResult identityResult = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!identityResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = identityResult.Errors });
            }

            return Ok();
        }
    }
}