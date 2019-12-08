using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository;

namespace Api.Controllers
{
    [ApiController]
    [Route("")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IFileRepository _fileRepository;

        public FileController(
            ILogger<FileController> logger,
            IFileRepository fileRepository)
        {
            _logger = logger;
            _fileRepository = fileRepository;
        }
        
        [HttpGet("{**path}")]
        public async Task<IActionResult> Get(string path)
        {
            _logger.LogTrace($"Fetching of object on path {path}");
            var result = await _fileRepository.FetchFile(path);
            if (result.IsError)
                return BadRequest(result.Message);
            return new FileStreamResult(result.Result.stream, result.Result.contentType);
        }

        [HttpPut("{**path}")]
        [HttpPost("{**path}")]
        public async Task<IActionResult> Post(string path)
        {
            _logger.LogTrace($"Persisting of object on path {path}");
            
            await using var stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            var fileContentType = Request.ContentType;
            var result = await _fileRepository.PersistFile(path, stream, fileContentType);

            if (result.IsSuccess)
                return Ok();
            return BadRequest(result.Message);
        }

        [HttpDelete("")]
        public async Task<IActionResult> Delete(DeleteResourceModel deleteResourceModel)
        {
            _logger.LogTrace($"Deletion of object on path {deleteResourceModel.Path}");

            if (string.IsNullOrWhiteSpace(deleteResourceModel.Path))
                return BadRequest("You must specify resource you want to delete");

            var additionalParameters = new Dictionary<string, object>()
            {
                { "version", deleteResourceModel.Version }
            };
            
            var result = await _fileRepository.RemoveFile(deleteResourceModel.Path, additionalParameters);
            
            if (result.IsSuccess)
                return Ok();
            return BadRequest(result.Message);
        }
    }
}