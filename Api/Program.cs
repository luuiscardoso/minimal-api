using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTO;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enums;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

#region Builder
var builder = WebApplication.CreateBuilder(args);

string? key = builder.Configuration["JWT:ChaveSecreta"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
    };
});

builder.Services.AddAuthorization();

string? conexao = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<BdContext>(options => options.UseSqlServer(conexao));

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "apicatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer JWT ",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home()));
#endregion

#region Adms

string GerarToken(Administrador adm)
{
    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

    List<Claim> claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Email, adm.Email),
        new Claim(ClaimTypes.Role, adm.Perfil),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) ?? throw new InvalidOperationException();

    SigningCredentials assinatura = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

    SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(10),
        SigningCredentials = assinatura 
    };

    var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
app.MapPost("administradores/login", ([FromBody]LoginDTO loginDto, [FromServices] IAdministradorServico admService) => {

    var adm = admService.Login(loginDto);

    if (adm != null)
    {
        string token = GerarToken(adm);
        return Results.Ok(new
        {
            Token = token
        });
    } 
    else 
    {
        return Results.Unauthorized();
    }

}).AllowAnonymous();

app.MapPost("/administradores", ([FromBody] AdministradorDTO admDTO, [FromServices] IAdministradorServico admService) => {

    var erros = new ErrosValidacao();

    if (string.IsNullOrEmpty(admDTO.Email)) erros.Mensagens.Add("Email vazio.");

    if (string.IsNullOrEmpty(admDTO.Senha)) erros.Mensagens.Add("Senha vazia.");

    if (admDTO.Perfil == null) erros.Mensagens.Add("Perfil vazio.");

    if (erros.Mensagens.Count > 0) return Results.BadRequest(erros);

    Administrador? adm = admService.Incluir(new Administrador()
    {
        Email = admDTO.Email,
        Senha = admDTO.Senha,
        Perfil = admDTO.Perfil.ToString()
    }); 

    

    return Results.Created($"veiculo/{adm.AdministradorId}", adm);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

app.MapGet("/administradores/{id}", ([FromRoute] int id, [FromServices] IAdministradorServico admService) => {

    var adm = admService.BuscaPorId(id);
    if (adm is null) return Results.NotFound();

    return Results.Ok(adm);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

app.MapGet("/administradores", ([FromQuery] int? pagina, [FromServices] IAdministradorServico admService) => {


    var adms = admService.Todos(pagina);
    if (!adms.Any()) return Results.NotFound();

    List<AdministradorModelView> admModelView = new List<AdministradorModelView>();

    foreach(var adm in adms)
    {
        admModelView.Add(new AdministradorModelView()
        {
            Id = adm.AdministradorId,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }

    return Results.Ok(admModelView);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"});
#endregion

#region Veiculos
ErrosValidacao Valida(VeiculoDTO dto)
{
    var erros = new ErrosValidacao();

    if (string.IsNullOrEmpty(dto.Nome)) erros.Mensagens.Add("Nome vazio.");

    if (string.IsNullOrEmpty(dto.Marca)) erros.Mensagens.Add("Marca vazia.");

    if (dto.Ano < 1950) erros.Mensagens.Add("O ano deve ser de anos superiores a 1950.");

    return erros;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDto, [FromServices] IVeiculoServico veiculoService) => {

    var erros = Valida(veiculoDto);

    if (erros.Mensagens.Count > 0) return Results.BadRequest(erros);

    var veiculo = new Veiculo()
    {
        Nome = veiculoDto.Nome,
        Marca = veiculoDto.Marca,
        Ano = veiculoDto.Ano
    };

    veiculoService.Incluir(veiculo);

    return Results.Created($"veiculo/{veiculo.VeiculoId}", veiculo);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

app.MapGet("/veiculos", ([FromQuery] int? pagina, string? nome, string? marca, [FromServices] IVeiculoServico veiculoService) => {


    var veiculos = veiculoService.Todos(pagina, nome, marca);
    if (!veiculos.Any()) return Results.NotFound();

    return Results.Ok(veiculos);

}).RequireAuthorization();

app.MapGet("/veiculos/{id}", ([FromRoute]int id, [FromServices] IVeiculoServico veiculoService) => {


    var veiculos = veiculoService.BuscaPorId(id);
    if (veiculos is null) return Results.NotFound();

    return Results.Ok(veiculos);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculo, [FromServices] IVeiculoServico veiculoService) => {

    var veiculoDb = veiculoService.BuscaPorId(id);
    if (veiculoDb is null) return Results.NotFound();

    var erros = Valida(veiculo);
    if (erros.Mensagens.Count > 0) return Results.BadRequest(erros);

    
    veiculoDb.Nome = veiculo.Nome;
    veiculoDb.Marca = veiculo.Marca;

    veiculoService.Atualizar(veiculoDb);

    return Results.Ok(veiculoDb);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, [FromServices] IVeiculoServico veiculoService) => {


    var veiculoDb = veiculoService.BuscaPorId(id);
    if (veiculoDb is null) return Results.NotFound();

    veiculoService.Excluir(veiculoDb);

    return Results.NoContent();

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion

