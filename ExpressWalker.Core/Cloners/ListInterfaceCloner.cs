using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressWalker.Core.Cloners
{
    internal sealed class ListInterfaceCloner<TItem> : ClonerBase
    {
        private ClonerBase _itemsCloner;

        public ListInterfaceCloner() : base()
        {
            _itemsCloner = Create(typeof(TItem));
        }

        public override object Clone(object list)
        {
            if (list == null)
            {
                return null;
            }

            if (!(list is IList<TItem>))
            {
                throw new Exception(string.Format("Parameter 'list' must be of type '{0}'", typeof(IList<TItem>).Name));
            }

            if (list == null || list.Equals(default(IList<TItem>)))
            {
                return default(IList<TItem>);
            }

            return ((IEnumerable<TItem>)list).Select(i => (TItem)_itemsCloner.Clone(i)).ToList();
        }
    }
}
