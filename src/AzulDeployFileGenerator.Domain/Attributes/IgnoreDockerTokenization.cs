namespace AzulDeployFileGenerator.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
public sealed class IgnoreDockerTokenization : Attribute
{
}
