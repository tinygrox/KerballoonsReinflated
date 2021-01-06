using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ClickThroughFix;
using SpaceTuxUtility;
using B9PartSwitch;
using static KerBalloons.Statics;

namespace KerBalloons
{
    public class ModuleKerBalloon : PartModule
    {
        public GameObject balloonObject;
        public GameObject ropeObject;
        public GameObject capObject;
        public GameObject liftPointObject;
        public GameObject balloonPointObject;


        [KSPField(isPersistant = true)]
        public int balloonSize = 0;

        [KSPField(isPersistant = true)]
        public string recommendedBody;
        [KSPField(isPersistant = true)]
        public float minAtmoPressure;
        [KSPField(isPersistant = true)]
        public float maxAtmoPressure;
        [KSPField(isPersistant = true)]
        public float minScale;
        [KSPField(isPersistant = true)]
        public float maxScale;
        [KSPField(isPersistant = true)]
        public float minLift;
        [KSPField(isPersistant = true)]
        public float maxLift;
        [KSPField(isPersistant = true)]
        public float targetTWR;
        [KSPField(isPersistant = false)]
        public float liftLimit;
        [KSPField(isPersistant = true)]
        public bool speedLimiter;
        [KSPField(isPersistant = true)]
        public float maxSpeed;
        [KSPField(isPersistant = true)]
        public float maxSpeedTolerence;
        [KSPField(isPersistant = true)]
        public float speedAdjustStep;
        [KSPField(isPersistant = true)]
        public float speedAdjustMin;
        [KSPField(isPersistant = true)]
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
        [KSPField(isPersistant = true)]
        public string CFGballoonObject;
        [KSPField(isPersistant = true)]
        public string CFGropeObject;
        [KSPField(isPersistant = true)]
        public string CFGcapObject;
        [KSPField(isPersistant = true)]
        public string CFGliftPointObject;
        [KSPField(isPersistant = true)]
        public string CFGballoonPointObject;



        [KSPField(isPersistant = false)]
        public float scaleInc;

        [KSPField(isPersistant = true)]
        public string bodyName;
        [KSPField(isPersistant = true)]
        public string payload = "Standard";
        [KSPField(isPersistant = false)]
        public float bodyG;

        ModuleB9PartSwitch sizeSwitch;
        ModuleB9PartSwitch planetSwitch;

        public override void OnStart(StartState state)
        {
            Debug.Log("ModuleKerBalloon Loaded");

            if (HighLogic.LoadedSceneIsEditor)
            {
                var switches = part.FindModulesImplementing<ModuleB9PartSwitch>();
                sizeSwitch = switches.Find(s => s.moduleID == "balloonSizeSwitch");
                planetSwitch = switches.Find(s => s.moduleID == "planetSwitch");
                if (sizeSwitch == null)
                    Log.Error("sizeSwitch is null");
                if (planetSwitch == null)
                    Log.Error("planetSwitch is null");
                foreach (var s in planetSwitch.Fields)
                {
                    s.guiActiveEditor = false;
                }
                planetSwitch.Events["ShowSubtypesWindow"].guiActiveEditor = false;
                bodyName = recommendedBody;
                UpdatePersistentData();

            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                balloonObject = getChildGameObject(this.part.gameObject, CFGballoonObject, balloonSize);
                ropeObject = getChildGameObject(this.part.gameObject, CFGropeObject, balloonSize);
                capObject = getChildGameObject(this.part.gameObject, CFGcapObject, balloonSize);
                liftPointObject = getChildGameObject(this.part.gameObject, CFGliftPointObject, balloonSize);
                balloonPointObject = getChildGameObject(this.part.gameObject, CFGballoonPointObject, balloonSize);

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

        [KSPField(isPersistant = true)]
        int lastBalloonSize = 0;
        Rect infoRect = new Rect(0, 0, 400, 400);
        Rect winRect = new Rect(0, 0, 400, 200);
        int winId = WindowHelper.NextWindowId("ModuleKerBalloonConfig");
        int infoId = WindowHelper.NextWindowId("ModuleKerBalloonInfo");
        bool visibleShowInfo = false;
        bool visible = false;
        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Configure Balloon")]
        public void ConfigureBalloon()
        {
            ConfigureWinPos();
        }

        void ConfigureWinPos()
        {
            visible = true;
            winRect.x = Mouse.screenPos.x;
            winRect.y = Mouse.screenPos.y;

            winRect.x = Mathf.Min(Mouse.screenPos.x, Screen.width - winRect.width);
            winRect.y = Mathf.Min(Mouse.screenPos.y, Screen.height - winRect.height);

        }
        void OnGUI()
        {
            if (visibleShowInfo)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<KerBSettings>().altskin)
                    GUI.skin = HighLogic.Skin;
                infoRect = ClickThruBlocker.GUILayoutWindow(infoId, infoRect, InfoBalloonWin, "KerBalloon");
            }
            if (visible && HighLogic.LoadedSceneIsEditor)
            {
                if (!visibleShowInfo && !HighLogic.CurrentGame.Parameters.CustomParams<KerBSettings>().altskin)
                    GUI.skin = HighLogic.Skin;
                winRect = ClickThruBlocker.GUILayoutWindow(winId, winRect, ConfigBalloonWin, "KerBalloon");
            }
        }

