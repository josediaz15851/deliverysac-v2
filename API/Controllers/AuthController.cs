using System.Security.Claims;
using API.Data;
using API.Domain;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    public record LoginRequest(string Usuario, string Password);
    public record LoginResponse(string Token, string Rol);

    private readonly DeliverySacContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly JwtTokenService _tokens;

    public AuthController(DeliverySacContext context, IPasswordHasher hasher, JwtTokenService tokens)
    {
        _context = context;
        _hasher = hasher;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.UsuarioNombre == request.Usuario);

        if (usuario is null || !_hasher.Verify(request.Password, usuario.PasswordHash))
            return Unauthorized();

        var token = _tokens.CreateToken(usuario);
        return Ok(new LoginResponse(token, usuario.Rol.ToString()));
    }

    [HttpGet("perfil")]
    [Authorize]
    public ActionResult<object> Perfil()
    {
        return Ok(new
        {
            usuario = User.Identity?.Name,
            rol = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
