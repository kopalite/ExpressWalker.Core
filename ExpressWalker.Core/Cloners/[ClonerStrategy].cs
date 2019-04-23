using ExpressWalker.Core.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExpressWalker.Core.Cloners
{
    internal abstract class ClonerStrategy
    {
        public abstract int Priority { get; }

        public abstract bool IsMatch(Type elementType);

        public abstract ClonerBase GetCloner(Type elementType);

        protected object Create(Type genericTypeDef, params Type[] genericTypeArgs)
        {
            var concreteType = genericTypeDef.MakeGenericType(genericTypeArgs);
            return Activator.CreateInstance(concreteType);
        }
    }


    internal class DictionaryStrategy : ClonerStrategy
    {
        public override int Priority { get { return 45; } }

        public override bool IsMatch(Type elementType)
        {
            if (Util.IsSimpleType(elementType) || !Util.IsDictionary(elementType))
            {
                return false;
            }

            var keysType = Util.GetDictionaryKeysType(elementType);
            if (keysType == null)
            {
                return false;
            }

            var itemsType = Util.GetDictionaryItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            return true;
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var keysType = Util.GetDictionaryKeysType(elementType);
            var itemsType = Util.GetDictionaryItemsType(elementType);
            return (ClonerBase)Create(typeof(DictionaryClonner<,,>), elementType, keysType, itemsType);
        }
    }

    
    /// <summary>
    /// Makes cloner for collection type that has constructor accepting single parameter of type IEnumerable<T> (like List<T>).
    /// </summary>
    internal class ListStrategy : ClonerStrategy
    {
        public override int Priority { get { return 40; } }

        public override bool IsMatch(Type elementType)
        {
            if (Util.IsSimpleType(elementType) || !Util.ImplementsGenericIEnumerable(elementType))
            {
                return false;
            }

            var itemsType = Util.GetCollectionItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            var enumParamType = typeof(IEnumerable<>).MakeGenericType(itemsType);
            return Util.HasCollectionCtor(elementType, enumParamType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetCollectionItemsType(elementType);
            return (ClonerBase)Create(typeof(ListCloner<,>), elementType, itemsType);
        }
    }

    /// <summary>
    /// Makes cloner for IList<T> collection type.
    /// </summary>
    internal class ListInterfaceStrategy : ClonerStrategy
    {
        public override int Priority { get { return 35; } }

        public override bool IsMatch(Type elementType)
        {
            if (Util.IsSimpleType(elementType) || !Util.ImplementsGenericIEnumerable(elementType))
            {
                return false;
            }

            var itemsType = Util.GetCollectionItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            var iListType = typeof(IList<>).MakeGenericType(itemsType);
            return elementType.Equals(iListType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetCollectionItemsType(elementType);
            return (ClonerBase)Create(typeof(ListInterfaceCloner<>), itemsType);
        }
    }

    /// <summary>
    ///Makes cloner for collection type that has constructor accepting single parameter of type IList<T> (like Collection<T>).
    /// </summary>
    internal class CollectionStrategy : ClonerStrategy
    {
        public override int Priority { get { return 30; } }

        public override bool IsMatch(Type elementType)
        {

            if (Util.IsSimpleType(elementType) || !Util.ImplementsGenericIEnumerable(elementType))
            {
                return false;
            }

            var itemsType = Util.GetCollectionItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            var listParamType = typeof(IList<>).MakeGenericType(itemsType);
            return Util.HasCollectionCtor(elementType, listParamType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetCollectionItemsType(elementType);
            return (ClonerBase)Create(typeof(CollectionClonner<,>), elementType, itemsType); 
        }
    }

    /// <summary>
    /// Makes cloner for IList<T> collection type.
    /// </summary>
    internal class CollectionInterfaceStrategy : ClonerStrategy
    {
        public override int Priority { get { return 25; } }

        public override bool IsMatch(Type elementType)
        {
            if (Util.IsSimpleType(elementType) || !Util.ImplementsGenericIEnumerable(elementType))
            {
                return false;
            }

            var itemsType = Util.GetCollectionItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            var iCollectionType = typeof(ICollection<>).MakeGenericType(itemsType);
            return elementType.Equals(iCollectionType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetCollectionItemsType(elementType);
            return (ClonerBase)Create(typeof(CollectionInterfaceCloner<>), itemsType);
        }
    }

    /// <summary>
    /// Makes cloner for explicit IEnumerable<T> types or concrete array types (like T[]).
    /// </summary>
    internal class ArrayListStrategy : ClonerStrategy
    {
        public override int Priority { get { return 22; } }

        public override bool IsMatch(Type elementType)
        {
            return !Util.IsSimpleType(elementType) && elementType.Equals(typeof(ArrayList));
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetCollectionItemsType(elementType);
            return new ArrayListClonner();
        }
    }

    /// <summary>
    /// Makes cloner for explicit IEnumerable<T> types or concrete array types (like T[]).
    /// </summary>
    internal class ArrayStrategy : ClonerStrategy
    {
        public override int Priority { get { return 20; } }

        public override bool IsMatch(Type elementType)
        {
            return !Util.IsSimpleType(elementType) && (Util.IsGenericEnumerable(elementType) || elementType.IsArray);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetCollectionItemsType(elementType);
            return (ClonerBase)Create(typeof(ArrayClonner<,>), elementType, itemsType);
        }
    }

    /// <summary>
    /// Makes cloner for non-collection types.
    /// </summary>
    internal class InstanceStrategy : ClonerStrategy
    {
        public override int Priority { get { return 10; } }

        public override bool IsMatch(Type elementType)
        {
            return !Util.IsSimpleType(elementType) && Util.HasParameterlessCtor(elementType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            return (ClonerBase)Create(typeof(InstanceCloner<>), elementType);
        }
    }
}
