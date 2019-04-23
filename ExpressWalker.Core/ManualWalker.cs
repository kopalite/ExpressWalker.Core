using ExpressWalker.Core.Helpers;
using ExpressWalker.Core.Visitors;
using System;
using System.Collections;
using System.Linq.Expressions;

namespace ExpressWalker
{
    public static class ManualWalker
    {
        public static IElementVisitor<TElement> Create<TElement>()
        {
            return new ElementVisitor<TElement>(typeof(TElement));
        }

        public static IElementVisitor<TElement> Element<TElement, TChildElement>(this IElementVisitor<TElement> element, 
                                                                                 Expression<Func<TElement, object>> childElementName,
                                                                                 Expression<Action<IElementVisitor<TChildElement>>> childElementSetup)
        {
            if (Util.IsGenericEnumerable(typeof(TElement)) || Util.ImplementsGenericIEnumerable(typeof(TElement)))
            {
                throw new Exception(string.Format("Element of type '{0}' is IEnumerable. Use Collection() method in order to configure visit to it.", typeof(TElement)));
            }

            var myElement = (ElementVisitor<TElement>)element;
            var extractedName = Util.NameOf(childElementName);
            var childElement = myElement.AddElementVisitor<TChildElement>(extractedName, false);

            if (childElementSetup != null)
            {
                childElementSetup.Compile()(childElement);
            }

            return element;
        }

        public static IElementVisitor<TElement> Collection<TElement, TCollectionElement>(this IElementVisitor<TElement> element,
                                                                                    Expression<Func<TElement, object>> childElementName,
                                                                                    Expression<Action<IElementVisitor<TCollectionElement>>> childElementSetup)
            
        {
            if (Util.IsDictionary(typeof(TElement)))
            {
                throw new Exception(string.Format("Element of type '{0}' is IDictionary. It is not yet supported for visit configuration.", typeof(TElement)));
            }

            var myElement = (ElementVisitor<TElement>)element;
            var extractedName = Util.NameOf(childElementName);
            var extractedType = Util.TypeOf(childElementName);
            var collectionElement = myElement.AddCollectionVisitor<TCollectionElement>(extractedType, extractedName, false);

            if (childElementSetup != null)
            {
                childElementSetup.Compile()(collectionElement);
            }

            return element;
        }

        public static IElementVisitor<TElement> Property<TElement, TProperty>(this IElementVisitor<TElement> element,
                                                                              Expression<Func<TElement, object>> propertyName,
                                                                              Expression<Func<TProperty, object, TProperty>> getNewValue) 
            
            
        {
            var myElement = (ElementVisitor<TElement>)element;
            var extractedName = Util.NameOf(propertyName);
            var childElement = myElement.AddPropertyVisitor(extractedName, getNewValue);
            return element;
        }

        public static IVisitor Build<TElement>(this IElementVisitor<TElement> element)
        {
            return element;
        }
    }
}
