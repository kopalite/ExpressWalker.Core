using ExpressWalker.Core.Visitors;
using System;
using System.Linq.Expressions;

namespace ExpressWalker.Core.Factories
{
    public interface IVisitorsFactory
    {
        IVisitorsFactory WithSettings(string name, int depth = Constants.MaxDepth, bool usePropertyGuard = false, bool supportsCloning = true);

        IVisitorsFactory ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue);

        IVisitorsFactory ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName,
                                                                  Expression<Func<TPropertyType, object, TPropertyType>> getNewValue);

        IVisitor GetVisitor(string name, Type type);
    }
}
