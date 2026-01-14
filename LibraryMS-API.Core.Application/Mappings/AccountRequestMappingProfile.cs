using AutoMapper;
using LibraryMS_API.Core.Application.Dtos.AccountRequest;
using LibraryMS_API.Core.Domain.Entities;

namespace LibraryMS_API.Core.Application.Mappings
{
    public class AccountRequestMappingProfile : Profile
    {
        public AccountRequestMappingProfile()
        {
            CreateMap<AccountRequest, AccountRequestDto>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

        }
    }
}
