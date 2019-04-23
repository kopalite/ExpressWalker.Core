using ExpressWalker.Core.Cloners;
using ExpressWalker.Core.Helpers;
using ExpressWalker.Core.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker
{
    internal abstract class ElementVisitor
    {
        public abstract Type ElementType { get; }

        public abstract string ElementName { get; protected set; }

        public abstract bool AnyElement { get; }

        public abstract bool AnyCollection { get; }

        public abstract bool AnyDictionary { get; }

        public abstract bool AnyProperty { get; }

        public abstract ElementVisitor AddElement(Type elementType, string childElementName, bool isHierarchy);

        public abstract void RemoveElement(Type elementType, string childElementName);

        public abstract ElementVisitor AddCollection(Type elementType, Type collectionType, string collectionName, bool isHierarchy);

        public abstract void RemoveCollection(Type collectionType, string collectionName);

        public abstract ElementVisitor AddDictionary(Type elementType, Type dictionaryType, string dictionaryName, bool isHierarchy);

        public abstract void RemoveDictionary(Type dictionaryType, string dictionaryName);

        //getNewValue is convertible to Expression<Func<TPropertyType, TPropertyType>> where TPropertyType is specified in derived class.
        public abstract ElementVisitor AddProperty(Type propertyType, string propertyName,  Expression getNewValue);
    }

    internal partial class ElementVisitor<TElement> : IElementVisitor<TElement>
    {
        protected ExpressAccessor _elementAccessor;

        protected ClonerBase _elementCloner;

        protected HashSet<IElementVisitor> _elementVisitors;

        protected HashSet<IElementVisitor> _collectionVisitors;

        protected HashSet<IElementVisitor> _dictionaryVisitors;

        protected HashSet<IPropertyVisitor<TElement>> _propertyVisitors;

        public override Type ElementType { get { return typeof(TElement); } }

        public override string ElementName { get; protected set; }

        public override bool AnyElement { get { return _elementVisitors.Any(); } }

        public override bool AnyCollection { get { return _collectionVisitors.Any(); } }

        public override bool AnyDictionary { get { return _dictionaryVisitors.Any(); } }

        public override bool AnyProperty { get { return _propertyVisitors.Any(); } }

        public PropertyGuard Guard { get; protected set; }

        public bool SupportsCloning { get; protected set; }

        protected ElementVisitor(PropertyGuard guard, bool supportsCloning)
        {
            _elementVisitors = new HashSet<IElementVisitor>();

            _collectionVisitors = new HashSet<IElementVisitor>();

            _dictionaryVisitors = new HashSet<IElementVisitor>();

            _propertyVisitors = new HashSet<IPropertyVisitor<TElement>>();

            Guard = guard;

            SupportsCloning = supportsCloning;
        }

        public ElementVisitor(Type ownerType, 
                              string elementName = null, 
                              PropertyGuard guard = null, 
                              bool supportsCloning = true) : this(guard, supportsCloning)
        {
            ElementName = elementName;

            if (!string.IsNullOrWhiteSpace(elementName))
            {
                _elementAccessor = ExpressAccessor.Create(ownerType, typeof(TElement), elementName);
            }

            if (SupportsCloning)
            {
                _elementCloner = ClonerBase.Create(typeof(TElement));
            }
        }

        public object Extract(object parent)
        {
            return _elementAccessor.Get(parent);
        }

        public object SetCopy(object parent, object element)
        {
            if (!SupportsCloning)
            {
                throw new Exception("This visitor doesn't support cloning. Please build visitor by TypeWalker.Build(supportsHierarchy:true).");
            }

            var blueprint = _elementCloner.Clone(element);

            _elementAccessor.Set(parent, blueprint);

            return blueprint;
        }

        public void Visit(object element, object blueprint = null, int depth = Constants.MaxDepth, InstanceGuard guard = null, HashSet<PropertyValue> values = null)
        {
            if (element == null)
            {
                return;
            }

            if (!(element is TElement))
            {
                throw new Exception(string.Format("Given element and must be of type '{0}'", typeof(TElement).ToString()));
            }
            

            if (blueprint != null && (!(blueprint is TElement)))
            {
                throw new Exception(string.Format("Given blueprint must be of type '{0}'", typeof(TElement).ToString()));
            }

            Visit((TElement)element, (TElement)blueprint, depth, guard, values);
        }

        public void Visit(TElement element, TElement blueprint = default(TElement), int depth = Constants.MaxDepth, InstanceGuard guard = null, HashSet<PropertyValue> values = null)
        {
            if (depth > Constants.MaxDepth)
            {
                throw new Exception(string.Format("Depth of visit cannot be more than {0}.", Constants.MaxDepth));
            }

            //If the depth reached given maximum at begining or instance was already visited (we have circular reference), we will just return.

            if (depth < 0 || (guard != null && guard.IsGuarded(element)))
            {
                return;
            }

            //Protecting the instance to be visited again.

            if (guard != null)
            {
                guard.Guard(element);
            }

            //Visiting properties.

            foreach (var propertyVisitor in _propertyVisitors)
            {
                var value = propertyVisitor.Visit(element, blueprint);

                if (values != null)
                {
                    values.Add(value);
                }
            }

            //Visiting elements.

            foreach (var elementVisitor in _elementVisitors)
            {
                var childElement = elementVisitor.Extract(element);

                object childBlueprint = null;

                if (blueprint != null)
                {
                    childBlueprint = elementVisitor.SetCopy(blueprint, childElement);
                }

                //Setting the InstanceGuard of child element visitor with already visited instances.

                elementVisitor.Visit(childElement, childBlueprint, depth - 1, guard, values);
            }

            //Visiting collections.

            foreach (var collectionVisitor in _collectionVisitors)
            {
                var childCollection = (IEnumerable)collectionVisitor.Extract(element);

                IEnumerable childCollectionBlueprint = null;

                if (blueprint != null)
                {
                    childCollectionBlueprint = (IEnumerable)collectionVisitor.SetCopy(blueprint, childCollection);
                }

                if (childCollection == null)
                {
                    continue;
                }

                var originalEnumerator = childCollection.GetEnumerator();
                var blueprintEnumerator = childCollectionBlueprint != null ? childCollectionBlueprint.GetEnumerator() : null;

                if (blueprintEnumerator != null)
                {
                    while (originalEnumerator.MoveNext() && blueprintEnumerator.MoveNext())
                    {
                        var originalElement = originalEnumerator.Current;
                        var blueprintElement = blueprintEnumerator.Current;

                        collectionVisitor.Visit(originalElement, blueprintElement, depth - 1, guard, values);
                    }
                }
                else
                {
                    while (originalEnumerator.MoveNext())
                    {
                        var originalElement = originalEnumerator.Current;

                        collectionVisitor.Visit(originalElement, null, depth - 1, guard, values);
                    }
                }
            }

            //Visiting dictionaries.

            foreach (IDictionaryVisitor dictionaryVisitor in _dictionaryVisitors)
            {
                var childDictionary = (IEnumerable)dictionaryVisitor.Extract(element);

                IEnumerable childDictionaryBlueprint = null;

                if (blueprint != null)
                {
                    childDictionaryBlueprint = (IEnumerable)dictionaryVisitor.SetCopy(blueprint, childDictionary);
                }

                if (childDictionary == null)
                {
                    continue;
                }

                var originalEnumerator = childDictionary.GetEnumerator();
                var blueprintEnumerator = childDictionaryBlueprint != null ? childDictionaryBlueprint.GetEnumerator() : null;

                if (blueprintEnumerator != null)
                {
                    while (originalEnumerator.MoveNext() && blueprintEnumerator.MoveNext())
                    {
                        var originalElement = dictionaryVisitor.KeyValueAccessor.Get(originalEnumerator.Current);
                        var blueprintElement = dictionaryVisitor.KeyValueAccessor.Get(blueprintEnumerator.Current);

                        dictionaryVisitor.Visit(originalElement, blueprintElement, depth - 1, guard, values);
                    }
                }
                else
                {
                    while (originalEnumerator.MoveNext())
                    {
                        var originalElement = dictionaryVisitor.KeyValueAccessor.Get(originalEnumerator.Current);

                        dictionaryVisitor.Visit(originalElement, null, depth - 1, guard, values);
                    }
                }
            }
        }
    }

    internal partial class ElementVisitor<TElement> : ElementVisitor
    {
        private static Type VisitorHierarchyAttributeType = typeof(VisitorHierarchyAttribute);

        internal IElementVisitor<TChildElement> AddElementVisitor<TChildElement>(string childElementName, bool isHierarchy)
        {
            var childElementType = typeof(TChildElement);

            if (_elementVisitors.Any(ev => ev.ElementType == childElementType && ev.ElementName == childElementName))
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), childElementName));
            }

            PropertyGuard guard = null; 
            if (Guard != null)
            {
                //If circular reference between properties is detected, we will return null as a sign that we cannot continue cycling ([VisitorHierarchy] is suppressing it).

                if (!isHierarchy && Guard.IsRepeating(childElementType, childElementName))
                {
                    return null;
                }

                guard = Guard.Copy();
                guard.Add(childElementType, childElementName);
            }

            var elementVisitor = new ElementVisitor<TChildElement>(typeof(TElement), childElementName, guard, SupportsCloning);
            _elementVisitors.Add(elementVisitor);
            return elementVisitor;
        }

        public override ElementVisitor AddElement(Type elementType, string childElementName, bool isHierarchy)
        {
            var methodDef = AddElementVisitorMethod;
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { childElementName, isHierarchy});
            return visitor;
        }

        internal void RemoveElementVisitor<TChildElement>(string childElementName)
        {
            var elementVisitor = _elementVisitors.FirstOrDefault(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == childElementName);
            if (elementVisitor == null)
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is not found!", typeof(TElement), childElementName));
            }
            _elementVisitors.Remove(elementVisitor);
        }

        public override void RemoveElement(Type elementType, string childElementName)
        {
            var methodDef = RemoveElementVisitorMethod;
            var method = methodDef.MakeGenericMethod(elementType);
            method.Invoke(this, new[] { childElementName });
            
        }

        internal IElementVisitor<TChildElement> AddCollectionVisitor<TChildElement>(Type collectionType, string collectionName, bool isHierarchy)
        {
            var childElementType = typeof(TChildElement);
            var itemsType = Util.GetCollectionItemsType(childElementType);

            if (_collectionVisitors.Any(ev => ev.ElementType == itemsType && ev.ElementName == collectionName))
            {
                throw new ArgumentException(string.Format("Collection visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), collectionName));
            }

            PropertyGuard guard = null;
            if (Guard != null)
            {
                //If circular reference between properties is detected, we will return null as a sign that we cannot continue cycling ([VisitorHierarchy] is suppressing it).

                if (!isHierarchy && Guard.IsRepeating(childElementType, collectionName))
                {
                    return null;
                }

                guard = Guard.Copy();
                guard.Add(childElementType, collectionName);
            }

            var collectionVisitor = new CollectionVisitor<TChildElement>(typeof(TElement), collectionType, collectionName, guard, SupportsCloning);
            _collectionVisitors.Add(collectionVisitor);
            return collectionVisitor;
        }

        public override ElementVisitor AddCollection(Type elementType, Type collectionType, string collectionName, bool isHierarchy)
        {
            var methodDef = AddCollectionVisitorMethod;
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { collectionType, collectionName, isHierarchy });
            return visitor;
        }

        internal void RemoveCollectionVisitor<TChildElement>(string collectionName)
        {
            var itemsType = Util.GetCollectionItemsType(typeof(TChildElement));
            var collectionVisitor = _collectionVisitors.FirstOrDefault(ev => ev.ElementType == itemsType && ev.ElementName == collectionName);
            if (collectionVisitor == null)
            {
                throw new ArgumentException(string.Format("Collection visitor for type '{0}' and name '{1}' is not found!", typeof(TElement), collectionName));
            }
            _collectionVisitors.Remove(collectionVisitor);
        }

        public override void RemoveCollection(Type elementType, string collectionName)
        {
            var methodDef = RemoveCollectionVisitorMethod;
            var method = methodDef.MakeGenericMethod(elementType);
            method.Invoke(this, new[] { collectionName });
        }

        internal IElementVisitor<TChildElement> AddDictionaryVisitor<TChildElement>(Type dictionaryType, string dictionaryName, bool isHierarchy)
        {
            var childElementType = typeof(TChildElement);
            var itemsType = Util.GetDictionaryItemsType(childElementType);

            if (_dictionaryVisitors.Any(ev => ev.ElementType == itemsType && ev.ElementName == dictionaryName))
            {
                throw new ArgumentException(string.Format("Dictionary visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), dictionaryName));
            }

            PropertyGuard guard = null;
            if (Guard != null)
            {
                //If circular reference between properties is detected, we will return null as a sign that we cannot continue cycling ([VisitorHierarchy] is suppressing it).

                if (!isHierarchy && Guard.IsRepeating(childElementType, dictionaryName))
                {
                    return null;
                }

                guard = Guard.Copy();
                guard.Add(childElementType, dictionaryName);
            }

            var dictionaryVisitor = new DictionaryVisitor<TChildElement>(typeof(TElement), dictionaryType, dictionaryName, guard, SupportsCloning);
            _dictionaryVisitors.Add(dictionaryVisitor);
            return dictionaryVisitor;
        }

        public override ElementVisitor AddDictionary(Type elementType, Type dictionaryType, string dictionaryName, bool isHierarchy)
        {
            var methodDef = AddDictionaryVisitorMethod;
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { dictionaryType, dictionaryName, isHierarchy });
            return visitor;
        }

        internal void RemoveDictionaryVisitor<TChildElement>(string dictionaryName)
        {
            var itemsType = Util.GetDictionaryItemsType(typeof(TChildElement));
            var dictionaryVisitor = _dictionaryVisitors.FirstOrDefault(ev => ev.ElementType == itemsType && ev.ElementName == dictionaryName);
            if (dictionaryVisitor == null)
            {
                throw new ArgumentException(string.Format("Dictionary visitor for type '{0}' and name '{1}' is not found!", typeof(TElement), dictionaryName));
            }
            _dictionaryVisitors.Remove(dictionaryVisitor);
        }

        public override void RemoveDictionary(Type elementType, string dictionaryName)
        {
            var methodDef = RemoveDictionaryVisitorMethod;
            var method = methodDef.MakeGenericMethod(elementType);
            method.Invoke(this, new[] { dictionaryName });
        }

        internal IElementVisitor<TElement> AddPropertyVisitor<TProperty>(string propertyName, Expression<Func<TProperty, object, TProperty>> getNewValue)
        {
            var elementType = typeof(TElement);

            if (_propertyVisitors.Any(pv => pv.ElementType == elementType && pv.PropertyName == propertyName))
            {
                throw new ArgumentException(string.Format("Property visitor for type '{0}' and name '{1}' is already added!", elementType, propertyName));
            }

            var propertyVisitor = new PropertyVisitor<TElement, TProperty>(propertyName, getNewValue, GetPropertyMetadata(propertyName));
            _propertyVisitors.Add(propertyVisitor);
            return this;
        }

        public override ElementVisitor AddProperty(Type propertyType, string propertyName, Expression getNewValue)
        {
            var methodDef = AddPropertyVisitorMethod;
            var method = methodDef.MakeGenericMethod(propertyType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { propertyName, getNewValue });
            return visitor;
        }

        private object GetPropertyMetadata(string propertyName)
        {
            var elementType = typeof(TElement);
            var property = elementType.GetProperty(propertyName);
            var metadataAttribute = property.GetCustomAttribute<VisitorMetadataAttribute>(false);
            return (metadataAttribute == null) ? null : metadataAttribute.Metadata;
        }

        private static MethodInfo _addElementVisitorMethod;
        private static MethodInfo AddElementVisitorMethod { get { return _addElementVisitorMethod ?? (_addElementVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("AddElementVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }

        private static MethodInfo _removeElementVisitorMethod;
        private static MethodInfo RemoveElementVisitorMethod { get { return _removeElementVisitorMethod ?? (_removeElementVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("RemoveElementVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }

        private static MethodInfo _addCollectionVisitorMethod;
        private static MethodInfo AddCollectionVisitorMethod { get { return _addCollectionVisitorMethod ?? (_addCollectionVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("AddCollectionVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }

        private static MethodInfo _removeCollectionVisitorMethod;
        private static MethodInfo RemoveCollectionVisitorMethod { get { return _removeCollectionVisitorMethod ?? (_removeCollectionVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("RemoveCollectionVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }

        private static MethodInfo _addDictionaryVisitorMethod;
        private static MethodInfo AddDictionaryVisitorMethod { get { return _addDictionaryVisitorMethod ?? (_addDictionaryVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("AddDictionaryVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }

        private static MethodInfo _removeDictionaryVisitorMethod;
        private static MethodInfo RemoveDictionaryVisitorMethod { get { return _removeDictionaryVisitorMethod ?? (_removeDictionaryVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("RemoveDictionaryVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }

        private static MethodInfo _addPropertyVisitorMethod;
        private static MethodInfo AddPropertyVisitorMethod { get { return _addPropertyVisitorMethod ?? (_addPropertyVisitorMethod = typeof(ElementVisitor<TElement>).GetMethod("AddPropertyVisitor", BindingFlags.NonPublic | BindingFlags.Instance)); } }
    }
}
