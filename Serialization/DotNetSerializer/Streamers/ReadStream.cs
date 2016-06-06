using DotNetSerializer.Descriptors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotNetSerializer.Streamers
{
    /// <summary>
    /// This class is responsible for reading a stream and translating it into a <see cref="BaseDescriptor"/>
    /// </summary>
    internal class ReadStream
    {
        #region Nested

        /// <summary>
        /// This class is responsible for building the <see cref="BaseDescriptor"/> using a chainging technics
        /// <remarks>
        /// <para>Design according to <c>Chain of Responsibility DP</c></para>
        /// </remarks>
        /// </summary>
        private abstract class DescriptorBuilderChain
        {
            #region Properties

            /// <summary>
            /// Gets or sets the next <see cref="DescriptorBuilderChain"/>.
            /// </summary>
            /// <value>
            /// The next.
            /// </value>
            public DescriptorBuilderChain Next { get; set; }

            /// <summary>
            /// Gets the handling <see cref="BaseDescriptor.Descriptor"/> this instance can handle.
            /// </summary>
            /// <value>
            /// The handling descriptor.
            /// </value>
            protected abstract BaseDescriptor.Descriptor HandlingDescriptor { get; }

            #endregion

            #region Public

            /// <summary>
            /// Handles the specified element.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="chain">The source chain which we relay to build our object model</param>
            /// <returns></returns>
            public BaseDescriptor Handle(XElement element, DescriptorBuilderChain chain)
            {
                BaseDescriptor result = null;
                if (CanHandle(element))
                {
                    result = HandleInternal(element, chain);
                }
                else if (Next != null)
                {
                    result = Next.Handle(element, chain);
                }

                return result;
            }

            #endregion

            #region Protected

            /// <summary>
            /// each derived class will handle the <see cref="XElement"/> according to it's logic.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="chain">The chain.</param>
            /// <returns></returns>
            protected abstract BaseDescriptor HandleInternal(XElement element, DescriptorBuilderChain chain);

            /// <summary>
            /// Gets the descriptors (of type <see cref="BaseDescriptor"/>) from the <param name="startElement"></param>.
            /// </summary>
            /// <param name="startElement">The start element.</param>
            /// <param name="collectionName">Name of the collection.</param>
            /// <param name="chain">The chain.</param>
            /// <returns></returns>
            protected IEnumerable<BaseDescriptor> GetDescriptors(XElement startElement,
                string collectionName,
                DescriptorBuilderChain chain)
            {
                var query = startElement.Element(collectionName).Elements()
                    .Select(element => chain.Handle(element, chain));

                return query;
            }

            #endregion

            #region Private

            /// <summary>
            /// Determines whether this instance can handle the specified elemnt.
            /// </summary>
            /// <param name="element">The elemnt.</param>
            /// <returns></returns>
            private bool CanHandle(XElement element)
            {
                bool can = HandlingDescriptor.ToString()
                    .Equals(element.Name.LocalName, StringComparison.OrdinalIgnoreCase);
                return can;
            }

            #endregion
        }

        /// <summary> 
        /// This class is responsible for building the <see cref="PrimitiveDescriptor"/>
        /// </summary>
        private class PrimitiveDescriptorBuilderChain : DescriptorBuilderChain
        {
            #region Properties

            /// <summary>
            /// Gets the handling <see cref="BaseDescriptor.Descriptor" /> this instance can handle.
            /// </summary>
            /// <value>
            /// The handling descriptor.
            /// </value>
            protected override BaseDescriptor.Descriptor HandlingDescriptor
            {
                get { return BaseDescriptor.Descriptor.Primitive; }
            }

            #endregion

            #region Protected

            /// <summary>
            /// each derived class will handle the <see cref="XElement" /> according to it's logic.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="chain">The chain.</param>
            /// <returns></returns>
            protected override BaseDescriptor HandleInternal(XElement element, DescriptorBuilderChain chain)
            {
                var nameAttr = element.Attribute("name");
                var typeAttr = element.Attribute("type");
                var value = element.Value;

                return new PrimitiveDescriptor(nameAttr.Value, typeAttr.Value, value);
            }

            #endregion
        }

        /// <summary> 
        /// This class is responsible for building the <see cref="ObjectDescriptor"/>
        /// </summary>
        private class ObjectDescriptorBuilderChain : DescriptorBuilderChain
        {
            #region Properties

            /// <summary>
            /// Gets the handling <see cref="BaseDescriptor.Descriptor" /> this instance can handle.
            /// </summary>
            /// <value>
            /// The handling descriptor.
            /// </value>
            protected override BaseDescriptor.Descriptor HandlingDescriptor
            {
                get { return BaseDescriptor.Descriptor.Object; }
            }

            #endregion

            #region Protected

            /// <summary>
            /// each derived class will handle the <see cref="XElement" /> according to it's logic.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="chain">The chain.</param>
            /// <returns></returns>
            protected override BaseDescriptor HandleInternal(XElement element, DescriptorBuilderChain chain)
            {
                var nameAttr = element.Attribute("name");
                var typeAttr = element.Attribute("type");
                var idAttr = element.Attribute("id");

                var objDescriptor = new ObjectDescriptor(nameAttr.Value, typeAttr.Value, idAttr.Value);

                var fields = GetDescriptors(element, "fields", chain);
                foreach (var fieldDesciptor in fields)
                {
                    objDescriptor.AddField(fieldDesciptor);
                }

                var properties = GetDescriptors(element, "properties", chain);
                foreach (var propDescriptor in properties)
                {
                    objDescriptor.AddProperty(propDescriptor);
                }

                return objDescriptor;
            }

            #endregion
        }

        /// <summary> 
        /// This class is responsible for building the <see cref="NullDescriptor"/>
        /// </summary>
        private class NullDescriptorBuilderChain : DescriptorBuilderChain
        {
            #region Properties

            /// <summary>
            /// Gets the handling <see cref="BaseDescriptor.Descriptor" /> this instance can handle.
            /// </summary>
            /// <value>
            /// The handling descriptor.
            /// </value>
            protected override BaseDescriptor.Descriptor HandlingDescriptor
            {
                get { return BaseDescriptor.Descriptor.Null; }
            }

            #endregion

            #region Protected

            /// <summary>
            /// each derived class will handle the <see cref="XElement" /> according to it's logic.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="chain">The chain.</param>
            /// <returns></returns>
            protected override BaseDescriptor HandleInternal(XElement element, DescriptorBuilderChain chain)
            {
                var nameAttr = element.Attribute("name");
                return new NullDescriptor(nameAttr.Value);
            }

            #endregion
        }
        /// <summary> 
        /// This class is responsible for building the <see cref="CopyReferenceDescriptor"/>
        /// </summary>
        private class CopyReferenceDescriptorBuilderChain : DescriptorBuilderChain
        {
            #region Properties

            /// <summary>
            /// Gets the handling <see cref="BaseDescriptor.Descriptor" /> this instance can handle.
            /// </summary>
            /// <value>
            /// The handling descriptor.
            /// </value>
            protected override BaseDescriptor.Descriptor HandlingDescriptor
            {
                get { return BaseDescriptor.Descriptor.CopyRef; }
            }

            #endregion

            #region Protected

            /// <summary>
            /// each derived class will handle the <see cref="XElement" /> according to it's logic.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="chain">The chain.</param>
            /// <returns></returns>
            protected override BaseDescriptor HandleInternal(XElement element, DescriptorBuilderChain chain)
            {
                var nameAttr = element.Attribute("name");
                var typeAttr = element.Attribute("type");
                var value = element.Value;

                return new CopyReferenceDescriptor(nameAttr.Value, typeAttr.Value, value);
            }

            #endregion
        }

        #endregion

        #region Public

        /// <summary>
        /// Reads the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The requested <see cref="BaseDescriptor"/> of the object</returns>
        public BaseDescriptor Read(Stream stream)
        {
            XElement root = XElement.Load(stream);
            ValidateStreamRoot(root);
            
            /*
             * Here we are chaining our building sequence
             */
            DescriptorBuilderChain chainBuilder = new PrimitiveDescriptorBuilderChain //starting with primitive descriptor
            {
                Next = new ObjectDescriptorBuilderChain //if no success, then object descriptor
                {
                    Next = new NullDescriptorBuilderChain //if no success, then null descriptor
                    {
                        Next = new CopyReferenceDescriptorBuilderChain //if no success, then a similar reference
                        {
                            Next = null //if no success, then we stop
                        }
                    }
                }
            };

            BaseDescriptor descriptor = chainBuilder.Handle(root.Elements().First(), chainBuilder);
            return descriptor;
        }

        #endregion

        #region Private

        /// <summary>
        /// Validates the stream root.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <exception cref="System.ArgumentException">Not correct serialization format</exception>
        private void ValidateStreamRoot(XElement root)
        {
            if (root.Name.LocalName.Equals("serializeInfo", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            throw new ArgumentException("Not correct serialization format");
        }

        #endregion
    }
}
