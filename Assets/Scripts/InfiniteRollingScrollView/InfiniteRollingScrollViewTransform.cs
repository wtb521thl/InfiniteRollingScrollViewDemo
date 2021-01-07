using System;
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


        public void AddItemByTransform(Transform tempCurTrans)
        {
            AddItemByTransformFunc(tempCurTrans, null, 0);
            RefreshNodeItemChildInfo();
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
                tempParentName = parentTrans.name;
            }
            NodeItemSerializable curItem = new NodeItemSerializable(tempName, tempParentName, curLevel, tempCurTrans.name);
            curItem.parentNode = parent;
            allNodesInfo.Add(curItem);
            List<NodeItemSerializable> childItem = new List<NodeItemSerializable>();
            if (tempCurTrans.childCount != 0)
            {
                curLevel += 1;
                for (int i = 0; i < tempCurTrans.childCount; i++)
                {
                    if (CantAddCondititon(tempCurTrans))
                    {
                        continue;
                    }
                    childItem.Add(AddItemByTransformFunc(tempCurTrans.GetChild(i), tempCurTrans, curLevel, curItem));
                }
            }
            curItem.childNodes = childItem;

            return curItem;
        }
    }
}