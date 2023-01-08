namespace MagicVilla_CouponAPI.Endpoints;

public static class AuthEndpoints
{
    public static void ConfigurationAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/login", Login).WithName("Login").Accepts<LoginRequestDTO>("application/json").Produces<APIResponse>(200).Produces(400);

        app.MapPost("/api/register", Register).WithName("Register").Accepts<RegistrationRequestDTO>("application/json").Produces<APIResponse>(200).Produces(400);
    }

    private async static Task<IResult> Login(
            IAuthRepository _authRepo,
            [FromBody] LoginRequestDTO model) 
    {
        APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

        var loginResponse = await _authRepo.Login(model);
        if (loginResponse is null)
        {
            response.ErrorMessages.Add("Username or password is incorrect!");
            return Results.BadRequest(response);
        }

        response.Result = loginResponse;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }

    private async static Task<IResult> Register(
        IAuthRepository _authRepo,
        [FromBody] RegistrationRequestDTO model)
    {
        APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

        if (!_authRepo.IsUniqueUser(model.UserName))
        {
            response.ErrorMessages.Add("Username is already Exists!");
            return Results.BadRequest(response);
        }
        var registerResponse = await _authRepo.Register(model);
        if (registerResponse is null || string.IsNullOrWhiteSpace(registerResponse.UserName))
        {
            response.ErrorMessages.Add("Something went wrong!");
            return Results.BadRequest(response);
        }

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }
}
