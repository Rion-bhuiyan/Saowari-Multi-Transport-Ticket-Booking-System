using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Scaffolder
{
    class Program
    {
        static void Main(string[] args)
        {
            string basePath = @"..\Saowari";
            string entitiesDir = Path.Combine(basePath, "Models", "Entities");
            string dtosDir = Path.Combine(basePath, "Models", "DTOs");
            string interfacesDir = Path.Combine(basePath, "Interfaces");
            string servicesDir = Path.Combine(basePath, "Services");
            string controllersDir = Path.Combine(basePath, "Controllers");

            Directory.CreateDirectory(dtosDir);
            Directory.CreateDirectory(interfacesDir);
            Directory.CreateDirectory(servicesDir);
            Directory.CreateDirectory(controllersDir);

            var entityFiles = Directory.GetFiles(entitiesDir, "*.cs");
            var entityNames = entityFiles.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();

            foreach (var file in entityFiles)
            {
                string entityName = Path.GetFileNameWithoutExtension(file);
                var properties = ParseProperties(file, entityNames);

                GenerateDtos(entityName, properties, dtosDir);
                GenerateInterface(entityName, interfacesDir);
                GenerateService(entityName, servicesDir);
                GenerateController(entityName, controllersDir);
            }
            Console.WriteLine("Scaffolding complete.");
        }

        static List<(string type, string name)> ParseProperties(string filepath, List<string> entityNames)
        {
            var properties = new List<(string type, string name)>();
            string content = File.ReadAllText(filepath);
            var matches = Regex.Matches(content, @"public\s+([A-Za-z0-9_<>\[\]\?]+)\s+([A-Za-z0-9_]+)\s*\{\s*get;\s*set;\s*\}");
            
            foreach (Match match in matches)
            {
                string typeName = match.Groups[1].Value;
                string propName = match.Groups[2].Value;

                if (typeName.Contains("ICollection") || typeName.Contains("IEnumerable") || typeName.Contains("List<")) continue;
                
                string cleanType = typeName.Replace("?", "");
                if (entityNames.Contains(cleanType)) continue;

                properties.Add((typeName, propName));
            }
            return properties;
        }

        static void GenerateDtos(string entityName, List<(string type, string name)> properties, string dtosDir)
        {
            string entityDtoDir = Path.Combine(dtosDir, entityName);
            Directory.CreateDirectory(entityDtoDir);

            string propsStr = string.Join("\n", properties.Select(p => $"        public {p.type} {p.name} {{ get; set; }}"));

            File.WriteAllText(Path.Combine(entityDtoDir, $"{entityName}CreateDto.cs"),
$@"namespace Saowari.Models.DTOs.{entityName}
{{
    public class {entityName}CreateDto
    {{
{propsStr}
    }}
}}");

            File.WriteAllText(Path.Combine(entityDtoDir, $"{entityName}UpdateDto.cs"),
$@"namespace Saowari.Models.DTOs.{entityName}
{{
    public class {entityName}UpdateDto
    {{
{propsStr}
    }}
}}");

            File.WriteAllText(Path.Combine(entityDtoDir, $"{entityName}ResponseDto.cs"),
$@"namespace Saowari.Models.DTOs.{entityName}
{{
    public class {entityName}ResponseDto
    {{
{propsStr}
    }}
}}");
        }

        static void GenerateInterface(string entityName, string interfacesDir)
        {
            File.WriteAllText(Path.Combine(interfacesDir, $"I{entityName}Service.cs"),
$@"using Saowari.Models.DTOs.{entityName};
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{{
    public interface I{entityName}Service
    {{
        Task<ApiResponse<IEnumerable<{entityName}ResponseDto>>> GetAllAsync();
        Task<ApiResponse<{entityName}ResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<{entityName}ResponseDto>> CreateAsync({entityName}CreateDto dto);
        Task<ApiResponse<{entityName}ResponseDto>> UpdateAsync(int id, {entityName}UpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }}
}}");
        }

        static void GenerateService(string entityName, string servicesDir)
        {
            File.WriteAllText(Path.Combine(servicesDir, $"{entityName}Service.cs"),
$@"using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.{entityName};
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{{
    public class {entityName}Service : I{entityName}Service
    {{
        private readonly IRepository<{entityName}> _repository;
        private readonly IMapper _mapper;

        public {entityName}Service(IRepository<{entityName}> repository, IMapper mapper)
        {{
            _repository = repository;
            _mapper = mapper;
        }}

        public async Task<ApiResponse<IEnumerable<{entityName}ResponseDto>>> GetAllAsync()
        {{
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<{entityName}ResponseDto>>(entities);
            return ApiResponse<IEnumerable<{entityName}ResponseDto>>.Ok(dtos);
        }}

        public async Task<ApiResponse<{entityName}ResponseDto>> GetByIdAsync(int id)
        {{
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<{entityName}ResponseDto>.Fail(""Not found"");
            return ApiResponse<{entityName}ResponseDto>.Ok(_mapper.Map<{entityName}ResponseDto>(entity));
        }}

        public async Task<ApiResponse<{entityName}ResponseDto>> CreateAsync({entityName}CreateDto dto)
        {{
            var entity = _mapper.Map<{entityName}>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<{entityName}ResponseDto>.Ok(_mapper.Map<{entityName}ResponseDto>(entity), ""Created successfully"");
        }}

        public async Task<ApiResponse<{entityName}ResponseDto>> UpdateAsync(int id, {entityName}UpdateDto dto)
        {{
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<{entityName}ResponseDto>.Fail(""Not found"");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<{entityName}ResponseDto>.Ok(_mapper.Map<{entityName}ResponseDto>(entity), ""Updated successfully"");
        }}

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {{
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail(""Not found"");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, ""Deleted successfully"");
        }}
    }}
}}");
        }

        static void GenerateController(string entityName, string controllersDir)
        {
            File.WriteAllText(Path.Combine(controllersDir, $"{entityName}sController.cs"),
$@"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.{entityName};
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Controllers
{{
    [Route(""api/[controller]"")]
    [ApiController]
    public class {entityName}sController : ControllerBase
    {{
        private readonly I{entityName}Service _service;

        public {entityName}sController(I{entityName}Service service)
        {{
            _service = service;
        }}

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<{entityName}ResponseDto>>>> GetAll()
        {{
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }}

        [HttpGet(""{{id}}"")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<{entityName}ResponseDto>>> GetById(int id)
        {{
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }}

        [HttpPost]
        [Authorize(Policy = ""AdminOnly"")]
        public async Task<ActionResult<ApiResponse<{entityName}ResponseDto>>> Create([FromBody] {entityName}CreateDto dto)
        {{
            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new {{ id = 0 }}, result);
        }}

        [HttpPut(""{{id}}"")]
        [Authorize(Policy = ""AdminOnly"")]
        public async Task<ActionResult<ApiResponse<{entityName}ResponseDto>>> Update(int id, [FromBody] {entityName}UpdateDto dto)
        {{
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }}

        [HttpDelete(""{{id}}"")]
        [Authorize(Policy = ""AdminOnly"")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {{
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }}
    }}
}}");
        }
    }
}
