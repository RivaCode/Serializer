using DotNetSerializer.Interfaces;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DotNetSerializer.Descriptors
{
    /// <summary>
    /// This class is responsible for describing an .Net object (Ref and Value types) in an abstract manner
    /// <remarks>
    /// <para>Design according to <c>Composition DP</c></para>
    /// </remarks>
    /// </summary>
    [DebuggerTypeProxy(typeof(DescriptorDebugView))]
    [DebuggerDisplay("Descriptor={Description}; Name={SourceName}; Type={SourceType}")]
    internal abstract class BaseDescriptor : IVisitibleDescriptor
    {
        /// <summary>
        /// This enum is responsible for describing the different types of <see cref="BaseDescriptor"/>
        /// </summary>
        internal enum Descriptor
        {
            Object,
            Primitive,
            CopyRef,
            Null,
            Collection
        }

        #region Properties

        /// <summary>
        /// Gets the name of the source object.
        /// <remarks>
        /// may be an property or field
        /// </remarks>
        /// </summary>
        /// <value>
        /// The name of the source.
        /// </value>
        public string SourceName { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the source as a <see cref="string"/>.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        public string SourceType { get; private set; }

        /// <summary>
        /// Gets the description of this <see cref="BaseDescriptor"/>.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get { return Type.ToString(); } }


        /// <summary>
        /// Gets the descriptor type <see cref="Descriptor"/>.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        protected abstract Descriptor Type { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDescriptor"/> class.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="sourceType">Type of the source.</param>
        protected BaseDescriptor(string sourceName, string sourceType)
        {
            SourceName = sourceName;
            SourceType = sourceType;
        }

        #endregion

        #region IVisitbleDescriptor members
        
        /// <summary>
        /// Accepts the visit of a <see cref="IDescriptorVisitor" />
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns></returns>
        public abstract object AcceptVisit(IDescriptorVisitor visitor);

        #endregion

        private class DescriptorDebugView : IDescriptorVisitor
        {
            private BaseDescriptor DescriptorSource { get; set; }

            public XElement DebugDisplay
            {
                get { return new XElement("DebugView", DescriptorSource.AcceptVisit(this)); }
            }

            public DescriptorDebugView(BaseDescriptor descriptorSource)
            {
                DescriptorSource = descriptorSource;
            }


            #region IDescriptorVisitor members

            /// <summary>
            /// Visits the specified <see cref="PrimitiveDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object Visit(PrimitiveDescriptor descriptor)
            {
                XElement primitiveElement = new XElement(descriptor.Description,
                    new XAttribute("name", descriptor.SourceName),
                    new XAttribute("type", descriptor.SourceType),
                    descriptor.Value);

                return primitiveElement;
            }

            /// <summary>
            /// Visits the specified <see cref="CopyReferenceDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object Visit(CopyReferenceDescriptor descriptor)
            {
                XElement copyRefElement = new XElement(descriptor.Description,
                     new XAttribute("name", descriptor.SourceName),
                     new XAttribute("type", descriptor.SourceType),
                     descriptor.Value);

                return copyRefElement;
            }

            /// <summary>
            /// Visits the specified <see cref="ObjectDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object Visit(ObjectDescriptor descriptor)
            {
                XElement objectElement = new XElement(descriptor.Description,
                    new XAttribute("name", descriptor.SourceName),
                    new XAttribute("type", descriptor.SourceType),
                    new XAttribute("id", descriptor.Id),
                    new XElement("fields",
                        from fieldDescriptor in descriptor.Fields
                        select fieldDescriptor.AcceptVisit(this)),
                    new XElement("properties",
                        from propertyDescriptor in descriptor.Properties
                        select propertyDescriptor.AcceptVisit(this)));

                return objectElement;
            }

            public object Visit(NullDescriptor descriptor)
            {
                XElement nullElement = new XElement(descriptor.Description,
                    new XAttribute("name", descriptor.SourceName));

                return nullElement;
            }

            #endregion
        }
    }

}
