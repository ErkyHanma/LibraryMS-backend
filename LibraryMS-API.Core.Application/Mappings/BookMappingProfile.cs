using AutoMapper;
using LibraryMS_API.Core.Application.Dtos.Book;
using LibraryMS_API.Core.Domain.Entities;

namespace LibraryMS_API.Core.Application.Mappings
{
    public class BookMappingProfile : Profile
    {
        public BookMappingProfile()
        {
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.Categories, opt =>
                    opt.MapFrom(src => src.BookCategories.Select(bc => bc.Category).ToList()))
                .ForMember(dest => dest.CoverUrl, opt =>
                    opt.MapFrom(src => src.CoverImageUrl))
                .ReverseMap()
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowRecords, opt => opt.Ignore());

            CreateMap<Book, AddBookDto>()
                .ForMember(dest => dest.CoverFile, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.BookId, opt => opt.Ignore())
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore())
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowRecords, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Book, EditBookDto>()
                .ForMember(dest => dest.CoverFile, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.BookId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowRecords, opt => opt.Ignore());
        }
    }
}
