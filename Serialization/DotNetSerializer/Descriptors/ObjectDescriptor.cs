using DotNetSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSerializer.Descriptors
{
    /// <summary>
    /// This class is responsible for describing an object 
    /// </summary>
    internal class ObjectDescriptor : BaseDescriptor
    {
        #region Nested

        /// <summary>
        /// This enum is responsible for seperating between fields and properties for later re-construct
        /// </summary>
        private enum Category
        {
            Field,
            Property
        }

        #endregion

        #region Fields

        private Dictionary<Category, Dictionary<string, BaseDescriptor>> _map;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the map of field/properties for this object description, using a backing field <see cref="_map"/>.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        private Dictionary<Category, Dictionary<string, BaseDescriptor>> Map
        {
            get { return _map ?? (_map = new Dictionary<Category, Dictionary<string, BaseDescriptor>>()); }
        }

        /// <summary>
        /// Gets the descriptor type <see cref="BaseDescriptor.Descriptor" />.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        protected override Descriptor Type
        {
            get { return Descriptor.Object; }
        }

        /// <summary>
        /// Gets the properties of the object this class is describing.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IEnumerable<BaseDescriptor> Properties
        {
            get
            {
                return Map.ContainsKey(Category.Property)
                    ? Map[Category.Property].Values
                    : Enumerable.Empty<BaseDescriptor>();
            }
        }

        /// <summary>
        /// Gets the fields of the object this class is describing.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<BaseDescriptor> Fields
        {
            get
            {
                return Map.ContainsKey(Category.Field)
                    ? Map[Category.Field].Values
                    : Enumerable.Empty<BaseDescriptor>();
            }
        }

        /// <summary>
        /// Gets the unique identifier for this ref type.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id
        {
            get;
            private set;
        }

        #endregion

        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor"/> class.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="id">The identifier.</param>
        public ObjectDescriptor(string sourceName, string sourceType, string id)
            : base(sourceName, sourceType)
        {
            Id = id;
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

        #region Public

        /// <summary>
        /// Adds a property description of the represented object.
        /// </summary>
        /// <param name="property">The property.</param>
        public virtual void AddProperty(BaseDescriptor property)
        {
            ValidateCategory(Category.Property);
            ValidateValue(Category.Property, property);

            Map[Category.Property].Add(property.SourceName, property);
        }

        /// <summary>
        /// Adds the field description of the represented object.
        /// </summary>
        /// <param name="field">The field.</param>
        public virtual void AddField(BaseDescriptor field)
        {
            ValidateCategory(Category.Field);
            ValidateValue(Category.Field, field);

            Map[Category.Field].Add(field.SourceName, field);
        }

        #endregion

        #region Private

        /// <summary>
        /// Ensures this <see cref="Category"/> exists in the <see cref="Map"/>
        /// </summary>
        /// <param name="newCategory">The new category.</param>
        private void ValidateCategory(Category newCategory)
        {
            if (!Map.ContainsKey(newCategory))
            {
                Map.Add(newCategory, new Dictionary<string, BaseDescriptor>());
            }
        }

        /// <summary>
        /// Validates the field/property will not be duplicate in this object description.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <exception cref="System.ArgumentException"></exception>
        private void ValidateValue(Category destination, BaseDescriptor descriptor)
        {
            if (Map[destination].ContainsKey(descriptor.SourceName))
            {
                throw new ArgumentException(string.Format("{0} named {1} already exists", destination, descriptor.SourceName));
            }
        }

        #endregion
    }
}
