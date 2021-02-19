using System;
using System.Collections.Generic;

namespace DSPPlugins_ALT
{
    public class Filter<T>
    {
        public bool enabled;
        public float value;

        public Func<Filter<T>, bool> onGUI;

        //public LINQFilterDelegate<T> LINQFilter;

        public Func<Filter<T>, IEnumerable<T>, IEnumerable<T>> LINQFilter;

    }
}
