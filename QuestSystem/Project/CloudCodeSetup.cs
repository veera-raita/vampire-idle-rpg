using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace QuestSystem;

public class CloudCodeSetup : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton<IQuestService, QuestService>();
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton(PushClient.Create());
    }
}