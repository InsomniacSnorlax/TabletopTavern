#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace TJ
{
	[ExecuteInEditMode]
	[CustomEditor(typeof(GreyCompanyBattlefield))]
	[RequireComponent(typeof(GreyCompanyBattlefield))]
	public class BattlefieldEditor : Editor
	{
		public override void OnInspectorGUI() {
			GreyCompanyBattlefield greyCompanyBattlefield = (GreyCompanyBattlefield)target;

			if (DrawDefaultInspector()) {
			}

			if (GUILayout.Button("Generate Battlefield")) {
				ConsoleUtility.ClearConsole();
				greyCompanyBattlefield.GenerateBattlefield();
			}
			if (GUILayout.Button("Load Battlefield Parameters")) {
				greyCompanyBattlefield.LoadBattlefieldParameters();
			}
			if (GUILayout.Button("Save Battlefield Parameters")) {
				greyCompanyBattlefield.SaveBattlefieldParameters();
			}
		}
	}
}
#endif