using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExpressWalker.Core.Cloners
{
    internal class ArrayListClonner : ClonerBase
    {
        public ArrayListClonner() : base()
        {
            
        }

        public override object Clone(object arrayList)
        {
            throw new Exception("ArrayList is not supported for cloning. Cannot predict the type of items inside.");
        }
    }
}
