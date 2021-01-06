using UnityEngine;

namespace KerBalloons
{
	public class ModuleKerBalloon : PartModule
	{
		[KSPField(isPersistant = false)]
		public string CFGballoonObject;

		[KSPField(isPersistant = false)]
		public string CFGropeObject;

		[KSPField(isPersistant = false)]
		public string CFGcapObject;

		[KSPField(isPersistant = false)]
		public string CFGliftPointObject;

		[KSPField(isPersistant = false)]
		public string CFGballoonPointObject;

		public GameObject balloonObject;

		public GameObject ropeObject;

		public GameObject capObject;

		public GameObject liftPointObject;

		public GameObject balloonPointObject;

		[KSPField(isPersistant = false)]
		public float minAtmoPressure;

		[KSPField(isPersistant = false)]
		public float maxAtmoPressure;

		[KSPField(isPersistant = false)]
		public float minScale;

		[KSPField(isPersistant = false)]
		public float maxScale;

		[KSPField(isPersistant = false)]
		public float minLift;

		[KSPField(isPersistant = false)]
		public float maxLift;

		[KSPField(isPersistant = false)]
		public string recommendedBody;

		[KSPField(isPersistant = false)]
		public float targetTWR;

		[KSPField(isPersistant = false)]
		public float liftLimit;

		[KSPField(isPersistant = false)]
		public bool speedLimiter;

		[KSPField(isPersistant = false)]
		public float maxSpeed;

		[KSPField(isPersistant = false)]
		public float maxSpeedTolerence;

		[KSPField(isPersistant = false)]
		public float speedAdjustStep;

		[KSPField(isPersistant = false)]
		public float speedAdjustMin;

		[KSPField(isPersistant = false)]
		public float speedAdjustMax;

		public float speedAdjust;

		[KSPField(isPersistant = true)]
		public bool isInflating;

		[KSPField(isPersistant = true)]
		public bool hasInflated;

		[KSPField(isPersistant = true)]
		public bool isInflated;

		[KSPField(isPersistant = true)]
		public bool isDeflating;

		[KSPField(isPersistant = true)]
		public bool hasBurst;

		[KSPField(isPersistant = true)]
		public bool isRepacked;

		[KSPField(isPersistant = false)]
		public float scaleInc;

		[KSPField(isPersistant = false)]
		public string bodyName;

		[KSPField(isPersistant = false)]
		public float bodyG;

		public Vector3 initialBalloonScale;

		public Vector3 initialBalloonPos;

		public Vector3 initialRopeScale;

		public override void OnStart(StartState state)
		{
			Debug.Log("ModuleKerBalloon Loaded");
			if (HighLogic.LoadedSceneIsFlight)
			{
				balloonObject = getChildGameObject(base.part.gameObject, CFGballoonObject);
				ropeObject = getChildGameObject(base.part.gameObject, CFGropeObject);
				capObject = getChildGameObject(base.part.gameObject, CFGcapObject);
				liftPointObject = getChildGameObject(base.part.gameObject, CFGliftPointObject);
				balloonPointObject = getChildGameObject(base.part.gameObject, CFGballoonPointObject);
				balloonObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				initialBalloonScale = balloonObject.transform.localScale;
				initialBalloonPos = balloonObject.transform.transform.localPosition;
				initialRopeScale = ropeObject.transform.localScale;
				if (hasInflated && !isInflated)
				{
					balloonObject.SetActive(false);
					ropeObject.SetActive(false);
					capObject.SetActive(false);
				}
				else if (isInflating)
				{
					repackBalloon();
				}
				else if (isDeflating)
				{
					balloonObject.SetActive(false);
					ropeObject.SetActive(false);
					isInflated = false;
					isDeflating = false;
					isRepacked = false;
				}
			}
		}

