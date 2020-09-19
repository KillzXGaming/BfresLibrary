using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary.WiiU.Core
{
    /// <summary>
    /// Loads the hierachy and data of a <see cref="Bfres.ResFile"/>.
    /// </summary>
    public class ResFileWiiULoader : ResFileLoader
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------



        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileLoader"/> class loading data into the given
        /// <paramref name="resFile"/> from the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to load data into.</param>
        /// <param name="stream">The <see cref="Stream"/> to read data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        internal ResFileWiiULoader(ResFile resFile, Stream stream, bool leaveOpen = false)
            : base(resFile, stream, leaveOpen)
        {
            ByteOrder = ByteOrder.BigEndian;
        }

        internal ResFileWiiULoader(IResData resData, ResFile resFile, Stream stream, bool leaveOpen = false)
    : base(resData, resFile, stream, leaveOpen)
        {
            ByteOrder = ByteOrder.BigEndian;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileLoader"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to load data into.</param>
        /// <param name="fileName">The name of the file to load the data from.</param>
        internal ResFileWiiULoader(ResFile resFile, string fileName)
            : base(resFile, fileName)
        {
            ByteOrder = ByteOrder.BigEndian;
        }

        internal ResFileWiiULoader(IResData resData, ResFile resFile, string fileName)
            : base(resData, resFile, fileName)
        {
            ByteOrder = ByteOrder.BigEndian;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
    }
}
