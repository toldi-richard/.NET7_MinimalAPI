﻿namespace MagicVilla_CouponAPI;

public class MappingConfig : Profile
{
	public MappingConfig()
	{
		CreateMap<Coupon, CouponCreateDTO>().ReverseMap();
		CreateMap<Coupon, CouponDTO>().ReverseMap();
		CreateMap<Coupon, CouponUpdateDTO>().ReverseMap();
		CreateMap<LocalUser, RegistrationRequestDTO>().ReverseMap();
		CreateMap<LocalUser, UserDTO>().ReverseMap();
	}
}
