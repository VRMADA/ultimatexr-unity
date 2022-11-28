// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateXR.Extensions.System
{
    /// <summary>
    ///     <see cref="string" /> extensions.
    /// </summary>
    public static class StringExt
    {
        #region Public Methods

        /// <summary>
        ///     Gets the number of occurrences of a string in another string.
        /// </summary>
        /// <param name="self">The string where to perform the search</param>
        /// <param name="key">The string to find</param>
        /// <param name="caseSensitive">Whether the search should be case sensitive</param>
        /// <returns>Number of occurrences of <paramref name="key" /> in the source string</returns>
        public static int GetOccurrenceCount(this string self, string key, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(self) || string.IsNullOrEmpty(key))
            {
                return 0;
            }

            if (caseSensitive)
            {
                return (self.Length - self.Replace(key, string.Empty).Length) / key.Length;
            }

            return (self.Length - self.ToLower().Replace(key.ToLower(), string.Empty).Length) / key.Length;
        }

        /// <summary>
        ///     Gets the SHA-256 hash value of a string.
        /// </summary>
        /// <param name="self">String to get the SHA-256 hash value of</param>
        /// <returns>SHA-256 hash value of the string</returns>
        public static byte[] GetSha256(this string self)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.ASCII.GetBytes(self));
        }

        /// <summary>
        ///     Gets the MD5 hash value of a string.
        /// </summary>
        /// <param name="self">String to get the MD5 hash value of</param>
        /// <returns>MD5 hash value of the string</returns>
        public static byte[] GetMd5(this string self)
        {
            using HashAlgorithm algorithm = MD5.Create();
            return algorithm.ComputeHash(Encoding.ASCII.GetBytes(self));
        }

        /// <summary>
        ///     Gets the double SHA-256 hash value of a string.
        /// </summary>
        /// <param name="self">String to get the double SHA-256 hash value of</param>
        /// <returns>Double SHA-256 hash value of the string</returns>
        public static string GetSha256x2(this string self)
        {
            byte[] sha256 = self.GetSha256();
            return sha256.Aggregate(new StringBuilder(sha256.Length * 2), (sb, b) => sb.AppendFormat("{0:x2}", b)).ToString();
        }

        /// <summary>
        ///     Gets the double MD5 hash value of a string.
        /// </summary>
        /// <param name="self">String to get the double MD5 hash value of</param>
        /// <returns>Double MD5 hash value of the string</returns>
        public static string GetMd5x2(this string self)
        {
            byte[] md5 = self.GetMd5();
            return md5.Aggregate(new StringBuilder(md5.Length * 2), (sb, b) => sb.AppendFormat("{0:x2}", b)).ToString();
        }

        /// <summary>
        ///     Replaces the invalid characters in a path with a given character.
        /// </summary>
        /// <param name="self">The path to process</param>
        /// <param name="fallbackChar">The valid character to use as replacement</param>
        /// <param name="invalidChars">The invalid characters to replace</param>
        /// <returns>New string with the replacements</returns>
        /// <exception cref="ArgumentOutOfRangeException">The replacement character is part of the invalid characters</exception>
        public static string ReplaceInvalidPathChars(this string self, char fallbackChar = PathFallbackChar, params char[] invalidChars)
        {
            if (invalidChars.Length == 0)
            {
                return self.ReplaceInvalidDirPathChars(fallbackChar);
            }

            if (invalidChars.Contains(fallbackChar))
            {
                throw new ArgumentOutOfRangeException(nameof(fallbackChar), fallbackChar, "Fallback should be a valid character");
            }

            return string.Join(fallbackChar.ToString(), self.Split(invalidChars));
        }

        /// <summary>
        ///     Replaces the invalid characters in a directory path with a given character.
        /// </summary>
        /// <param name="self">The directory path to process</param>
        /// <param name="fallbackChar">The valid character to use as replacement</param>
        /// <returns>New string with the replacements</returns>
        /// <exception cref="ArgumentOutOfRangeException">The replacement character is part of the invalid characters</exception>
        public static string ReplaceInvalidDirPathChars(this string self, char fallbackChar = PathFallbackChar)
        {
            return self.ReplaceInvalidPathChars(fallbackChar, Path.GetInvalidPathChars());
        }

        /// <summary>
        ///     Replaces the invalid characters in a file path with a given character.
        /// </summary>
        /// <param name="self">The file path to process</param>
        /// <param name="fallbackChar">The valid character to use as replacement</param>
        /// <returns>New string with the replacements</returns>
        /// <exception cref="ArgumentOutOfRangeException">The replacement character is part of the invalid characters</exception>
        public static string ReplaceInvalidFilePathChars(this string self, char fallbackChar = PathFallbackChar)
        {
            return self.ReplaceInvalidPathChars(fallbackChar, Path.GetInvalidFileNameChars());
        }

        /// <summary>
        ///     Checks if a path is a child of another path.
        ///     Adapted from https://stackoverflow.com/questions/8091829/how-to-check-if-one-path-is-a-child-of-another-path
        /// </summary>
        /// <param name="candidate">Path candidate</param>
        /// <param name="other">Path to check against</param>
        /// <param name="canBeSame">Whether to also consider the same directory as valid</param>
        /// <returns>Whether the path is child of the parent path</returns>
        public static bool IsSubDirectoryOf(this string candidate, string other, bool canBeSame = true)
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
                var message = $"Unable to check directories {candidate} and {other}: {error}";
                Debug.LogError(message);
            }

            return isChild;
        }

        /// <summary>
        ///     Creates a random string.
        /// </summary>
        /// <param name="length">String length</param>
        /// <param name="includeLetters">Include letters in the string?</param>
        /// <param name="includeNumbers">Include numbers in the string?</param>
        /// <returns>Random string with given length or <see cref="string.Empty" /> if no letters and number were specified</returns>
        public static string RandomString(int length, bool includeLetters, bool includeNumbers)
        {
            const string lettersOnly       = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbersOnly       = "0123456789";
            const string lettersAndNumbers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            if (includeLetters && !includeNumbers)
            {
                return new string(Enumerable.Repeat(lettersOnly, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
            }
            if (!includeLetters && includeNumbers)
            {
                return new string(Enumerable.Repeat(numbersOnly, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
            }
            if (includeLetters && includeNumbers)
            {
                return new string(Enumerable.Repeat(lettersAndNumbers, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
            }

            return string.Empty;
        }

        /// <summary>
        ///     Splits a string using CamelCase.
        /// </summary>
        /// <param name="self">Input string</param>
        /// <returns>Output string with added spaces</returns>
        public static string SplitCamelCase(this string self)
        {
            return Regex.Replace(self, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        /// <summary>
        ///     Throws an exception if the string is null or only contains whitespaces.
        /// </summary>
        /// <param name="self">The string to check</param>
        /// <param name="paramName">The parameter name, used as argument for the exceptions</param>
        /// <exception cref="ArgumentNullException"><paramref name="self" /> is <see langword="null" /></exception>
        /// <exception cref="ArgumentException">Whitespace string is not allowed</exception>
        public static void ThrowIfNullOrWhitespace(this string self, string paramName)
        {
            if (self is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (string.IsNullOrWhiteSpace(self))
            {
                throw new ArgumentOutOfRangeException(paramName, self, "Value cannot be whitespace");
            }
        }

        /// <summary>
        ///     Throws an exception if the string is null or empty.
        /// </summary>
        /// <param name="self">The string</param>
        /// <param name="paramName">The parameter name, used as arguments for the exceptions</param>
        /// <exception cref="ArgumentNullException"><paramref name="self" /> is <see langword="null" /></exception>
        /// <exception cref="ArgumentException">Empty string is not allowed</exception>
        public static void ThrowIfNullOrEmpty(this string self, string paramName)
        {
            if (self == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (string.IsNullOrEmpty(self))
            {
                throw new ArgumentException("Empty string is not allowed", paramName);
            }
        }

        #endregion

        #region Private Types & Data

        private const char PathFallbackChar = '_';

        #endregion
    }
}