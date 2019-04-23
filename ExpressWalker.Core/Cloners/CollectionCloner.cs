using ExpressWalker.Core.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressWalker.Core.Cloners
{
    internal sealed class CollectionClonner<TCollection, TItem> : ClonerBase
    {
        private Func<IList<TItem>, TCollection> _constructor;

        private ClonerBase _itemsCloner;

        public CollectionClonner() : base()
        {
            _constructor = Constructor();

            _itemsCloner = Create(typeof(TItem));
        }

        private TCollection Clone(TCollection collection)
        {
            if (collection == null || collection.Equals(default(TCollection)))
            {
                return default(TCollection);
            }

            var items = ((IList<TItem>)collection).Select(i => (TItem)_itemsCloner.Clone(i)).ToArray();

            var clone = _constructor(items);

            return clone;
        }

        public override object Clone(object collection)
        {
            if (collection == null)
            {
                return null;
            }

            if (!(collection is TCollection))
            {
                throw new Exception(string.Format("Parameter 'collection' must be of type '{0}'", typeof(TCollection).Name));
            }

            return Clone((TCollection)collection);
        }

        private Func<IList<TItem>, TCollection> Constructor()
        {
            var type = typeof(TCollection);
            var ctor = Util.GetCollectionCtor(type, typeof(IList<TItem>));
            var input = Expression.Parameter(typeof(IList<TItem>));
            var body = Expression.New(ctor, input);
            var lambda = Expression.Lambda<Func<IList<TItem>, TCollection>>(body, input);
            return lambda.Compile();
        }
    }
}