        void SetValues(BalloonInfo b)
        {
            Log.Info("SetValues");
            recommendedBody = b.recommendedBody;
            var body = FlightGlobals.Bodies.Find(x => x.bodyName == b.recommendedBody);
            if (body == null)
                Log.Info("Cannot find recommended body");
            bodyG = (float)body.GeeASL;
            Log.Info("recommendedBody: " + b.recommendedBody + ", bodyName: " + body.bodyName + ", bodyG: " + bodyG);
            minAtmoPressure = b.minAtmoPressure;
            maxAtmoPressure = b.maxAtmoPressure;
            minScale = b.minScale;
            maxScale = b.maxScale;
            minLift = b.minLift;
            maxLift = b.maxLift;
            targetTWR = b.targetTWR;
            speedLimiter = b.speedLimiter;
            maxSpeed = b.maxSpeed;
            maxSpeedTolerence = b.maxSpeedTolerence;
            speedAdjustStep = b.speedAdjustStep;
            speedAdjustMin = b.speedAdjustMin;
            speedAdjustMax = b.speedAdjustMax;
            CFGballoonObject = b.CFGballoonObject;
            CFGropeObject = b.CFGropeObject;
            CFGcapObject = b.CFGcapObject;
            CFGliftPointObject = b.CFGliftPointObject;
            CFGballoonPointObject = b.CFGballoonPointObject;

        }


        List<string> availPlanets = new List<string>();
        int selectedPlanet = 0;

