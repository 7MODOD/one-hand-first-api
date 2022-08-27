using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<JsonSerializerOptions>(o => o.PropertyNameCaseInsensitive = true);

var app = builder.Build();
var UsersList = new List<Users>();

//registration
app.MapPost("/users", async (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req) =>
{   
    var exist = UsersList.FirstOrDefault(x => (x.UserName == req.UserObj.UserName || x.Email==req.UserObj.Email));
    if (exist == null)
    {
        var resp = new Users(req.UserObj.UserName, req.UserObj.Email, req.UserObj.Password, $"{Guid.NewGuid()}", "", "");

        UsersList.Add(resp);
        await ctx.Response.WriteAsJsonAsync(new UserRequestEnv<Users>(resp));
    }
    else
    {
        await ctx.Response.WriteAsync("this Username or Email is already exist ");

    }

});


//login
app.MapPost("/users/login", async (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req) =>
{
    var resp = UsersList.FirstOrDefault(x => (x.UserName == req.UserObj.UserName && x.Password == req.UserObj.Password));
    if (resp != null)
    {
        await ctx.Response.WriteAsJsonAsync(new UserRequestEnv<Users>(resp));
    }
    else
    {
        await ctx.Response.WriteAsync("there is no user with this username and password");
    }


});


//get the current user
app.MapGet("/user", (HttpRequest req) =>
{
    var resp = UsersList.FirstOrDefault(x => x.Token == req.Headers["Authoriszation"]);
    return new UserRequestEnv<Users>(resp); 

});



//update user information
app.MapPut("/user", async (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req) =>
{
    var user = UsersList.FirstOrDefault(x => x.Token == ctx.Request.Headers["Authorization"]);
    if (user != null)
    {
        UsersList.Remove(user);
        var resp = new Users(user.UserName, req.UserObj.Email, user.Password, user.Token, "", "");
        UsersList.Add(resp);
        await ctx.Response.WriteAsJsonAsync(new UserRequestEnv<Users>(resp));
    }
    else
    {
        await ctx.Response.WriteAsync("the user is not found");
    }

});

app.MapControllers();
app.Run("http://localhost:5500");


public record UserRequest(string UserName, string Email, string Password);

public record UserRequestEnv<T>(T UserObj);
record Users(string? UserName, string? Email, string? Password, string? Token, string? Bio, string? Image);
