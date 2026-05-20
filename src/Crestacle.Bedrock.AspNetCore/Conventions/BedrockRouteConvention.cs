using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Crestacle.Bedrock.AspNetCore.Conventions;

internal sealed class BedrockRouteConvention : IApplicationModelConvention
{
    private readonly string _basePath;

    public BedrockRouteConvention(string basePath)
    {
        _basePath = basePath.Trim('/');
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!controller.ControllerType.Namespace?.StartsWith("Crestacle.Bedrock.AspNetCore.Controllers") ?? true)
                continue;

            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel is null)
                    continue;

                var existing = selector.AttributeRouteModel.Template?.TrimStart('/') ?? string.Empty;

                selector.AttributeRouteModel = new AttributeRouteModel
                {
                    Template = string.IsNullOrEmpty(_basePath)
                        ? existing
                        : $"{_basePath}/{existing}"
                };
            }
        }
    }
}
