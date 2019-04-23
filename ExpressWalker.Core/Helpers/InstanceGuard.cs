using System;
using System.Collections.Generic;

namespace ExpressWalker
{
    public class InstanceGuard
    {
        private HashSet<int> _hashes;

        public InstanceGuard()
        {
            _hashes = new HashSet<int>();
        }
        
        public void Guard(object instance)
        {
            if (instance == null)
            {
                return;
            }

            var hash = instance.GetHashCode();

            if (!_hashes.Contains(hash))
            {
                _hashes.Add(hash);
            }
        }

        public bool IsGuarded(object instance)
        {
            if (instance == null)
            {
                return false;
            }

            return _hashes.Contains(instance.GetHashCode());
        }
    }
}
