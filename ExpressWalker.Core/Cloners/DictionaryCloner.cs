using ExpressWalker.Core.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressWalker.Core.Cloners
{
    internal sealed class DictionaryClonner<TDictionary, TKey, TItem> : ClonerBase
    {
        private Func<IDictionary<TKey, TItem>, TDictionary> _constructor;

        private ClonerBase _itemsCloner;

        public DictionaryClonner() : base()
        {
            _constructor = Constructor();

            _itemsCloner = Create(typeof(TItem));
        }

        private TDictionary Clone(TDictionary dictionary)
        {
            if (dictionary == null || dictionary.Equals(default(TDictionary)))
            {
                return default(TDictionary);
            }

            var items = ((IDictionary<TKey, TItem>)dictionary).ToDictionary(kvp => kvp.Key, i => (TItem)_itemsCloner.Clone(i.Value));

            var clone = _constructor(items);

            return clone;
        }

        public override object Clone(object dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            if (!(dictionary is TDictionary))
            {
                throw new Exception(string.Format("Parameter 'dictionary' must be of type '{0}'", typeof(TDictionary).Name));
            }

            return Clone((TDictionary)dictionary);
        }

        private Func<IDictionary<TKey, TItem>, TDictionary> Constructor()
        {
            var type = typeof(Dictionary<TKey, TItem>);
            var ctor = Util.GetDictionaryCtor(type, typeof(IDictionary<TKey, TItem>));
            var input = Expression.Parameter(typeof(IDictionary<TKey, TItem>));
            var body = Expression.New(ctor, input);
            var lambda = Expression.Lambda<Func<IDictionary<TKey, TItem>, TDictionary>>(body, input);
            return lambda.Compile();
        }
    }
}
