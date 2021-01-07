using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Tianbo.Wang
{
    public class InfiniteRollingScrollView : MonoBehaviour
    {
        /// <summary>
        /// 所有的节点 
        /// </summary>
        [HideInInspector]
        public List<NodeItemSerializable> allNodesInfo = new List<NodeItemSerializable>();

        /// <summary>
        /// 当前可以显示的节点
        /// </summary>
        List<NodeItemSerializable> canShowNodes = new List<NodeItemSerializable>();

        /// <summary>
        /// 当前不可以显示的节点
        /// </summary>
        List<NodeItemSerializable> cantShowNodes = new List<NodeItemSerializable>();

        /// <summary>
        /// 显示在界面上的物体节点
        /// </summary>
        public List<GameObject> allNodeObjs = new List<GameObject>();

        /// <summary>
        /// 在可显示物体之前的节点信息
        /// </summary>
        Stack<NodeItemSerializable> beforeNodes = new Stack<NodeItemSerializable>();
        /// <summary>
        /// 在可显示物体之后的节点信息
        /// </summary>
        Stack<NodeItemSerializable> afterNodes = new Stack<NodeItemSerializable>();

        /// <summary>
        /// 自身的recttransform
        /// </summary>
        RectTransform selfRect;

        /// <summary>
        /// 节点生成的父物体
        /// </summary>
        public RectTransform parent;
        float startParentSizeX;
        float startParentPosX;
        /// <summary>
        /// 每个分段的间距
        /// </summary>
        public int offset = 50;

        /// <summary>
        /// 第一层的行间距
        /// </summary>
        public int firstLevelOffset = 0;

        /// <summary>
        /// 预设在resource路径
        /// </summary>
        public string prefabPath = "Prefabs/TitleNodeItem";

        /// <summary>
        /// 显示节点名字的Text组件物体的名字
        /// </summary>
        public string nameStr = "NameStr";

        /// <summary>
        /// 当前选中的节点列表
        /// </summary>
        public List<NodeItemSerializable> selectedNodes = new List<NodeItemSerializable>();


        /// <summary>
        /// 当前点击的节点事件
        /// </summary>
        public Action<NodeItemSerializable[]> ClickNodeAction;
        /// <summary>
        /// 点击、选中节点事件
        /// </summary>
        public Action<NodeItem, bool> ChangeNodeSelectStateAction;

        /// <summary>
        /// 每次生成物体后对节点进行初始化操作时候执行
        /// </summary>
        public Action<NodeItem> SetNodeItemInfo;

        /// <summary>
        /// 是否是静态的，所有的都打开，不可关闭
        /// </summary>
        public bool isStaticAndOpenAlways = false;
        /// <summary>
        /// 是否可以点击两次以上
        /// </summary>
        public bool canClickTwice = true;

        public bool scrollViewAutoValue = false;

        /// <summary>
        /// 用一个bool区分 点击的是物体还是node
        /// </summary>
        bool inSideClick = false;

        public Vector2 limitRange;

        /// <summary>
        /// 是否可以移动
        /// </summary>
        public bool canMove;
        RectTransform tempChildRect;
        float tempItemHeight;

        /// <summary>
        /// 上一帧鼠标位置
        /// </summary>
       // Vector3 lastFrameMousePos;

        /// <summary>
        /// 当前显示的物体最后一个所引值
        /// </summary>
        public int curShowObjLastIndex;

        /// <summary>
        /// 当前界面最多能放多少个预设
        /// </summary>
        public int maxShowNodeObjsCount;


        public Scrollbar scrollbar;

        private void Update()
        {

            limitRange = Vector2.zero;
            if (canMove)
            {
                limitRange = new Vector2(0, canShowNodes.Count - (selfRect.GetSize().y / tempItemHeight));
                if (MouseEnterAndExit.IsMouseEnter(selfRect))
                {
                    float offsetY = 0;
                    //if (Input.GetMouseButton(0))
                    //{
                    //    offsetY = Input.mousePosition.y - lastFrameMousePos.y;
                    //}
                    if (Input.GetAxis("Mouse ScrollWheel") != 0)
                    {
                        offsetY = -Input.GetAxis("Mouse ScrollWheel") * 1000;
                    }

                    if (offsetY > 0)
                    {
                        if (curShowObjLastIndex >= canShowNodes.Count)
                        {
                            offsetY = 0;
                        }
                        //Debug.Log("向上拽，item向下移动，最上头的item给before，最下层从after取出");
                        float tempY = parent.anchoredPosition.y;
                        tempY += offsetY;
                        if (offsetY != 0 && tempY >= tempItemHeight && afterNodes.Count > 0)
                        {
                            beforeNodes.Push(canShowNodes[curShowObjLastIndex - allNodeObjs.Count]);
                            DestroyImmediate(allNodeObjs[0]);
                            allNodeObjs.RemoveAt(0);
                            GameObject tempAddGo = Instantiate(tempChildRect.gameObject, parent);
                            SetNodeObjInfo(tempAddGo, afterNodes.Pop());
                            allNodeObjs.Add(tempAddGo);
                            tempY = 0;
                            curShowObjLastIndex += 1;
                            SetScrollBarValue();
                        }
                        parent.anchoredPosition = new Vector2(parent.anchoredPosition.x, tempY);
                    }
                    else if (offsetY < 0)
                    {
                        //当前索引减去总显示物体数量==0就代表拉到最顶端了
                        if (curShowObjLastIndex - allNodeObjs.Count <= 0)
                        {
                            offsetY = 0;
                        }
                        //Debug.Log("向下拽" + offsetY);
                        float tempY = parent.anchoredPosition.y;
                        tempY += offsetY;

                        if (offsetY != 0 && tempY <= -tempItemHeight && beforeNodes.Count > 0)
                        {
                            afterNodes.Push(canShowNodes[curShowObjLastIndex - 1]);
                            DestroyImmediate(allNodeObjs[allNodeObjs.Count - 1]);
                            allNodeObjs.RemoveAt(allNodeObjs.Count - 1);
                            GameObject tempAddGo = Instantiate(tempChildRect.gameObject, parent);
                            tempAddGo.transform.SetAsFirstSibling();
                            SetNodeObjInfo(tempAddGo, beforeNodes.Pop());
                            allNodeObjs.Insert(0, tempAddGo);
                            tempY = 0;
                            curShowObjLastIndex -= 1;
                            SetScrollBarValue();
                        }
                        parent.anchoredPosition = new Vector2(parent.anchoredPosition.x, tempY);
                    }

                }
                //  lastFrameMousePos = Input.mousePosition;
            }
        }

        /// <summary>
        /// 新增节点
        /// </summary>
        /// <param name="addItem"></param>
        public void AddOne(NodeItemSerializable addItem)
        {
            allNodesInfo.Add(addItem);
        }

        /// <summary>
        /// 修改其中一个节点的名字
        /// </summary>
        /// <param name="oldParamName"></param>
        /// <param name="changeItemParam"></param>
        public void ChangeOne(string oldParamName, string changeItemParam)
        {
            GameObject tempItem = allNodeObjs.Find((p) => { return p.GetComponent<NodeItem>().nodeInfo.nodeParam == oldParamName; });
            tempItem.transform.Find(nameStr).GetComponent<Text>().text = changeItemParam;
            NodeItem tempNodeItem = tempItem.GetComponent<NodeItem>();
            tempNodeItem.nodeInfo.nodeName = changeItemParam;

            GameObject tempParentItem = allNodeObjs.Find((p) => { return p.GetComponent<NodeItem>().nodeInfo.parentNode != null && p.GetComponent<NodeItem>().nodeInfo.parentNode.nodeParam == oldParamName; });
            if (tempParentItem != null)
            {
                tempParentItem.GetComponent<NodeItem>().nodeInfo.parentNode.nodeParam = changeItemParam;
            }
            RefreshSelfActiveState();
        }


        /// <summary>
        /// 删除所有的节点
        /// </summary>
        public void RemoveAllNode()
        {
            for (int i = 0; i < allNodeObjs.Count; i++)
            {
                DestroyImmediate(allNodeObjs[i]);
            }
            allNodeObjs.Clear();
            allNodesInfo.Clear();
        }

        /// <summary>
        /// 每次增加或者减少节点后调用此方法，刷新节点信息------------初始化调用一次
        /// </summary>
        public void RefreshNodeItemChildInfo()
        {
            selfRect = GetComponent<RectTransform>();
            startParentSizeX = parent.GetSize().x;
            startParentPosX = parent.position.x;
            if (scrollbar != null)
            {
                scrollbar.onValueChanged.AddListener(ScrollBarChange);
            }

            beforeNodes.Clear();
            afterNodes.Clear();

            RefreshCanShowItem();

            tempChildRect = Resources.Load<GameObject>(prefabPath).GetComponent<RectTransform>();
            tempItemHeight = tempChildRect.GetSize().y;

            curShowObjLastIndex = (int)(selfRect.GetSize().y / tempItemHeight + 1);

            SetScrollBarValue();

            maxShowNodeObjsCount = curShowObjLastIndex;

            InitScrollViewNodes();
        }

        private void ScrollBarChange(float arg0)
        {
            curShowObjLastIndex = Mathf.Clamp((int)((canShowNodes.Count - allNodeObjs.Count) * arg0 + allNodeObjs.Count), allNodeObjs.Count, canShowNodes.Count);
            RefreshCanShowItem();
            InitScrollViewNodes();
        }

        void SetScrollBarValue()
        {
            if (scrollbar != null)
            {
                scrollbar.SetValueWithoutNotify((float)(curShowObjLastIndex - allNodeObjs.Count) / (float)(canShowNodes.Count - allNodeObjs.Count));
            }
        }

        /// <summary>
        /// 刷新可见与不可见的节点列表
        /// </summary>
        private void RefreshCanShowItem()
        {

            canShowNodes.Clear();
            cantShowNodes.Clear();

            for (int i = 0; i < allNodesInfo.Count; i++)
            {
                if (allNodesInfo[i].parentNode != null)
                {
                    if (!allNodesInfo[i].parentNode.isOpen)
                    {
                        allNodesInfo[i].isOpen = false;
                    }
                }
                if (!allNodesInfo[i].isOpen)
                {
                    GetChildUnActiveNodes(allNodesInfo[i]);
                }

                if (isStaticAndOpenAlways)
                {
                    canShowNodes.Add(allNodesInfo[i]);
                    continue;
                }

                if (!canShowNodes.Contains(allNodesInfo[i]) && !cantShowNodes.Contains(allNodesInfo[i]))
                {
                    canShowNodes.Add(allNodesInfo[i]);
                }
                if (allNodesInfo[i].isOpen)
                {
                    GetChildActiveNodes(allNodesInfo[i]);
                }
            }
        }

        /// <summary>
        /// 获取可见的子节点
        /// </summary>
        /// <param name="curItem"></param>
        void GetChildActiveNodes(NodeItemSerializable curItem)
        {
            int tempInsertIndex = canShowNodes.IndexOf(curItem) + 1;
            for (int i = curItem.childNodes.Count - 1; i >= 0; i--)
            {
                if (!canShowNodes.Contains(curItem.childNodes[i]))
                {
                    canShowNodes.Insert(tempInsertIndex, curItem.childNodes[i]);
                }
            }
        }
        /// <summary>
        /// 获取不可见的子节点
        /// </summary>
        /// <param name="curItem"></param>
        void GetChildUnActiveNodes(NodeItemSerializable curItem)
        {
            for (int i = 0; i < curItem.childNodes.Count; i++)
            {
                if (!cantShowNodes.Contains(curItem.childNodes[i]))
                {
                    cantShowNodes.Add(curItem.childNodes[i]);
                }
            }
        }

        /// <summary>
        /// 刷新scrollView的size等信息
        /// </summary>
        private void InitScrollViewNodes()
        {

            //多余的删除，缺少的生成
            int offset = canShowNodes.Count - allNodeObjs.Count;

            if (offset > 0)
            {
                for (int i = 0; i < offset; i++)
                {
                    if (allNodeObjs.Count >= maxShowNodeObjsCount)
                    {
                        break;
                    }
                    GameObject tempItem = Instantiate(tempChildRect.gameObject, parent);
                    allNodeObjs.Add(tempItem);
                }
            }
            else if (offset < 0)
            {
                for (int i = 0; i < -offset; i++)
                {
                    DestroyImmediate(allNodeObjs[allNodeObjs.Count - 1]);
                    allNodeObjs.RemoveAt(allNodeObjs.Count - 1);
                }
            }

            if (canShowNodes.Count >= curShowObjLastIndex)
            {
                //开启滑动
                canMove = true;


                afterNodes.Clear();
                for (int i = canShowNodes.Count - curShowObjLastIndex - 1; i >= 0; i--)
                {
                    afterNodes.Push(canShowNodes[curShowObjLastIndex + i]);
                }

                beforeNodes.Clear();
                for (int i = 0; i < curShowObjLastIndex - allNodeObjs.Count; i++)
                {
                    beforeNodes.Push(canShowNodes[i]);
                }

                int curIndex = 0;
                if (curShowObjLastIndex < allNodeObjs.Count)
                {
                    curIndex = 0;
                }
                else
                {
                    curIndex = curShowObjLastIndex - allNodeObjs.Count;
                }
                for (int i = 0; i < allNodeObjs.Count; i++)
                {
                    SetNodeObjInfo(allNodeObjs[i], canShowNodes[curIndex + i]);
                }
            }
            else
            {
                canMove = false;
                for (int i = 0; i < allNodeObjs.Count; i++)
                {
                    SetNodeObjInfo(allNodeObjs[i], canShowNodes[i]);
                }
            }

            if (scrollbar == null)
            {
                return;
            }

            if (canMove)
            {
                float scrollBarSizeX = scrollbar.transform.GetRectTransform().GetSize().x;
                parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, startParentSizeX - scrollBarSizeX);
                parent.position = new Vector3(startParentPosX - scrollBarSizeX / 2f, parent.position.y, parent.position.z);
                scrollbar.size = (float)allNodeObjs.Count / (float)canShowNodes.Count;
                SetScrollBarValue();
                scrollbar.gameObject.SetActive(true);
            }
            else
            {
                parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, startParentSizeX);
                parent.position = new Vector3(startParentPosX, parent.position.y, parent.position.z);
                scrollbar.gameObject.SetActive(false);
            }


        }


        /// <summary>
        /// 设置显示的物体信息
        /// </summary>
        /// <param name="tempItemObj"></param>
        /// <param name="itemInfo"></param>
        void SetNodeObjInfo(GameObject tempItemObj, NodeItemSerializable itemInfo)
        {
            NodeItem tempNodeItem = tempItemObj.GetComponent<NodeItem>();
            tempItemObj.name = itemInfo.nodeName;
            tempNodeItem.nodeInfo = itemInfo;
            tempNodeItem.Init(itemInfo.childNodes.Count > 0, isStaticAndOpenAlways);
            tempNodeItem.MouseClickAction = MouseClickAction;
            tempNodeItem.IsSelected = itemInfo.isSelected;
            tempNodeItem.Open = itemInfo.isOpen;
            tempNodeItem.SelectNodeAction = SelectNodeAction;

            if (!isStaticAndOpenAlways)
            {
                tempItemObj.GetComponent<HorizontalLayoutGroup>().padding.left = firstLevelOffset + itemInfo.nodeLevel * offset;
            }
            tempItemObj.transform.Find(nameStr).GetComponent<Text>().text = itemInfo.nodeName;

            SetNodeItemInfo?.Invoke(tempNodeItem);
        }

        private void SelectNodeAction(NodeItem _nodeItem, bool _isSelected)
        {
            ChangeNodeSelectStateAction?.Invoke(_nodeItem, _isSelected);
        }


        /// <summary>
        /// 外部调用
        /// </summary>
        /// <param name="allSelectObjs"></param>
        public void ClickNodeByObjects(GameObject[] allSelectObjs)
        {
            for (int i = 0; i < selectedNodes.Count; i++)
            {
                selectedNodes[i].isSelected = false;
                for (int j = 0; j < allNodeObjs.Count; j++)
                {
                    NodeItem nodeItem = allNodeObjs[j].GetComponent<NodeItem>();
                    if (selectedNodes[i] == nodeItem.nodeInfo)
                    {
                        nodeItem.IsSelected = false;
                    }
                }
            }
            selectedNodes.Clear();
            if (allSelectObjs != null)
            {
                for (int i = 0; i < allSelectObjs.Length; i++)
                {
                    selectedNodes.Add(allNodesInfo.Find((p) => { return p.nodeParam == allSelectObjs[i].name; }));
                    OpenParentNode(selectedNodes[i]);
                }
            }
            RefreshCanShowItem();
            if (selectedNodes.Count > 0 && scrollViewAutoValue && !inSideClick)
            {
                ScrollViewAutoChangeValue(canShowNodes.FindIndex((p) => { return p.nodeParam == selectedNodes[0].nodeParam; }));
            }

            RefreshSelectNodeState();

            inSideClick = false;
        }

        /// <summary>
        /// 外部调用
        /// </summary>
        public void ClickNodeInfo(NodeItemSerializable nodeItemSerializable)
        {
            for (int i = 0; i < allNodeObjs.Count; i++)
            {
                NodeItem tempNodeItem = allNodeObjs[i].GetComponent<NodeItem>();
                if (tempNodeItem.nodeInfo.nodeParam == nodeItemSerializable.nodeParam)
                {
                    MouseClickAction(tempNodeItem);
                    break;
                }
            }
        }

        void OpenParentNode(NodeItemSerializable nodeItemSerializable)
        {
            if (nodeItemSerializable.parentNode != null)
            {
                nodeItemSerializable.parentNode.isOpen = true;
                OpenParentNode(nodeItemSerializable.parentNode);
            }
        }


        private void ScrollViewAutoChangeValue(int selectIndex)
        {
            curShowObjLastIndex = Mathf.Clamp(selectIndex + maxShowNodeObjsCount / 2, 0, canShowNodes.Count);
            SetScrollBarValue();
        }

        private void MouseClickAction(NodeItem obj)
        {

            inSideClick = true;
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                for (int i = 0; i < selectedNodes.Count; i++)
                {
                    selectedNodes[i].isSelected = false;
                    for (int j = 0; j < allNodeObjs.Count; j++)
                    {
                        NodeItem nodeItem = allNodeObjs[j].GetComponent<NodeItem>();
                        if (selectedNodes[i] == nodeItem.nodeInfo)
                        {
                            nodeItem.IsSelected = false;
                        }
                    }
                }
                selectedNodes.Clear();
            }

            selectedNodes.Add(obj.nodeInfo);


            ClickNodeAction?.Invoke(selectedNodes.ToArray());

            RefreshSelectNodeState();
        }

        void RefreshSelectNodeState()
        {

            if (selectedNodes.Count <= 0)
            {
                return;
            }
            for (int i = 0; i < selectedNodes.Count; i++)
            {
                for (int j = 0; j < allNodeObjs.Count; j++)
                {
                    NodeItem nodeItem = allNodeObjs[j].GetComponent<NodeItem>();
                    if (selectedNodes[i] == nodeItem.nodeInfo)
                    {
                        nodeItem.IsSelected = true;
                    }
                }
                selectedNodes[i].isSelected = true;
            }
            RefreshSelfActiveState();
        }
        /// <summary>
        /// 根据父节点判定自身是否显示隐藏
        /// </summary>
        public void RefreshSelfActiveState()
        {
            if (isStaticAndOpenAlways)
            {
                return;
            }

            RefreshCanShowItem();
            InitScrollViewNodes();
        }
    }
}