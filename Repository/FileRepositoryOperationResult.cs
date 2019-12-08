namespace Repository
{
    public sealed class FileRepositoryOperationResult<T>
    {
        private FileRepositoryOperationResult(T result, string message)
        {
            Message = message;
            Result = result;
        }

        public static FileRepositoryOperationResult<T> Success(T result)
        {
            return new FileRepositoryOperationResult<T>(result, string.Empty);
        }
        
        public static FileRepositoryOperationResult<T> Error(string message)
        {
            return new FileRepositoryOperationResult<T>(default, message);
        }
        
        public bool IsSuccess => string.IsNullOrWhiteSpace(Message);
        
        public bool IsError => !string.IsNullOrWhiteSpace(Message);
        
        public string Message { get; }
        
        public T Result { get; }
    }
}