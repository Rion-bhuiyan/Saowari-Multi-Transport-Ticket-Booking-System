using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.BookingStatus;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class BookingStatusService : IBookingStatusService
    {
        private readonly IRepository<BookingStatus> _repository;
        private readonly IMapper _mapper;

        public BookingStatusService(IRepository<BookingStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<BookingStatusResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<BookingStatusResponseDto>>(entities);
            return ApiResponse<IEnumerable<BookingStatusResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<BookingStatusResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<BookingStatusResponseDto>.Fail("Not found");
            return ApiResponse<BookingStatusResponseDto>.Ok(_mapper.Map<BookingStatusResponseDto>(entity));
        }

        public async Task<ApiResponse<BookingStatusResponseDto>> CreateAsync(BookingStatusCreateDto dto)
        {
            var entity = _mapper.Map<BookingStatus>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<BookingStatusResponseDto>.Ok(_mapper.Map<BookingStatusResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<BookingStatusResponseDto>> UpdateAsync(int id, BookingStatusUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<BookingStatusResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<BookingStatusResponseDto>.Ok(_mapper.Map<BookingStatusResponseDto>(entity), "Updated successfully");
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