using System;
using UnityEngine;

namespace RBush
{
	public readonly struct Envelope
	{
		public float MinX { get; }
		public float MinY { get; }
		public float MaxX { get; }
		public float MaxY { get; }

		public float Area => Math.Max(this.MaxX - this.MinX, 0) * Math.Max(this.MaxY - this.MinY, 0);
		public float Margin => Math.Max(this.MaxX - this.MinX, 0) + Math.Max(this.MaxY - this.MinY, 0);

		public Envelope(float minX, float minY, float maxX, float maxY)
		{
			this.MinX = minX;
			this.MinY = minY;
			this.MaxX = maxX;
			this.MaxY = maxY;
		}
        public Envelope(Vector2 p)
        {
            this.MinX = p.x;
            this.MinY = p.y;
            this.MaxX = p.x;
            this.MaxY = p.y;
        }
        public Envelope Extend(in Envelope other) =>
			new Envelope(
				minX: Math.Min(this.MinX, other.MinX),
				minY: Math.Min(this.MinY, other.MinY),
				maxX: Math.Max(this.MaxX, other.MaxX),
				maxY: Math.Max(this.MaxY, other.MaxY));

		public Envelope Clone()
		{
			return new Envelope(this.MinX, this.MinY, this.MaxX, this.MaxY);
		}

		public Envelope Intersection(in Envelope other) =>
			new Envelope(
				minX: Math.Max(this.MinX, other.MinX),
				minY: Math.Max(this.MinY, other.MinY),
				maxX: Math.Min(this.MaxX, other.MaxX),
				maxY: Math.Min(this.MaxY, other.MaxY)
			);

		public Envelope Enlargement(in Envelope other)
		{
			var clone = this.Clone();
			clone.Extend(other);
			return clone;
		}

		public bool Contains(in Envelope other)
		{
			return
				this.MinX <= other.MinX &&
				this.MinY <= other.MinY &&
				this.MaxX >= other.MaxX &&
				this.MaxY >= other.MaxY;
		}

		public bool Intersects(in Envelope other)
		{
			return
				this.MinX <= other.MaxX &&
				this.MinY <= other.MaxY &&
				this.MaxX >= other.MinX &&
				this.MaxY >= other.MinY;
		}

		public static Envelope InfiniteBounds { get; } =
			new Envelope(
				minX: float.NegativeInfinity,
				minY: float.NegativeInfinity,
				maxX: float.PositiveInfinity,
				maxY: float.PositiveInfinity);

		public static Envelope EmptyBounds { get; } =
			new Envelope(
				minX: float.PositiveInfinity,
				minY: float.PositiveInfinity,
				maxX: float.NegativeInfinity,
				maxY: float.NegativeInfinity);
        public override string ToString()
        {
            return string.Format("[MinX:{0}, MinY:{1}, MaxX:{2}, MaxY:{3}]", MinX, MinY,MaxX,MaxY);
        }
    }
}
