using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Target))]
public class TargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myTarget = target as Target;

        myTarget.maxHealth = EditorGUILayout.IntField("Max Health", myTarget.maxHealth);
        myTarget.rageFill = EditorGUILayout.IntField("Rage Fill", myTarget.rageFill);
        myTarget.respawn = EditorGUILayout.Toggle("Respawns", myTarget.respawn);

        if (myTarget.respawn)
        {
            EditorGUI.indentLevel = 1;
            myTarget.respawnTime = EditorGUILayout.FloatField("Respawn Time", myTarget.respawnTime);
            EditorGUI.indentLevel = 0;
        }

        myTarget.stunTime = EditorGUILayout.FloatField("Stun Time", myTarget.stunTime);
        myTarget.stunCooldown = EditorGUILayout.FloatField("Stun Cooldown", myTarget.stunCooldown);

    }
}
