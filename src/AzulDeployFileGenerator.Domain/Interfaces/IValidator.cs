namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IValidator<T> where T : class, new()
{
    /// <summary>
    /// Validates <see cref="T"/> model and throws a <see cref="ValidationException"/> if it fails.
    /// </summary>
    /// <param name="T">The model to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ValidateAndThrowAsync(T instance, CancellationToken cancellationToken = default);
}
