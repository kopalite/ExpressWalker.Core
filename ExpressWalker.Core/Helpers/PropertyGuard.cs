using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace ExpressWalker
{
    public class PropertyGuard 
    {
        private List<int> _path;

        public PropertyGuard()
        {
            _path = new List<int>();
        }

        private PropertyGuard(IEnumerable<int> path)
        {
            _path = new List<int>(path);
        }

        public void Add(Type declaringType, string propertyName)
        {
            var hash = GetHash(declaringType.FullName, propertyName);
            
            _path.Add(hash);
        }

        public bool IsRepeating(Type declaringType, string propertyName)
        {
            var hash = GetHash(declaringType.FullName, propertyName);

            return IsRepeating(hash);
        }

        private bool IsRepeating(int hash)
        {
            var count = _path.Count;

            if (count <= 1)
            {
                return false;
            }

            var sliceLength = 1;

            //Trying to detect if there is same repeated sequence (slice) of elements that starts with the 'hash' value.

            while (sliceLength <= count / 2)
            {
                var currentIndex = count - sliceLength;

                var doubledIndex = count - (sliceLength * 2);

                if (hash == _path[currentIndex] && _path[currentIndex] == _path[doubledIndex])
                {
                    var currentSequence = _path.Skip(currentIndex).Take(sliceLength);

                    var doubledSequence = _path.Skip(doubledIndex).Take(sliceLength);

                    if (currentSequence.SequenceEqual(doubledSequence))
                    {
                        return true;
                    }
                }

                sliceLength++;
            }

            return false;
        }

        public PropertyGuard Copy()
        {
            return new PropertyGuard(_path);
        }

        private int GetHash(string declaringType, string propertyName)
        {
            return string.Format("{0}|{1}", declaringType, propertyName).GetHashCode();
        }
    }
}
