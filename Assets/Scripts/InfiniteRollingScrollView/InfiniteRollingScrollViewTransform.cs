﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tianbo.Wang
{
    public class InfiniteRollingScrollViewTransform : InfiniteRollingScrollView
    {


        /// <summary>
        /// 是否可以添加节点的条件（用于筛选不需要的节点）
        /// </summary>
        public Func<Transform, bool> CantAdd;




        public bool CantAddCondititon(Transform tempCurTrans)
        {
            if (CantAdd != null)
                return CantAdd(tempCurTrans);
            return false;
        }


        public NodeItemSerializable AddItemByTransform(Transform tempCurTrans, Transform parentTrans = null, int level = 0)
        {
            return AddItemByTransformFunc(tempCurTrans, parentTrans, level);
        }


        NodeItemSerializable AddItemByTransformFunc(Transform tempCurTrans, Transform parentTrans, int curLevel, NodeItemSerializable parent = null)
        {

            if (CantAddCondititon(tempCurTrans))
            {
                return null;
            }
            string tempName = tempCurTrans.name;
            string tempParentName = "";
            if (parentTrans != null)
            {
                tempParentName = GameObjectIDHelper.GetID(parentTrans.gameObject);
            }
            NodeItemSerializable curItem = new NodeItemSerializable(tempName, tempParentName, curLevel, GameObjectIDHelper.GetID(tempCurTrans.gameObject));
            curItem.parentNode = parent;
            allNodesInfo.Add(curItem);
            List<NodeItemSerializable> childItem = new List<NodeItemSerializable>();
            if (tempCurTrans.childCount != 0)
            {
                curLevel += 1;
                for (int i = 0; i < tempCurTrans.childCount; i++)
                {
                    if (CantAddCondititon(tempCurTrans.GetChild(i)))
                    {
                        continue;
                    }
                    childItem.Add(AddItemByTransformFunc(tempCurTrans.GetChild(i), tempCurTrans, curLevel, curItem));
                }
            }
            curItem.childNodes = childItem;

            return curItem;
        }

        public void RemoveCurRootNodes(Transform tempCurTrans)
        {
            Transform[] allChildTrans = tempCurTrans.GetComponentsInChildren<Transform>();
            for (int i = 0; i < allChildTrans.Length; i++)
            {
                RemoveOne(GameObjectIDHelper.GetID(allChildTrans[i].gameObject));
            }
        }
    }
}