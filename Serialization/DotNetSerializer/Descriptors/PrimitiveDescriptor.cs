using DotNetSerializer.Interfaces;

namespace DotNetSerializer.Descriptors
{
    /// <summary>
    /// This class is responsible for describing an .Net primitive type (string and other primitive types)
    /// </summary>
    internal class PrimitiveDescriptor : BaseDescriptor
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
            get { return Descriptor.Primitive; }
        }

        /// <summary>
        /// Gets the value for this primitive type.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveDescriptor"/> class.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="value">The value.</param>
        public PrimitiveDescriptor(string sourceName, string sourceType, string value)
            : base(sourceName, sourceType)
        {
            Value = value;
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
