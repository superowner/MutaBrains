using System.Collections.Generic;

namespace MutaBrains.Core.ML
{
    public class State
    {
        public static List<State> StatesList = new List<State>();

        public object value;

        public State(object value)
        {
            this.value = value;
        }
    }
}
