using ExpressWalker.Core.Visitors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressWalker.Core.Helpers
{
    internal class ReflectionCache
    {
        private static Dictionary<string, TypeInfo> TypeInfo = new Dictionary<string, TypeInfo>();

        private static Dictionary<string, TypeProperties> TypeProperties = new Dictionary<string, TypeProperties>();

        private static Dictionary<int, TypeMethods> TypeMethods = new Dictionary<int, TypeMethods>();

        public static TypeInfo GetInfo(Type type)
        {
            TypeInfo info = null;

            if (!TypeInfo.TryGetValue(type.FullName, out info))
            {
                info = new TypeInfo(type);

                TypeInfo.Add(type.FullName, info);
            }

            return info;
        }

        public static TypeProperties GetProperties(Type type)
        {
            TypeProperties properties = null;

            if (!TypeProperties.TryGetValue(type.FullName, out properties))
            {
                properties = new TypeProperties(type);

                TypeProperties.Add(type.FullName, properties);
            }

            return properties;
        }

        public static MethodData GetMethod(Type type, string methodName)
        {
            TypeMethods methods = null;

            var typeHash = type.GetHashCode();

            if (!TypeMethods.TryGetValue(typeHash, out methods))
            {
                methods = new TypeMethods(type);

                TypeMethods.Add(typeHash, methods);
            }

            return methods.GetMethod(methodName);
        }
    }

    internal class TypeInfo
    {
        public Type RawType { get; private set; }

        public string Name { get; private set; }

        public bool IsSimpleType { get; private set; }

        public bool IsDictionary { get; private set; }

        public bool IsGenericEnumerable { get; private set; }

        public bool ImplementsGenericIEnumerable { get; private set; }

        public Type CollectionItemsType { get; private set; }

        public bool IsCollectionItemsTypeSimple { get; private set; }

        public TypeInfo(Type type)
        {
            RawType = type;

            Name = type.FullName;

            IsSimpleType = Util.IsSimpleType(type);

            IsDictionary = Util.IsDictionary(type);

            IsGenericEnumerable = Util.IsGenericEnumerable(type);

            ImplementsGenericIEnumerable = Util.ImplementsGenericIEnumerable(type);

            CollectionItemsType = Util.GetCollectionItemsType(type);

            if (CollectionItemsType != null)
            {
                IsCollectionItemsTypeSimple = Util.IsSimpleType(CollectionItemsType);
            }
        }
    }

    internal class TypeProperties
    {
        public TypeInfo TypeInfo { get; private set; }

        public Dictionary<string, PropertyData> Properties { get; private set; }

        public TypeProperties(Type type)
        {
            TypeInfo = ReflectionCache.GetInfo(type);

            Properties = new Dictionary<string, PropertyData>();

            foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Properties.Add(propInfo.Name, new PropertyData(propInfo, TypeInfo));
            }
        }
    }

    internal class PropertyData
    {
        private PropertyInfo _rawInfo;

        public string PropertyName { get; private set; }

        public TypeInfo DeclaringType { get; private set; }

        public TypeInfo PropertyType { get; private set; }

        private object _visitorMetadata;
        private bool _isVisitorMetadataSet;
        public object VisitorMetadata
        {
            get
            {
                if (!_isVisitorMetadataSet)
                {
                    var metadataAttribute = _rawInfo.GetCustomAttribute<VisitorMetadataAttribute>(false);
                    _visitorMetadata = (metadataAttribute == null) ? null : metadataAttribute.Metadata;
                    _isVisitorMetadataSet = true;
                }
                return _visitorMetadata;
            }
        }

        private bool _isVisitorHierarchy;
        private bool _isVisitorHierarchySet;
        public bool IsVisitorHierarchy
        {
            get
            {
                if (!_isVisitorHierarchySet)
                {
                    _isVisitorHierarchy = _rawInfo.GetCustomAttribute<VisitorHierarchyAttribute>() != null;
                    _isVisitorHierarchySet = true;
                }
                return _isVisitorHierarchy;
            }
        }

        public PropertyData(PropertyInfo propInfo, TypeInfo typeInfo)
        {
            _rawInfo = propInfo;

            PropertyName = propInfo.Name;

            DeclaringType = typeInfo;

            PropertyType =  ReflectionCache.GetInfo(propInfo.PropertyType);
        }
    }

    internal class TypeMethods
    {
        public Type _type;

        private Dictionary<string, MethodData> _methods;

        public TypeMethods(Type type)
        {
            _type = type;

            _methods = new Dictionary<string, MethodData>();
        }

        public MethodData GetMethod(string methodName)
        {
            MethodData retVal = null;

            if (!_methods.TryGetValue(methodName, out retVal))
            {
                var methodInfo = _type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                retVal = new MethodData(methodInfo);

                _methods.Add(methodName, retVal);
            }

            return retVal;
        }
    }

    internal class MethodData
    {
        private Dictionary<Type, MethodInfo> _genericMethods;

        public MethodInfo RawInfo { get; private set; }

        public MethodData(MethodInfo info)
        {
            RawInfo = info;

            _genericMethods = new Dictionary<Type, MethodInfo>();
        }

        public MethodInfo GetGenericMethod(Type typeArg)
        {
            if (!RawInfo.IsGenericMethod)
            {
                return null;
            }

            MethodInfo retVal = null;

            if (!_genericMethods.TryGetValue(typeArg, out retVal))
            {
                //if (RawInfo.GetGenericMethodDefinition().GetGenericArguments().Length != 1)
                //{
                //    throw new Exception("Only generic methods with single type argument can be cached.");
                //}

                retVal = RawInfo.MakeGenericMethod(new[] { typeArg });

                _genericMethods.Add(typeArg, retVal);
            }

            return retVal;
        }
    }
}

    
