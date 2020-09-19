using System;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a 16-bit fixed-point decimal consisting of 1 sign bit, 10 integer bits and 5 fractional bits (denoted
    /// as Q10.5). Note that the implementation is not reporting over- and underflowing errors.
    /// </summary>
    /// <remarks>
    /// Examples:
    ///   SIIIIIII_IIIFFFFF
    /// 0b00000000_00010000 = 0.5
    /// 0b00000000_00100000 = 1
    /// 0b00000001_00000000 = 8
    /// 0b01000000_00000000 = 512
    /// 0b10000000_00000000 = -1024
    /// </remarks>
    public struct Decimal10x5
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Represents the largest possible value of <see cref="Decimal10x5"/>.
        /// </summary>
        public static readonly Decimal10x5 MaxValue = new Decimal10x5(Int16.MaxValue);

        /// <summary>
        /// Represents the smallest possible value of <see cref="Decimal10x5"/>.
        /// </summary>
        public static readonly Decimal10x5 MinValue = new Decimal10x5(Int16.MinValue);

        private const int _m = 10; // Number of integral part bits.
        private const int _n = 5; // Number of fractional part bits.

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimal10x5"/> struct from the given <paramref name="raw"/>
        /// representation.
        /// </summary>
        /// <param name="raw">The raw representation of the internally stored bits.</param>
        internal Decimal10x5(int raw)
        {
            unchecked { Raw = (short)raw; }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the internally stored value to represent the instance.
        /// </summary>
        /// <remarks>Signed to get arithmetic rather than logical shifts.</remarks>
        internal short Raw { get; private set; }

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the given <see cref="Decimal10x5"/>.
        /// </summary>
        /// <param name="a">The <see cref="Decimal10x5"/>.</param>
        /// <returns>The result.</returns>
        public static Decimal10x5 operator +(Decimal10x5 a)
        {
            return a;
        }

        /// <summary>
        /// Adds the first <see cref="Decimal10x5"/> to the second one.
        /// </summary>
        /// <param name="a">The first <see cref="Decimal10x5"/>.</param>
        /// <param name="b">The second <see cref="Decimal10x5"/>.</param>
        /// <returns>The addition result.</returns>
        public static Decimal10x5 operator +(Decimal10x5 a, Decimal10x5 b)
        {
            return new Decimal10x5(a.Raw + b.Raw);
        }

        /// <summary>
        /// Negates the given <see cref="Decimal10x5"/>.
        /// </summary>
        /// <param name="a">The <see cref="Decimal10x5"/> to negate.</param>
        /// <returns>The negated result.</returns>
        public static Decimal10x5 operator -(Decimal10x5 a)
        {
            return new Decimal10x5(-a.Raw);
        }

        /// <summary>
        /// Subtracts the first <see cref="Decimal10x5"/> from the second one.
        /// </summary>
        /// <param name="a">The first <see cref="Decimal10x5"/>.</param>
        /// <param name="b">The second <see cref="Decimal10x5"/>.</param>
        /// <returns>The subtraction result.</returns>
        public static Decimal10x5 operator -(Decimal10x5 a, Decimal10x5 b)
        {
            return new Decimal10x5(a.Raw - b.Raw);
        }

        /// <summary>
        /// Multiplicates the given <see cref="Decimal10x5"/> by the scalar.
        /// </summary>
        /// <param name="a">The <see cref="Decimal10x5"/>.</param>
        /// <param name="s">The scalar.</param>
        /// <returns>The multiplication result.</returns>
        public static Decimal10x5 operator *(Decimal10x5 a, int s)
        {
            return new Decimal10x5(a.Raw * s);
        }

        /// <summary>
        /// Multiplicates the first <see cref="Decimal10x5"/> by the second one.
        /// </summary>
        /// <param name="a">The first <see cref="Decimal10x5"/>.</param>
        /// <param name="b">The second <see cref="Decimal10x5"/>.</param>
        /// <returns>The multiplication result.</returns>
        public static Decimal10x5 operator *(Decimal10x5 a, Decimal10x5 b)
        {
            int k = 1 << (_n - 1);
            return new Decimal10x5((a.Raw * b.Raw + k) >> _n);
        }

        /// <summary>
        /// Divides the given <see cref="Decimal10x5"/> through the scalar.
        /// </summary>
        /// <param name="a">The <see cref="Decimal10x5"/>.</param>
        /// <param name="s">The scalar.</param>
        /// <returns>The division result.</returns>
        public static Decimal10x5 operator /(Decimal10x5 a, int s)
        {
            return new Decimal10x5(a.Raw / s);
        }

        /// <summary>
        /// Divides the first <see cref="Decimal10x5"/> through the second one.
        /// </summary>
        /// <param name="a">The first <see cref="Decimal10x5"/>.</param>
        /// <param name="b">The second <see cref="Decimal10x5"/>.</param>
        /// <returns>The division result.</returns>
        public static Decimal10x5 operator /(Decimal10x5 a, Decimal10x5 b)
        {
            return new Decimal10x5((a.Raw << _n) / b.Raw);
        }

        /// <summary>
        /// Gets a value indicating whether the first specified <see cref="Decimal10x5"/> is the same as the second
        /// specified <see cref="Decimal10x5"/>.
        /// </summary>
        /// <param name="a">The first <see cref="Decimal10x5"/> to compare.</param>
        /// <param name="b">The second <see cref="Decimal10x5"/> to compare.</param>
        /// <returns>true, if both <see cref="Decimal10x5"/> are the same.</returns>
        public static bool operator ==(Decimal10x5 a, Decimal10x5 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Gets a value indicating whether the first specified <see cref="Decimal10x5"/> is not the same as the second
        /// specified <see cref="Decimal10x5"/>.
        /// </summary>
        /// <param name="a">The first <see cref="Decimal10x5"/> to compare.</param>
        /// <param name="b">The second <see cref="Decimal10x5"/> to compare.</param>
        /// <returns>true, if both <see cref="Decimal10x5"/> are not the same.</returns>
        public static bool operator !=(Decimal10x5 a, Decimal10x5 b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="Decimal10x5"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to represent in the new <see cref="Decimal10x5"/>
        /// instance.</param>
        public static explicit operator Decimal10x5(Int32 value)
        {
            return new Decimal10x5(value << _n);
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="Decimal10x5"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to represent in the new <see cref="Decimal10x5"/>
        /// instance.</param>
        public static explicit operator Decimal10x5(Double value)
        {
            return new Decimal10x5((int)Math.Round(value * (1 << _n)));
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="Decimal10x5"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to represent in the new <see cref="Decimal10x5"/>
        /// instance.</param>
        public static explicit operator Decimal10x5(Single value)
        {
            return new Decimal10x5((int)Math.Round(value * (1 << _n)));
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="Double"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Decimal10x5"/> value to represent in the new <see cref="Double"/>
        /// instance.</param>
        public static explicit operator Double(Decimal10x5 value)
        {
            return (double)value.Raw / (1 << _n);
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="Int32"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Decimal10x5"/> value to represent in the new <see cref="Int32"/>
        /// instance.</param>
        public static explicit operator Int32(Decimal10x5 value)
        {
            int k = 1 << (_n - 1);
            return (value.Raw + k) >> _n;
        }

        /// <summary>
        /// Converts the given <paramref name="value"/> value to a <see cref="Single"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Decimal10x5"/> value to represent in the new <see cref="Single"/>
        /// instance.</param>
        public static explicit operator Single(Decimal10x5 value)
        {
            return (float)((double)value.Raw / (1 << _n));
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this <see cref="Decimal10x5"/> is the same as the second specified
        /// <see cref="Decimal10x5"/>.
        /// </summary>
        /// <param name="obj">The object to compare, if it is a <see cref="Decimal10x5"/>.</param>
        /// <returns>true, if both <see cref="Decimal10x5"/> are the same.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Decimal10x5))
            {
                return false;
            }
            Decimal10x5 decimal10x5 = (Decimal10x5)obj;
            return Equals(decimal10x5);
        }

        /// <summary>
        /// Gets a hash code as an indication for object equality.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Raw;
        }

        /// <summary>
        /// Gets a string describing this <see cref="Decimal10x5"/>.
        /// </summary>
        /// <returns>A string describing this <see cref="Decimal10x5"/>.</returns>
        public override string ToString()
        {
            return ((double)this).ToString();
        }

        /// <summary>
        /// Indicates whether the current <see cref="Decimal10x5"/> is equal to another <see cref="Decimal10x5"/>.
        /// </summary>
        /// <param name="other">A <see cref="Decimal10x5"/> to compare with this <see cref="Decimal10x5"/>.</param>
        /// <returns>true if the current <see cref="Decimal10x5"/> is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(Decimal10x5 other)
        {
            return Equals(Raw == other.Raw);
        }
    }
}
