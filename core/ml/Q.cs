namespace MutaBrains.Core.ML
{
    public class Q
    {
        public Action action;
        public State state;
        public double value = 0.0;

        public Q(Action action, State state)
        {
            this.action = action;
            this.state = state;
        }
    }
}
