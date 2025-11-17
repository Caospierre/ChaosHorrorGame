using UnityEngine;
using UnityEditor;

public static class ResetTransformHotkey
{
    [MenuItem("Tools/Reset Transform #r")]
    private static void ResetTransform()
    {
        foreach (var obj in Selection.transforms)
        {
            Undo.RecordObject(obj, "Reset Transform");
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
            obj.localScale = Vector3.one;
        }
    }
}
