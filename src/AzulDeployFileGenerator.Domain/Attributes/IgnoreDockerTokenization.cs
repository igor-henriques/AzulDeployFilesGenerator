namespace AzulDeployFileGenerator.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
public sealed class IgnoreDockerTokenization : Attribute
{
    public readonly bool IncludeOnExcel;
    
    public IgnoreDockerTokenization(bool includeOnExcel = false)
    {
        IncludeOnExcel = includeOnExcel;
    }
}