		public void FixedUpdate()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				float num = (float)FlightGlobals.getStaticPressure(base.part.transform.position);
				if (hasInflated && !hasBurst)
				{
					if (isInflated)
					{
						float lift = BalloonProperties.getLift(this);
						base.part.Rigidbody.AddForceAtPosition(base.vessel.upAxis * (double)lift, liftPointObject.transform.position);
						Vector3 localScale = new Vector3(BalloonProperties.getScale(this), BalloonProperties.getScale(this), BalloonProperties.getScale(this));
						balloonObject.transform.localScale = localScale;
						ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(base.vessel.upAxis, base.vessel.upAxis), BalloonProperties.getLift(this) / 10f);
						balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(base.vessel.upAxis, base.vessel.upAxis), BalloonProperties.getLift(this) / 8f);
						balloonObject.transform.position = balloonPointObject.transform.position;
						if (num < minAtmoPressure || num > maxAtmoPressure)
						{
							hasBurst = true;
						}
					}
					else if (isDeflating)
					{
						if (scaleInc > 0f)
						{
							scaleInc -= BalloonProperties.getScale(this) / 100f;
							balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);
							float num2 = scaleInc / BalloonProperties.getScale(this);
							float num3 = BalloonProperties.getLift(this) * num2;
							base.part.Rigidbody.AddForceAtPosition(base.vessel.upAxis * (double)num3, liftPointObject.transform.position);
							ropeObject.transform.localScale = new Vector3(1f, 1f, num2);
							balloonObject.transform.position = balloonPointObject.transform.position;
						}
						else
						{
							balloonObject.SetActive(false);
							ropeObject.SetActive(false);
							isInflated = false;
							isDeflating = false;
							isRepacked = false;
						}
					}
					else if (!isInflated && !isInflating && !isDeflating && !isRepacked)
					{
						base.Events["repackBalloon"].active = true;
					}
				}
				else if (isInflating && !hasBurst)
				{
					if (scaleInc < BalloonProperties.getScale(this))
					{
						scaleInc += BalloonProperties.getScale(this) / 200f;
						balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);
						float num4 = scaleInc / BalloonProperties.getScale(this);
						float num5 = BalloonProperties.getLift(this) * num4;
						base.part.Rigidbody.AddForceAtPosition(base.vessel.upAxis * (double)num5, liftPointObject.transform.position);
						ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(base.vessel.upAxis, base.vessel.upAxis), BalloonProperties.getLift(this) / 10f);
						balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(base.vessel.upAxis, base.vessel.upAxis), BalloonProperties.getLift(this) / 8f);
						ropeObject.transform.localScale = new Vector3(1f, 1f, num4);
						balloonObject.transform.position = balloonPointObject.transform.position;
					}
					else
					{
						hasInflated = true;
						isInflated = true;
						isInflating = false;
					}
				}
				else if (hasBurst && (isInflated || isInflating || isDeflating))
				{
					base.part.Effect("burst", -1);
					isInflated = false;
					isInflating = false;
					isDeflating = false;
					balloonObject.SetActive(false);
					ropeObject.SetActive(false);
					base.Events["inflateBalloon"].active = false;
					base.Events["deflateBalloon"].active = false;
					base.Actions["inflateAction"].active = false;
					base.Actions["deflateAction"].active = false;
				}
			}
		}

		[KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, unfocusedRange = 4f, externalToEVAOnly = true, guiName = "Repack Balloon")]
		public void repackBalloon()
		{
			isInflated = false;
			isInflating = false;
			isDeflating = false;
			hasBurst = false;
			hasInflated = false;
			isRepacked = true;
			balloonObject.transform.localScale = initialBalloonScale;
			balloonObject.transform.localPosition = initialBalloonPos;
			ropeObject.transform.localScale = initialBalloonScale;
			capObject.SetActive(true);
			balloonObject.SetActive(true);
			ropeObject.SetActive(true);
			base.Events["repackBalloon"].active = false;
			base.Events["inflateBalloon"].active = true;
			base.Events["deflateBalloon"].active = false;
			base.Actions["inflateAction"].active = true;
			base.Actions["deflateAction"].active = false;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Inflate Balloon")]
		public void inflateBalloon()
		{
			if (!isInflated)
			{
				float num = (float)FlightGlobals.getStaticPressure(base.part.transform.position);
				if (num > minAtmoPressure && num < maxAtmoPressure)
				{
					Debug.Log("Inflating Balloon!");
					base.part.Effect("inflate", -1);
					speedAdjust = 1f;
					isInflating = true;
					capObject.SetActive(false);
					base.Events["inflateBalloon"].active = false;
					base.Events["deflateBalloon"].active = true;
				}
				else if (num <= 0f)
				{
					ScreenMessages.PostScreenMessage("Cannot inflate balloon in vacuum", 3f, ScreenMessageStyle.UPPER_CENTER);
				}
				else if (num < minAtmoPressure)
				{
					ScreenMessages.PostScreenMessage("Cannot Inflate: Air pressure too low", 3f, ScreenMessageStyle.UPPER_CENTER);
				}
				else if (num > maxAtmoPressure)
				{
					ScreenMessages.PostScreenMessage("Cannot Inflate: Air pressure too high", 3f, ScreenMessageStyle.UPPER_CENTER);
				}
			}
		}

		[KSPEvent(active = false, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Deflate Balloon")]
		public void deflateBalloon()
		{
			if (isInflated)
			{
				Debug.Log("Deflating Balloon!");
				if (!hasBurst)
				{
					base.part.Effect("deflate", -1);
				}
				base.Events["deflateBalloon"].active = false;
				isInflated = false;
				isDeflating = true;
			}
		}

		[KSPAction("Inflate Balloon")]
		public void inflateAction(KSPActionParam param)
		{
			inflateBalloon();
			base.Actions["inflateAction"].active = false;
		}

		[KSPAction("Deflate Balloon")]
		public void deflateAction(KSPActionParam param)
		{
			deflateBalloon();
			base.Actions["deflateAction"].active = false;
		}

		public static GameObject getChildGameObject(GameObject fromGameObject, string withName)
		{
			Transform[] componentsInChildren = ((Component)fromGameObject.transform).GetComponentsInChildren<Transform>();
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				if (transform.gameObject.name == withName)
				{
					return transform.gameObject;
				}
			}
			return null;
		}

		public override string GetInfo()
		{
			bodyName = recommendedBody;
			if (recommendedBody == "Sun")
			{
				bodyG = 17.1f;
			}
			if (recommendedBody == "Kerbin")
			{
				bodyG = 9.81f;
			}
			if (recommendedBody == "Mun")
			{
				bodyG = 1.63f;
			}
			if (recommendedBody == "Minmus")
			{
				bodyG = 0.491f;
			}
			if (recommendedBody == "Moho")
			{
				bodyG = 2.7f;
			}
			if (recommendedBody == "Eve")
			{
				bodyG = 16.7f;
			}
			if (recommendedBody == "Duna")
			{
				bodyG = 2.94f;
			}
			if (recommendedBody == "Ike")
			{
				bodyG = 1.1f;
			}
			if (recommendedBody == "Jool")
			{
				bodyG = 7.85f;
			}
			if (recommendedBody == "Laythe")
			{
				bodyG = 7.85f;
			}
			if (recommendedBody == "Vall")
			{
				bodyG = 2.31f;
			}
			if (recommendedBody == "Bop")
			{
				bodyG = 0.589f;
			}
			if (recommendedBody == "Tylo")
			{
				bodyG = 7.85f;
			}
			if (recommendedBody == "Gilly")
			{
				bodyG = 0.049f;
			}
			if (recommendedBody == "Pol")
			{
				bodyG = 0.373f;
			}
			if (recommendedBody == "Dres")
			{
				bodyG = 1.13f;
			}
			if (recommendedBody == "Eeloo")
			{
				bodyG = 1.69f;
			}
			string str = "Recommended Body: " + bodyName;
			str = str + "\nMin pressure: " + minAtmoPressure.ToString() + "kPa";
			str = str + "\nMax pressure: " + maxAtmoPressure.ToString() + "kPa";
			str = str + "\nMax lift: " + maxLift.ToString() + "kN";
			return str + "\nMax payload (" + bodyName + "):\n" + (Mathf.Floor(maxLift / bodyG * 1000f) / 1000f).ToString() + "t (at " + maxAtmoPressure + "kPa)";
		}
	}
}
