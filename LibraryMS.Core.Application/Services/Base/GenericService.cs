using AutoMapper;
using LibraryMS.Core.Application.Interfaces.Base;
using LibraryMS.Infrastructure.Persistence.Repositories.Base;

namespace LibraryMS.Core.Application.Services.Base
{
    public class GenericService<Entity, DtoModel> : IGenericService<DtoModel> // requires mappers to work
        where Entity : class
        where DtoModel : class
    {
        private readonly IGenericRepository<Entity> _repository;
        private readonly IMapper _mapper;
        public GenericService(IGenericRepository<Entity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<List<DtoModel>> GetAllAsync()
        {
            try
            {
                var listEntities = await _repository.GetAllListAsync();
                var listEntityDtos = _mapper.Map<List<DtoModel>>(listEntities);

                return listEntityDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public virtual async Task<DtoModel?> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return null;
                }

                DtoModel dto = _mapper.Map<DtoModel>(entity);
                return dto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<DtoModel?> AddAsync<DtoParam>(DtoParam dto)
        {
            try
            {
                Entity entity = _mapper.Map<Entity>(dto);
                Entity? returnEntity = await _repository.AddAsync(entity);

                if (returnEntity == null)
                {
                    return null;
                }

                return _mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<DtoModel?> EditAsync<DtoParam>(int id, DtoParam dto)
        {
            try
            {
                Entity entity = _mapper.Map<Entity>(dto);
                Entity? returnEntity = await _repository.EditAsync(id, entity);

                if (returnEntity == null)
                {
                    return null;
                }

                return _mapper.Map<DtoModel>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
