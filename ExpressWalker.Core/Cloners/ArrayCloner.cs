using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressWalker.Core.Cloners
{
    internal class ArrayClonner<TArray, TItem> : ClonerBase
    {
        private ClonerBase _itemsCloner;

        public ArrayClonner() : base()
        {
            _itemsCloner = Create(typeof(TItem));
        }

        public override object Clone(object array)
        {
            if (array == null)
            {
                return null;
            }

            if (!(array is TArray))
            {
                throw new Exception(string.Format("Parameter 'array' must be of type '{0}'", typeof(TArray).Name));
            }

            if (array == null || array.Equals(default(TArray)))
            {
                return default(TArray);
            }

            return ((IEnumerable<TItem>)array).Select(i => (TItem)_itemsCloner.Clone(i)).ToArray();
        }
    }
}
