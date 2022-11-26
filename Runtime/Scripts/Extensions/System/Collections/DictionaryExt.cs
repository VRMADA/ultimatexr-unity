// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryExt.cs" company="VRMADA">
//   Copyright (c) VRMADA, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace UltimateXR.Extensions.System.Collections
{
    /// <summary>
    ///     <see cref="IDictionary{TKey,TValue}" /> and <see cref="Dictionary{TKey,TValue}" /> extensions.
    /// </summary>
    public static class DictionaryExt
    {
        #region Public Methods

        /// <summary>
        ///     Adds all elements in another dictionary to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="self">Destination dictionary</param>
        /// <param name="other">Source dictionary</param>
        /// <param name="overrideExistingKeys">Determines if duplicated keys must be overriden with other's values</param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> other, bool overrideExistingKeys = false)
        {
            foreach (var kv in other)
            {
                if (!self.ContainsKey(kv.Key))
                {
                    self.Add(kv.Key, kv.Value);
                }
                else if (overrideExistingKeys)
                {
                    self[kv.Key] = kv.Value;
                }
            }
        }

        /// <summary>
        ///     Gets a given value defined by a key in a dictionary. If the key is not found, it is added and the value is given
        ///     the default value.
        /// </summary>
        /// <param name="self">Dictionary</param>
        /// <param name="key">Key to look for</param>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <returns>
        ///     Value in the dictionary. If the key doesn't exist it will be added and the return value will be the default
        ///     value
        /// </returns>
        public static TValue GetOrAddValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
                    where TValue : new()
        {
            if (!self.TryGetValue(key, out TValue value))
            {
                value = new TValue();
                self.Add(key, value);
            }

            return value;
        }

        #endregion
    }
}