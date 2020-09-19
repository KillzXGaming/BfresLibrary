using System;

namespace BfresLibrary.Core
{
    /// <summary>
    /// Represents a <see cref="StringComparer.Ordinal"/> sorting empty strings to the end of lists.
    /// </summary>
    internal class ResStringComparer : StringComparer
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private static ResStringComparer _instance;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        private ResStringComparer()
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        internal static ResStringComparer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResStringComparer();
                }
                return _instance;
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        public override int Compare(string x, string y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (String.IsNullOrEmpty(x)) return 1;
            if (String.IsNullOrEmpty(y)) return -1;
            return String.CompareOrdinal(x, y);
        }

        public override bool Equals(string x, string y)
        {
            return Ordinal.Equals(x, y);
        }

        public override int GetHashCode(string obj)
        {
            return Ordinal.GetHashCode(obj);
        }
    }
}
