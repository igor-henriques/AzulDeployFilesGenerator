namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record KubernetesDeployment
{
    public Namespace Namespace { get; init; }
    public Deployment Deployment { get; init; }
    public HorizontalPodAutoscaler HorizontalPodAutoscaler { get; init; }
    public Service Service { get; init; }
    public Secret Secret { get; init; }
    public Strategy Strategy { get; init; }

    public KubernetesDeployment(IOptions<CliCommandOptions> cliOptions)
    {
        if (string.IsNullOrWhiteSpace(cliOptions.Value.DeployName))
        {
            throw new ArgumentNullException(nameof(cliOptions.Value.DeployName));
        }

        if (string.IsNullOrWhiteSpace(cliOptions.Value.ApplicationType))
        {
            throw new ArgumentNullException(nameof(cliOptions.Value.ApplicationType));
        }

        string deploymentFullName = $"{cliOptions.Value.DeployName}-__environment__";

        Namespace = new Namespace()
        {
            ApiVersion = "v1",
            Kind = "Namespace",
            Metadata = new Metadata()
            {
                Name = "__k8s.namespace__"
            }
        };

        Deployment = new Deployment()
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = new Metadata()
            {
                Namespace = "__k8s.namespace__",
                Name = deploymentFullName,
                Annotations = new Annotations()
                {
                    ApiGtw = "private"
                },
                Labels = new Labels()
                {
                    App = deploymentFullName,
                    Tier = "api",
                    Version = "v1"
                }
            },
            Spec = new DeploymentSpec()
            {
                Selector = new Selector()
                {
                    MatchLabels = new Labels()
                    {
                        App = deploymentFullName,
                        Tier = "api",
                        Version = "v1"
                    }
                },
                Replicas = 1,
                Template = new Template()
                {
                    Metadata = new Metadata()
                    {
                        Labels = new Labels()
                        {
                            App = deploymentFullName,
                            Tier = "api",
                            Version = "v1"
                        }
                    },
                    Spec = new TemplateSpec()
                    {
                        HostAliases = new List<HostAlias>(),
                        Containers = new List<Container>()
                        {
                            new Container()
                            {
                                Name = deploymentFullName,
                                Image = "acrdevopsbr.azurecr.io/maintenance/flight/api:latest",
                                Lifecycle = new Lifecycle()
                                {
                                    PostStart = new PostStart()
                                    {
                                        Exec = new Exec()
                                        {
                                            Command = new List<string>()
                                            {
                                                "/bin/sh",
                                                "-c",
                                                "/bin/echo 'options single-request-reopen' >> /etc/resolv.conf"
                                            }
                                        }
                                    }
                                },
                                Env = new List<EnvVariable>()
                                {
                                    // Adicione suas variáveis de ambiente aqui
                                },
                                Ports = new List<Port>()
                                {
                                    new Port()
                                    {
                                        Name = "http",
                                        ContainerPort = 80,
                                        Protocol = "TCP"
                                    }
                                },
                                LivenessProbe = new LivenessProbe()
                                {
                                    HttpGet = new HttpGet()
                                    {
                                        Path = "/health/live",
                                        Port = 80
                                    },
                                    InitialDelaySeconds = 15,
                                    PeriodSeconds = 20
                                }
                                // Continue adicionando as propriedades necessárias, como resources, terminationMessagePath, etc.
                            }
                        }
                    }
                }
            }
        };

        Strategy = new Strategy()
        {
            Type = "RollingUpdate",
            RollingUpdate = new RollingUpdate()
            {
                MaxUnavailable = "25%",
                MaxSurge = "25%"
            }
        };

        HorizontalPodAutoscaler = new HorizontalPodAutoscaler()
        {
            ApiVersion = "autoscaling/v2",
            Kind = "HorizontalPodAutoscaler",
            Metadata = new Metadata()
            {
                Namespace = "__k8s.namespace__",
                Name = $"{deploymentFullName}-hpa"
            },
            Spec = new HorizontalPodAutoscalerSpec()
            {
                MinReplicas = "__k8s.app.resources.min.replicas__",
                MaxReplicas = "__k8s.app.resources.max.replicas__",
                ScaleTargetRef = new ScaleTargetRef()
                {
                    ApiVersion = "apps/v1",
                    Kind = "Deployment",
                    Name = deploymentFullName
                },
                Metrics = new List<Metric>()
                {
                    new Metric()
                    {
                        Type = "Resource",
                        Resource = new ResourceMetric()
                        {
                            Name = "cpu",
                            Target = new Target()
                            {
                                AverageUtilization = 95,
                                Type = "Utilization"
                            }
                        }
                    },
                    new Metric()
                    {
                        Type = "Resource",
                        Resource = new ResourceMetric()
                        {
                            Name = "memory",
                            Target = new Target()
                            {
                                AverageUtilization = 95,
                                Type = "Utilization"
                            }
                        }
                    }
                }
            }
        };
    }
}
