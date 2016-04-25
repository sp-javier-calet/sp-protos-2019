using System.Text.RegularExpressions;
using System.IO;
using System;

namespace SocialPointEditor.Assets.PlatformEx
{
    public static class PlatformEx
    {
        static readonly Regex volumeRexp = new Regex(@"^[a-zA-Z0-9 -_]+:\\"); //Based on the valid charaters for a volume label on windows(includes LETTER)

        /// <summary>
        /// Extension. Returns a system path in normalized forward-slash, no volume, form.
        /// </summary>
        /// <returns>The normalized path.</returns>
        /// <param name="str">Input path.</param>
        public static String NormalizedPath(this String str)
        {
            var replaced = str;
            //If is rooted first of all normalize root path
            if(IsUniversalPathRooted(replaced))
            {
                var root = GetUniversalPathRoot(replaced);
                var regex = new Regex(Regex.Escape(root)); //Escape so it cannot be interpreted as a regex but a string literal
                replaced = regex.Replace(replaced, "/", 1); //Replace just the root
            }

            return replaced.Replace('\\', '/');
        }

        /// <summary>
        /// Extension. Normalized both input and replacement and returns the replaced string in normalized forward-slash, no volume, form.
        /// </summary>
        /// <returns>The ex.</returns>
        /// <param name="str">String.</param>
        /// <param name="input">Input.</param>
        /// <param name="replacement">Replacement.</param>
        public static String NormalizedReplace(this String str, String input, String replacement)
        {
            return str.NormalizedPath().Replace(input.NormalizedPath(), replacement.NormalizedPath());
        }

        /// <summary>
        /// Gets the current system formatted path given any other kind of path
        /// </summary>
        /// <returns>The sys path.</returns>
        /// <param name="str">String.</param>
        public static String ToSysPath(this String str)
        {
            var normalized = str.NormalizedPath();
            if(Path.IsPathRooted(normalized))
            {
                return Path.GetFullPath(normalized);
            }
            return normalized.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Determines if a path is rooted even if the path does not conform the current system format.
        /// </summary>
        /// <returns><c>true</c> if path is rooted; otherwise, <c>false</c>.</returns>
        /// <param name="path">Path.</param>
        static bool IsUniversalPathRooted(string path)
        {
            path = path.Trim();
            if(Path.IsPathRooted(path))
            {
                return true;
            }

            if(volumeRexp.IsMatch(path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// If the path is rooted, return the root system independently. Otherwise throw an exception.
        /// </summary>
        /// <returns>The path root.</returns>
        /// <param name="path">Path.</param>
        static string GetUniversalPathRoot(string path)
        {
            var root = Path.GetPathRoot(path);
            if(!root.Equals(String.Empty))
            {
                return root;
            }

            var matchO = volumeRexp.Match(path);
            if(matchO.Success)
            {
                return matchO.Value;
            }
            throw new Exception(String.Format("Could not match any path root. Make sure the path you provided is rooted. (path:'{0}')",
			                                  path));
        }
    }
}