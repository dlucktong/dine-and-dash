
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class LineComparer : IEqualityComparer<Line>
    {
        public bool Equals(Line l1, Line l2)
        {
            return (Mathf.Approximately(l1.Start,l2.Start) && Mathf.Approximately(l1.End, l2.End) || 
                    Mathf.Approximately(l1.Start,l2.End) && Mathf.Approximately(l1.End, l2.Start));
        }

        public int GetHashCode(Line line)
        {
            return line.Start.GetHashCode() * line.End.GetHashCode() * line.Center.GetHashCode() *
                   line.Type.GetHashCode();
        }
    }
}