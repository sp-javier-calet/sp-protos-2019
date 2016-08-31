using System;
using System.Text;

namespace SocialPoint.Base
{
    /// <summary>
    ///     Class that stores version numbers in the format Major.Minor.Revision.Build (e.g. 1.22.3.43434)
    ///     Version numbers must be positive. 
    ///     Comparisions between version numbers are made comparing each individual version number 
    ///     in the order as they appear (first Major, then Minor, etc)
    ///     Revision and Build numbers may be ignored but they are treated internally as zero
    /// </summary>
    /// <remarks>
    ///     This is basically a rewrite of System.Version and it was made to fulfill two objectives
    ///     - Avoid throwing exceptions on errors
    ///     - Avoid allocating new objects 
    /// </remarks>
    public sealed class AppVersion
    {
        // Used in the ToString method to avoid string allocations
        static StringBuilder _stringBuilder = new StringBuilder();

        // Temp used when comparing with strings to avoid creating a new AppVersion object
        static internal AppVersion _tempAppVersion = new AppVersion(0, 0);

        // Temp array used to hold the components of a string containing a version
        static readonly int[] _parseComponentsTmp = new int[4];

        /// <summary>
        ///     Shared instance representing a version Zero. Use it to check the format of a parsed version string.
        /// </summary>
        public static readonly AppVersion Zero = new AppVersion(0, 0);

        // Saves the version in string format
        string _versionStr;

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Revision { get; private set; }

        public int Build { get; private set; }

        /// <summary>
        ///     Creates a new AppVersion instance from a version string (x.y.z) in
        ///     a safe way: if the format is incorrect it returns version 0.0
        /// </summary>
        public AppVersion(string versionStr)
        {
            InitVersionFromString(versionStr);
        }

        /// <summary>
        ///     Creates a new AppVersion instance from a version numbers in
        ///     a safe way: if the format is incorrect it returns version 0.0
        /// </summary>
        public AppVersion(int major, int minor, int revision = 0, int build = 0)
        {
            if(AppVersion.Zero != InitVersionWithNumbers(major, minor, revision, build))
            {
                _versionStr = CreateVersionString(major, minor, revision, build);
            }
        }

        /// <summary>
        ///     Parses an string containing a version and replaces the current instance version values
        ///     with the values readed from the string. If the string does not have a valid format
        ///     replaces the current instance with AppVersion.Zero
        /// </summary>
        public AppVersion Parse(string versionStr)
        {
            if(this == AppVersion.Zero)
            {
                return this;
            }
            
            return InitVersionFromString(versionStr);
        }

        #region Comparision operators overrides. Compare with a string

        public static bool operator<(AppVersion ver, string versionStr)
        {
            return ver < _tempAppVersion.InitVersionFromString(versionStr);
        }

        public static bool operator<=(AppVersion ver, string versionStr)
        {
            return ver <= _tempAppVersion.InitVersionFromString(versionStr);
        }

        public static bool operator>(AppVersion ver, string versionStr)
        {
            return ver > _tempAppVersion.InitVersionFromString(versionStr);
        }

        public static bool operator>=(AppVersion ver, string versionStr)
        {
            return ver >= _tempAppVersion.InitVersionFromString(versionStr);
        }

        #endregion

        #region Comparision operators overrides. Compare with another AppVersion instance

        public static bool operator<(AppVersion ver, AppVersion ver2)
        {
            return ver.CompareTo(ver2) < 0;
        }

        public static bool operator<=(AppVersion ver, AppVersion ver2)
        {
            return ver.CompareTo(ver2) <= 0;
        }

        public static bool operator>(AppVersion ver, AppVersion ver2)
        {
            return ver.CompareTo(ver2) > 0;
        }

        public static bool operator>=(AppVersion ver, AppVersion ver2)
        {
            return ver.CompareTo(ver2) >= 0;
        }

        #endregion

        #region Equality operators overrides

        // NOTE
        // We compare with an object in the equality operators because creating two overrides of the equality
        // comparer (one for strings and another for AppVersion instances as we did for the rest of comparision operators)
        // will create a compilation error if we want to compare an AppVersion instance with null (myAppVersion == null)
        // This is caused because the compiler can't select which one of the two overrides (AppVersion or string) to use
        // when comparing against null.

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

