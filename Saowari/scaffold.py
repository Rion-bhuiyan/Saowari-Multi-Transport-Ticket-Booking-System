import os
import re

entities_dir = r"Models/Entities"
dtos_dir = r"Models/DTOs"
interfaces_dir = r"Interfaces"
services_dir = r"Services"
controllers_dir = r"Controllers"

# Ensure dirs exist
for d in [dtos_dir, interfaces_dir, services_dir, controllers_dir]:
    os.makedirs(d, exist_ok=True)

# List all .cs files
entity_files = [f for f in os.listdir(entities_dir) if f.endswith(".cs")]

def get_entity_name(filename):
    return filename.replace(".cs", "")

def parse_properties(filepath):
    properties = []
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
        # Find properties like: public int Id { get; set; }
        matches = re.finditer(r"public\s+([A-Za-z0-9_<>]+[\?]?)\s+([A-Za-z0-9_]+)\s*\{\s*get;\s*set;\s*\}", content)
        for match in matches:
            type_name = match.group(1)
            prop_name = match.group(2)
            # Skip ICollection navigation properties
            if "ICollection" in type_name or "IEnumerable" in type_name or "List<" in type_name:
                continue
            # Skip virtual navigation properties that are complex types (usually classes, not starting with string/int/etc)
            # A simple heuristic: if it has 'virtual' before 'public', or if we just want basic types.
            properties.append((type_name, prop_name))
    return properties

# Define a standard mapping for generation
def generate_dtos(entity_name, properties):
    # Dto dir
    entity_dto_dir = os.path.join(dtos_dir, entity_name)
    os.makedirs(entity_dto_dir, exist_ok=True)

    # We will just put basic properties in DTO
    props_str = ""
    for pt, pn in properties:
        # Exclude navigation properties loosely by checking if type matches another entity name
        is_complex = any(pt == e.replace(".cs", "") or pt == e.replace(".cs", "") + "?" for e in entity_files)
        if not is_complex:
            props_str += f"        public {pt} {pn} {{ get; set; }}\n"

    create_dto = f"""namespace Saowari.Models.DTOs.{entity_name}
{{
    public class {entity_name}CreateDto
    {{
{props_str}
    }}
}}"""
    with open(os.path.join(entity_dto_dir, f"{entity_name}CreateDto.cs"), 'w') as f:
        f.write(create_dto)

    update_dto = f"""namespace Saowari.Models.DTOs.{entity_name}
{{
    public class {entity_name}UpdateDto
    {{
{props_str}
    }}
}}"""
    with open(os.path.join(entity_dto_dir, f"{entity_name}UpdateDto.cs"), 'w') as f:
        f.write(update_dto)

    response_dto = f"""namespace Saowari.Models.DTOs.{entity_name}
{{
    public class {entity_name}ResponseDto
    {{
{props_str}
    }}
}}"""
    with open(os.path.join(entity_dto_dir, f"{entity_name}ResponseDto.cs"), 'w') as f:
        f.write(response_dto)

def generate_interface(entity_name):
    content = f"""using Saowari.Models.DTOs.{entity_name};
using Saowari.Models.Responses;

namespace Saowari.Interfaces
{{
    public interface I{entity_name}Service
    {{
        Task<ApiResponse<IEnumerable<{entity_name}ResponseDto>>> GetAllAsync();
        Task<ApiResponse<{entity_name}ResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<{entity_name}ResponseDto>> CreateAsync({entity_name}CreateDto dto);
        Task<ApiResponse<{entity_name}ResponseDto>> UpdateAsync(int id, {entity_name}UpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }}
}}"""
    with open(os.path.join(interfaces_dir, f"I{entity_name}Service.cs"), 'w') as f:
        f.write(content)

def generate_service(entity_name):
    # Find primary key (usually Id or EntityNameId)
    # This is a bit tricky, assume it's entity_name + "Id" or "Id"
    # Or just use the generic update
    content = f"""using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.{entity_name};
using Saowari.Models.Entities;
using Saowari.Models.Responses;

namespace Saowari.Services
{{
    public class {entity_name}Service : I{entity_name}Service
    {{
        private readonly IRepository<{entity_name}> _repository;
        private readonly IMapper _mapper;

        public {entity_name}Service(IRepository<{entity_name}> repository, IMapper mapper)
        {{
            _repository = repository;
            _mapper = mapper;
        }}

        public async Task<ApiResponse<IEnumerable<{entity_name}ResponseDto>>> GetAllAsync()
        {{
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<{entity_name}ResponseDto>>(entities);
            return ApiResponse<IEnumerable<{entity_name}ResponseDto>>.Ok(dtos);
        }}

        public async Task<ApiResponse<{entity_name}ResponseDto>> GetByIdAsync(int id)
        {{
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<{entity_name}ResponseDto>.Fail("Not found");
            return ApiResponse<{entity_name}ResponseDto>.Ok(_mapper.Map<{entity_name}ResponseDto>(entity));
        }}

        public async Task<ApiResponse<{entity_name}ResponseDto>> CreateAsync({entity_name}CreateDto dto)
        {{
            var entity = _mapper.Map<{entity_name}>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<{entity_name}ResponseDto>.Ok(_mapper.Map<{entity_name}ResponseDto>(entity), "Created successfully");
        }}

        public async Task<ApiResponse<{entity_name}ResponseDto>> UpdateAsync(int id, {entity_name}UpdateDto dto)
        {{
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<{entity_name}ResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<{entity_name}ResponseDto>.Ok(_mapper.Map<{entity_name}ResponseDto>(entity), "Updated successfully");
        }}

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {{
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }}
    }}
}}"""
    with open(os.path.join(services_dir, f"{entity_name}Service.cs"), 'w') as f:
        f.write(content)

def generate_controller(entity_name):
    # Standard CRUD controller. Will be customized later based on prompt rules.
    # Group 1 are AdminOnly. We will just add [Authorize(Policy="AdminOnly")] for all PUT/POST/DELETE as default
    # and adjust later.
    content = f"""using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.{entity_name};
using Saowari.Models.Responses;

namespace Saowari.Controllers
{{
    [Route("api/[controller]")]
    [ApiController]
    public class {entity_name}sController : ControllerBase
    {{
        private readonly I{entity_name}Service _service;

        public {entity_name}sController(I{entity_name}Service service)
        {{
            _service = service;
        }}

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<{entity_name}ResponseDto>>>> GetAll()
        {{
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }}

        [HttpGet("{{id}}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<{entity_name}ResponseDto>>> GetById(int id)
        {{
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }}

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<{entity_name}ResponseDto>>> Create([FromBody] {entity_name}CreateDto dto)
        {{
            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new {{ id = 0 }}, result); // Will fix ID return later if needed
        }}

        [HttpPut("{{id}}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<{entity_name}ResponseDto>>> Update(int id, [FromBody] {entity_name}UpdateDto dto)
        {{
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }}

        [HttpDelete("{{id}}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {{
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }}
    }}
}}"""
    with open(os.path.join(controllers_dir, f"{entity_name}sController.cs"), 'w') as f:
        f.write(content)

for e_file in entity_files:
    e_name = get_entity_name(e_file)
    props = parse_properties(os.path.join(entities_dir, e_file))
    generate_dtos(e_name, props)
    generate_interface(e_name)
    generate_service(e_name)
    generate_controller(e_name)

print("Scaffolding complete.")
