using AutoMapper;
using LibraryMS.Core.Application.Dtos.AccountRequest;
using LibraryMS.Core.Domain.Entities;

namespace LibraryMS.Core.Application.Mappings
{
    public class AccountRequestMappingProfile : Profile
    {
        public AccountRequestMappingProfile()
        {
            CreateMap<AccountRequest, AccountRequestDto>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore());


        }
    }
}
