using DotNetSerializer.Descriptors;
using DotNetSerializer.Streamers;
using System;
using System.IO;

namespace DotNetSerializer
{
    /// <summary>
    /// This class is responsible for Serializing/Deserializing an object in .Net FW
    /// </summary>
    public class ObjectSerializer
    {
        private Predicate<string> MemberFilter { get; set; }

        public ObjectSerializer()
            : this(member => false)
        {
        }

        public ObjectSerializer(Predicate<string> p_memberFilter)
        {
            MemberFilter = p_memberFilter;
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="stream">The stream.</param>
        public void Serialize(object obj, Stream stream)
        {
            var interpertor = new Interpreter();
            //Creates internal object model from an object, which XmlStreamWrite will write into a stream
            BaseDescriptor descriptor = interpertor.Interpret(obj, MemberFilter);

            StreamFactory.CreateWriter().Write(stream, descriptor);
        }

        /// <summary>
        /// De serializes the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The object which deserialize</returns>
        public object Deserialize(Stream stream)
        {
            //Creates internal object model from a stream, which Interpreter will analyze into an object
            BaseDescriptor descriptor = StreamFactory.CreateReader().Read(stream);

            var interperter = new Interpreter();
            var result = interperter.Analyze(descriptor);

            return result;
        }

        /// <summary>
        /// This class is responsible for providing Stream
        /// </summary>
        private abstract class StreamFactory
        {
            /// <summary>
            /// Creates the writer.
            /// </summary>
            /// <returns></returns>
            public static WriteStream CreateWriter()
            {
                return new WriteStream();
            }

            /// <summary>
            /// Creates the reader.
            /// </summary>
            /// <returns></returns>
            public static ReadStream CreateReader()
            {
                return new ReadStream();
            }
        }
    }
}
