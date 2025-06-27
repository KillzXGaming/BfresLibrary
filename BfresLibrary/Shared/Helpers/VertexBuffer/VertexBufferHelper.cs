using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;
using Syroot.Maths;
using BfresLibrary.Core;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace BfresLibrary.Helpers
{
    /// <summary>
    /// Represents a helper class for working with <see cref="VertexBuffer"/> instances.
    /// </summary>
    public class VertexBufferHelper
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferHelper"/> class.
        /// </summary>
        public VertexBufferHelper()
        {
            ByteOrder = ByteOrderHelper.SystemByteOrder;
            Attributes = new List<VertexBufferHelperAttrib>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBufferHelper"/> class with data read from the given
        /// <paramref name="vertexBuffer"/>. The data is available in the <paramref name="byteOrder"/>, which defaults
        /// to system byte order.
        /// </summary>
        /// <param name="vertexBuffer">The <see cref="VertexBuffer"/> to initially read data from.</param>
        /// <param name="byteOrder">The <see cref="ByteOrder"/> in which vertex data is available. <c>null</c> to use
        /// system byte order.</param>
        public VertexBufferHelper(VertexBuffer vertexBuffer, ByteOrder? byteOrder = null)
        {
            ByteOrder = byteOrder ?? ByteOrderHelper.SystemByteOrder;
            VertexSkinCount = vertexBuffer.VertexSkinCount;

            Attributes = new List<VertexBufferHelperAttrib>(vertexBuffer.Attributes.Count);
            foreach (VertexAttrib attrib in vertexBuffer.Attributes.Values)
            {
                Attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = attrib.Name,
                    BufferIndex = attrib.BufferIndex,
                    Format = attrib.Format,
                    Offset = attrib.Offset,
                    Data = FromRawData(vertexBuffer, attrib)
                }); 
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="ByteOrder"/> in which vertex data will be stored when calling
        /// <see cref="ToVertexBuffer()"/>. This should be the same as the remainder of the <see cref="ResFile"/> in
        /// which it will be stored.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        /// <summary>
        /// Gets or sets the number of bones influencing the vertices stored in the buffer. 0 influences equal
        /// rigidbodies (no skinning), 1 equal rigid skinning and 2 or more smooth skinning.
        /// </summary>
        public byte VertexSkinCount { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="VertexBufferHelperAttrib"/> instances which store the data.
        /// </summary>
        public IList<VertexBufferHelperAttrib> Attributes { get; set; }

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="VertexBufferHelperAttrib"/> instance at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="VertexBufferHelperAttrib"/> instance.</param>
        /// <returns>The <see cref="VertexBufferHelperAttrib"/> instance at the given index.</returns>
        public VertexBufferHelperAttrib this[int index]
        {
            get { return Attributes[index]; }
            set { Attributes[index] = value; }
        }

        /// <summary>
        /// Gets or sets the first <see cref="VertexBufferHelperAttrib"/> instance with the given
        /// <paramref name="attribName"/>.
        /// </summary>
        /// <param name="attribName">The name of the <see cref="VertexBufferHelperAttrib"/> instance.</param>
        /// <returns>The <see cref="VertexBufferHelperAttrib"/> instance with the given name.</returns>
        public VertexBufferHelperAttrib this[string attribName]
        {
            get
            {
                foreach (VertexBufferHelperAttrib attrib in Attributes)
                {
                    if (attrib.Name == attribName)
                    {
                        return attrib;
                    }
                }
                throw new ArgumentException($"No attribute with name {attribName} was found.", nameof(attribName));
            }
            set
            {
                int i = 0;
                foreach (VertexBufferHelperAttrib attrib in Attributes)
                {
                    if (attrib.Name == attribName)
                    {
                        Attributes[i] = value;
                        return;
                    }
                    i++;
                }
                throw new ArgumentException($"No attribute with name {attribName} was found.", nameof(attribName));
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Determines if the helper contains the provided attribute name.
        /// </summary>
        /// <returns></returns>
        public bool Contains(string attribName)
        {
            foreach (VertexBufferHelperAttrib attrib in Attributes)
            {
                if (attrib.Name == attribName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a <see cref="VertexBuffer"/> instance out of the stored helper data.
        /// </summary>
        /// <returns>A new <see cref="VertexBuffer"/>.</returns>
        public VertexBuffer ToVertexBuffer()
        {
            VertexBuffer vertexBuffer = new VertexBuffer();
            vertexBuffer.Attributes = new ResDict<VertexAttrib>();
            vertexBuffer.VertexSkinCount = VertexSkinCount;

            // Go through each attribute and store it into its own buffer.
            int lastElementCount = Attributes[0].Data.Length;
            int numBuffers = Attributes.Max(x => x.BufferIndex + 1);
            vertexBuffer.Buffers = new List<Buffer>(numBuffers);
            vertexBuffer.VertexCount = (uint)lastElementCount;

            Buffer[] buffers = new Buffer[numBuffers];
            for (int i = 0; i < numBuffers; i++)
                buffers[i] = new Buffer();

            uint[] buffer_pos = new uint[numBuffers];

            foreach (VertexBufferHelperAttrib helperAttrib in Attributes)
            {
                // Check if the length of data does not match another attribute's data length.
                if (lastElementCount != helperAttrib.Data.Length)
                {
                    throw new InvalidDataException("Attribute data arrays have different sizes.");
                }

                buffers[helperAttrib.BufferIndex].Stride += (ushort)helperAttrib.Stride;

                // Add a VertexAttrib instance from the helper attribute.   
                vertexBuffer.Attributes.Add(helperAttrib.Name, new VertexAttrib()
                {
                    Name = helperAttrib.Name,
                    Format = helperAttrib.Format,
                    BufferIndex = helperAttrib.BufferIndex,
                    Offset = (ushort)buffer_pos[helperAttrib.BufferIndex],
                }); 
                buffer_pos[helperAttrib.BufferIndex] += helperAttrib.Stride;
            }

            // Set buffer data
            for (byte index = 0; index < numBuffers; index++)
            {
                List<VertexBufferHelperAttrib> attributes = Attributes.Where(x => x.BufferIndex == index).ToList();
                if (attributes.Count == 0)
                    continue;

                if (attributes.Count == 1)
                {
                    buffers[index].Data = new byte[1][] { ToRawData(attributes[0]) };
                }
                else
                {
                    // Create the buffer containing all attribute data.
                    buffers[index].Data = new byte[1][] { ToRawData(attributes) };
                }
            }
            vertexBuffer.Buffers = buffers.ToList();

/*
            for (byte index = 0; index < numBuffers; index++)
            {
                List<VertexBufferHelperAttrib> attributes = Attributes.Where(x => x.BufferIndex == index).ToList();
                if (attributes.Count == 0)
                    throw new ArgumentException($"No attributes found for buffer index {index}. Total {numBuffers}");

                attributes = attributes.OrderBy(x => x.Offset).ToList();
                foreach (var att in attributes) {
                    Console.WriteLine($"{att.Name} {att.BufferIndex} {att.Stride} {att.Format} {att.Data.Length}");
                }

                if (attributes.Count > 1 && numBuffers == 1 ||
                    Attributes.Count == 1 && attributes[0].Stride == 12)
                {
                    //Add 4 bytes of alignment for 12 stride if buffers are combined
                    //Typically used for the position
                  /*  foreach (var att in attributes)
                        if (att.Stride == 12)
                            att.Stride = 16;
                }

                ushort stride = (ushort)attributes.Sum(x => x.Stride);
                if (attributes.Count == 1)
                {
                    vertexBuffer.Buffers.Add(new Buffer()
                    {
                        Stride = stride,
                        Data = new byte[1][] { ToRawData(attributes[0]) }
                    });
                }
                else
                {
                    // Create the buffer containing all attribute data.
                    vertexBuffer.Buffers.Add(new Buffer()
                    {
                        Stride = stride,
                        Data = new byte[1][] { ToRawData(attributes) }
                    });
                }

                uint offset = 0;
                foreach (VertexBufferHelperAttrib helperAttrib in attributes)
                {
                    // Add a VertexAttrib instance from the helper attribute.   
                    vertexBuffer.Attributes.Add(helperAttrib.Name, new VertexAttrib()
                    {
                        Name = helperAttrib.Name,
                        Format = helperAttrib.Format,
                        BufferIndex = helperAttrib.BufferIndex,
                        Offset = (ushort)offset,
                    });
                    offset += helperAttrib.Stride;
                }
            }*/

            return vertexBuffer;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private Vector4F[] FromRawData(VertexBuffer vertexBuffer, VertexAttrib attrib)
        {
            // Create a reader on the raw bytes of the correct endianness.
            Buffer buffer = vertexBuffer.Buffers[attrib.BufferIndex];
            using (BinaryDataReader reader = new BinaryDataReader(new MemoryStream(buffer.Data[0])))
            {
                reader.ByteOrder = ByteOrder;

                // Get a conversion callback transforming the raw data into a Vector4F instance.
                Func<BinaryDataReader, Vector4F> callback = reader.GetGX2AttribCallback(attrib.Format);

                // Read the elements.
                Vector4F[] elements = new Vector4F[vertexBuffer.VertexCount];
                for (int i = 0; i < vertexBuffer.VertexCount; i++)
                {
                    reader.Position = attrib.Offset + i * buffer.Stride;
                    elements[i] = callback.Invoke(reader);
                    if (attrib.Name.Contains("_i"))
                    {
                        switch (attrib.Format)
                        {
                            case GX2.GX2AttribFormat.Format_16_SNorm: elements[i].X = elements[i].X * 32767f; break;
                        }
                    }
                }
                return elements;
            }
        }

        private byte[] ToRawData(List<VertexBufferHelperAttrib> helperAttribs)
        {
            int length = 0;
            for (int i = 0; i < helperAttribs.Count; i++) {
                length += helperAttribs[i].Data.Length * (int)helperAttribs[i].Stride;
            }

            // Create a write for the raw bytes of the correct endianness.
            byte[] raw = new byte[length];
            using (BinaryDataWriter writer = new BinaryDataWriter(new MemoryStream(raw, true)))
            {
                writer.ByteOrder = ByteOrder;

                for (int v = 0; v < helperAttribs[0].Data.Length; v++)
                {
                    for (int i = 0; i < helperAttribs.Count; i++)
                    {
                        long pos = writer.Position;

                        // Get a conversion callback transforming the Vector4F instances into raw data.
                        Action<BinaryDataWriter, Vector4F> callback = writer.GetGX2AttribCallback(helperAttribs[i].Format);
                        callback.Invoke(writer, helperAttribs[i].Data[v]);

                        writer.Seek(pos + helperAttribs[i].Stride, SeekOrigin.Begin);
                    }
                }
            }
            return raw;
        }

        private byte[] ToRawData(VertexBufferHelperAttrib helperAttrib)
        {
            // Create a write for the raw bytes of the correct endianness.
            byte[] raw = new byte[helperAttrib.Data.Length * helperAttrib.Stride];
            using (BinaryDataWriter writer = new BinaryDataWriter(new MemoryStream(raw, true)))
            {
                writer.ByteOrder = ByteOrder;

                // Get a conversion callback transforming the Vector4F instances into raw data.
                Action<BinaryDataWriter, Vector4F> callback = writer.GetGX2AttribCallback(helperAttrib.Format);

                // Write the elements.
                foreach (Vector4F element in helperAttrib.Data)
                {
                    long pos = writer.Position;
                    callback.Invoke(writer, element);
                    writer.Seek(pos + helperAttrib.Stride, SeekOrigin.Begin);
                }
            }
            return raw;
        }
    }
}
