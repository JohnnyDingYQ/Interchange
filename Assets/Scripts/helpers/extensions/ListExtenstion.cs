using System.Collections.Generic;

namespace ListExtensions
{
    public static class MyExtension
    {
        public static Intersection GetIntersection(this List<Node> nodes)
        {
            Intersection intersection = null;
            foreach (Node n in nodes)
                if (n.Intersection != null)
                {
                    intersection = n.Intersection;
                    break;
                }
            return intersection;
        }
    }
}