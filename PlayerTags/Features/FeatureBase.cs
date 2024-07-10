using PlayerTags.Configuration;
using PlayerTags.Data;

namespace PlayerTags.Features;

public class FeatureBase
{
    protected readonly PluginConfiguration pluginConfiguration;
    protected readonly PluginData pluginData;

    public virtual bool EnableGlobal => pluginConfiguration.EnabledGlobal;

    protected FeatureBase(PluginConfiguration pluginConfiguration, PluginData pluginData)
    {
        this.pluginConfiguration = pluginConfiguration;
        this.pluginData = pluginData;
    }
}
