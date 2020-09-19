using System;

namespace BfresLibrary.Core
{
    /// <summary>
    /// Represents extension methods for <see cref="UInt32"/> instances.
    /// </summary>
    internal static class UInt32Extensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------
        
        /// <summary>
        /// Returns an <see cref="UInt32"/> instance represented by the given number of <paramref name="bits"/>, starting
        /// at the <paramref name="firstBit"/>.
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="firstBit">The first bit of the encoded value.</param>
        /// <param name="bits">The number of least significant bits which are used to store the <see cref="UInt32"/>
        /// value.</param>
        /// <returns>The decoded <see cref="UInt32"/>.</returns>
        internal static uint Decode(this uint self, int firstBit, int bits)
        {
            // Shift to the first bit and keep only the required bits.
            return (self >> firstBit) & ((1u << bits) - 1);
        }

        /// <summary>
        /// Returns the current <see cref="UInt32"/> with the bit at the <paramref name="index"/> set (being 1).
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="index">The 0-based index of the bit to enable.</param>
        /// <returns>The current <see cref="UInt32"/> with the bit enabled.</returns>
        internal static uint EnableBit(this uint self, int index)
        {
            return self | (1u << index);
        }
        
        /// <summary>
        /// Returns the current <see cref="UInt32"/> with the given <paramref name="value"/> set into the given number
        /// of <paramref name="bits"/> starting at <paramref name="firstBit"/>.
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="value">The value to encode.</param>
        /// <param name="firstBit">The first bit used for the encoded value.</param>
        /// <param name="bits">The number of bits which are used to store the <see cref="UInt32"/> value.</param>
        /// <returns>The current <see cref="UInt32"/> with the value encoded into it.</returns>
        internal static uint Encode(this uint self, uint value, int firstBit, int bits)
        {
            // Clear the bits required for the value and fit it into them by truncating.
            uint mask = ((1u << bits) - 1) << firstBit;
            self &= ~mask;
            value = (value << firstBit) & mask;

            // Set the value.
            return self | value;
        }

        /// <summary>
        /// Returns the current <see cref="UInt32"/> with the bit at the <paramref name="index"/> cleared (being 0).
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="index">The 0-based index of the bit to disable.</param>
        /// <returns>The current <see cref="UInt32"/> with the bit disabled.</returns>
        internal static uint DisableBit(this uint self, int index)
        {
            return self & ~(1u << index);
        }

        /// <summary>
        /// Returns a value indicating whether the bit at the <paramref name="index"/> in the current
        /// <see cref="UInt32"/> is enabled or disabled.
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="index">The 0-based index of the bit to check.</param>
        /// <returns><c>true</c> when the bit is set; otherwise <c>false</c>.</returns>
        internal static bool GetBit(this uint self, int index)
        {
            return (self & (1 << index)) != 0;
        }

        /// <summary>
        /// Returns the current <see cref="UInt32"/> with all bits rotated in the given <paramref name="direction"/>,
        /// where positive directions rotate left and negative directions rotate right.
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="direction">The direction in which to rotate, where positive directions rotate left.</param>
        /// <returns>The current <see cref="UInt32"/> with the bits rotated.</returns>
        internal static uint RotateBits(this uint self, int direction)
        {
            int bits = sizeof(uint) * 8;
            if (direction > 0)
            {
                return (self << direction) | (self >> (bits - direction));
            }
            else if (direction < 0)
            {
                direction = -direction;
                return (self >> direction) | (self << (bits - direction));
            }
            return self;
        }

        /// <summary>
        /// Returns the current <see cref="UInt32"/> with the bit at the <paramref name="index"/> enabled or disabled,
        /// according to <paramref name="enable"/>.
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="index">The 0-based index of the bit to enable or disable.</param>
        /// <param name="enable"><c>true</c> to enable the bit; otherwise <c>false</c>.</param>
        /// <returns>The current <see cref="UInt32"/> with the bit enabled or disabled.</returns>
        internal static uint SetBit(this uint self, int index, bool enable)
        {
            if (enable)
            {
                return EnableBit(self, index);
            }
            else
            {
                return DisableBit(self, index);
            }
        }

        /// <summary>
        /// Returns the current <see cref="UInt32"/> with the bit at the <paramref name="index"/> enabled when it is
        /// disabled or disabled when it is enabled.
        /// </summary>
        /// <param name="self">The extended <see cref="UInt32"/> instance.</param>
        /// <param name="index">The 0-based index of the bit to toggle.</param>
        /// <returns>The current <see cref="UInt32"/> with the bit toggled.</returns>
        internal static uint ToggleBit(this uint self, int index)
        {
            if (GetBit(self, index))
            {
                return DisableBit(self, index);
            }
            else
            {
                return EnableBit(self, index);
            }
        }
    }
}
