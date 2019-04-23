using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker.Core.Helpers
{
    internal sealed class Util
    {
        #region [ Expression based extractions ]

        public static string NameOf<TType>(Expression<Func<TType, object>> expression)
        {
            return NameOf((Expression)expression);
        }

        private static string NameOf(Expression expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                return NameOf(lambda.Body);
            }

            var unary = expression as UnaryExpression;
            if (unary != null)
            {
                return NameOf(unary.Operand);
            }

            var member = expression as MemberExpression;
            if (member != null)
            {
                return member.Member.Name;
            }

            throw new ArgumentException("Must be unary expression or member expression!");
        }

        public static Type TypeOf<TType>(Expression<Func<TType, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                var member = lambda.Body as MemberExpression;
                if (member != null)
                {
                    var property = member.Member as PropertyInfo;
                    if (property != null)
                    {
                        return property.PropertyType;
                    }

                    var field = member.Member as FieldInfo;
                    if (field != null)
                    {
                        return field.FieldType;
                    }
                }
            }

            throw new ArgumentException("Must be member expression of class' field property!");
        }

        #endregion

        #region [ Simple type validation ]

        private static Type[] _valueTypes = new Type[]
        {
             typeof(Enum),
             typeof(string),
             typeof(decimal),
             typeof(DateTime),
             typeof(DateTimeOffset),
             typeof(TimeSpan),
             typeof(Guid)
        };

        public static bool IsSimpleType(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || _valueTypes.Contains(type) || Convert.GetTypeCode(type) != TypeCode.Object)
            {
                return true;
            }

            var isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            if (isNullableType)
            {
                var genericTypeArgs = type.GetGenericArguments().First();

                return IsSimpleType(genericTypeArgs);
            }

            return false;
        }

        #endregion

        #region [ Collection type validation ]

        public static bool IsGenericEnumerable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static bool ImplementsGenericIEnumerable(Type type)
        {
            return type.GetInterfaces().Any(IsGenericEnumerable);
        }

        public static Type GetCollectionItemsType(Type type)
        {
            if (IsGenericEnumerable(type))
            {
                return type.GenericTypeArguments.FirstOrDefault();   
            }
            else if (ImplementsGenericIEnumerable(type))
            {
                return GetCollectionItemsType(type.GetInterfaces().FirstOrDefault(IsGenericEnumerable));
            }

            return null;
        }

        #endregion

        #region [ Dictionary type validation ]

        public static bool IsDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static Type GetDictionaryKeysType(Type type)
        {
            if (IsDictionary(type))
            {
                return type.GenericTypeArguments.FirstOrDefault();
            }

            return null;
        }

        public static Type GetDictionaryItemsType(Type type)
        {
            if (IsDictionary(type))
            {
                return type.GenericTypeArguments.LastOrDefault();
            }

            return null;
        }

        #endregion

        #region [ Constructor validation ]

        public static bool HasParameterlessCtor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool HasCollectionCtor(Type type, Type ctorParamType)
        {
            Func<ConstructorInfo, Type> getParameterType = c => c.GetParameters().First().ParameterType;
            return type.GetConstructors().Any(c => c.GetParameters().Length == 1 && getParameterType(c).Equals(ctorParamType) &&
                                                   (IsGenericEnumerable(getParameterType(c)) || 
                                                    ImplementsGenericIEnumerable(getParameterType(c))));
        }

        public static ConstructorInfo GetCollectionCtor(Type type, Type ctorParamType)
        {
            Func<ConstructorInfo, Type> getParameterType = c => c.GetParameters().First().ParameterType;
            return type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1 && getParameterType(c).Equals(ctorParamType) &&
                                                             (IsGenericEnumerable(getParameterType(c)) ||
                                                              ImplementsGenericIEnumerable(getParameterType(c))));
        }

        public static bool HasDictionaryCtor(Type type, Type ctorParamType)
        {
            Func<ConstructorInfo, Type> getParameterType = c => c.GetParameters().First().ParameterType;
            return type.GetConstructors().Any(c => c.GetParameters().Length == 1 && getParameterType(c).Equals(ctorParamType) &&
                                                   (IsGenericEnumerable(getParameterType(c)) ||
                                                    ImplementsGenericIEnumerable(getParameterType(c))));
        }

        public static ConstructorInfo GetDictionaryCtor(Type type, Type ctorParamType)
        {
            Func<ConstructorInfo, Type> getParameterType = c => c.GetParameters().First().ParameterType;
            return type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1 && getParameterType(c).Equals(ctorParamType) &&
                                                             (IsGenericEnumerable(getParameterType(c)) ||
                                                              ImplementsGenericIEnumerable(getParameterType(c))));
        }

        #endregion
    }
}
