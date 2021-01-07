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
        infiniteRollingScrollView.ChangeNodeSelectStateAction += ChangeNodeSelectStateAction;

        normalMat = new Material(Shader.Find("Standard"));
        normalMat.color = Color.white;

        selectMat = new Material(Shader.Find("Standard"));
        selectMat.color = Color.blue;

        for (int i = 0; i < testObjTrans.GetComponentsInChildren<Renderer>().Length; i++)
        {
            testObjTrans.GetComponentsInChildren<Renderer>()[i].sharedMaterial = normalMat;
        }
    }

    private void ChangeNodeSelectStateAction(NodeItem obj, bool isSelect)
    {
        GameObject tempObj = GameObject.Find(obj.nodeInfo.nodeName);
        Renderer renderer = tempObj.GetComponent<Renderer>();
        if (renderer)
        {
            renderer.sharedMaterial = isSelect ? selectMat : normalMat;
        }
    }
}
