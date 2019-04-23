using ExpressWalker.Core.Helpers;
using ExpressWalker.Core.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker
{
    public class TypeWalker<TRootType>
    {
        private List<PropertyTarget> _properties;

        private TypeWalker()
        {
            _properties = new List<PropertyTarget>();
        }

        public static TypeWalker<TRootType> Create()
        {
            return new TypeWalker<TRootType>();
        }

        public TypeWalker<TRootType> ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _properties.Add(new PropertyTarget<TPropertyType>(null, typeof(TPropertyType), null, getNewValue));

            return this;
        }

        public TypeWalker<TRootType> ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName,
                                                                              Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _properties.Add(new PropertyTarget<TPropertyType>(typeof(TElementType), typeof(TPropertyType), Util.NameOf(propertyName), getNewValue));

            return this;
        }

        /// <summary>
        /// Builds visitor for visiting <typeparamref name="TRootType"/> instance.
        /// </summary>
        /// <param name="depth">
        /// The depth (level) of object graph that visitor will be able to visit. 
        /// Decreases build performance for large values.
        /// </param>
        /// <param name="guard">
        /// If given, property guard will significantly decrease build time by avoiding circular references between property types.
        /// If you wish to use it and intencionaly allow obvious property type cycles like hierarchy structures, use [VisitorHierarchy] attribute on property that forms hierarchy.
        /// </param>
        /// <param name="supportsCloning">
        /// In order to support object cloning while visiting in IVisitor.Visit() method, many cloning expressions should be built, which is very costly time-wise.
        /// To significantly decrease build time of visitors that don't need to clone objects, set this parameter to false (omits cloning expressions creation).
        /// </param>
        /// <returns>IVisitor for visiting instances of type <typeparamref name="TRootType"/></returns>
        public IVisitor Build(int depth = Constants.MaxDepth, PropertyGuard guard = null, bool supportsCloning = true)
        {
            var visitor = new ElementVisitor<TRootType>(null, null, guard, supportsCloning);
            Build(visitor, depth);
            return visitor;
        }

        private void Build(ElementVisitor visitor, int depth)
        {
            if (depth > Constants.MaxDepth)
            {
                throw new Exception(string.Format("Depth of visit cannot be more than {0}.", Constants.MaxDepth));
            }

            if (depth < 0)
            {
                return;
            }

            var currentNodeType = visitor.ElementType;

            foreach (var property in currentNodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                //Trying to find property match, first by name, owner type and property type. If not found, we will try only with property type.

                var match = _properties.FirstOrDefault(p => p.ElementType == property.DeclaringType && p.PropertyName == property.Name && p.PropertyType == property.PropertyType);

                if (match == null)
                {
                    match = _properties.FirstOrDefault(p => p.ElementType == null && p.PropertyName == null && p.PropertyType == property.PropertyType);
                }

                if (match != null)
                {
                    visitor.AddProperty(property.PropertyType, property.Name, match.GetNewValue);
                }

                if (Util.IsSimpleType(property.PropertyType))
                {
                    continue;
                }

                //If property type is not primitive, we will assume we should add it as an element/collection, but after it's being built and turns out it's 'empty', we will remove it.

                if (Util.IsDictionary(property.PropertyType))
                {
                    var dictionaryItemType = Util.GetDictionaryItemsType(property.PropertyType);

                    if (Util.IsSimpleType(dictionaryItemType))
                    {
                        continue;
                    }

                    var isHierarchy = property.GetCustomAttribute<VisitorHierarchyAttribute>() != null;

                    var childVisitor = visitor.AddDictionary(dictionaryItemType, property.PropertyType, property.Name, isHierarchy);

                    if (childVisitor == null) //AddDirection() will return null in case of issues like circular reference.
                    {
                        continue;
                    }

                    Build(childVisitor, depth - 1);

                    if (!childVisitor.AnyElement && !childVisitor.AnyCollection && !childVisitor.AnyDictionary && !childVisitor.AnyProperty)
                    {
                        visitor.RemoveDictionary(property.PropertyType, property.Name);
                    }
                }
                else if (Util.IsGenericEnumerable(property.PropertyType) || Util.ImplementsGenericIEnumerable(property.PropertyType))
                {
                    var collectionItemType = Util.GetCollectionItemsType(property.PropertyType);

                    if (Util.IsSimpleType(collectionItemType))
                    {
                        continue;
                    }

                    var isHierarchy = property.GetCustomAttribute<VisitorHierarchyAttribute>() != null;

                    var childVisitor = visitor.AddCollection(collectionItemType, property.PropertyType, property.Name, isHierarchy);

                    if (childVisitor == null) //AddCollection() will return null in case of issues like circular reference.
                    {
                        continue;
                    }

                    Build(childVisitor, depth - 1);

                    if (!childVisitor.AnyElement && !childVisitor.AnyCollection && !childVisitor.AnyProperty)
                    {
                        visitor.RemoveCollection(property.PropertyType, property.Name);
                    }
                }
                else
                {
                    var isHierarchy = property.GetCustomAttribute<VisitorHierarchyAttribute>() != null;

                    var childVisitor = visitor.AddElement(property.PropertyType, property.Name, isHierarchy);

                    if (childVisitor == null) //AddElement() will return null in case of issues like circular reference.
                    {
                        continue;
                    }

                    Build(childVisitor, depth - 1);

                    if (!childVisitor.AnyElement && !childVisitor.AnyCollection && !childVisitor.AnyProperty)
                    {
                        visitor.RemoveElement(property.PropertyType, property.Name);
                    }
                }
            }
        }
    }
}
