using System.Collections;
using System.Collections.Generic;
using Tianbo.Wang;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeItem))]
public class NodeItemEditor : Editor
{
    NodeItem nodeItem;

    public override void OnInspectorGUI()
    {
        if (nodeItem == null)
        {
            nodeItem = (NodeItem)target;
        }
        // base.OnInspectorGUI();
        bool isStatic = EditorGUILayout.Toggle("是否是静态：", nodeItem.isStatic);
        if (!isStatic)
        {
            EditorGUILayout.Toggle("是否是开启状态：", nodeItem.Open);
        }
        EditorGUILayout.Toggle("是否是选中状态：", nodeItem.IsSelected);

        if (nodeItem.nodeInfo.parentNode != null)
        {
            EditorGUILayout.LabelField("父物体：", nodeItem.nodeInfo.parentNode.nodeParam);
        }

        ShowChildNodes(nodeItem.nodeInfo, 0);
    }
    void ShowChildNodes(NodeItemSerializable nodeItemSerializable, int tempIndex)
    {
        tempIndex += 1;
        for (int i = 0; i < nodeItemSerializable.childNodes.Count; i++)
        {
            EditorGUI.indentLevel = tempIndex;
            EditorGUILayout.LabelField("第"+ tempIndex + "层子物体：",nodeItemSerializable.childNodes[i].nodeParam);
            if (nodeItemSerializable.childNodes[i].childNodes.Count != 0)
            {
                ShowChildNodes(nodeItemSerializable.childNodes[i], tempIndex);
            }
        }

    }
}
