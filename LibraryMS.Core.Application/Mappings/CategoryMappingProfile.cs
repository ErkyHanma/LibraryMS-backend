namespace LibraryMS.Core.Application.Mappings
{
    using AutoMapper;
    using LibraryMS.Core.Application.Dtos.Category;
    using LibraryMS.Core.Domain.Entities;

    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.BooksCount, opt => opt.MapFrom(src => src.BookCategories != null ? src.BookCategories.Count : 0))
                .ReverseMap()
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Category, AddCategoryDto>()
                .ReverseMap()
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Category, EditCategoryDto>()
                .ReverseMap()
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BookCategories, opt => opt.Ignore());
        }
    }

}
