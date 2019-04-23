using System;

namespace ExpressWalker.Core.Visitors
{
    /// <summary>
    /// Used as property value wrapper in parameter of <see cref="IVisitor.VIsit"/> (in case you wish to collect old and new property values).
    /// </summary>
    public sealed class PropertyValue
    {
        public PropertyValue(Type propertyType, object oldValue, object newValue, object metadata)
        {
            PropertyType = propertyType;

            OldValue = oldValue;

            NewValue = newValue;

            Metadata = metadata;
        }

        public Type PropertyType { get; private set; }

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public object Metadata { get; private set; }
    }
}
