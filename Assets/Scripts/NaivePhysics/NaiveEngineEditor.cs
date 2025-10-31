using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NaivePhysics
{
    [CustomEditor(typeof(NaiveEngine), true)]
    public class NaiveEngineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // collisions
            NaiveEngine ne = target as NaiveEngine;
            List<Collision> collisions = ne.GetCollisionPairs_SweepAndPrune(out _);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Collision Pairs", EditorStyles.boldLabel);
            foreach (Collision collision in collisions)
            {
                EditorGUILayout.LabelField(collision.A.name, collision.B.name);
            }
            GUILayout.EndVertical();
        }
    }
}