        public int CompareTo(AppVersion value)
        {
            if(value == null)
                return 1;

            if(Major != value.Major)
            {
                if(Major > value.Major)
                    return 1;
                else
                    return -1;
            }

            if(Minor != value.Minor)
            {
                if(Minor > value.Minor)
                    return 1;
                else
                    return -1;
            }

            if(Revision != value.Revision)
            {
                if(Revision > value.Revision)
                    return 1;
                else
                    return -1;
            }

            if(Build != value.Build)
            {
                if(Build > value.Build)
                    return 1;
                else
                    return -1;
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            var appVersion = obj as AppVersion;

            // Using ReferenceEquals because using appVersion == null will trigger the
            // AppVersion.operator== overload and cause an infinite recursion
            if(!object.ReferenceEquals(appVersion, null))
            {
                return this.Equals(appVersion);
            }

            var versionStr = obj as string;
            if(versionStr != null)
            {
                return this.Equals(_tempAppVersion.InitVersionFromString(versionStr));
            }   

            return object.ReferenceEquals(this, obj);
        }

        public bool Equals(AppVersion obj)
        {
            if(obj == null)
                return false;

            if((Major != obj.Major)
               || (Minor != obj.Minor)
               || (Revision != obj.Revision)
               || (Build != obj.Build))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Object overrides

        public override int GetHashCode()
        {
            int hash = 0;

            hash |= (Major & 0x000000FF) << 28;
            hash |= (Minor & 0x000000FF) << 20;
            hash |= (Revision & 0x00000FF) << 12;
            hash |= (Build & 0x000000FF);

            return hash;
        }

        public override string ToString()
        {
            return _versionStr;
        }

        static string CreateVersionString(int major, int minor, int revision, int build)
        {
            _stringBuilder.Length = 0;
            _stringBuilder.Append(major).Append(".").Append(minor);

            if(revision > 0)
            {
                _stringBuilder.Append(".").Append(revision);
            }

            if(build > 0)
            {
                _stringBuilder.Append(".").Append(build);
            }

            return _stringBuilder.ToString();
        }

        #endregion

        #region Internal Initialization method helpers to avoid allocating new objects

        internal AppVersion InitVersionWithNumbers(int major, int minor, int revision, int build)
        {
            if(major < 0 || minor < 0 || revision < 0 || build < 0)
            {
                return InitZeroVersion();
            }

            Major = major;
            Minor = minor;
            Revision = revision;
            Build = build;

            return this;
        }

        internal AppVersion InitVersionFromString(string versionStr)
        {
            int major = 0, minor = 0, revision = 0, build = 0;

            if((object)versionStr == null)
            {
                InitZeroVersion();
            }

            if(ParseVersionString(versionStr, out major, out minor, out revision, out build))
            {
                _versionStr = versionStr;
                return InitVersionWithNumbers(major, minor, revision, build);
            }
            else
            {
                _versionStr = AppVersion.Zero.ToString();
                return AppVersion.Zero;
            }
        }

        internal AppVersion InitZeroVersion()
        {
            _versionStr = AppVersion.Zero.ToString();

            return InitVersionWithNumbers(
                AppVersion.Zero.Major,
                AppVersion.Zero.Minor,
                AppVersion.Zero.Revision,
                AppVersion.Zero.Build
            );
        }

        #endregion

        #region Helpers

        // Parse an string containing a version specified with a format of Major.minor.Revision.Build)
        static bool ParseVersionString(string versionStr, out int major, out int minor, out int revision, out int build)
        {
            _parseComponentsTmp[0] = 0;
            _parseComponentsTmp[1] = 0;
            _parseComponentsTmp[2] = 0;
            _parseComponentsTmp[3] = 0;

            bool hasError = false;

            int currentComponentIdx = 0;
            int currentValue = 0;

            for(var idx = 0; idx < versionStr.Length && !hasError; ++idx)
            {

                char c = versionStr[idx];


                if(c == '.')
                {
                    _parseComponentsTmp[currentComponentIdx] = currentValue;
                    currentComponentIdx += 1;
                    currentValue = 0;
                    // A version string can't have more than 4 components
                    if(currentComponentIdx > 3)
                    {
                        hasError = true;
                    }
                }
                else if(c >= '0' && c <= '9')
                {
                    currentValue = (c - '0') + (currentValue * 10);
                }
                else
                {
                    hasError = true;
                }
            }

            // A version string needs at least 2 components
            if(currentComponentIdx <= 0)
            {
                hasError = true;
            }

            if(hasError)
            {
                major = minor = revision = build = 0;
                return false;
            }

            // Save the last component
            _parseComponentsTmp[currentComponentIdx] = currentValue;

            major = _parseComponentsTmp[0];
            minor = _parseComponentsTmp[1];
            revision = _parseComponentsTmp[2];
            build = _parseComponentsTmp[3];

            return true;
        }

        #endregion
    }

}