        List<string> availPayloads = new List<string>();
        int selectedPayload = 0;
        void ConfigBalloonWin(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            // Planet

            string lastPlanet = "";
            int cnt = 0;
            bool changed = false;
            if (availPlanets.Count == 0)
            {
                foreach (var binfo in Statics.balloonSizes["Size" + balloonSize.ToString()].balloonInfoDict.Values)
                {
                    if (lastPlanet != binfo.recommendedBody)
                    {
                        lastPlanet = binfo.recommendedBody;
                        if (ResearchAndDevelopment.GetTechnologyState(binfo.techRequired) == RDTech.State.Available)
                        {
                            availPlanets.Add(binfo.recommendedBody);
                            if (bodyName == binfo.recommendedBody)
                                selectedPlanet = cnt;
                            cnt++;
                        }
                    }
                }
            }
            if (Utils.DrawRadioListVertical(ref selectedPlanet, availPlanets))
            {
                bodyName = availPlanets[selectedPlanet];
                recommendedBody = bodyName;
                planetSwitch.SwitchSubtype(bodyName);
                availPayloads.Clear();
                changed = true;
                Log.Info("Planet changed, selectedPlanet : "+ selectedPlanet + ", recommendedBody: " + recommendedBody + ", bodyName: " + bodyName);
            }
            GUILayout.EndVertical();

            // Payload

            GUILayout.BeginVertical();
            if (availPayloads.Count == 0)
            {
                string lastPayload = "";
                cnt = 0;
                foreach (var binfo in Statics.balloonSizes["Size" + balloonSize.ToString()].balloonInfoDict.Values)
                {
                    if (binfo.recommendedBody == bodyName && lastPayload != binfo.payload)
                    {
                        lastPayload = binfo.payload;
                        if (ResearchAndDevelopment.GetTechnologyState(binfo.techRequired) == RDTech.State.Available)
                        {
                            availPayloads.Add(binfo.payload);
                            if (payload == binfo.payload)
                                selectedPayload = cnt;
                            cnt++;
                        }
                    }
                }
            }
            if (Utils.DrawRadioListVertical(ref selectedPayload, availPayloads))
            {
                payload = availPayloads[selectedPayload];
                changed = true;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (changed)
            {
                UpdatePersistentData();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
                visible = false;
            GUI.DragWindow();
        }

        void UpdatePersistentData()
        {
            Log.Info("UpdatePersistentData, bodyName: " + bodyName);
            string size = "Size" + balloonSize.ToString();
            if (Statics.balloonSizes.ContainsKey(size))
            {
                var sizeDict = Statics.balloonSizes[size].balloonInfoDict;
                string key = BalloonInfo.MakeKey(size, payload, bodyName);
                if (sizeDict.ContainsKey(key))
                {
                    var binfo = sizeDict[key];
                   // if (binfo.recommendedBody != recommendedBody)
                        SetValues(binfo);
                }
                else
                    Log.Info("balloonSizes, key: " + key + ", not found");
            }
            else
                Log.Info("balloonSizes, size: " + size + ", not found");

        }
        void UpdateBalloonType()
        {
            if (lastBalloonSize != balloonSize)
            {
                lastBalloonSize = balloonSize;
                ConfigureWinPos();
            }
        }


        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor)
                UpdateBalloonType();
            if (HighLogic.LoadedSceneIsFlight)
            {
                //print("hasInflated: " + hasInflated + " | isInflated: " + isInflated + " | isInflating: " + isInflating + " | isDeflating: " + isDeflating + " | hasBurst: " + hasBurst + " | isRepacked: " + isRepacked);
                float currentPressure = (float)FlightGlobals.getStaticPressure(this.part.transform.position);
                if (hasInflated && !hasBurst)
                {
                    if (isInflated)
                    {
                        float lift = BalloonProperties.getLift(this);
                        this.part.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);
                        Vector3 scale = new Vector3(BalloonProperties.getScale(this), BalloonProperties.getScale(this), BalloonProperties.getScale(this));
                        balloonObject.transform.localScale = scale;

                        ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 10);
                        balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 8);

                        balloonObject.transform.position = balloonPointObject.transform.position;

                        if (currentPressure < minAtmoPressure || currentPressure > maxAtmoPressure) hasBurst = true;
                    }
                    else if (isDeflating)
                    {
                        if (scaleInc > 0)
                        {
                            scaleInc -= BalloonProperties.getScale(this) / 100;
                            balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);

                            float progress = scaleInc / BalloonProperties.getScale(this);

                            float lift = BalloonProperties.getLift(this) * progress;
                            this.part.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);

