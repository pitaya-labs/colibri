namespace colibri_api.Common.Pagination;

public interface IPagedRequest
{
    int PageNumber { get; }
    int PageSize { get; }
}
