using Application.Features.Users.Validators;
using FluentValidation;
using MediatR;
using ResponseResult.Wrappers;

namespace Application.Pipelines
{
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, IValidateMe
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task
                    .WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                if (!validationResults.Any(r => !r.IsValid))
                {
                    var errorMessage = new List<string>();
                    var failures = validationResults.SelectMany(r => r.Errors)
                        .Where(f => f != null).ToList();

                    foreach (var failure in failures)
                    {
                        errorMessage.Add(failure.ErrorMessage);
                    }
                    return (TResponse)await ResponseWrapper.FailAsync(errorMessage);
                }
            }
            return await next();
        }
    }
}