                            ropeObject.transform.localScale = new Vector3(1, 1, progress);
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
                        Events["repackBalloon"].active = true;
                    }
                }
                else if (isInflating && !hasBurst)
                {
                    if (scaleInc < BalloonProperties.getScale(this))
                    {
                        scaleInc += BalloonProperties.getScale(this) / 200;
                        balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);

                        float progress = scaleInc / BalloonProperties.getScale(this);

                        float lift = BalloonProperties.getLift(this) * progress;
                        this.part.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);


                        ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 10);
                        balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 8);

                        ropeObject.transform.localScale = new Vector3(1, 1, progress);
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
                    this.part.Effect("burst");
                    isInflated = false;
                    isInflating = false;
                    isDeflating = false;
                    balloonObject.SetActive(false);
                    ropeObject.SetActive(false);
                    Events["inflateBalloon"].active = false;
                    Events["deflateBalloon"].active = false;
                    Actions["inflateAction"].active = false;
                    Actions["deflateAction"].active = false;
                }
            }

        }


        public Vector3 initialBalloonScale;
        public Vector3 initialBalloonPos;
        public Vector3 initialRopeScale;

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, unfocusedRange = 4, externalToEVAOnly = true, guiName = "Repack Balloon")]
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

            Events["repackBalloon"].active = false;
            Events["inflateBalloon"].active = true;
            Events["deflateBalloon"].active = false;
            Actions["inflateAction"].active = true;
            Actions["deflateAction"].active = false;
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Inflate Balloon")]
        public void inflateBalloon()
        {
            if (!isInflated)
            {
                float currentPressure = (float)FlightGlobals.getStaticPressure(this.part.transform.position);
                if (currentPressure > minAtmoPressure && currentPressure < maxAtmoPressure)
                {
                    Debug.Log("Inflating Balloon!");
                    this.part.Effect("inflate");
                    speedAdjust = 1;
                    isInflating = true;
                    capObject.SetActive(false);
                    Events["inflateBalloon"].active = false;
                    Events["deflateBalloon"].active = true;
                }
                else
                {

                    if (currentPressure <= 0)
                    {
                        ScreenMessages.PostScreenMessage("Cannot inflate balloon in vacuum", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else if (currentPressure < minAtmoPressure)
                    {
                        ScreenMessages.PostScreenMessage("Cannot Inflate: Air pressure too low", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else if (currentPressure > maxAtmoPressure)
                    {
                        ScreenMessages.PostScreenMessage("Cannot Inflate: Air pressure too high", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
            }
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Deflate Balloon")]
        public void deflateBalloon()
        {
            if (isInflated)
            {
                Debug.Log("Deflating Balloon!");
                if (!hasBurst) { this.part.Effect("deflate"); }
                Events["deflateBalloon"].active = false;
                isInflated = false;
                isDeflating = true;
            }
        }

        [KSPAction("Inflate Balloon")]
        public void inflateAction(KSPActionParam param)
        {
            inflateBalloon();
            Actions["inflateAction"].active = false;
        }

        [KSPAction("Deflate Balloon")]
        public void deflateAction(KSPActionParam param)
        {
            deflateBalloon();
            Actions["deflateAction"].active = false;
        }

        static public GameObject getChildGameObject(GameObject fromGameObject, string withName, int balloonSize)
        {
            int cnt = 0;
            Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts)
                if (t.gameObject.name == withName)
                {
                    if (cnt++ == balloonSize)
                    return t.gameObject;
                }
            return null;
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Show Balloon Info")]
        public void ShowInfo()
        {
            visibleShowInfo = !visibleShowInfo;
        }

        Vector2 infoPos;
        void InfoBalloonWin(int id)
        {
            string moreInfoText;
            moreInfoText = "Recommended Body: " + bodyName;
            moreInfoText = moreInfoText + "\nMin pressure: " + minAtmoPressure.ToString() + "kPa";
            moreInfoText = moreInfoText + "\nMax pressure: " + maxAtmoPressure.ToString() + "kPa";
            moreInfoText = moreInfoText + "\nMax lift: " + maxLift.ToString() + "kN";
            moreInfoText = moreInfoText + "\nMax payload " + "(" + bodyName + "): " + (Mathf.Floor((maxLift / bodyG) * 1000) / 1000).ToString() + "t" + " (at " + maxAtmoPressure + "kPa)";
            // + " (" + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t)";
            //moreInfoText = moreInfoText + "\n  At max pressure: " + (Mathf.Floor((maxLift/bodyG)*1000)/1000).ToString() + "t";
            //moreInfoText = moreInfoText + "\n  At min pressure: " + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t";

            if (HighLogic.CurrentGame.Parameters.CustomParams<KerBSettings>().DebugMode)
            {
                moreInfoText += "\nminAtmoPressure: " + minAtmoPressure.ToString("F3") + "\n";
                moreInfoText += "maxAtmoPressure: " + maxAtmoPressure.ToString("F3") + "\n";
                moreInfoText += "minLift: " + minLift.ToString("F3") + "\n";
                moreInfoText += "maxLift: " + maxLift.ToString("F3") + "\n";
                moreInfoText += "minScale: " + minScale.ToString("F3") + "\n";
                moreInfoText += "maxScale: " + maxScale.ToString("F3") + "\n";
                moreInfoText += "targetTWR: " + targetTWR.ToString("F3") + "\n";
                moreInfoText += "liftLimit: " + liftLimit.ToString("F3") + "\n";
                moreInfoText += "speedLimiter: " + speedLimiter.ToString() + "\n";
                moreInfoText += "maxSpeed: " + maxSpeed.ToString("F3") + "\n";
                moreInfoText += "maxSpeedTolerence: " + maxSpeedTolerence.ToString("F3") + "\n";
                moreInfoText += "speedAdjustStep: " + speedAdjustStep.ToString("F3") + "\n";
                moreInfoText += "speedAdjustMin: " + speedAdjustMin.ToString("F3") + "\n";
                moreInfoText += "speedAdjustMax: " + speedAdjustMax.ToString("F3") + "\n";
                moreInfoText += "bodyG: " + bodyG.ToString("F3") + "\n";
            }
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            infoPos = GUILayout.BeginScrollView(infoPos, GUILayout.MaxHeight(360));
            GUILayout.TextArea(moreInfoText);
            GUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
                visibleShowInfo = false;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
        public override string GetInfo()
        {
            //I know about FlightGlobal.Bodies() but for some reason when I use it in this function the game freezes on load
            //Also it can't be put in OnStart() because that isn't called until the part is created
            bodyName = recommendedBody;
#if false
            if (recommendedBody == "Sun") bodyG = 17.1f;
            if (recommendedBody == "Kerbin") bodyG = 9.81f;
            if (recommendedBody == "Mun") bodyG = 1.63f;
            if (recommendedBody == "Minmus") bodyG = 0.491f;
            if (recommendedBody == "Moho") bodyG = 2.70f;
            if (recommendedBody == "Eve") bodyG = 16.7f;
            if (recommendedBody == "Duna") bodyG = 2.94f;
            if (recommendedBody == "Ike") bodyG = 1.10f;
            if (recommendedBody == "Jool") bodyG = 7.85f;
            if (recommendedBody == "Laythe") bodyG = 7.85f;
            if (recommendedBody == "Vall") bodyG = 2.31f;
            if (recommendedBody == "Bop") bodyG = 0.589f;
            if (recommendedBody == "Tylo") bodyG = 7.85f;
            if (recommendedBody == "Gilly") bodyG = 0.049f;
            if (recommendedBody == "Pol") bodyG = 0.373f;
            if (recommendedBody == "Dres") bodyG = 1.13f;
            if (recommendedBody == "Eeloo") bodyG = 1.69f;
#endif
#if false
            string moreInfoText;
            moreInfoText = "Recommended Body: " + bodyName;
            moreInfoText = moreInfoText + "\nMin pressure: " + minAtmoPressure.ToString() + "kPa";
            moreInfoText = moreInfoText + "\nMax pressure: " + maxAtmoPressure.ToString() + "kPa";
            moreInfoText = moreInfoText + "\nMax lift: " + maxLift.ToString() + "kN";
            moreInfoText = moreInfoText + "\nMax payload " + "(" + bodyName + "):\n" + (Mathf.Floor((maxLift / bodyG) * 1000) / 1000).ToString() + "t" + " (at " + maxAtmoPressure + "kPa)";
            // + " (" + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t)";
            //moreInfoText = moreInfoText + "\n  At max pressure: " + (Mathf.Floor((maxLift/bodyG)*1000)/1000).ToString() + "t";
            //moreInfoText = moreInfoText + "\n  At min pressure: " + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t";

            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)
                return moreInfoText;

            return moreInfoText;
#else
            return "See Balloon Info window for specifics";
#endif
        }

    }
}
