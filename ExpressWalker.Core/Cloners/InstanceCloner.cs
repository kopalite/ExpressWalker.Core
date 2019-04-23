using ExpressWalker.Core.Helpers;
using System;
using System.Collections.Generic;

using System.Linq.Expressions;

namespace ExpressWalker.Core.Cloners
{
    /// <summary>
    /// Clones non-collection reference types.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    internal sealed class InstanceCloner<TElement> : ClonerBase
    {
        private Func<TElement> _constructor;

        private List<ExpressAccessor> _accessors;

        public InstanceCloner() : base()
        {
            //Creating constructor function (1st step: createing initial instance).

            _constructor = Constructor<TElement>();

            //creating property getter/setter for each property that is of primitive (value) type (2nd step: cloning primitive values into new instance)

            _accessors = new List<ExpressAccessor>();

            foreach (var property in typeof(TElement).GetProperties())
            {
                if (Util.IsSimpleType(property.PropertyType))
                {
                    var accessor = ExpressAccessor.Create(typeof(TElement), property.PropertyType, property.Name);

                    _accessors.Add(accessor);
                }
            }
        }

        private TElement Clone(TElement element)
        {
            if (element == null || element.Equals(default(TElement)))
            {
                return default(TElement);
            }

            var clone = _constructor();

            _accessors.ForEach(a =>
            {
                var value = a.Get(element);

                a.Set(clone, value);
            });

            return clone;
        }

        public override object Clone(object element)
        {
            if (element == null)
            {
                return null;
            }

            if (!(element is TElement))
            {
                throw new Exception(string.Format("Parameter 'element' must be of type '{0}'", typeof(TElement).Name));
            }

            return Clone((TElement)element);
        }

        private Func<TEntity> Constructor<TEntity>()
        {
            var type = typeof(TEntity);
            var body = Expression.New(type);
            var lambda = Expression.Lambda<Func<TEntity>>(body);
            return lambda.Compile();
        }
    }
}
