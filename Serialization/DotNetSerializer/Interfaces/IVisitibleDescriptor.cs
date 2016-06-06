namespace DotNetSerializer.Interfaces
{
    /// <summary>
    /// This interface is responsible for providing an access method for <see cref="IDescriptorVisitor"/>
    /// <remarks>
    /// <para>Design according to <c>Visitor DP</c>, see also <see cref="IDescriptorVisitor"/></para>
    /// </remarks>
    /// </summary>
    internal interface IVisitibleDescriptor
    {
        /// <summary>
        /// Accepts the visit of a <see cref="IDescriptorVisitor"/>
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns></returns>
        object AcceptVisit(IDescriptorVisitor visitor);
    }
}
