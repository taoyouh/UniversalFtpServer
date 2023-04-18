using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

#nullable enable
namespace UniversalFtpServer
{
    public class Settings
    {
        private readonly ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
        
        public string? RootFolder
        {
            get => settings.Values[Keys.RootFolder] as string;
            set => settings.Values[Keys.RootFolder] = value;
        }

        public int? SettingVersion
        {
            get => settings.Values[Keys.SettingVersion] as int?;
            set => settings.Values[Keys.SettingVersion] = value;
        }

        public int? PortNumber
        {
            get => settings.Values[Keys.PortNumber] as int?;
            set => settings.Values[Keys.PortNumber] = value;
        }

        public bool? AllowAnonymous
        {
            get => settings.Values[Keys.AllowAnonymous] as bool?;
            set => settings.Values[Keys.AllowAnonymous] = value;
        }

        public string? UserName
        {
            get => settings.Values[Keys.UserName] as string;
            set => settings.Values[Keys.UserName] = value;
        }

        public string? Password
        {
            get => settings.Values[Keys.Password] as string;
            set => settings.Values[Keys.Password] = value;
        }

        public int? AcceptedPrivacyPolicyVersion
        {
            get => settings.Values[Keys.AcceptedPrivacyPolicyVersion] as int?;
            set => settings.Values[Keys.AcceptedPrivacyPolicyVersion] = value;
        }

        private static class Keys
        {
            public const string RootFolder = "RootFolder";
            public const string PortNumber = "PortNumber";
            public const string AllowAnonymous = "AllowAnonymous";
            public const string UserName = "UserName";
            public const string Password = "Password";
            public const string SettingVersion = "SettingVersion";
            public const string AcceptedPrivacyPolicyVersion = "AcceptedPrivacyPolicyVersion";

        }
    }
}
