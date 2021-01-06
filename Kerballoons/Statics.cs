using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using KSP_Log;

namespace KerBalloons
{
    public class BalloonInfo
    {
        public string payload;
        public string size;
        public string recommendedBody;
        public string techRequired;
        public float minAtmoPressure;
        public float maxAtmoPressure;
        public float minScale;
        public float maxScale;
        public float minLift;
        public float maxLift;
        public float targetTWR;
        public bool speedLimiter;
        public float maxSpeed;
        public float maxSpeedTolerence;
        public float speedAdjustStep;
        public float speedAdjustMin;
        public float speedAdjustMax;
        public string CFGballoonObject;
        public string CFGropeObject;
        public string CFGcapObject;
        public string CFGliftPointObject;
        public string CFGballoonPointObject;

        public string Key { get { return size+payload + recommendedBody; } }

        public static string MakeKey(string size, string payload, string planet)
        {
            return size +":"+ payload +":"+ planet;
        }
        public static BalloonInfo FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            Statics.Log.Info("csvLine: " + csvLine);
            BalloonInfo info = new BalloonInfo();
            info.payload = values[0];
            info.size = values[1];
            info.recommendedBody = values[2];
            info.techRequired = values[3];
            info.minAtmoPressure = Statics.SafeParse(values[4], 0f);
            info.maxAtmoPressure = Statics.SafeParse(values[5], 0f);
            info.minScale = Statics.SafeParse(values[6], 0f);
            info.maxScale = Statics.SafeParse(values[7], 0f);
            info.minLift = Statics.SafeParse(values[8], 0f);
            info.maxLift = Statics.SafeParse(values[9], 0f);
            info.targetTWR = Statics.SafeParse(values[10], 0f);
            info.speedLimiter = Statics.SafeParse(values[11], false);
            info.maxSpeed = Statics.SafeParse(values[12], 0f);
            info.maxSpeedTolerence = Statics.SafeParse(values[13], 0f);
            info.speedAdjustStep = Statics.SafeParse(values[14], 0f);
            info.speedAdjustMin = Statics.SafeParse(values[15], 0f);
            info.speedAdjustMax = Statics.SafeParse(values[16], 0f);
            info.CFGballoonObject = values[17];
            info.CFGropeObject = values[18];
            info.CFGcapObject = values[19];
            info.CFGliftPointObject = values[20];
            info.CFGballoonPointObject = values[21];

            return info;
        }
    }

    public class BalloonSize
    {
        string size;
        public  Dictionary<string, BalloonInfo> balloonInfoDict;

        public BalloonSize(string size)
        {
            this.size = size;
            balloonInfoDict = new Dictionary<string, BalloonInfo>();
        }
    }


    public static class Statics
    {
        public static Dictionary<string, BalloonSize> balloonSizes = new Dictionary<string, BalloonSize>();
        public static Log Log = new Log("KerBalloon", Log.LEVEL.INFO);
        public static float SafeParse(string value, float oldvalue)
        {
            try { return float.Parse(value); }
            catch { return oldvalue; }
        }
        public static bool SafeParse(string value, bool oldvalue)
        {
            try { return bool.Parse(value); }
            catch { return oldvalue; }
        }

        public static void LoadBalloonInfo()
        {
            string filename = KSPUtil.ApplicationRootPath + "/GameData/KerBalloons/PluginData/balloonSpecs.csv";
            filename = filename.Replace('/', Path.DirectorySeparatorChar);
            var binfolist = File.ReadAllLines(filename)
                                           .Skip(2)
                                           .Select(v => BalloonInfo.FromCsv(v))
                                           .ToList();
            Log.Info("binfolist.Count: " + binfolist.Count());
            foreach (var b in binfolist)
            {
                if (!balloonSizes.ContainsKey(b.size))
                    balloonSizes.Add(b.size, new BalloonSize(b.size));
                balloonSizes[b.size].balloonInfoDict.Add(BalloonInfo.MakeKey(b.size, b.payload, b.recommendedBody), b);
            }
            Log.Info("balloonSizes.Count: " + balloonSizes.Count());
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Init : MonoBehaviour
    {
        static bool initted = false;
        void Start()
        {
            if (!initted)
            {
                initted = true;
                Statics.LoadBalloonInfo();
            }
        }
    }
}
