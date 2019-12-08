using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Repository
{
    public interface IFileRepository
    {
        Task<FileRepositoryOperationResult<(Stream stream, string contentType)>> FetchFile(string filePath);

        Task<FileRepositoryOperationResult<bool>> PersistFile(string filePath, Stream fileContent, dynamic metadata);

        Task<FileRepositoryOperationResult<bool>> RemoveFile(string filePath, dynamic metadata);

    }
}