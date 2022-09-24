using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KerBalloons
{
	public class KBModuleEnviroSensor : PartModule, IResourceConsumer
	{
		public enum SensorType
		{
			TIME,
			DENS,
			SEXT
		}

		[KSPField]
		public SensorType sensorType;

		[KSPField(guiName = "#KerBalloons_EnviroSensor_Data", guiUnits = "", guiActive = true)] // Data
		public string readoutInfo = "Off"; // Local.EnviroSensor_Data_Off;

		[KSPField(isPersistant = true)]
		public bool sensorActive;

		private List<PartResourceDefinition> consumedResources;

		public List<PartResourceDefinition> GetConsumedResources()
		{
			return consumedResources;
		}

		[KSPEvent(guiName = "#KerBalloons_EnviroSensor_Toggle", guiActive = true)] // Toggle Display
		public void Toggle()
		{
			sensorActive = !sensorActive;
		}

		public override void OnAwake()
		{
			if (consumedResources == null)
			{
				consumedResources = new List<PartResourceDefinition>();
			}
			else
			{
				consumedResources.Clear();
			}
			int i = 0;
			for (int count = base.resHandler.inputResources.Count; i < count; i++)
			{
				consumedResources.Add(PartResourceLibrary.Instance.GetDefinition(base.resHandler.inputResources[i].name));
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			if (base.resHandler.inputResources.Count == 0 && (node.HasValue("resourceName") || node.HasValue("powerConsumption") || base.part.partInfo == null || (Object)base.part.partInfo.partPrefab == (Object)null))
			{
				string text = "ElectricCharge";
				float powerConsumption = 0.0075f;
				node.TryGetValue("resourceName", ref text);
				node.TryGetValue("powerConsumption", ref powerConsumption);
				ModuleResource moduleResource = new ModuleResource();
				moduleResource.name = text;
				moduleResource.title = KSPUtil.PrintModuleName(text);
				moduleResource.id = text.GetHashCode();
				moduleResource.rate = (double)powerConsumption;
				base.resHandler.inputResources.Add(moduleResource);
			}
		}

		public void FixedUpdate()
		{
			if (sensorActive && base.part.started)
			{
				//double num = (double)TimeWarp.fixedDeltaTime;
				if (base.resHandler.UpdateModuleResourceInputs(ref readoutInfo, 1.0, 0.9, true, true))
				{
					if ((Object)UIPartActionController.Instance != (Object)null && UIPartActionController.Instance.ItemListContains(base.part, false))
					{
						switch (sensorType)
						{
						case SensorType.TIME:
							readoutInfo = Planetarium.GetUniversalTime().ToString();
							break;
						case SensorType.DENS:
							readoutInfo = base.vessel.atmDensity.ToString();
							break;
						case SensorType.SEXT:
							readoutInfo = Localizer.Format("#KerBalloons_EnviroSensor_SEXT", base.vessel.latitude, base.vessel.longitude);  // "Lat: " + base.vessel.latitude.ToString() + " Long: " + base.vessel.longitude
							break;
						}
					}
				}
				else if (readoutInfo != "Inactive")  // 
				{
					readoutInfo = "Active";
				}
			}
			else
			{
				readoutInfo = "Off";
			}
		}
	}
}
