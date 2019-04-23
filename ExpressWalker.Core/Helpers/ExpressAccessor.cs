using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker
{
    public abstract class ExpressAccessor
    {
        public abstract object Get(object @object);
        
        public abstract void Set(object @object, object value);

        public static ExpressAccessor Create(Type ownerType, Type propertyType, string propertyName)
        {
            var property = ownerType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var isReadOnly = !property.CanWrite;
            var typeDefinition = typeof(ExpressAccessor<,>);
            var concreteType = typeDefinition.MakeGenericType(ownerType, propertyType);
            return (ExpressAccessor)Activator.CreateInstance(concreteType, propertyName, isReadOnly);
        }
    }

    public class ExpressAccessor<TObject, TProperty> : ExpressAccessor
    {
        public Func<TObject, TProperty> Getter { get; private set; }
        public Action<TObject, TProperty> Setter { get; private set; }

        public ExpressAccessor(string propertyName, bool isReadOnly)
        {
            SetGetter(propertyName);
            SetSetter(propertyName, isReadOnly);
        }

        public override object Get(object @object)
        {
            return Getter((TObject)@object);
        }

        public override void Set(object @object, object value)
        {
            Setter((TObject)@object, (TProperty)value);
        }

        private void SetGetter(string propertyName)
        {
            var propertyParameter = Expression.Parameter(typeof(TObject), "value");
            var propertyExpression = Expression.Property(propertyParameter, propertyName);
            var result = Expression.Lambda<Func<TObject, TProperty>>(propertyExpression, propertyParameter);
            Getter = result.Compile();   
        }

        
        private void SetSetter(string propertyName, bool isReadOnly)
        {
            if (isReadOnly)
            {
                //In case of read-only property we will create fake setter.
                Setter = (x, m) => { };
                return;
            }

            var objectParameter =   Expression.Parameter(typeof(TObject));
            var propertyParameter = Expression.Parameter(typeof(TProperty), propertyName);
            var propertyExpresson = Expression.Property(objectParameter, propertyName);
            var binaryExpression = Expression.Assign(propertyExpresson, propertyParameter);
            var result = Expression.Lambda<Action<TObject, TProperty>>(binaryExpression, objectParameter, propertyParameter);
            Setter = result.Compile();
        }
    }
}
