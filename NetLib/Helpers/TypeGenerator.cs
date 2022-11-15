using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NetLib.Helpers
{
    public class TypeGenerator : TypeGeneratorGeneric<object>
    {
        public TypeGenerator(Dictionary<string, Type> properties) : base(properties)
        {
        }
    }

    /// <summary>
    /// Generate a new type that uses T as the base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeGeneratorGeneric<T> where T : class
    {
        private readonly Dictionary<string, MethodInfo> _setMethods;

        public TypeGeneratorGeneric(Dictionary<string, Type> properties)
        {
            Properties = properties;

            GeneratedType = TypeHelper.CreateType(properties, out _setMethods, typeof(T));
        }

        public Type GeneratedType { get; }

        public Dictionary<string, Type> Properties { get; }

        /// <summary>
        /// Create a new instance of your generated type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public T CreateInstance(Dictionary<string, object> values = null)
        {
            var instance = (T)Activator.CreateInstance(GeneratedType);

            if (values != null)
                SetValues(instance, values);

            return instance;
        }

        /// <summary>
        /// Update the property values on an instance of your generated type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="values"></param>
        public void SetValues(T instance, Dictionary<string, object> values)
        {
            foreach (var value in values)
                SetValue(instance, value.Key, value.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetValue(T instance, string propertyName, object propertyValue)
        {
            if (!_setMethods.TryGetValue(propertyName, out var setter))
                throw new ArgumentException($"Type does not contain settter for property {propertyName}", nameof(propertyName));
            setter.Invoke(instance, new[] { propertyValue });
        }

        /// <summary>
        /// Create a new list of your new type and populate on initialisation
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public IList CreateList(T[]? values = null)
        {
            var listGenericType = typeof(List<>);
            var list = listGenericType.MakeGenericType(GeneratedType);
            var constructor = list.GetConstructor(new Type[] { });
            var newList = (IList)constructor.Invoke(new object[] { });
            foreach (var value in values)
                newList.Add(value);
            return newList;
        }
    }
}