using System;
using System.Linq;
using System.Collections.Generic;

namespace ExpressWalker.Core.Cloners
{
    internal abstract class ClonerBase 
    {
        private static IEnumerable<ClonerStrategy> Strategies = typeof(ClonerBase).Assembly.GetTypes()
                                                                   .Where(t => typeof(ClonerStrategy).IsAssignableFrom(t) && !t.IsAbstract)
                                                                   .Select(t => (ClonerStrategy)Activator.CreateInstance(t))
                                                                   .OrderByDescending(s => s.Priority)
                                                                   .ToArray();
        public abstract object Clone(object element);

        public static ClonerBase Create(Type elementType)
        {
            var strategy = Strategies.FirstOrDefault(s => s.IsMatch(elementType));
            if (strategy == null)
            {
                throw new Exception(string.Format(@"Cannot clone objects of type '{0}'. It must be non-collection type with parameterless constructor 
                                                    or collection type with constructor accepting single parameter of type IList<T> or IEnumerable<T>.
                                                    typeof(T) also must be non-collection type with parameterless constructor.", elementType.FullName));
                                                
            }
            return strategy.GetCloner(elementType);
        }
    }
}
