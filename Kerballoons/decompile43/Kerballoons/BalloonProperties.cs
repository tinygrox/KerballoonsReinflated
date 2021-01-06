using System.Collections.Generic;
using UnityEngine;

namespace KerBalloons
{
	public class BalloonProperties
	{
		public static float getLift(ModuleKerBalloon thisBalloon)
		{
			float num = (float)FlightGlobals.getStaticPressure(thisBalloon.part.transform.position);
			float num2 = (thisBalloon.minLift - thisBalloon.maxLift) / Mathf.Pow(thisBalloon.maxAtmoPressure, 2f);
			float num3 = Mathf.Pow(num - thisBalloon.maxAtmoPressure - thisBalloon.minAtmoPressure, 2f);
			float maxLift = thisBalloon.maxLift;
			float num4 = num2 * num3 + maxLift;
			float max = num4;
			float num5 = thisBalloon.vessel.GetTotalMass() * (float)FlightGlobals.getGeeForceAtPosition(thisBalloon.transform.position).magnitude / num4;
			num4 *= num5 * thisBalloon.targetTWR;
			if (thisBalloon.speedLimiter)
			{
				if (thisBalloon.vessel.verticalSpeed < (double)(thisBalloon.maxSpeed * (1f - thisBalloon.maxSpeedTolerence)))
				{
					thisBalloon.speedAdjust += thisBalloon.speedAdjustStep * (thisBalloon.maxSpeed - (float)thisBalloon.vessel.verticalSpeed);
				}
				else if (thisBalloon.vessel.verticalSpeed > (double)(thisBalloon.maxSpeed * (1f + thisBalloon.maxSpeedTolerence)))
				{
					thisBalloon.speedAdjust -= thisBalloon.speedAdjustStep * ((float)thisBalloon.vessel.verticalSpeed - thisBalloon.maxSpeed);
				}
				thisBalloon.speedAdjust = Mathf.Clamp(thisBalloon.speedAdjust, thisBalloon.speedAdjustMin, thisBalloon.speedAdjustMax);
				num4 *= thisBalloon.speedAdjust;
			}
			if (thisBalloon.isInflated || thisBalloon.isInflating)
			{
				num4 /= (float)getInflatedBalloons(thisBalloon.vessel).Count;
			}
			return Mathf.Clamp(num4, 0f, max);
		}

		public static float getScale(ModuleKerBalloon thisBalloon)
		{
			float num = (float)FlightGlobals.getStaticPressure(thisBalloon.part.transform.position);
			float num2 = (thisBalloon.maxScale - thisBalloon.minScale) / Mathf.Pow(thisBalloon.maxAtmoPressure, 2f);
			float num3 = Mathf.Pow(num - thisBalloon.maxAtmoPressure - thisBalloon.minAtmoPressure, 2f);
			float minScale = thisBalloon.minScale;
			return num2 * num3 + minScale;
		}

		public static List<ModuleKerBalloon> getInflatedBalloons(Vessel vessel)
		{
			List<ModuleKerBalloon> list = new List<ModuleKerBalloon>();
			foreach (Part part in vessel.parts)
			{
				if ((bool)((Component)part).GetComponent<ModuleKerBalloon>())
				{
					ModuleKerBalloon component = ((Component)part).GetComponent<ModuleKerBalloon>();
					if ((component.isInflated || component.isInflating) && !component.hasBurst)
					{
						list.Add(component);
					}
				}
			}
			return list;
		}
	}
}
