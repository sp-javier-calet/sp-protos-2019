using System;

namespace SocialPoint.Base
{
    /// <summary>
    ///     Wrapper around System.Version to make it exception safe
    /// </summary>
    public sealed class AppVersion
    {
        public Version Version { get; private set; }

        // Temp used when comparing with strings to avoid creating a new AppVersion object
        static internal AppVersion _tempAppVersion = new AppVersion(0, 0);

        public static readonly AppVersion Zero = new AppVersion(0, 0);

        /// <summary>
        ///     Creates a new System.Version instance from a version string (x.y.z) in
        ///     a safe way: if the format  is incorrect it returns version 0.0
        /// </summary>
        public AppVersion(string versionStr)
        {
            InitVersionFromString(versionStr);
        }

        /// <summary>
        ///     Creates a new System.Version instance from a version numbers in
        ///     a safe way: if the format  is incorrect it returns version 0.0
        /// </summary>
        public AppVersion(int major, int minor, int build = 0)
        {
            try
            {
                if(build <= 0)
                {
                    Version = new Version(major, minor, 0);
                }
                else
                {
                    Version = new Version(major, minor, build);
                }
            }
            catch
            {
                Version = AppVersion.Zero.Version;
            }

        }

        internal AppVersion InitVersionFromString(string versionStr)
        {
            try
            {
                Version = new Version(versionStr);
                if(Version.Build < 0)
                {
                    Version = new Version(Version.Major, Version.Minor, 0);
                }
            }
            catch
            {
                Version = AppVersion.Zero.Version;
            }

            return this;
        }


        #region Comparision operators overrides. Compare with a string

        public static bool operator<(AppVersion ver, string versionStr)
        {
            return ver.Version < _tempAppVersion.InitVersionFromString(versionStr).Version;
        }

        public static bool operator<=(AppVersion ver, string versionStr)
        {
            return ver.Version <= _tempAppVersion.InitVersionFromString(versionStr).Version;
        }

        public static bool operator>(AppVersion ver, string versionStr)
        {
            return ver.Version > _tempAppVersion.InitVersionFromString(versionStr).Version;
        }

        public static bool operator>=(AppVersion ver, string versionStr)
        {
            return ver.Version >= _tempAppVersion.InitVersionFromString(versionStr).Version;
        }

        #endregion

        #region Comparision operators overrides. Compare with another AppVersion instance

        public static bool operator<(AppVersion ver, AppVersion ver2)
        {
            return ver.Version < ver2.Version;
        }

        public static bool operator<=(AppVersion ver, AppVersion ver2)
        {
            return ver.Version <= ver2.Version;
        }

        public static bool operator>(AppVersion ver, AppVersion ver2)
        {
            return ver.Version > ver2.Version;
        }

        public static bool operator>=(AppVersion ver, AppVersion ver2)
        {
            return ver.Version >= ver2.Version;
        }

        #endregion

        #region Equality operators overrides

        // NOTE
        // We compare with an object in the equality operators because creating two overrides of the equality
        // comparer, one for strings and another for AppVersion instances as for the rest of comparision operators
        // will create a compilation error if we want to compare an AppVersion instance with null (myAppVersion == null)
        // as the compiler could not select which one of the overrides to use.


        public static bool operator==(AppVersion ver, object ver2)
        {
            if(object.ReferenceEquals(ver, null))
            {
                return object.ReferenceEquals(ver, ver2);
            }
            return ver.Equals(ver2);
        }

        public static bool operator!=(AppVersion ver, object ver2)
        {
            if(object.ReferenceEquals(ver, null))
            {
                return !object.ReferenceEquals(ver, ver2);
            }
            return !ver.Equals(ver2);
        }

        public override bool Equals(object obj)
        {
            var appVersion = obj as AppVersion;

            // Using ReferenceEquals because using appVersion == null will trigger the
            // AppVersion.operator== overload and cause an infinite recursion
            if(!object.ReferenceEquals(appVersion, null))
            {
                return Version.Equals(appVersion.Version);
            }

            var versionStr = obj as string;
            if(versionStr != null)
            {
                return Version.Equals(_tempAppVersion.InitVersionFromString(versionStr).Version);
            }

            return object.ReferenceEquals(this, obj);
        }

        #endregion

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public override string ToString()
        {
            return Version.ToString();
        }
    }

}