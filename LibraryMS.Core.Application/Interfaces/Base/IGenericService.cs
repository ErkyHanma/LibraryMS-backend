namespace LibraryMS.Core.Application.Interfaces.Base
{
    public interface IGenericService<DtoModel>
         where DtoModel : class
    {
        Task<List<DtoModel>> GetAllAsync();
        Task<DtoModel?> GetByIdAsync(int id);
        Task<DtoModel?> AddAsync<DtoParam>(DtoParam dto);
        Task<DtoModel?> EditAsync<DtoParam>(int id, DtoParam dto);
        Task<bool> DeleteAsync(int id);

    }
}
