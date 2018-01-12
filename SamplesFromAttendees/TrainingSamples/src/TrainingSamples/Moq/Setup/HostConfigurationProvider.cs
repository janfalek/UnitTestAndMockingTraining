//Setup - Test if GetConfiguration returns correct object
//Firs without specifying args, than lets specify args
//Try to verify that caching works
using System;
using log4net;


namespace TrainingSamples.Moq.Setup
{
    public class HostConfigurationProvider //: IHostConfigurationProvider
    {
        private const string DefaultBaseConfigPath = @"spechost.config.json";
        private const string DefaultUserConfigPath = @"spechost.user.config.json";
        private static readonly object LockObj = new object();

        private static HostConfiguration userConfig;

        private readonly IAppSetting appSetting;
        private readonly IJsonService jsonService;
        private readonly ILog log;

        public HostConfigurationProvider(
            IAppSetting appSetting,
            ILog log,
            IJsonService jsonService)
        {
            this.appSetting = appSetting;
            this.log = log;
            this.jsonService = jsonService;
        }

        public HostConfiguration GetConfiguration()
        {
            lock (LockObj)
            {
                if (userConfig != null)
                {
                    return userConfig;
                }

                var userConfigPath = appSetting.GetSetting("HostConfigurationProvider.UserConfigPath", DefaultUserConfigPath);
                var baseConfigPath = appSetting.GetSetting("HostConfigurationProvider.BaseConfigPath", DefaultBaseConfigPath);

                userConfig = jsonService.GetObjectFromPath<HostConfiguration>(userConfigPath);
                if (userConfig == null)
                {
                    log.Warn($"HostConfigurationProvider - wrong user configuration from: {userConfigPath}");
                    var baseConfig = jsonService.GetObjectFromPath<HostConfiguration>(baseConfigPath);
                    jsonService.SaveObjectToFile(baseConfig, userConfigPath);
                    userConfig = baseConfig;
                }

                if (userConfig != null)
                {
                    return userConfig;
                }

                log.Fatal($"HostConfigurationProvider - wrong user (from: {userConfigPath}) and base configuration (from: {baseConfigPath})");
                throw new Exception("Wrong configuration");
            }
        }
    }

    public interface IAppSetting
    {
        T GetSetting<T>(string key, T @default = default(T));
    }

    public interface IJsonService
    {
        void SaveObjectToFile<T>(T obj, string path);
        T GetObjectFromPath<T>(string path);
    }

    public class HostConfiguration
    {
    }
}