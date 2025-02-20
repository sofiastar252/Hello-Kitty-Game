using System;

namespace FlappyBird.Web.Models
{
	public class KittyModel
	{
		public int DistanceFromGround { get; private set; } = 100;
		public int JumpStrength { get; private set; } = 50;

		public bool IsOnGround()
		{
			return DistanceFromGround <= 0;
		}

		public void Fall(int gravity)
		{
			DistanceFromGround -= Math.Min(gravity, DistanceFromGround);
		}

		public void Jump()
		{
			if (DistanceFromGround <= 530)
				DistanceFromGround += JumpStrength;
		}
	}
}
