using DotNetSerializer.Descriptors;
using DotNetSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DotNetSerializer
{
    /// <summary>
    /// This class is responsible for Interpret/Analyze a object
    /// </summary>
    internal class Interpreter
    {
        #region Nested

        /// <summary>
        /// This class is responsible for parsing a <see cref="object"/> into a <see cref="BaseDescriptor"/>
        /// </summary>
        private class Parser
        {

            #region Constants

            /// <summary>
            /// The token used for sepereting the reference type id
            /// </summary>
            private const string ID_CODE_TOKEN = "&";

            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the member filter.
            /// </summary>
            /// <value>
            /// The member filter.
            /// </value>
            private Predicate<string> MemberFilter { get; set; }
            /// <summary>
            /// Helper ID generator
            /// </summary>
            /// <value>
            /// The identifier gen.
            /// </value>
            private ObjectIDGenerator IdGen { get; set; }
            /// <summary>
            /// Keep track of the reference we incountered
            /// </summary>
            /// <value>
            /// The hash code tracker.
            /// </value>
            private List<string> IdTracker { get; set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="Parser"/> class.
            /// </summary>
            public Parser(Predicate<string> memberFilter)
            {
                MemberFilter = memberFilter;
                IdGen=new ObjectIDGenerator();
                IdTracker = new List<string>();
            }

            #endregion

            #region Public

            /// <summary>
            /// Parses the descriptor recursively.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <param name="sourceName">Name of the source.</param>
            /// <param name="objType">Type of the object.</param>
            /// <param name="memberFilter"></param>
            /// <returns></returns>
            /// <exception cref="System.Runtime.Serialization.SerializationException">Cannot serialize non public type</exception>
            public BaseDescriptor ParseDescriptor(object instance, string sourceName, Type objType)
            {
                if (instance == null)
                {
                    return new NullDescriptor(sourceName);
                }

                bool dummyRequiredOutputValue;
                long objId = IdGen.GetId(instance, out dummyRequiredOutputValue); 

                int hashCode = instance.GetHashCode();
                string typeRefId = objId
                                      + ID_CODE_TOKEN
                                      + objType.Name
                                      + ID_CODE_TOKEN
                                      + hashCode;

                if (IdTracker.Contains(typeRefId))
                {
                    return new CopyReferenceDescriptor(sourceName, objType.AssemblyQualifiedName, typeRefId);
                }

                if (objType.IsPrimitive || Type.GetTypeCode(objType) == TypeCode.String)
                {
                    return new PrimitiveDescriptor(sourceName, objType.AssemblyQualifiedName, instance.ToString());
                }

                if (!objType.IsVisible)
                {
                    throw new SerializationException("Cannot serialize non public type");
                }

                IdTracker.Add(typeRefId);

                //Todo - add a collection support
                //if (objType.IsGenericType || objType.IsGenericTypeDefinition)
                //{
                //    Type collectionType = typeof(ICollection<>).MakeGenericType(objType.GetGenericArguments());
                //    if (collectionType.IsAssignableFrom(objType))
                //    {
                //        var collectionDescriptor = new CollectionDescriptor(sourceName, objType.AssemblyQualifiedName, typeRefId);

                //        IEnumerable collection = (IEnumerable)instance;
                //        foreach (var item in collection)
                //        {
                //            Type itemType = item.GetType();
                //            BaseDescriptor descriptor = ParseDescriptor(item, itemType.Name, itemType);

                //            collectionDescriptor.Values.Add(descriptor);
                //        }

                //        return collectionDescriptor; 
                //    }
                //}

                var objectDescriptor = new ObjectDescriptor(sourceName, objType.AssemblyQualifiedName, typeRefId);

                var bindingFlags = BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance;

                ReadFields(objectDescriptor, instance, objType, bindingFlags);
                ReadProperties(objectDescriptor, instance, objType, bindingFlags);

                return objectDescriptor;
            }

            #endregion

            #region Private

            /// <summary>
            /// Reads the fields from the <param name="instance"></param>.
            /// </summary>
            /// <param name="objDescriptor">The object descriptor.</param>
            /// <param name="instance">The instance.</param>
            /// <param name="instanceType">Type of the instance.</param>
            /// <param name="readFlags">The read flags.</param>
            private void ReadFields(ObjectDescriptor objDescriptor, object instance, Type instanceType, BindingFlags readFlags)
            {
                var fieldDescriptorQuery = instanceType.GetFields(readFlags)
                    .Where(f => !f.IsAutoBackingField() && ShouldInclude(f.Name))
                    .Select(f =>
                    {
                        var fieldValue = f.GetValue(instance);
                        var fieldDescriptor = ParseDescriptor(fieldValue, f.Name, f.FieldType);

                        return fieldDescriptor;
                    });

                foreach (var descriptor in fieldDescriptorQuery)
                {
                    objDescriptor.AddField(descriptor);
                }
            }

            /// <summary>
            /// Reads the properties from the <param name="instance"></param>.
            /// </summary>
            /// <param name="objDescriptor">The object descriptor.</param>
            /// <param name="instance">The instance.</param>
            /// <param name="instanceType">Type of the instance.</param>
            /// <param name="readFlags">The read flags.</param>
            private void ReadProperties(ObjectDescriptor objDescriptor, object instance, Type instanceType, BindingFlags readFlags)
            {
                var propertyDescriptorQuery = instanceType.GetProperties(readFlags)
                    .Where(p => p.CanWrite && p.IsAutoProperty() && ShouldInclude(p.Name))
                    .Select(p =>
                    {
                        var propertyValue = p.GetValue(instance);
                        var descriptor = ParseDescriptor(propertyValue, p.Name, p.PropertyType);

                        return descriptor;
                    });

                foreach (var descriptor in propertyDescriptorQuery)
                {
                    objDescriptor.AddProperty(descriptor);
                }
            }

            private bool ShouldInclude(string memberName)
            {
                bool shouldFilter = MemberFilter(memberName);
                return !shouldFilter;
            }
            #endregion
        }

        /// <summary>
        /// This class is responsible for building a <see cref="object"/> from a <see cref="BaseDescriptor"/>
        /// </summary>
        private class Builder : IDescriptorVisitor
        {
            #region Properties

            /// <summary>
            /// Keep track of the same object reference (for circular references)
            /// </summary>
            /// <value>
            /// The object tracker.
            /// </value>
            private Dictionary<string, object> ObjectTracker { get; set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
                ObjectTracker = new Dictionary<string, object>();
            }

            #endregion

            #region IDescriptorVisitor Members

            /// <summary>
            /// Visits the specified <see cref="NullDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object Visit(NullDescriptor descriptor)
            {
                return null;
            }

            /// <summary>
            /// Visits the specified <see cref="ObjectDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object Visit(ObjectDescriptor descriptor)
            {
                Type creationType = Type.GetType(descriptor.SourceType);
                ValidateType(creationType);

                object instance = Activator.CreateInstance(creationType);
                ObjectTracker.Add(descriptor.Id, instance);

                Type instanceType = instance.GetType();
                var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                WriteFields(descriptor, instance, instanceType, bindingFlags);
                WriteProperties(descriptor, instance, instanceType, bindingFlags);

                return instance;
            }

            /// <summary>
            /// Visits the specified <see cref="PrimitiveDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object Visit(PrimitiveDescriptor descriptor)
            {
                Type creationType = Type.GetType(descriptor.SourceType);
                ValidateType(creationType);

                object value = Convert.ChangeType(descriptor.Value, creationType);
                return value;
            }

            /// <summary>
            /// Visits the specified <see cref="CopyReferenceDescriptor" /> descriptor.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            /// <exception cref="System.FormatException"></exception>
            public object Visit(CopyReferenceDescriptor descriptor)
            {
                if (!ObjectTracker.ContainsKey(descriptor.Value))
                {
                    string errMsg = string.Format("Reference for member <{0}> was not found", descriptor.SourceName);
                    throw new FormatException(errMsg);
                }

                object backRef = ObjectTracker[descriptor.Value];
                return backRef;
            }

            #endregion

            #region Public

            /// <summary>
            /// Builds the object from <see cref="BaseDescriptor"/>.
            /// </summary>
            /// <param name="descriptor">The descriptor.</param>
            /// <returns></returns>
            public object BuildObject(BaseDescriptor descriptor)
            {
                object instance = descriptor.AcceptVisit(this);
                return instance;
            }

            #endregion

            #region Private

            /// <summary>
            /// Writes the fields from the <see cref="BaseDescriptor"/> into the <param name="instance"></param>.
            /// </summary>
            /// <param name="objDescriptor">The object descriptor.</param>
            /// <param name="instance">The instance.</param>
            /// <param name="instanceType">Type of the instance.</param>
            /// <param name="writeFlags">The write flags.</param>
            private void WriteFields(ObjectDescriptor objDescriptor, object instance,
                Type instanceType, BindingFlags writeFlags)
            {
                foreach (var fieldDescriptor in objDescriptor.Fields)
                {
                    FieldInfo fieldInfo = instanceType.GetField(fieldDescriptor.SourceName, writeFlags);
                    ValidateInfo(fieldInfo, fieldDescriptor.SourceName, instanceType.Name);

                    object fieldValue = fieldDescriptor.AcceptVisit(this);
                    fieldInfo.SetValue(instance, fieldValue);
                }
            }

            /// <summary>
            /// Writes the properties from the <see cref="BaseDescriptor"/> into the <param name="instance"></param>.
            /// </summary>
            /// <param name="objDescriptor">The object descriptor.</param>
            /// <param name="instance">The instance.</param>
            /// <param name="instanceType">Type of the instance.</param>
            /// <param name="writeFlags">The write flags.</param>
            private void WriteProperties(ObjectDescriptor objDescriptor, object instance,
                Type instanceType, BindingFlags writeFlags)
            {
                foreach (var propertyDescriptor in objDescriptor.Properties)
                {
                    PropertyInfo propInfo = instanceType.GetProperty(propertyDescriptor.SourceName, writeFlags);
                    ValidateInfo(propInfo, propertyDescriptor.SourceName, instanceType.Name);

                    object propValue = propertyDescriptor.AcceptVisit(this);
                    propInfo.SetValue(instance, propValue);
                }
            }

            /// <summary>
            /// Validates the <param name="info"/> still exists on the object.
            /// </summary>
            /// <param name="info">The information.</param>
            /// <param name="memberName">Name of the member.</param>
            /// <param name="memberType">Type of the member.</param>
            /// <exception cref="System.MissingMemberException"></exception>
            private void ValidateInfo(MemberInfo info, string memberName, string memberType)
            {
                if (info != null)
                {
                    return;
                }

                var errMsg = string.Format("Member <{0}> is missing on object of type {1}", memberName, memberType);
                throw new MissingMemberException(errMsg);
            }

            /// <summary>
            /// Validates the creation type.
            /// </summary>
            /// <param name="creationType">Type of the creation.</param>
            /// <exception cref="System.ArgumentException">Object type incorrect</exception>
            private void ValidateType(Type creationType)
            {
                if (creationType != null)
                {
                    return;
                }
                throw new ArgumentException("Object type incorrect");
            }

            #endregion
        }

        #endregion

        #region Public

        /// <summary>
        /// Interprets the specified object into <see cref="BaseDescriptor"/>.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="p_memberFilter"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Cannot serialize null</exception>
        public BaseDescriptor Interpret(object obj, Predicate<string> p_memberFilter)
        {
            if (obj == null)
            {
                throw new ArgumentException("Cannot serialize null");
            }

            var parser = new Parser(p_memberFilter);
            BaseDescriptor result = parser.ParseDescriptor(obj, string.Empty, obj.GetType());

            return result;
        }

        /// <summary>
        /// Analyzes the specified <see cref="BaseDescriptor"/> into an object.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        public object Analyze(BaseDescriptor descriptor)
        {
            var builder = new Builder();
            object result = builder.BuildObject(descriptor);

            return result;
        }

        #endregion
    }

    /// <summary>
    /// This class is responsible for providing the <see cref="Interpreter"/> extension method for the <code>Interpret</code> process
    /// </summary>
    internal static class ReflectionExtension
    {
        /// <summary>
        /// Determines whether [is automatic property].
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        public static bool IsAutoProperty(this PropertyInfo prop)
        {
            return prop.DeclaringType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                     .Any(f => f.Name.Contains("<" + prop.Name + ">"));
        }

        /// <summary>
        /// Determines whether [is automatic backing field].
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static bool IsAutoBackingField(this FieldInfo field)
        {
            return field.Name.EndsWith("BackingField", StringComparison.OrdinalIgnoreCase);
        }
    }
}
