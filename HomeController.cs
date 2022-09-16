using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AUTH_TEST1;

[ApiController]

public class HomeController : ControllerBase
{
    private List<Users> UserList = new List<Users>();
    private List<Profiles> ProfilesList = new List<Profiles>();
    private List<Articles> ArticlesList = new List<Articles>();

    private Dictionary<string, List<string>> followedBy = new Dictionary<string, List<string>>();

    [HttpGet]
    [Route("/MVC/users")]

    public ActionResult GetCurrentUser()
    {
        var resp = UserList.FirstOrDefault(x => x.Token == Request.Headers["Authorization"]);
        return new JsonResult(new UserRequestEnv<Users>(resp));
    }


    [Route("MVC/profiles/{username}")]
    public ActionResult GetProfile(string username)
    {

        var user = UserList.FirstOrDefault(x => x.Token == Request.Headers["Authorization"]);
        var profile = ProfilesList.FirstOrDefault(x => x.Username == username);
        var checkFollow = followedBy[user.UserName].FirstOrDefault(x => x == username);
        if (checkFollow != null)
        {
            var resp = new Profiles(profile.Username, profile.Bio, profile.Img, true);
            return new JsonResult(new UserRequestEnv<Profiles>(resp));
        }
        else
        {
            var resp = new Profiles(profile.Username, profile.Bio, profile.Img, false);
            return new JsonResult(new UserRequestEnv<Profiles>(resp));
        }

    }


    [Route("/MVC/articles")]
    public ActionResult GetAllArticles()
    {

        return new JsonResult(new UserRequestEnv<List<Articles>>(ArticlesList));
    }

    [Route("/MVC/articles/")]
    public ActionResult GetArticlesByAuthor([FromQuery] string author)
    {
        var articles = ArticlesList.FindAll(x => x.author.Username == author);
        return new JsonResult(new UserRequestEnv<List<Articles>>(articles));
    }

    [Route("/MVC/articles/")]
    public ActionResult GetArticlesFavorited([FromQuery] string favorited)
    {
        var user = UserList.FirstOrDefault(x => x.UserName == favorited);
        var fav = ArticlesList.FindAll(x => x.favorited == true);

        return new JsonResult(new UserRequestEnv<List<Articles>>(fav));
    }

    [Route("/MVC/articles/")]
    public ActionResult GetArticlesByTag([FromQuery] string tag)
    {

        var articleByTag = ArticlesList.FindAll(x => x.tagList.Contains(tag));
        int articlesCount = articleByTag.Count;
        return new JsonResult(new UserRequestEnv<List<Articles>>(articleByTag), articlesCount);
    }


    [Route("/MVC/articles/feed")]
    public ActionResult Feed()
    {
        int articlesCount = ArticlesList.Count;
        return new JsonResult(new Feed(ArticlesList, articlesCount));

    }







    [HttpPost]
    [Route("/MVC/users")]
    public ActionResult CreateUser([FromBody] UserRequestEnv<UserRequest> req)
    {

        var validator = new UserRequestValidator();
        var res = validator.Validate(req.UserObj);

        if (res.IsValid)
        {
            var resp = new Users(req.UserObj.UserName, req.UserObj.Email, req.UserObj.Password, $"{Guid.NewGuid()}", "", "");

            var profile = new Profiles(req.UserObj.UserName, "", "", null);
            followedBy.Add(req.UserObj.UserName, new List<string>());

            UserList.Add(resp);
            return new JsonResult(new UserRequestEnv<Users>(resp));

        }
        else
        {
            return BadRequest(res.Errors);
        }
    }

    [Route("/MVC/users/login")]
    public ActionResult Login(HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req)
    {   

        var resp = UserList.FirstOrDefault(x => x.Email == req.UserObj.Email && x.Password == req.UserObj.Password);
        return new JsonResult(new UserRequestEnv<Users>(resp));
    }



    [Route("/MVC/profiles/{username}/follow")]
    public ActionResult FollowProfile(string username)
    {
        var validator = new ProfileRequestValidator();
        var res = validator.Validate(username);
        if (res.IsValid)
        {

            var user = UserList.FirstOrDefault(x => x.Token == Request.Headers["Authorization"]);
            var profile = ProfilesList.FirstOrDefault(x => x.Username == username);


            followedBy[user.UserName].Add(profile.Username);
            var resp = new Profiles(profile.Username, profile.Bio, profile.Img, true);


            return new JsonResult(new UserRequestEnv<Profiles>(resp));
        }
        else
        {
            return BadRequest(res.Errors);
        }


    }

    //record Articles(string slug,string title,string desc,string body,List<string>? tagList,DateTime createdAt,
    //DateTime updatedAt,bool favorited,int favoritedCount, Profiles author);


    [Route("/MVC/articles")]
    public ActionResult CreateArticles([FromBody] UserRequestEnv<ArticlesRequest> req)
    {
        var validator = new ArticlesValidator();
        var res = validator.Validate(req.UserObj);
        if (res.IsValid)
        {

            var user = UserList.FirstOrDefault(x => x.Token == Request.Headers["Authorization"]);
            var profile = new Profiles(user.UserName, user.Bio, user.Image, null);
            var slug = req.UserObj.title.ToLower();
            slug = slug.Replace(' ', '-');



            string current = DateTime.Now.ToString();
            var resp = new Articles(slug, req.UserObj.title, req.UserObj.desc, req.UserObj.body,
                req.UserObj.tagList, current, current, false, 0, profile);
            ArticlesList.Add(resp);

            return new JsonResult(new UserRequestEnv<Articles>(resp));
        }
        else
        {
            return BadRequest(res.Errors);
        }

    }


    [HttpPut]
    [Route("/MVC/user")]
    public async Task<ActionResult> EditEmail()
    {
        var body = "";
        using (var read = new StreamReader(Request.Body))
        {
            body = await read.ReadToEndAsync();

        }
        var req = JsonSerializer.Deserialize<UserRequestEnv<UserRequest>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var user = UserList.FirstOrDefault(x => x.Token == Request.Headers["Authorization"]);
        UserList.Remove(user);
        var resp = new Users(user.UserName, req.UserObj.Email, user.Password, $"{Guid.NewGuid}", "", "");
        UserList.Add(resp);
        return new JsonResult(new UserRequestEnv<Users>(resp));

    }



    [HttpDelete]
    [Route("MVC/profiles/{username}/follow")]
    public ActionResult UnFollowProfile(string username)
    {
        var user = UserList.FirstOrDefault(x => x.Token == Request.Headers["Authorization"]);
        var profile = ProfilesList.FirstOrDefault(x => x.Username == username);

        var resp = new Profiles(profile.Username, profile.Bio, profile.Img, false);
        followedBy[username].Remove(resp.Username);
        return new JsonResult(new UserRequestEnv<Profiles>(resp));

    }




}





public record UserRequest(string UserName, string Email, string Password);

public record UserRequestEnv<T>(T UserObj);
record Users(string? UserName, string? Email, string? Password, string? Token, string? Bio, string? Image);

record Profiles(string Username, string? Bio, string? Img, bool? Following);

public record ProfileFollowRequest(string username);


record Articles(string slug, string title, string desc, string body, List<string>? tagList, string createdAt, string updatedAt, bool favorited, int favoritedCount, Profiles author);
public record ArticlesRequest(string title, string desc, string body, List<string> tagList);

record Feed(List<Articles> artList, int length);