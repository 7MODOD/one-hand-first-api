using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<JsonSerializerOptions>(o => o.PropertyNameCaseInsensitive = true);

var app = builder.Build();
var UsersList = new List<Users>();
var ProfilesList = new List<UserProfile>();
//registration
app.MapPost("/users", async (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req) =>
{   
    var exist = UsersList.FirstOrDefault(x => (x.UserName == req.UserObj.UserName || x.Email==req.UserObj.Email));
    if (exist == null)
    {
        
        var resp = new Users(req.UserObj.UserName, req.UserObj.Email, req.UserObj.Password, $"{Guid.NewGuid()}", "", "");
        
        UsersList.Add(resp);
        ProfilesList.Add(new UserProfile(req.UserObj.UserName, req.UserObj.Email, 0, 0));
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

app.MapGet("profiles/{username}", (String username) =>
{
    var exist = ProfilesList.FirstOrDefault(x => x.UserName == username);
    if(exist != null)
    {   
        return new UserRequestEnv<UserProfile>(exist);
    }
    else
    {
        return new UserRequestEnv<UserProfile>(null);
    }
});


app.MapPost("profile/{username}/follow", async (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req, String username) =>
{
    var email = req.UserObj.Email;
    if (email != null)
    {   
        var user = ProfilesList.FirstOrDefault(x => x.Email == email);
        if (user != null)
        {
            ProfilesList.Remove(user);
            var resp = new UserProfile(user.UserName, user.Email, user.Followers + 1, user.Following);
            ProfilesList.Add(resp);
            
            await ctx.Response.WriteAsJsonAsync<UserProfile>(resp);
            
        }

    }
    await ctx.Response.WriteAsync("this profile is not found");


});


app.MapDelete("profile/{username}/follow", async (HttpContext ctx, String username) =>
{
    
    var user = ProfilesList.FirstOrDefault(x => x.UserName == username);
    if (user != null)
    {
        ProfilesList.Remove(user);
        var resp = new UserProfile(user.UserName, user.Email, user.Followers - 1, user.Following);
        ProfilesList.Add(resp);

        await ctx.Response.WriteAsJsonAsync<UserProfile>(resp);

    }

    await ctx.Response.WriteAsync("this profile is not found");


});




app.MapControllers();
app.Run("http://localhost:5500");


public record UserRequest(string UserName, string Email, string Password);

public record UserRequestEnv<T>(T UserObj);
record Users(string? UserName, string? Email, string? Password, string? Token, string? Bio, string? Image);

record UserProfile(string UserName, string Email, int Followers, int Following);