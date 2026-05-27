namespace Saowari.Models.DTOs.Common
{
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public T Data { get; set; }

        public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.TotalRecords = totalRecords;
            this.Data = data;
            this.TotalPages = (int)System.Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}
