using System.Collections.Generic;
using MutaBrains.Core.GUI;

namespace MutaBrains.Core.Managers
{
    static class GUIManager
    {
        private static List<Component> components = new List<Component>();
        public static void Initialize()
        {
            TextDrawer.Initialize();
        }
    }
}
