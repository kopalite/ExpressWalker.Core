using System;
using System.Linq.Expressions;

namespace ExpressWalker
{
    internal class ElementTarget
    {
        public Type ElementType { get; private set; }
        
        public ElementTarget(Type elementType)
        {
            ElementType = elementType;
        }
    }

    internal abstract class PropertyTarget
    {
        public Type ElementType { get; private set; }

        public string PropertyName { get; private set; }

        public Type PropertyType { get; private set; }

        // An object convertible to Action<TPropertyType> where TPropertyType is specified in derived class.
        public Expression GetNewValue { get; protected set; }

        protected PropertyTarget(Type elementType, Type propertyType, string propertyName)
        {
            ElementType = elementType;
            PropertyType = propertyType;
            PropertyName = propertyName;
        }
    }

    internal class PropertyTarget<TPropertyType> : PropertyTarget
    {

        public PropertyTarget(Type elementType,
                              Type propertyType,
                              string propertyName,
                              Expression<Func<TPropertyType, object, TPropertyType>> getNewValue) 
            : base(elementType, propertyType, propertyName)
        {
            GetNewValue = getNewValue;
        }
    }
}
