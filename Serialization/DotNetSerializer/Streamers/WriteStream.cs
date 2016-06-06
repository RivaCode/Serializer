using DotNetSerializer.Descriptors;
using DotNetSerializer.Interfaces;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotNetSerializer.Streamers
{
    /// <summary>
    /// This class is responsible for writing a <see cref="BaseDescriptor"/> into a <see cref="Stream"/>
    /// </summary>
    internal class WriteStream : IDescriptorVisitor
    {
        #region Public

        /// <summary>
        /// Writes the specified <param name="dataDescriptor"/> into <param name="stream"></param>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="dataDescriptor">The data descriptor.</param>
        public void Write(Stream stream, BaseDescriptor dataDescriptor)
        {
            XElement root = new XElement("serializeInfo", dataDescriptor.AcceptVisit(this));
            root.Save(stream);
        } 

        #endregion

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
