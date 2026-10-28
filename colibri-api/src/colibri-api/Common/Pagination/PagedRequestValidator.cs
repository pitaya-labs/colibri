using FluentValidation;

namespace colibri_api.Common.Pagination;

public abstract class PagedRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IPagedRequest
{
    protected PagedRequestValidator(int maxPageSize = 100)
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, maxPageSize);
    }
}
