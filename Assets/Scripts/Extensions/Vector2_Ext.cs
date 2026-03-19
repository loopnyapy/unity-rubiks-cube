using UnityEngine;

namespace Vector2Extensions
{
    public static class Vector2Extension
    {
        public static readonly Vector2 upRight = new Vector2(1, 1);
        public static readonly Vector2 upLeft = new Vector2(-1, 1);
        public static readonly Vector2 downLeft = new Vector2(-1, -1);
        public static readonly Vector2 downRight = new Vector2(1, -1);
    }
}
