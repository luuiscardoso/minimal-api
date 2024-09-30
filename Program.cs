using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDto) => {
    if(loginDto.Email.Equals("adm@teste.com") && loginDto.Senha.Equals("123456")){
        return Results.Ok("Login efetuado com sucesso.");
    } else {
        return Results.Unauthorized();
    }
});

app.Run();

