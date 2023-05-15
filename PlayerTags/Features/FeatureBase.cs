using PlayerTags.Configuration;
using PlayerTags.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerTags.Features
{
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
}
