#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAi))]
public class MonsterAIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MonsterAi monster = (MonsterAi)target;

        // EnemyVersion 선택 (1: Mushroom, 2: Skeleton)
        monster.EnemyVersion = EditorGUILayout.IntField("Enemy Version (1: Mushroom, 2: Skeleton)", monster.EnemyVersion);

        // Monster Version에 따라 UI 변경
        if (monster.EnemyVersion == 1)
        {
            EditorGUILayout.HelpBox("Mushroom 몬스터 설정", MessageType.Info);
        }
        else if (monster.EnemyVersion == 2)
        {
            EditorGUILayout.HelpBox("Skeleton 몬스터 설정", MessageType.Info);

            // MonsterNumber는 Skeleton에서만 보이도록 설정
            // monster.MonsterNumber = EditorGUILayout.IntField("Monster Number (Skeleton 전용)", monster.MonsterNumber);
        }
        else
        {
            EditorGUILayout.HelpBox("올바른 Monster Version을 선택하세요 (1: Mushroom, 2: Skeleton)", MessageType.Warning);
        }

        // 기본 Inspector GUI 표시
        DrawDefaultInspector();
    }
}
#endif
