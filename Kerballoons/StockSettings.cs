using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP.Localization;



namespace KerBalloons
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings
    // HighLogic.CurrentGame.Parameters.CustomParams<KerBSettings>().
    public class KerBSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "KerBalloons"; } }
        public override string DisplaySection { get { return "KerBalloons"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("DebugMode",
            toolTip = "Enable some additional logging")]
        public bool DebugMode = false;

        [GameParameters.CustomParameterUI("Alternate Skin",
            toolTip = "Use alternate skin")]
        public bool altskin = false;

        [GameParameters.CustomParameterUI("Enable Speed Limiter",
                    toolTip = "Limit the top speed of the balloon")]
        public bool speedLimiterEnabled = true;

        [GameParameters.CustomParameterUI("Log data in mod folder",
                    toolTip = "If true, will save to folder KerBalloons/LogData, if false,\n" +
            "will save in the save folder:  saves/<save>/KerBalloonDataLogs")]
        public bool saveInModFolder = false;



        public override void SetDifficultyPreset(GameParameters.Preset preset)
        { }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        { return true; }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        { return true; }

        public override IList ValidValues(MemberInfo member)
        { return null; }
    }

}
