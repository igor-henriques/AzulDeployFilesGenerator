namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record EnvVariable
{
    public string Name { get; set; }
    public string Value { get; set; }

    public EnvVariable(string name, string value)
    {
        Name = name;
        Value = $"'{value}'";
    }

    public EnvVariable() { }
}
