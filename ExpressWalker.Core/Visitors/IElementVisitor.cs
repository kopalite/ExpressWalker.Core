using System;
using System.Collections.Generic;

namespace ExpressWalker.Core.Visitors
{
    public interface IVisitor
    {
        void Visit(object element, object blueprint = null, int depth = Constants.MaxDepth, InstanceGuard guard = null, HashSet<PropertyValue> values = null);
    }

    public interface IElementVisitor : IVisitor
    {
        Type ElementType { get; }

        string ElementName { get; }

        object Extract(object parent);

        object SetCopy(object parent, object element);
    }

    public interface IElementVisitor<TElement> : IElementVisitor
    {

    }
}
