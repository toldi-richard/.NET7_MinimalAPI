using Microsoft.AspNetCore.Authorization;

namespace MagicVilla_CouponAPI.Endpoints;

public static class CouponEndpoints
{
    public static void ConfigurationCouponEndpoints(this WebApplication app)
    {
        app.MapGet("/api/coupon", GetAllCoupon)
            .WithName("GetCoupons")
            .Produces<APIResponse>(200)
            .RequireAuthorization("AdminOnly");

        app.MapGet("/api/coupon%/{id:int}", GetCoupon)
            .WithName("GetCoupon")
            .Produces<APIResponse>(200)
            .Produces(400)
            .AddEndpointFilter(async (context, next) =>
            {
                var id = context.GetArgument<int>(1);
                if (id == 0)
                {
                    return Results.BadRequest("Cannot have 0 in id!");
                }

                // action to do before execution of endpoint
                Console.WriteLine("Before 1st filter");
                var result = await next(context);
                // action to do after execution of endpoint
                Console.WriteLine("After 1st filter");
                return result;
            })
            .AddEndpointFilter(async (context, next) =>
            {
                // action to do before execution of endpoint
                Console.WriteLine("Before 2nd filter");
                var result = await next(context);
                // action to do after execution of endpoint
                Console.WriteLine("After 2nd filter");
                return result;
            });

        app.MapPost("/api/coupon", CreateCoupon)
            .WithName("CreateCoupon")
            .Accepts<CouponCreateDTO>("application/json")
            .Produces<APIResponse>(201)
            .Produces(400);

        app.MapPut("/api/coupon", UpdateCoupon)
            .WithName("UpdateCoupon")
            .Accepts<CouponUpdateDTO>("application/json")
            .Produces<APIResponse>(200)
            .Produces(400);

        app.MapDelete("/api/coupon{id:int}", DeleteCoupon)
            .WithName("DeleteCoupon")
            .Produces<APIResponse>(204)
            .Produces(400);
    }

    private async static Task<IResult> GetAllCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger)
    {
        APIResponse response = new();
        _logger.Log(LogLevel.Information, "Getting all Coupons!");
        response.Result = await _couponRepo.GetAllAsync();
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }

    private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, int id)
    {
        Console.WriteLine("Endpoint executed.");
        APIResponse response = new();
        response.Result = await _couponRepo.GetAsync(id);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }

    [Authorize]
    private async static Task<IResult> CreateCoupon(
            ICouponRepository _couponRepo,
            IValidator<CouponCreateDTO> _validation,
            IMapper _mapper,
            [FromBody] CouponCreateDTO coupon_C_DTO)
    {
        APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

        var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
        if (!validationResult.IsValid)
        {
            response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
            return Results.BadRequest(response);
        }

        if (await _couponRepo.GetAsync(coupon_C_DTO.Name) is not null)
        {
            response.ErrorMessages.Add("Coupon Name already Exists!");
            return Results.BadRequest(response);
        }

        var coupon = _mapper.Map<Coupon>(coupon_C_DTO);
        await _couponRepo.CreateAsync(coupon);
        await _couponRepo.SaveAsync();
        var couponDTO = _mapper.Map<CouponDTO>(coupon);

        response.Result = couponDTO;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.Created;
        return Results.Ok(response);
    }

    [Authorize]
    private async static Task<IResult> UpdateCoupon(
            ICouponRepository _couponRepo,
            IValidator<CouponUpdateDTO> _validation,
            IMapper _mapper,
            [FromBody] CouponUpdateDTO coupon_U_DTO)
    {
        APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

        var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
        if (!validationResult.IsValid)
        {
            response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
            return Results.BadRequest(response);
        }

        Coupon couponFromStore = await _couponRepo.GetAsync(coupon_U_DTO.Id);
        if (couponFromStore is null)
        {
            response.ErrorMessages.Add("Coupon doesn't Exist!");
            return Results.BadRequest(response);
        }

        couponFromStore.Name = coupon_U_DTO.Name;
        couponFromStore.Percent = coupon_U_DTO.Percent;
        couponFromStore.IsActive = coupon_U_DTO.IsActive;
        couponFromStore.LastUpdated = DateTime.Now;
        await _couponRepo.SaveAsync();

        var couponDTO = _mapper.Map<CouponDTO>(couponFromStore);
        response.Result = couponDTO;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }

    [Authorize(Roles = "admin")]
    private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
    {
        APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
        Coupon couponFromStore = await _couponRepo.GetAsync(id);
        if (couponFromStore is not null)
        {
            await _couponRepo.RemoveAsync(couponFromStore);
            await _couponRepo.SaveAsync();
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.Ok(response);
        }
        else
        {
            response.ErrorMessages.Add("Coupon doesn't Exist!");
            return Results.BadRequest(response);
        }
    }
}
