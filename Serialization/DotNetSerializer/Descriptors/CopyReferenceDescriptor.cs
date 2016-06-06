using DotNetSerializer.Interfaces;

namespace DotNetSerializer.Descriptors
{
    /// <summary>
    /// This class is responsible for describing a similar reference pointer in the heap
    /// </summary>
    internal class CopyReferenceDescriptor : PrimitiveDescriptor
    {
        #region Properties

        /// <summary>
        /// Gets the descriptor type <see cref="BaseDescriptor.Descriptor" />.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        protected override Descriptor Type
        {
            get { return Descriptor.CopyRef; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyReferenceDescriptor"/> class.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="refId">The reference identifier.</param>
        public CopyReferenceDescriptor(string sourceName, string sourceType, string refId)
            : base(sourceName, sourceType, refId)
        {
        }

        #endregion

        #region IVisitbleDescriptor members

        /// <summary>
        /// Accepts the visit of a <see cref="IDescriptorVisitor" />
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns></returns>
        public override object AcceptVisit(IDescriptorVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }
}
