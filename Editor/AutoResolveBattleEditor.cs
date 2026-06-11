#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace TJ.Engagement
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(AutoResolveBattleManager))]
    [RequireComponent(typeof(AutoResolveBattleManager))]
    public class AutoResolveBattleEditor : Editor
    {
        public override void OnInspectorGUI() {
            AutoResolveBattleManager autoResolveBattle = (AutoResolveBattleManager)target;

            if (GUILayout.Button("Set Up Armies Test")) {
                ConsoleUtility.ClearConsole();
                autoResolveBattle.SetUpArmiesTest();
            }
            if (GUILayout.Button("Auto Resolve Battle")) {
                ConsoleUtility.ClearConsole();
                autoResolveBattle.AutoResolveThroughEditor();
            }
            if (GUILayout.Button("Run Simulation Loop")) {
                ConsoleUtility.ClearConsole();
                autoResolveBattle.RunSimulationLoop();
            }

            if (DrawDefaultInspector()) {
            }
        }
    }
}
#endif