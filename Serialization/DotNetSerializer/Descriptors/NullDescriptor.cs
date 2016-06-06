using DotNetSerializer.Interfaces;

namespace DotNetSerializer.Descriptors
{
    /// <summary>
    /// This class is responsible for describing a null pointer
    /// <remarks>
    /// <para>Design according to <c>Null DP</c></para>
    /// </remarks>
    /// </summary>
    internal class NullDescriptor : BaseDescriptor
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
            get { return Descriptor.Null; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NullDescriptor"/> class.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        public NullDescriptor(string sourceName)
            : base(sourceName, string.Empty)
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
