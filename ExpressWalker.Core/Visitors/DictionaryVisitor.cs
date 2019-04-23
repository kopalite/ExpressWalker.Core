using ExpressWalker.Core.Cloners;
using System;
using System.Collections.Generic;

namespace ExpressWalker.Core.Visitors
{
    internal sealed class DictionaryVisitor<TElement> : ElementVisitor<TElement>, IDictionaryVisitor
    {
        public DictionaryVisitor(Type ownerType, 
                                 Type dictionaryType, 
                                 string elementName = null, 
                                 PropertyGuard guard = null, 
                                 bool supportsCloning = true) : base(guard, supportsCloning)
        {
            ElementName = elementName;
                
            if (!string.IsNullOrWhiteSpace(elementName))
            {
                _elementAccessor = ExpressAccessor.Create(ownerType, dictionaryType, elementName);
            }

            if (SupportsCloning)
            {
                _elementCloner = ClonerBase.Create(dictionaryType);
            }

            var keyType = dictionaryType.GenericTypeArguments[0];
            var valueType = dictionaryType.GenericTypeArguments[1];
            var kvpType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
            KeyValueAccessor = ExpressAccessor.Create(kvpType, valueType, "Value");
        }

        public ExpressAccessor KeyValueAccessor { get; private set; }
    }
}
