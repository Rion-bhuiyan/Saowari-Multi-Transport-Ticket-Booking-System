using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Ticket;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class TicketService : ITicketService
    {
        private readonly IRepository<Ticket> _repository;
        private readonly IMapper _mapper;

        public TicketService(IRepository<Ticket> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<TicketResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TicketResponseDto>>(entities);
            return ApiResponse<IEnumerable<TicketResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<TicketResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<TicketResponseDto>.Fail("Not found");
            return ApiResponse<TicketResponseDto>.Ok(_mapper.Map<TicketResponseDto>(entity));
        }

        public async Task<ApiResponse<TicketResponseDto>> CreateAsync(TicketCreateDto dto)
        {
            var entity = _mapper.Map<Ticket>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<TicketResponseDto>.Ok(_mapper.Map<TicketResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<TicketResponseDto>> UpdateAsync(int id, TicketUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<TicketResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<TicketResponseDto>.Ok(_mapper.Map<TicketResponseDto>(entity), "Updated successfully");
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