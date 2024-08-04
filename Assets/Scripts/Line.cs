using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class Line 
    {
        public float Start
        {
            get => start;
            set => start = value;
        }

        public float End
        {
            get => end;
            set => end = value;
        }

        public char Dir 
        {
            get => dir;
            set => dir = value;
        }

        public float Center
        {
            get => center;
            set => center = value;
        }

        public char Type
        {
            get => type;
            set => type = value;
        }

        private float start;
        private float end;
        private float center;
        private char dir;
        private char type;
        
        public Line(float start, float end, float center, char type, char dir)
        {
            this.start = start;
            this.end = end;
            this.center = center;
            this.type = type;
            this.dir = dir;
            
        }

        public Line(string linestring)
        {
            string[] split = linestring.Split(",");
            start = float.Parse(split[0]);
            end = float.Parse(split[1]);
            center = float.Parse(split[2]);
            type = split[3][0];
            dir = split[4][0];
        }

        public override string ToString()
        {
            return start + "," + end + "," + center + "," + type + "," + dir;
        }

        public float[] AsInterval()
        {
            return new float[]{start, end};
        }

        public Vector3 StartPos()
        {
            return type == 'V' ? new Vector3(Center, 100, Start) : new Vector3(Start, 100, Center);
            
        }

        public Vector3 EndPos()
        {
            return type == 'V' ? new Vector3(Center, 100, End) : new Vector3(End, 100, Center);
        }

        public Vector2 RandomPoint()
        {
            if (dir == 'N')
            {
                return new Vector2(center, Random.Range(start, end-10));
            }

            if (dir == 'S')
            {
                return new Vector2(center, Random.Range(end+10, start));
            }

            if (dir == 'E')
            {
                return new Vector2(Random.Range(start, end-10), center);
            }

            return new Vector2(Random.Range(end+10, start), center);
        }
        
    }
}