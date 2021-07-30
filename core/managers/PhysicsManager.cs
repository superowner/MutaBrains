using System;
using System.Collections.Generic;
using System.Text;
using BepuPhysics;
using BepuUtilities.Memory;
using MutaBrains.Core.Physics;

namespace MutaBrains.Core.Managers
{
    static class PhysicsManager
    {
        public static Simulation simulation;
        public static BufferPool bufferPool;

        public static void Initialize()
        {
            bufferPool = new BufferPool();
            simulation = Simulation.Create(
                bufferPool,
                new NarrowPhaseCallback(),
                new PoseIntegratorCallback(
                    new System.Numerics.Vector3(0, -7, 0)
                ),
                new PositionFirstTimestepper()
            );
        }
    }
}
