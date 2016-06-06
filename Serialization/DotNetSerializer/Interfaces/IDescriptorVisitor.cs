using DotNetSerializer.Descriptors;

namespace DotNetSerializer.Interfaces
{
    /// <summary>
    /// This interface is responsible for providing a Visit method implemented by <see cref="IVisitibleDescriptor"/>
    /// <remarks>
    /// <para>Design according to <c>Visitor DP</c>, see also <see cref="IVisitibleDescriptor"/></para>
    /// </remarks>
    /// </summary>
    internal interface IDescriptorVisitor
    {
        /// <summary>
        /// Visits the specified <see cref="NullDescriptor"/> descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        object Visit(NullDescriptor descriptor);
        /// <summary>
        /// Visits the specified <see cref="ObjectDescriptor"/> descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        object Visit(ObjectDescriptor descriptor);
        /// <summary>
        /// Visits the specified <see cref="PrimitiveDescriptor"/> descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        object Visit(PrimitiveDescriptor descriptor);
        /// <summary>
        /// Visits the specified <see cref="CopyReferenceDescriptor"/> descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        object Visit(CopyReferenceDescriptor descriptor);
    }
}
