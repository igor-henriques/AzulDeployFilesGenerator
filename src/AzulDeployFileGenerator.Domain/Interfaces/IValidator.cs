namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IValidator<T> where T : class, new()
{
    Task ValidateAndThrowAsync(T instance, CancellationToken cancellationToken = default);
}
