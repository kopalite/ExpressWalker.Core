using ExpressWalker.Core.Visitors;
using System;
using System.Linq.Expressions;

namespace ExpressWalker
{
    internal class PropertyVisitor<TElement>
    {
        public Type ElementType { get { return typeof(TElement); } }
    }

    internal class PropertyVisitor<TElement, TProperty> : PropertyVisitor<TElement>, IPropertyVisitor<TElement>
    {
        public Type PropertyType { get { return typeof(TProperty); } }

        public string PropertyName { get; private set; }

        private readonly ExpressAccessor _propertyAccessor;

        private readonly Func<TProperty, object, TProperty> _getNewValue;

        private readonly object _metadata;
        
        internal PropertyVisitor(string propertyName, Expression<Func<TProperty, object, TProperty>> getNewValue, object metadata)
        {
            PropertyName = propertyName;

            _propertyAccessor = ExpressAccessor.Create(typeof(TElement), typeof(TProperty), propertyName);

            if (getNewValue != null)
            {
                _getNewValue = getNewValue.Compile();
            }

            _metadata = metadata;
        }
        
        public PropertyValue Visit(TElement element, TElement blueprint)
        {
            var oldValue = (TProperty)_propertyAccessor.Get(element);

            var newValue = oldValue;

            if (_getNewValue != null)
            {
                newValue = _getNewValue(oldValue, _metadata);

                _propertyAccessor.Set(element, newValue);

                if (blueprint != null)
                {
                    _propertyAccessor.Set(blueprint, newValue);
                }
            }

            return new PropertyValue(typeof(TProperty), oldValue, newValue, _metadata);
        }
    }
}
