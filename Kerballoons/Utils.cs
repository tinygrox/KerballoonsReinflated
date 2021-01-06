using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerBalloons
{
    internal class Utils
    {
        //********************************************************************************
        //
        // The following code to draw the radio list is copied from Kerbal Alarm Clock, 
        // and is under MIT license
        internal static bool DrawRadioListVertical(ref int Selected, List<string> Choices)
        {
            return DrawRadioList(false, ref Selected, Choices);
        }
        // internal bool DrawRadioList(ref int Selected, params string[] Choices)
        // {
        //     return DrawRadioList(true, ref Selected, Choices);
        // }
        internal static bool DrawRadioList(bool Horizontal, ref int Selected, List<string> Choices)
        {
            int InitialChoice = Selected;

            if (Horizontal)
                GUILayout.BeginHorizontal();
            else
                GUILayout.BeginVertical();

            for (int intChoice = 0; intChoice < Choices.Count; intChoice++)
            {
                //checkbox
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle((intChoice == Selected), Choices[intChoice]))
                    Selected = intChoice;
                GUILayout.EndHorizontal();
            }
            if (Horizontal)
                GUILayout.EndHorizontal();
            else
                GUILayout.EndVertical();

            if (InitialChoice != Selected)
                Statics.Log.Info(string.Format("Radio List Changed:{0} to {1}", InitialChoice, Selected));


            return !(InitialChoice == Selected);
        }

        //
        //
        //********************************************************************************

    }
}
