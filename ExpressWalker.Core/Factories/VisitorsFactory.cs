using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ExpressWalker.Core.Visitors;
using System.Reflection;

namespace ExpressWalker.Core.Factories
{
    public sealed class VisitorsFactory : IVisitorsFactory
    {
        private readonly List<WalkerSettings> _settings;

        private WalkerSettings _currentSettings;

        private readonly Dictionary<VisitorKey, IVisitor> _visitors;

        private bool _isLocked;

        public VisitorsFactory()
        {
            _settings = new List<WalkerSettings>();


            _visitors = new Dictionary<VisitorKey, IVisitor>();
        }

        public IVisitorsFactory WithSettings(string name, int depth = Constants.MaxDepth, bool usePropertyGuard = false, bool supportsCloning = true)
        {
            var settings = _settings.FirstOrDefault(x => x.Name == name);

            if (settings == null)
            {
                settings = new WalkerSettings(name, depth, usePropertyGuard, supportsCloning);

                _settings.Add(settings);
            }

            _currentSettings = settings;

            return this;
        }

        public IVisitorsFactory ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            if (_isLocked)
            {
                throw new Exception("Factory can only be set before calling GetVisitor() method!");
            }

            if (_currentSettings == null)
            {
                throw new Exception("Please specify the visitors settings by calling WithSettings() method first!");
            }

            _currentSettings.ForProperty(getNewValue);

            return this;
        }

        public IVisitorsFactory ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName, Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            if (_isLocked)
            {
                throw new Exception("Factory can only be set before calling GetVisitor() method!");
            }

            if (_currentSettings == null)
            {
                throw new Exception("Please specify the visitors settings by calling WithSettings() method first!");
            }

            _currentSettings.ForProperty(propertyName, getNewValue);

            return this;
        }

        public IVisitor GetVisitor(string name, Type type)
        {
            _isLocked = true;

            var visitorKey = new VisitorKey(name, type);
            if (_visitors.ContainsKey(visitorKey))
            {
                return _visitors[visitorKey];
            }

            var settings = _settings.FirstOrDefault(x => x.Name == name);
            if (settings == null)
            {
                throw new Exception("Visitors settings with name '{0}' is not being set in visitors factory. It can be set by calling .WithSettings() method before asking for visitors.");
            }

            var visitor = settings.GetVisitor(type);
            _visitors.Add(visitorKey, visitor);
            return visitor;
        }
    }

    internal struct VisitorKey
    {
        private readonly string _name;
        private readonly Type _type;

        public VisitorKey(string name, Type type)
        {
            _name = name;
            _type = type;
        }
    }

    internal sealed class WalkerSettings
    {
        private List<Action<dynamic>> _walkerActions;

        public string Name { get; private set; }
        public int Depth { get; private set; }
        public bool UsePropertyGuard { get; private set; }
        public bool SupportsCloning { get; private set; }

        public WalkerSettings(string name, int depth, bool usePropertyGuard, bool supportsCloning)
        {
            _walkerActions = new List<Action<dynamic>>();

            Name = name;
            Depth = depth;
            UsePropertyGuard = usePropertyGuard;
            SupportsCloning = supportsCloning;
        }

        public void ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _walkerActions.Add(x => x.ForProperty(getNewValue));
        }

        public void ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName, Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _walkerActions.Add(x => x.ForProperty(propertyName, getNewValue));
        }

        public IVisitor GetVisitor(Type type)
        {
            var getVisitor = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                      .FirstOrDefault(x => x.IsGenericMethod && x.Name.StartsWith("GetVisitor"))
                                      .MakeGenericMethod(type);

            return getVisitor.Invoke(this, null) as IVisitor; 
        }

        private IVisitor GetVisitor<TRootType>()
        {
            var walker = TypeWalker<TRootType>.Create();
            _walkerActions.ForEach(x => x(walker));
            return walker.Build(Depth, UsePropertyGuard ? new PropertyGuard() : null, SupportsCloning);
        }
    }
}
