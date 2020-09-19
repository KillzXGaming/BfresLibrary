using System.Diagnostics;
using System.Text;
using Syroot.BinaryData;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a <see cref="String"/> which is stored in a <see cref="ResFile"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(String) + "}")]
    public class ResString : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// The textual <see cref="String"/> represented by this instance.
        /// </summary>
        public string String
        {
            get; set;
        }

        /// <summary>
        /// The <see cref="Encoding"/> with which this string was read or will be written.
        /// </summary>
        public Encoding Encoding
        {
            get; set;
        }

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="ResString"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to represent in the new <see cref="ResString"/> instance.
        /// </param>
        public static implicit operator ResString(string value)
        {
            return new ResString() { String = value };
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to an <see cref="String"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="ResString"/> value to represent in the new <see cref="String"/> instance.
        /// </param>
        public static implicit operator string(ResString value)
        {
            return value.String;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the value of the <see cref="String"/> property.
        /// </summary>
        /// <returns>The value of the <see cref="String"/> property.</returns>
        public override string ToString()
        {
            return String;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
                String = loader.LoadString(Encoding ?? loader.Encoding);
            else
                String = loader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding ?? loader.Encoding);
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(String, Encoding);
        }
    }
}
