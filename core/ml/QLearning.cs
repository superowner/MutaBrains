using System;
using System.Collections.Generic;

namespace MutaBrains.Core.ML
{
    public class QLearning
    {
        public static int Episode { get; private set; }
        private static List<Q> QList = new List<Q>();
        private static Q preQ;
        private static int episodes;
        public static void Initialize(int episodes = 100)
        {
            QLearning.episodes = episodes;
            Episode = 0;

            Action.ActionsList.Add(new Action("up"));
            Action.ActionsList.Add(new Action("right"));
            Action.ActionsList.Add(new Action("down"));
            Action.ActionsList.Add(new Action("left"));

            State.StatesList.Add(new State("x:0 y:0"));
            State.StatesList.Add(new State("x:0 y:1"));
            State.StatesList.Add(new State("x:0 y:2"));
            State.StatesList.Add(new State("x:1 y:0"));
            State.StatesList.Add(new State("x:1 y:1"));
            State.StatesList.Add(new State("x:1 y:2"));
            State.StatesList.Add(new State("x:2 y:0"));
            State.StatesList.Add(new State("x:2 y:1"));
            State.StatesList.Add(new State("x:2 y:2"));

            foreach (Action a in Action.ActionsList) {
                foreach(State s in State.StatesList) {
                    QList.Add(new Q(a, s));
                }
            }
        }
    }
}