using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
namespace Extender {
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
            BaseType = baseType;
            TypeName = className.Replace(" ", "_");
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

        /// <summary>
        /// Sets the TypeBuilder instance to null to allow a new type creation
        /// </summary>
        public void Refresh() {
            _typeBuilder = null;
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
        /// Adds a property to the class being extended or created
        /// </summary>
        /// <typeparam name="T">Return type of the property</typeparam>
        /// <param name="propertyName">Name of the property to be added</param>
        /// <param name="isReadOnly">Indicates if the property is ReadOnly</param>
        public void AddProperty<T>(string propertyName, bool isReadOnly = false) {
            AddProperty(propertyName, typeof(T), isReadOnly);
        }
		
		/// <summary>
        /// Adds a property to the class being extended or created
        /// </summary>
        /// <typeparam name="T">Return type of the property</typeparam>
        /// <param name="propertyName">Name of the property to be added</param>
        /// <param name="propertyType">Return type of the property</param>
        /// <param name="attributesWithValues">Tuple with types of attributes and their parameter values</param>
        /// <param name="isReadOnly">Indicates if the property is ReadOnly</param>
        public void AddProperty<T>(string propertyName, IEnumerable<Tuple<Type, object[]>> attributesWithValues, bool isReadOnly = false){
            if (string.IsNullOrWhiteSpace(propertyName)){
                throw new ArgumentException("propertyName can not be null or empty");
            }

            addProperty(propertyName, typeof(T), attributesWithValues, isReadOnly);
        }

        /// <summary>
        /// Adds a property with a custom attribute to the class being extended or created
        /// </summary>
        /// <typeparam name="Tproperty">Return type of the property</typeparam>
        /// <typeparam name="Tattr">Type of the custom Attribute</typeparam>
        /// <param name="propertyName">Name of the property to be added</param>
        /// <param name="attributeValues">the parameters of the attribute</param>
        /// <param name="isReadOnly">Indicates if the property is ReadOnly</param>
        public void AddProperty<Tproperty, Tattr>(string propertyName, object[] attributeValues, bool isReadOnly) {
            AddProperty(propertyName, typeof(Tproperty), typeof(Tattr), attributeValues, isReadOnly);
        }

        /// <summary>
        /// Adds a property to the class being extended or created
        /// </summary>
        /// <param name="propertyName">Name of the property to be added</param>
        /// <param name="propertyType">Return type of the property</param>
        /// <param name="isReadOnly">Indicates if the property is ReadOnly</param>
        public void AddProperty(string propertyName, Type propertyType, bool isReadOnly = false) {
            if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new ArgumentException("propertyName can not be null or empty");
            }

            addProperty(propertyName, propertyType, null, null, isReadOnly);
        }

        /// <summary>
        /// Adds a property to the class being extended or created
        /// </summary>
        /// <param name="propertyName">Name of the property to be added</param>
        /// <param name="propertyType">Return type of the property</param>
        /// <param name="attributeType">Type of attribute you want to add to this property</param>
        /// <param name="attributeValues">The parameters of the attribute</param>
        /// <param name="isReadOnly">Indicates if the property is ReadOnly</param>
        public void AddProperty(string propertyName, Type propertyType, Type attributeType, object[] attributeValues, bool isReadOnly = false) {
            if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new ArgumentException("propertyName can not be null or empty");
            }

