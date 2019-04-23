using ExpressWalker.Core.Cloners;
using System;

namespace ExpressWalker.Core.Visitors
{
    internal sealed class CollectionVisitor<TElement> : ElementVisitor<TElement>
    {
        public CollectionVisitor(Type ownerType, 
                                 Type collectionType, 
                                 string elementName = null, 
                                 PropertyGuard guard = null, 
                                 bool supportsCloning = true) : base(guard, supportsCloning)
        {
            ElementName = elementName;
                
            if (!string.IsNullOrWhiteSpace(elementName))
            {
                _elementAccessor = ExpressAccessor.Create(ownerType, collectionType, elementName);
            }

            if (SupportsCloning)
            {
                _elementCloner = ClonerBase.Create(collectionType);
            }
        }
    }
}
