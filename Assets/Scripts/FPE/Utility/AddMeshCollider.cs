//EditorWindow类无法打包,注意添加这个判断
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class AddMeshCollider : EditorWindow
{
    [MenuItem("SKPL/添加移除碰撞体")]

    public static void Open()
    {
        EditorWindow.GetWindow(typeof(AddMeshCollider));
    }
    void OnGUI()
    {
        if (GUILayout.Button("添加碰撞体"))
        {
            Add();
        }

        if (GUILayout.Button("移除碰撞体"))
        {
            Remove();
        }
    }
    public static void Remove()
    {
        //寻找Hierarchy面板下所有的MeshRenderer
        var tArray = Resources.FindObjectsOfTypeAll(typeof(MeshRenderer));
        for (int i = 0; i < tArray.Length; i++)
        {

            MeshRenderer t = tArray[i] as MeshRenderer;

            //保存场景修改，或者执行完手动Ctrl+s保存
            Undo.RecordObject(t, t.gameObject.name);

            MeshCollider meshCollider = t.gameObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                DestroyImmediate(meshCollider);
            }

            //刷新场景
            EditorUtility.SetDirty(t);

        }
        Debug.Log("remove Succed");
    }

    public static void Add()
    {
        //寻找Hierarchy面板下所有的MeshRenderer
        var tArray = Resources.FindObjectsOfTypeAll(typeof(MeshRenderer));

        for (int i = 0; i < tArray.Length; i++)
        {

            MeshRenderer t = tArray[i] as MeshRenderer;

            //保存场景修改，或者执行完手动Ctrl+s保存
            Undo.RecordObject(t, t.gameObject.name);

            MeshCollider meshCollider = t.gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                t.gameObject.AddComponent<MeshCollider>();
            }

            //刷新场景
            EditorUtility.SetDirty(t);


        }
        Debug.Log("Add Succed");
    }
}

#endif