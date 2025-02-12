// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using UltimateXR.Core;
using UltimateXR.Core.Settings;
using UnityEngine;

namespace UltimateXR.Extensions.System.IO
{
    public static class PathExt
    {
        #region Public Methods

        /// <summary>
        ///     Like .NET's <see cref="Path.Combine(string,string)" /> but addressing some issues discussed in
        ///     https://www.davidboike.dev/2020/06/path-combine-isnt-as-cross-platform-as-you-think-it-is/
        /// </summary>
        /// <param name="basePath">Base path</param>
        /// <param name="additional">Additional segments or multi-segment paths</param>
        /// <returns>Path result of combining the base path and the additional segments or multi-segment paths</returns>
        public static string Combine(string basePath, params string[] additional)
        {
            string[][] splits      = additional.Select(s => s.Split(PathSplitCharacters)).ToArray();
            int        totalLength = splits.Sum(arr => arr.Length);
            string[]   segments    = new string[totalLength + 1];

            segments[0] = basePath;
            var i = 0;

            foreach (string[] split in splits)
            {
                foreach (string value in split)
                {
                    i++;
                    segments[i] = value;
                }
            }

            return Path.Combine(segments);
        }

        /// <summary>
        ///     Normalizes a path or sub-path so that any wrong directory separator char is fixed for the current platform.
        /// </summary>
        /// <param name="pathOrSubPath">The path or sub-path</param>
        /// <returns>Normalized path</returns>
        public static string Normalize(string pathOrSubPath)
        {
            if (pathOrSubPath == null)
            {
                return null;
            }
            
            if (Path.IsPathFullyQualified(pathOrSubPath))
            {
                return Path.GetFullPath(new Uri(pathOrSubPath).LocalPath).TrimEnd(PathSplitCharacters);
            }

            foreach (char separator in PathSplitCharacters.Where(c => c != Path.DirectorySeparatorChar))
            {
                pathOrSubPath = pathOrSubPath.Replace(separator, Path.DirectorySeparatorChar);
            }

            return pathOrSubPath;
        }

        /// <summary>
        ///     Checks if a path is a child of another path.
        ///     Adapted from https://stackoverflow.com/questions/8091829/how-to-check-if-one-path-is-a-child-of-another-path
        /// </summary>
        /// <param name="candidate">Path candidate</param>
        /// <param name="other">Path to check against</param>
        /// <param name="canBeSame">Whether to also consider the same directory as valid</param>
        /// <returns>Whether the path is child of the parent path</returns>
        public static bool IsSubDirectoryOf(string candidate, string other, bool canBeSame = true)
        {
            var isChild = false;
            try
            {
                // Some initial corrections to avoid false negatives:

                var candidateInfo = new DirectoryInfo(candidate.Replace(@"\", @"/").TrimEnd('/'));
                var otherInfo     = new DirectoryInfo(other.Replace(@"\", @"/").TrimEnd('/'));

                // Check if same directory

                if (canBeSame && string.Compare(candidateInfo.FullName, otherInfo.FullName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                // Start traversing upwards

                while (candidateInfo.Parent != null)
                {
                    if (string.Equals(candidateInfo.Parent.FullName, otherInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        isChild = true;
                        break;
                    }

                    candidateInfo = candidateInfo.Parent;
                }
            }
            catch (Exception error)
            {
                if (UxrGlobalSettings.Instance.LogLevelCore >= UxrLogLevel.Errors)
                {
                    Debug.LogError($"{UxrConstants.CoreModule} Unable to check directories {candidate} and {other}: {error}");
                }
            }

            return isChild;
        }

        /// <summary>
        ///     Gets the filename without the extension. Solves some issues with Android paths that
        ///     Path.GetFileNameWithoutExtension() has.
        /// </summary>
        /// <param name="uriPath">File path</param>
        /// <returns>Filename without the extension</returns>
        public static string GetFileNameWithoutExtension(string uriPath)
        {
            // Normalize separators to handle both '/' and '\'
            uriPath = uriPath.Replace('\\', '/');

            // Split the path by '/' to isolate the file name
            string[] parts = uriPath.Split('/');

            // Get the last segment, which should be the file with extension
            string fileWithExtension = parts[parts.Length - 1];

            // Find the last dot to remove the extension manually
            int dotIndex = fileWithExtension.LastIndexOf('.');

            // If there is no dot, return the file as is; otherwise, remove the extension
            if (dotIndex == -1)
            {
                return fileWithExtension; // No extension found
            }
            return fileWithExtension.Substring(0, dotIndex); // Remove extension
        }

        /// <summary>
        ///     Removes the top level folder from the path string if there is one.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Path without the top level folder</returns>
        public static string RemoveHighestLevelFolder(string path)
        {
            // Normalize the path by replacing backslashes with forward slashes
            path = path.Replace('\\', '/');

            // Split the path into components
            string[] components = path.Split('/');

            // If there's only one component or the first component is empty, return the original path
            if (components.Length <= 1 || string.IsNullOrEmpty(components[0]))
            {
                return Normalize(path);
            }

            // Join the components starting from the second one to form the new path
            string newPath = string.Join("/", components, 1, components.Length - 1);

            // Return the new path
            return Normalize(newPath);
        }

        #endregion

        #region Private Types & Data

        private static readonly char[] PathSplitCharacters = { '/', '\\' };

        #endregion
    }
}