using MutaBrains.Core.GUI;
using OpenTK.Mathematics;

namespace MutaBrains.Core.Collisions
{
    class CollisionDetector
    {
        public static bool checkGUIvsPointer(Component componetn, Vector2 mousePosition)
        {
            return componetn.getBoundingBox().Contains(mousePosition.X, mousePosition.Y);
        }
    }
}