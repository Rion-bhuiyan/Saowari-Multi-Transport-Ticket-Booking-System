using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.User;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        private readonly INotificationService _notificationService;

        public UserService(IRepository<User> repository, IMapper mapper, INotificationService notificationService)
        {
            _repository = repository;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<UserResponseDto>>(entities);
            return ApiResponse<IEnumerable<UserResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<UserResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<UserResponseDto>.Fail("Not found");
            return ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(entity));
        }

        public async Task<ApiResponse<UserResponseDto>> CreateAsync(UserCreateDto dto)
        {
            var entity = _mapper.Map<User>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            try { await _notificationService.NotifyNewUserRegisteredAsync(entity); } catch { }
            return ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateAsync(int id, UserUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<UserResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            try { await _notificationService.NotifyUserChangedAsync(entity, "Updated"); } catch { }
            return ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            try { await _notificationService.NotifyUserChangedAsync(entity, "Deleted"); } catch { }
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }
    }
}