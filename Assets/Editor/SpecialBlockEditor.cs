#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpecialBlock))]
public class SpecialBlockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpecialBlock block = (SpecialBlock)target;

        // version 값을 입력받고 해당하는 값만 인스펙터에 표시
        block.version = EditorGUILayout.IntField("Version", block.version);

        // version이 1일 경우 Movement Settings 표시
        if (block.version == 1)
        {
            block.pos1 = EditorGUILayout.Vector3Field("Position 1", block.pos1);
            block.pos2 = EditorGUILayout.Vector3Field("Position 2", block.pos2);
            block.speed = EditorGUILayout.FloatField("Speed", block.speed);
        }
        // version이 2일 경우 Scale Settings 표시
        else if (block.version == 2)
        {
            block.changeX = EditorGUILayout.Toggle("Change X", block.changeX);
            block.changeZ = EditorGUILayout.Toggle("Change Z", block.changeZ);
            block.changeY = EditorGUILayout.Toggle("Change Y", block.changeY);
            block.scaleStartDelay = EditorGUILayout.FloatField("Scale Start DelayY", block.scaleStartDelay);
            block.scaleDuration = EditorGUILayout.FloatField("Scale Duration", block.scaleDuration);
            block.minScale = EditorGUILayout.FloatField("Min Scale", block.minScale);
        }
        // version이 3일 경우 Visible Settings 표시
        else if (block.version == 3)
        {
            block.initialDisappearDelay = EditorGUILayout.FloatField("Initial Disappear Delay", block.initialDisappearDelay);
            block.disappearTime = EditorGUILayout.FloatField("Disappear Time", block.disappearTime);
            block.reappearTime = EditorGUILayout.FloatField("Reappear Time", block.reappearTime);
        }
        else if (block.version == 4)
        {
            block.applyForce = EditorGUILayout.Toggle("Apply Force", block.applyForce);
            block.force = EditorGUILayout.FloatField("Force", block.force);
            block.direction = EditorGUILayout.Vector3Field("Direction", block.direction);
            block.changeDir = EditorGUILayout.FloatField("Change Direction", block.changeDir);
        }
        else
        {
            GUILayout.Label("Select a valid version (1 or 2) for additional settings.");
        }
    }
}
#endif
