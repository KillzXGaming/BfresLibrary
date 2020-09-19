using System;
using System.Collections;
using System.Collections.Generic;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a dictionary which can quickly look up <see cref="IResData"/> instances of type
    /// <typeparamref name="T"/> via key or index.
    /// </summary>
    /// <typeparam name="T">The specialized type of the <see cref="IResData"/> instances.</typeparam>
    public sealed class ResDict<T> : ResDict, IEnumerable<KeyValuePair<string, T>>
        where T : IResData, new()
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResDict{T}"/> class.
        /// </summary>
        public ResDict() : base()
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets all stored instances.
        /// </summary>
        public new IEnumerable<T> Values
        {
            get
            {
                foreach (Node node in Nodes)
                {
                    yield return (T)node.Value;
                }
            }
        }

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value stored at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The 0-based index of the instance to get or set.</param>
        /// <returns>The instance at the specified <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is smaller than 0 or bigger or equal to
        /// <see cref="ResDict.Count"/>.</exception>
        public new T this[int index]
        {
            get { return (T)base[index]; }
            set { base[index] = value; }
        }

        /// <summary>
        /// Gets or sets the value stored under the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The textual key of the instance to get or set.</param>
        /// <returns>The instance with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentException">An instance with the same <paramref name="key"/> already exists.
        /// </exception>
        /// <exception cref="KeyNotFoundException">An instance with the given <paramref name="key"/> does not exist.
        /// </exception>
        public new T this[string key]
        {
            get { return (T)base[key]; }
            set { base[key] = value; }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Adds the given <paramref name="value"/> under the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The textual key under which the <see cref="IResData"/> instance will be stored.</param>
        /// <param name="value">The <see cref="IResData"/> to add.</param>
        /// <exception cref="ArgumentException">An <see cref="IResData"/> instance with the same <paramref name="key"/>
        /// already exists.</exception>
        public void Add(string key, T value)
        {
            Add(key, (IResData)value);
        }

        /// <summary>
        /// Determines whether the given <paramref name="value"/> is in the dictionary.
        /// </summary>
        /// <param name="value">The <see cref="IResData"/> instance to locate in the dictionary. The value can be
        /// <c>null</c>.</param>
        /// <returns><c>true</c> if <paramref name="value"/> was found in the dictionary; otherwise <c>false</c>.
        /// </returns>
        public bool ContainsValue(T value)
        {
            return ContainsValue((IResData)value);
        }

        /// <summary>
        /// Returns a generic <see cref="IEnumerator"/> which can be used to iterate over the items in the dictionary.
        /// </summary>
        /// <returns>An enumerator to iterate over the items in the dictionary.</returns>
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            foreach (Node node in Nodes)
            {
                yield return new KeyValuePair<string, T>(node.Key, (T)node.Value);
            }
        }

        /// <summary>
        /// Searches for the specified <paramref name="value"/> and returns the zero-based index of the first occurrence
        /// within the entire dictionary.
        /// </summary>
        /// <param name="value">The <see cref="IResData"/> instance to locate in the dictionary. The value can be
        /// <c>null</c>.</param>
        /// <returns>The zero-based index of the first occurence of <paramref name="value"/> within the entire
        /// dictionary if found; otherwise <c>-1</c>.</returns>
        public int IndexOf(T value)
        {
            return IndexOf((IResData)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific <paramref name="value"/> from the dictionary.
        /// </summary>
        /// <param name="value">The <see cref="IResData"/> instance to remove from the dictionary. The value can be
        /// <c>null</c>.</param>
        /// <returns><c>true</c> if <paramref name="value"/> was successfully removed; otherwise <c>false</c>. This
        /// method also returns <c>false</c> if <paramref name="value"/> was not found in the dictionary.</returns>
        public bool Remove(T value)
        {
            return Remove((IResData)value);
        }

        /// <summary>
        /// Copies the elements of the dictionary as <see cref="KeyValuePair{String, IResDat}"/> instances to a new
        /// array and returns it.
        /// </summary>
        /// <returns>An array containing copies of the elements.</returns>
        public new KeyValuePair<string, T>[] ToArray()
        {
            KeyValuePair<string, T>[] resData = new KeyValuePair<string, T>[Count];
            int i = 0;
            foreach (Node node in Nodes)
            {
                resData[i] = new KeyValuePair<string, T>(node.Key, (T)node.Value);
                i++;
            }
            return resData;
        }

        /// <summary>
        /// Returns <c>true</c> if a key was found for the given <paramref name="value"/> and has been assigned to
        /// <paramref name="key"/>, or <c>false</c> if no key was found for the value and <c>null</c> was assigned to
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="value">The <see cref="IResData"/> to look up a key for.</param>
        /// <param name="key">The variable receiving the found key or <c>null</c>.</param>
        /// <returns><c>true</c> if a key was found and assigned; otherwise <c>false</c>.</returns>
        public bool TryGetKey(T value, out string key)
        {
            return TryGetKey((IResData)value, out key);
        }

        /// <summary>
        /// Returns <c>true</c> if an instance was stored under the given <paramref name="key"/> and has been assigned
        /// to <paramref name="value"/>, or <c>false</c> if no instance is stored under the given <paramref name="key"/>
        /// and <c>null</c> was assigned to <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The textual key of the instance to get or set.</param>
        /// <param name="value">The variable receiving the found instance or <c>null</c>.</param>
        /// <returns><c>true</c> if an instance was found and assigned; otherwise <c>false</c>.</returns>
        public bool TryGetValue(string key, out T value)
        {
            if (TryGetValue(key, out IResData resData))
            {
                value = (T)resData;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Loads an <see cref="IResData"/> instance from the given <paramref name="loader"/>.
        /// </summary>
        /// <param name="loader">The <see cref="ResFileLoader"/> to load the instance with.</param>
        /// <returns>The loaded <see cref="IResData"/> instance.</returns>
        protected override IResData LoadNodeValue(ResFileLoader loader)
        {
            return loader.Load<T>();
        }
    }
}
