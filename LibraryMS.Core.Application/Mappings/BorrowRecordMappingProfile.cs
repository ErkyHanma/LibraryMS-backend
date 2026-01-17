namespace LibraryMS.Core.Application.Mappings
{
    using AutoMapper;
    using LibraryMS.Core.Application.Dtos.BorrowRecord;
    using LibraryMS.Core.Domain.Entities;

    public class BorrowRecordMappingProfile : Profile
    {
        public BorrowRecordMappingProfile()
        {

            CreateMap<BorrowRecord, BorrowRecordDto>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.MapFrom(src => src.Book!))
                .ReverseMap()
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


            CreateMap<BorrowRecord, AddBorrowRecordDto>()
                .ReverseMap()
                .ForMember(dest => dest.BorrowRecordId, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate,
                    opt => opt.MapFrom(src => src.BorrowDate.AddDays(14))) // DueDate = BorrowDate + 14 days
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnDate, opt => opt.Ignore());


        }
    }

}
