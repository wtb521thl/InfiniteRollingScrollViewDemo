using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tianbo.Wang;
using System;

public class TestModelToInfiniteRollingScrollView : MonoBehaviour
{
    public Transform testObjTrans;

    public InfiniteRollingScrollViewTransform infiniteRollingScrollView;

    Material selectMat;

    Material normalMat;

    void Start()
    {
        infiniteRollingScrollView.AddItemByTransform(testObjTrans);
        infiniteRollingScrollView.ClickNodeAction += ClickNodeAction;
        infiniteRollingScrollView.RefreshNodeItemChildInfo();

        normalMat = new Material(Shader.Find("Standard"));
        normalMat.color = Color.white;

        selectMat = new Material(Shader.Find("Standard"));
        selectMat.color = Color.blue;

        for (int i = 0; i < testObjTrans.GetComponentsInChildren<Renderer>().Length; i++)
        {
            testObjTrans.GetComponentsInChildren<Renderer>()[i].sharedMaterial = normalMat;
        }
    }

    NodeItemSerializable[] lastClickInfos;

    private void ClickNodeAction(NodeItemSerializable[] nodeInfos)
    {

        if (lastClickInfos != null)
        {
            for (int i = 0; i < lastClickInfos.Length; i++)
            {
                GameObject tempObj = GameObject.Find(lastClickInfos[i].nodeName);
                Renderer renderer = tempObj.GetComponent<Renderer>();
                if (renderer)
                {
                    renderer.sharedMaterial = normalMat;
                }
            }
        }

        for (int i = 0; i < nodeInfos.Length; i++)
        {
            GameObject tempObj = GameObject.Find(nodeInfos[i].nodeName);
            Renderer renderer = tempObj.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.sharedMaterial = selectMat;
            }
        }

        lastClickInfos = nodeInfos;
    }

}
