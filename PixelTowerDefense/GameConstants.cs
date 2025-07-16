namespace PixelTowerDefense
{
    /// <summary>
    /// Central repository for gameplay tuning values.
    /// </summary>
    internal static class GameConstants
    {
        /// <summary>Width of an enemy in pixels.</summary>
        public const int EnemyWidth = 1;

        /// <summary>Height of an enemy in pixels.</summary>
        public const int EnemyHeight = 3;

        /// <summary>Vertical position of the arena floor.</summary>
        public const int FloorY = 400;

        /// <summary>Left bound of the arena.</summary>
        public const int ArenaLeft = 0;

        /// <summary>Right bound of the arena.</summary>
        public const int ArenaRight = 100;

        /// <summary>Top bound of the arena.</summary>
        public const int ArenaTop = 100;

        /// <summary>Acceleration due to gravity.</summary>
        public const float Gravity = 98.0f;

        /// <summary>Walking speed of enemies.</summary>
        public const float WanderSpeed = 10f;

        /// <summary>Minimum time between direction changes while walking.</summary>
        public const float WanderTimeMin = 1f;

        /// <summary>Maximum time between direction changes while walking.</summary>
        public const float WanderTimeMax = 3f;

        /// <summary>Minimum wander time when recovering from stun.</summary>
        public const float StunWanderTimeMin = 0.5f;

        /// <summary>Maximum wander time when recovering from stun.</summary>
        public const float StunWanderTimeMax = 2.5f;

        /// <summary>Fall distance that causes an enemy to explode.</summary>
        public const float FallExplodeThreshold = 24f;

        /// <summary>Fall distance that stuns an enemy.</summary>
        public const float FallStunThreshold = 7f;

        /// <summary>Duration of stun state in seconds.</summary>
        public const float StunTime = 1.5f;

        /// <summary>Number of enemies spawned at startup.</summary>
        public const int InitialEnemyCount = 10;

        /// <summary>Distance from arena edges that enemies spawn.</summary>
        public const int SpawnEdgeOffset = 10;

        /// <summary>Preferred window width.</summary>
        public const int WindowWidth = 1280;

        /// <summary>Preferred window height.</summary>
        public const int WindowHeight = 720;

        /// <summary>Base camera movement speed in pixels per second.</summary>
        public const float CameraSpeed = 400f;

        /// <summary>Minimum camera zoom level.</summary>
        public const float MinZoom = 0.5f;

        /// <summary>Maximum camera zoom level.</summary>
        public const float MaxZoom = 8f;

        /// <summary>Starting camera zoom level.</summary>
        public const float DefaultZoom = 1f;

        /// <summary>Zoom multiplier when pressing plus.</summary>
        public const float ZoomInFactor = 2f;

        /// <summary>Zoom multiplier when pressing minus.</summary>
        public const float ZoomOutFactor = 0.5f;

        /// <summary>Distance the mouse must be from a body part to grab it.</summary>
        public const float GrabDistance = 2f;

        /// <summary>Offset multiplier used when dragging body parts.</summary>
        public const float DragPartOffset = 1.5f;

        /// <summary>Spring constant used while dragging enemies.</summary>
        public const float DragSpring = 16f;

        /// <summary>Damping applied while dragging enemies.</summary>
        public const float DragDamping = 8f;

        /// <summary>Factor applied to launch velocity to set angular velocity.</summary>
        public const float LaunchAngularFactor = 0.05f;

        /// <summary>Small value used to avoid division by zero when dragging.</summary>
        public const float DragEpsilon = 0.001f;

        /// <summary>Horizontal bounciness when hitting arena walls.</summary>
        public const float WallBounce = -0.6f;

        /// <summary>Damping applied to rotation after bouncing off walls.</summary>
        public const float WallAngularDamping = 0.7f;

        /// <summary>Bounciness applied to pixels.</summary>
        public const float PixelBounce = -0.5f;
    }
}