            addProperty(propertyName, propertyType, attributeType, attributeValues, isReadOnly);
        }

        /// <summary>
        /// Adds a collection of properties with the same type to the class being extended or created
        /// </summary>
        /// <typeparam name="T">The return type of all the properties in the collection</typeparam>
        /// <param name="propertyNames">A collection that holds the names of the properties to be added</param>
        public void AddProperty<T>(IEnumerable<string> propertyNames) {
            AddProperty(propertyNames, typeof(T));
        }
		
		/// <summary>
        /// Adds a property to the class being extended or created
        /// </summary>
        /// <param name="propertyName">Name of the property to be added</param>
        /// <param name="propertyType">Return type of the property</param>
        /// <param name="attributesWithValues">Tuple with types of attributes and their parameter values</param>
        /// <param name="isReadOnly">Indicates if the property is ReadOnly</param>
        public void AddProperty(string propertyName, Type propertyType,
            IEnumerable<Tuple<Type, object[]>> attributesWithValues, bool isReadOnly = false){
            if (string.IsNullOrWhiteSpace(propertyName)){
                throw new ArgumentException("propertyName can not be null or empty");
            }

            addProperty(propertyName, propertyType, attributesWithValues, isReadOnly);
        }

        /// <summary>
        /// Adds a collection of properties with the same type to the class being extended or created
        /// </summary>
        /// <param name="propertyNames">A collection that holds the names of the properties to be added</param>
        /// <param name="propertyType">The return type of all the properties in the collection</param>
        public void AddProperty(IEnumerable<string> propertyNames, Type propertyType) {
            if (propertyNames == null || propertyNames.Count() < 1) {
                throw new ArgumentException("Properties can not be null or empty");
            }

            addProperties(propertyNames, propertyType);
        }

        /// <summary>
        /// Adds a collection properties with separate data type to the class being extended or created
        /// </summary>
        /// <param name="properties">A collection that holds the names of the properties to be added</param>
        /// <param name="types">A collection that holds the types of the properties to be added</param>
        /// <param name="allReadOnly">A value that indicates if all the properties are readonly properties</param>
        public void AddProperty(IEnumerable<string> properties, IEnumerable<Type> types,bool allReadOnly) {
            if (properties == null || types == null || !properties.Any() || !types.Any()) {
                throw new ArgumentException("Properties or types can not be null or empty");
            }

            if (properties.Count() != types.Count()) {
                throw new ArgumentException("Properties count must equal types count to avoid type mis-match");
            }

            for (int index = 0; index < properties.Count(); index++) {
                addProperty(properties.ElementAt(index).Replace(" ","_"), types.ElementAt(index), null, null, allReadOnly);
            }
        }
        
        /// <summary>
        /// Adds a field to the class being extended or created
        /// </summary>
        /// <param name="fieldName">Name of the field to be added</param>
        /// <param name="fieldType">Return type of the field</param>
        public void AddField(string fieldName, Type fieldType) {
            if (string.IsNullOrWhiteSpace(fieldName)) {
                throw new ArgumentException("fieldName can not be null or empty");
            }

            addField(fieldName, fieldType, null, null);
        }   
                
        /// <summary>
        /// Adds a field to the class being extended or created
        /// </summary>
        /// <typeparam name="T">Return type of the field</typeparam>
        /// <param name="fieldName">Name of the field to be added</param>
        public void AddField<T>(string fieldName) {
            if (string.IsNullOrWhiteSpace(fieldName)) {
                throw new ArgumentException("fieldName can not be null or empty");
            }

            addField(fieldName, typeof(T), null, null);
        }   
        
        /// <summary>
        /// Adds a field to the class being extended or created
        /// </summary>
        /// <param name="fieldName">Name of the field to be added</param>
        /// <param name="fieldType">Return type of the field</param>
        /// <param name="attributeType">Type of attribute you want to add to this field</param>
        /// <param name="attributeValues">The parameters of the attribute</param>
        public void AddField(string fieldName, Type fieldType, Type attributeType, object[] attributeValues) {
            if (string.IsNullOrWhiteSpace(fieldName)) {
                throw new ArgumentException("fieldName can not be null or empty");
            }

            addField(fieldName, fieldType, attributeType, attributeValues);
        }    
        
        /// <summary>
        /// Adds a field to the class being extended or created
        /// </summary>
        /// <typeparam name="Tfield">Return type of the field</typeparam>
        /// <typeparam name="Tattr">Type of the custom Attribute</typeparam>
        /// <param name="fieldName">Name of the field to be added</param>
        /// <param name="attributeValues">The parameters of the attribute</param>
        public void AddField<Tfield, Tattr>(string fieldName, object[] attributeValues) {
            if (string.IsNullOrWhiteSpace(fieldName)) {
                throw new ArgumentException("fieldName can not be null or empty");
            }

            addField(fieldName, typeof(Tfield), typeof(Tattr), attributeValues);
        }  
        
        /// <summary>
        /// Adds a field to the class being extended or created
        /// </summary>
        /// <param name="fieldName">Name of the field to be added</param>
        /// <param name="fieldType">Return type of the field</param>
        /// <param name="attributeTypesAndParameters">Type of attribute you want to add to this field as key. The parameters of the attribute as values</param>
        public void AddField(string fieldName, Type fieldType, Dictionary<Type, List<object>> attributeTypesAndParameters) {
            if (string.IsNullOrWhiteSpace(fieldName)) {
                throw new ArgumentException("fieldName can not be null or empty");
            }

            addField(fieldName, fieldType, attributeTypesAndParameters);
        }

        /// <summary>
        /// Gets the name of the derived class
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the base class that the derived class extends
        /// </summary>
        public Type BaseType { get; }

        private void addProperties(IEnumerable<string> properties, Type propertyType) {
            initializeTypeConstruction();
            foreach (var prop in properties) {
                var field = _typeBuilder.DefineField($"_{prop}", propertyType, FieldAttributes.Private);
                var property = _typeBuilder.DefineProperty(prop, PropertyAttributes.HasDefault, propertyType, null);
                generateGetter(prop, field, property, propertyType);
                generateSetter(prop, field, property, propertyType);
            }
        }

        private void addField(string fieldName, Type type, Type attributeType, object[] attributeValues) {
            initializeTypeConstruction();
            var field = _typeBuilder.DefineField(fieldName, type, FieldAttributes.Public);
            AddAttributeToField(attributeType, attributeValues, field);
        }
        
        private void addField(string fieldName, Type type, Dictionary<Type, List<object>> attributeTypesAndParameters) {
            initializeTypeConstruction();
            var field = _typeBuilder.DefineField(fieldName, type, FieldAttributes.Public);
            
            if (attributeTypesAndParameters == null) {
                return;
            }

            foreach (var typeParametersPair in attributeTypesAndParameters) {
                AddAttributeToField(typeParametersPair.Key, typeParametersPair.Value.ToArray(), field);
            }
        }

        private static void AddAttributeToField(Type attributeType, object[] attributeValues, FieldBuilder field) {
            if (attributeType != null)
            {
                Type[] ctorArgsTypes;
                if (attributeValues != null) {
                    ctorArgsTypes = new Type[attributeValues.Length];

                    for (int index = 0; index < attributeValues.Length; index++) {
                        ctorArgsTypes[index] = attributeValues[index].GetType();
                    }
                }
                else {
                    attributeValues = new object[] { };
                    ctorArgsTypes = new Type[] { };
                }

                var attrCtorInfo = attributeType.GetConstructor(ctorArgsTypes);
                var attrBuilder =
                    new CustomAttributeBuilder(attrCtorInfo, attributeValues);
                field.SetCustomAttribute(attrBuilder);
            }
        }

        private void addProperty(string propertyName, Type type, Type attributeType, object[] attributeValues, bool isReadOnly){
            addProperty(propertyName, type, new[] {new Tuple<Type, object[]>(attributeType, attributeValues)}, isReadOnly);
        }

        private void addProperty(string propertyName, Type type, IEnumerable<Tuple<Type, object[]>> attributeTypesWithValues, bool isReadOnly){
            initializeTypeConstruction();
            var field = _typeBuilder.DefineField($"_{propertyName}", type, FieldAttributes.Private);
            var propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, type, null);
            foreach (var attributeTypesWithValue in attributeTypesWithValues){
                addAttributeToProperty(attributeTypesWithValue.Item1, attributeTypesWithValue.Item2, propertyBuilder);
            }

            generateGetter(propertyName, field, propertyBuilder, type);

            if (!isReadOnly){
                generateSetter(propertyName, field, propertyBuilder, type);
            }
        }

        private void addAttributeToProperty(Type attributeType, object[] attributeValues, PropertyBuilder propertyBuilder){
            if (attributeType != null){
                Type[] ctorArgsTypes;
                if (attributeValues != null){
                    ctorArgsTypes = new Type[attributeValues.Length];

                    for (var index = 0; index < attributeValues.Length; index++){
                        ctorArgsTypes[index] = attributeValues[index].GetType();
                    }
                }
                else{
                    attributeValues = new object[] { };
                    ctorArgsTypes = new Type[] { };
                }

                var attrCtorInfo = attributeType.GetConstructor(ctorArgsTypes);
                var attrBuilder = new CustomAttributeBuilder(attrCtorInfo, attributeValues);
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }
        }

        private void generateSetter(string propertyName, FieldBuilder field, PropertyBuilder propertyBuilder, Type paramType) {
            var setMethodBuilder = _typeBuilder.DefineMethod($"set_{propertyName}", _getSetAttr, typeof(void), new Type[] { paramType });
            var setMethodGenerator = setMethodBuilder.GetILGenerator();
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldarg_1);
            setMethodGenerator.Emit(OpCodes.Stfld, field);
            setMethodGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        private void generateGetter(string propertyName, FieldBuilder field, PropertyBuilder propertyBuilder, Type returnType) {
            var getMethodBuilder = _typeBuilder.DefineMethod($"get_{propertyName}", _getSetAttr, returnType, null);
            var getMethodGenerator = getMethodBuilder.GetILGenerator();
            getMethodGenerator.Emit(OpCodes.Ldarg_0);
            getMethodGenerator.Emit(OpCodes.Ldfld, field);
            getMethodGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        private void initializeTypeConstruction() {
            if (BaseType.Attributes.HasFlag(TypeAttributes.Sealed) || !BaseType.Attributes.HasFlag(TypeAttributes.Public) && !BaseType.Attributes.HasFlag(TypeAttributes.NestedPublic)) {
                throw new ArgumentException($"{BaseType} is either sealed or not public");
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
            _typeBuilder = _moduleBuilder.DefineType(TypeName, TypeAttributes.Public, BaseType);
        }

        private TypeBuilder _typeBuilder;
        private AssemblyName _assemblyName;
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private MethodAttributes _getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
    }
}
