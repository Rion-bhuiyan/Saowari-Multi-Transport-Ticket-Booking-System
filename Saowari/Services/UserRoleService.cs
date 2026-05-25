using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.UserRole;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IRepository<UserRole> _repository;
        private readonly IMapper _mapper;

        public UserRoleService(IRepository<UserRole> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<UserRoleResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<UserRoleResponseDto>>(entities);
            return ApiResponse<IEnumerable<UserRoleResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<UserRoleResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<UserRoleResponseDto>.Fail("Not found");
            return ApiResponse<UserRoleResponseDto>.Ok(_mapper.Map<UserRoleResponseDto>(entity));
        }

        public async Task<ApiResponse<UserRoleResponseDto>> CreateAsync(UserRoleCreateDto dto)
        {
            var entity = _mapper.Map<UserRole>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<UserRoleResponseDto>.Ok(_mapper.Map<UserRoleResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<UserRoleResponseDto>> UpdateAsync(int id, UserRoleUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<UserRoleResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<UserRoleResponseDto>.Ok(_mapper.Map<UserRoleResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }
    }
}