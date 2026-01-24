using FluentValidation;
using AuthService.Application.Common;

namespace AuthService.Application.Common
{
    public static class ValidatorExtensions
    {
        public static async Task<Result<T>> ValidateAndExecuteAsync<T, TCommand>(
            this IValidator<TCommand> validator,
            TCommand command,
            Func<Task<Result<T>>> executeFunc)
        {
            var validationResult = await validator.ValidateAsync(command);
            
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Result<T>.Failure(errors);
            }

            return await executeFunc();
        }
    }
}
