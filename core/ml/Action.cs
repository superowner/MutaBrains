using System.Collections.Generic;

namespace MutaBrains.Core.ML
{
    public class Action
    {
        public static List<Action> ActionsList = new List<Action>();

        public object value;

        public Action(object value)
        {
            this.value = value;
        }
    }
}
