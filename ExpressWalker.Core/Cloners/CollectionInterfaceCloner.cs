using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExpressWalker.Core.Cloners
{
    internal sealed class CollectionInterfaceCloner<TItem> : ClonerBase
    {
        private ClonerBase _itemsCloner;

        public CollectionInterfaceCloner() : base()
        {
            _itemsCloner = Create(typeof(TItem));
        }

        public override object Clone(object collection)
        {
            if (collection == null)
            {
                return null;
            }

            if (!(collection is ICollection<TItem>))
            {
                throw new Exception(string.Format("Parameter 'element' must be of type '{0}'", typeof(ICollection<TItem>).Name));
            }

            if (collection == null || collection.Equals(default(ICollection<TItem>)))
            {
                return default(ICollection<TItem>);
            }

            var items = ((IEnumerable<TItem>)collection).Select(i => (TItem)_itemsCloner.Clone(i)).ToArray();

            return new Collection<TItem>(items);
        }
    }
}
