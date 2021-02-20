using System;
using System.Collections.Generic;

namespace DSPPlugins_ALT
{
    public class Filter<T>
    {
        public string name = "";
        public bool enabled;
        public float value;
        public float value2;
        public float value3;
        public float value4;
        public float value5;
        public float value6;

        public Func<Filter<T>, bool> onGUI;

        //public LINQFilterDelegate<T> LINQFilter;

        public Func<Filter<T>, IEnumerable<T>, IEnumerable<T>> LINQFilter;

    }
}
