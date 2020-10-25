using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
namespace TypeExtender {
    /// <summary>
    /// A class the creates/extends other unsealed classes/types at runtime
    /// </summary>
    public class TypeExtender {
        /// <summary>
        /// Initializes a type extender object with the name of the derive class
        /// that will extend System.Object as the base class.
        /// </summary>
        /// <param name="className"></param>
        public TypeExtender(string className) : this(className, typeof(object)) {
        }

        /// <summary>
        /// Initializes a type extender object with the name of the derived class
        /// and the base class the new class should derive from.
        /// </summary>
        /// <param name="className"></param>
        /// <param name="baseType"></param>
        public TypeExtender(string className, Type baseType) {
            _baseType = baseType;
            _className = className.Replace(" ", "_");
            initializeTypeConstruction();
        }

        /// <summary>
        /// Returns the derived class containing all the properties and methods added to it
        /// </summary>
        /// <returns></returns>
        public Type FetchType() {
            if (_typeBuilder is null) {
                throw new Exception("Type has not been created");
            }
            return _typeBuilder.CreateTypeInfo()
                               .AsType();
        }

        private void initializeTypeConstruction() {
            if (!_baseType.Attributes.HasFlag(TypeAttributes.Public) || _baseType.Attributes.HasFlag(TypeAttributes.Sealed)) {
                throw new ArgumentException($"{_baseType} is either sealed or not public");
            }

            if (_typeBuilder != null && _typeBuilder.IsCreated()) {
                throw new ArgumentException($"The type {_typeBuilder.Name} has already been created. You need to refresh the type extender class or create a new instance.");
            }

            if (_typeBuilder != null) {
                return;
            }

            _assemblyName = new AssemblyName("DynamicAssembly");
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName.Name);
            _typeBuilder = _moduleBuilder.DefineType(_className, TypeAttributes.Public, _baseType);
        }

        /// <summary>
        /// Adds an attribute to the derived class
        /// </summary>
        /// <typeparam name="T">Attribute type to add</typeparam>
        public void AddAttribute<T>(object[] attributeCtorParams = null) {
            var type = typeof(T);
            AddAttribute(type, attributeCtorParams);
        }

        /// <summary>
        /// Adds an attribute to the derived class
        /// </summary>
        /// <param name="type">Attribute type to add</param>
        /// <param name="attributeCtorParams">constructor args for the attribute</param>
        public void AddAttribute(Type type, object[] attributeCtorParams = null) {
            Type[] ctorArgsTypes;
            if (attributeCtorParams != null) {
                ctorArgsTypes = new Type[attributeCtorParams.Length];

                for (int index = 0; index < attributeCtorParams.Length; index++) {
                    ctorArgsTypes[index] = attributeCtorParams[index].GetType();
                }
            }
            else {
                attributeCtorParams = new object[] { };
                ctorArgsTypes = new Type[] { };
            }

            var ctorInfo = type.GetConstructor(ctorArgsTypes);
            var attributeBuilder = new CustomAttributeBuilder(ctorInfo, attributeCtorParams);
            _typeBuilder.SetCustomAttribute(attributeBuilder);
        }

        /// <summary>
        /// Gets the name of the derived class
        /// </summary>
        public string TypeName { get => _className; }

        /// <summary>
        /// Gets the base class that the derived class extends
        /// </summary>
        public Type BaseType { get => _baseType; }

        private string _className;
        private Type _baseType;
        private TypeBuilder _typeBuilder;
        private AssemblyName _assemblyName;
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
    }
}
