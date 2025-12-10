namespace EventManagementSystem.ViewModels.Api
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T? data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class PaginatedApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public static PaginatedApiResponse<T> Ok(List<T> data, int pageNumber, int pageSize, int totalCount)
        {
            return new PaginatedApiResponse<T>
            {
                Success = true,
                Message = "Success",
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (totalCount + pageSize - 1) / pageSize
            };
        }

        public static PaginatedApiResponse<T> Error(string message)
        {
            return new PaginatedApiResponse<T>
            {
                Success = false,
                Message = message
            };
        }
    }
}
