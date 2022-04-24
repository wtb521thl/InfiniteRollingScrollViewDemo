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

        /// <summary>
        /// 选中物体的时候，scrollview自动定位
        /// </summary>
        public bool scrollViewAutoValue = false;

        /// <summary>
        /// 点击空白地方取消选中
        /// </summary>
        public bool clickSpaceAreaUnSelect = false;

        /// <summary>
        /// 是否只能点击icon开启子物体
        /// </summary>
        public bool isClickIconOpen = false;

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

        bool isClick = false;

        public Canvas canvas;

        private void Awake()
        {
            if (scrollbar != null)
            {
                scrollbar.onValueChanged.AddListener(ScrollBarChange);
            }
        }

        private void Update()
        {
            if (clickSpaceAreaUnSelect)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (selfRect != null && MouseEnterAndExit.IsMouseEnter(selfRect))
                    {
                        if (!isClick)
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
                            ClickNodeAction?.Invoke(selectedNodes.ToArray());
                            RefreshSelectNodeState();
                        }
                        isClick = false;
                    }
                }
            }
            limitRange = Vector2.zero;
            if (canMove && !isStaticAndOpenAlways)
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
                        if (offsetY != 0 && tempY >= tempItemHeight && afterNodes.Count > 0 && curShowObjLastIndex - allNodeObjs.Count >= 0)
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

                        if (offsetY != 0 && tempY <= -tempItemHeight && beforeNodes.Count > 0 && curShowObjLastIndex - 1 >= 0)
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
        /// 删除一个节点
        /// </summary>
        /// <param name="removeItemParam"></param>
        public void RemoveOne(string removeItemParam)
        {
            for (int i = allNodesInfo.Count - 1; i >= 0; i--)
            {
                if (allNodesInfo[i].nodeParam == removeItemParam)
                {
                    if (allNodesInfo[i].parentNode != null)
                    {
                        allNodesInfo[i].parentNode.childNodes.Remove(allNodesInfo[i]);
                        allNodesInfo.RemoveAt(i);
                    }
                    else
                    {
                        allNodesInfo.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 删除自己和孩子
        /// </summary>
        /// <param name="removeNodeParam"></param>
        public void RemoveNodeAndChilds(string removeNodeParam)
        {
            for (int i = allNodesInfo.Count - 1; i >= 0; i--)
            {
                if (allNodesInfo[i].nodeParam == removeNodeParam)
                {
                    if (allNodesInfo[i].parentNode != null)
                    {
                        allNodesInfo[i].parentNode.childNodes.Remove(allNodesInfo[i]);
                    }
                    RemoveSelfAndChilds(allNodesInfo[i]);
                    break;
                }
            }
        }

        void RemoveSelfAndChilds(NodeItemSerializable nodeItemSerializable)
        {
            allNodesInfo.Remove(nodeItemSerializable);
            if (nodeItemSerializable.childNodes != null)
            {
                for (int i = 0; i < nodeItemSerializable.childNodes.Count; i++)
                {
                    RemoveSelfAndChilds(nodeItemSerializable.childNodes[i]);
                }
            }
        }


        /// <summary>
        /// 把信息添加进来后设置一下父子关系
        /// </summary>
        public void SetChildAndParent()
        {
            for (int i = 0; i < allNodesInfo.Count; i++)
            {
                if (!string.IsNullOrEmpty(allNodesInfo[i].parentNodeName))
                {
                    NodeItemSerializable tempParentNode = allNodesInfo.Find((p) => { return p.nodeParam == allNodesInfo[i].parentNodeName; });
                    allNodesInfo[i].parentNode = tempParentNode;
                    if (allNodesInfo[i].parentNode != null && !allNodesInfo[i].parentNode.childNodes.Contains(allNodesInfo[i]))
                        allNodesInfo[i].parentNode.childNodes.Add(allNodesInfo[i]);
                }
            }
        }

        public void SetChildAndParent(NodeItemSerializable tempNode)
        {
            if (!string.IsNullOrEmpty(tempNode.parentNodeName))
            {
                NodeItemSerializable tempParentNode = allNodesInfo.Find((p) => { return p.nodeParam == tempNode.parentNodeName; });
                tempNode.parentNode = tempParentNode;
                if (tempNode.parentNode != null && !tempNode.parentNode.childNodes.Contains(tempNode))
                    tempNode.parentNode.childNodes.Add(tempNode);
            }
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

        }


        /// <summary>
        /// 删除所有的节点
        /// </summary>
        public void RemoveAllNode()
        {
            for (int i = allNodeObjs.Count - 1; i >= 0; i--)
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
            if (selfRect == null)
                selfRect = GetComponent<RectTransform>();

            beforeNodes.Clear();
            afterNodes.Clear();

            RefreshCanShowItem();

            tempChildRect = Resources.Load<GameObject>(prefabPath).GetComponent<RectTransform>();
            tempItemHeight = tempChildRect.GetSize().y;

            float tempSelfHeight = selfRect.GetSize().y / canvas.transform.localScale.y;

            if (tempSelfHeight < 0)
            {
                tempSelfHeight = -tempSelfHeight;
            }
            int tempInt = (int)(tempSelfHeight / tempItemHeight + 1);
            curShowObjLastIndex = Mathf.Min(canShowNodes.Count, tempInt);
            if (!isStaticAndOpenAlways)
            {
                maxShowNodeObjsCount = tempInt;
            }
            else
            {
                maxShowNodeObjsCount = canShowNodes.Count;
            }

            RefreshNodesObjs();
        }

        /// <summary>
        /// 手动拖动滑动条事件
        /// </summary>
        /// <param name="arg0"></param>
        private void ScrollBarChange(float arg0)
        {
            curShowObjLastIndex = Mathf.Clamp((int)((canShowNodes.Count - allNodeObjs.Count) * arg0 + allNodeObjs.Count), allNodeObjs.Count, canShowNodes.Count);
            RefreshCanShowItem();
            RefreshNodesObjs();
        }

        void SetScrollBarValue()
        {
            if (scrollbar != null)
            {
                float tempValue = (float)(curShowObjLastIndex + 1 - allNodeObjs.Count) / (float)(canShowNodes.Count - allNodeObjs.Count);
                if (canShowNodes.Count - allNodeObjs.Count <= 0)
                {
                    tempValue = 0;
                }
                scrollbar.SetValueWithoutNotify(Mathf.Clamp01(tempValue));
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
        /// 刷新节点物体 ，多余的删除，缺少的生成
        /// </summary>
        private void RefreshNodesObjs()
        {
            int offset = Mathf.Min(canShowNodes.Count, maxShowNodeObjsCount) - allNodeObjs.Count;

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

            curShowObjLastIndex = Mathf.Clamp(curShowObjLastIndex, allNodeObjs.Count, canShowNodes.Count);
            if (canShowNodes.Count >= curShowObjLastIndex && curShowObjLastIndex >= maxShowNodeObjsCount)
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

            RefreshScrollSlider();
        }

        /// <summary>
        /// 在每次物体刷新后刷新滑动条尺寸
        /// </summary>
        private void RefreshScrollSlider()
        {
            if (scrollbar == null)
            {
                return;
            }

            if (canMove)
            {
                float scrollBarSizeX = scrollbar.transform.GetRectTransform().GetSize().x;
                RefreshObjParentSize(parent.parent.GetRectTransform().GetSize().x - scrollBarSizeX, new Vector3(parent.parent.GetRectTransform().GetCenter().x - scrollBarSizeX / 2f, parent.position.y, parent.position.z));
                scrollbar.size = (float)allNodeObjs.Count / (float)canShowNodes.Count;

                SetScrollBarValue();
                scrollbar.gameObject.SetActive(true);
            }
            else
            {
                RefreshObjParentSize(parent.parent.GetRectTransform().GetSize().x, new Vector3(parent.parent.GetRectTransform().GetCenter().x, parent.position.y, parent.position.z));
                scrollbar.gameObject.SetActive(false);
            }

            parent.anchorMin = Vector2.zero;
            parent.anchorMax = Vector2.one;
        }

        public void RefreshObjParentSize(float sizeX, Vector3 pos)
        {
            parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeX);
            parent.position = pos;
            parent.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, parent.parent.GetRectTransform().GetLocalSize().x);
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
            tempNodeItem.Init(itemInfo.childNodes.Count > 0, isStaticAndOpenAlways, isClickIconOpen);
            tempNodeItem.MouseClickAction = MouseClickAction;
            tempNodeItem.IsSelected = itemInfo.isSelected;
            tempNodeItem.Open = itemInfo.isOpen;
            tempNodeItem.SelectNodeAction = SelectNodeAction;

            if (!isStaticAndOpenAlways)
            {
                tempItemObj.GetComponent<HorizontalLayoutGroup>().padding.left = firstLevelOffset + itemInfo.nodeLevel * offset;
            }
            RectTransform nameStrRect = tempItemObj.transform.Find(nameStr).GetRectTransform();
            nameStrRect.GetComponent<Text>().text = itemInfo.nodeName;
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameStrRect);

            SetNodeItemInfo?.Invoke(tempNodeItem);
        }
        /// <summary>
        /// 鼠标点击节点的事件，从每个几点发出
        /// </summary>
        /// <param name="obj"></param>
        private void MouseClickAction(NodeItem obj)
        {
            inSideClick = true;
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                UnselectNodes();
                selectedNodes.Add(obj.nodeInfo);
            }
            else
            {
                if (!selectedNodes.Contains(obj.nodeInfo))
                {
                    selectedNodes.Add(obj.nodeInfo);
                }
                else
                {
                    obj.IsSelected = false;
                    selectedNodes.Remove(obj.nodeInfo);
                }
            }

            RefreshSelectNodeState();

            RefreshCanShowItem();

            RefreshNodesObjs();


            ClickNodeAction?.Invoke(selectedNodes.ToArray());
        }

        /// <summary>
        /// 获取到selectedNodes后，刷新选中状态
        /// </summary>
        void RefreshSelectNodeState()
        {
            isClick = true;
            if (selectedNodes.Count <= 0)
            {
                SetNodeListNoSelect();
                return;
            }

            for (int i = 0; i < allNodeObjs.Count; i++)
            {
                NodeItem nodeItem = allNodeObjs[i].GetComponent<NodeItem>();
                NodeItemSerializable nodeItemSerializable = selectedNodes.Find((p) => { return p.nodeParam == nodeItem.nodeInfo.nodeParam; });
                if (nodeItemSerializable != null)
                {
                    nodeItemSerializable.isSelected = true;
                    nodeItem.IsSelected = true;
                }
                else
                {
                    nodeItem.IsSelected = false;
                }
            }
            SetNodeListSelect(canShowNodes);
            SetNodeListSelect(cantShowNodes);
        }

        void SetNodeListSelect(List<NodeItemSerializable> tempList)
        {
            for (int i = 0; i < tempList.Count; i++)
            {
                NodeItemSerializable nodeItemSerializable = selectedNodes.Find((p) => { return p.nodeParam == tempList[i].nodeParam; });
                if (nodeItemSerializable != null)
                {
                    nodeItemSerializable.isSelected = true;
                    tempList[i].isSelected = true;
                }
                else
                {
                    tempList[i].isSelected = false;
                }
            }
        }

        void SetNodeListNoSelect()
        {
            for (int i = 0; i < allNodeObjs.Count; i++)
            {
                NodeItem nodeItem = allNodeObjs[i].GetComponent<NodeItem>();
                nodeItem.IsSelected = false;
            }
            for (int i = 0; i < canShowNodes.Count; i++)
            {
                canShowNodes[i].isSelected = false;
            }
            for (int i = 0; i < cantShowNodes.Count; i++)
            {
                cantShowNodes[i].isSelected = false;
            }
        }

        /// <summary>
        /// 取消选中的所有节点
        /// </summary>
        private void UnselectNodes()
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
            SetNodeListNoSelect();
        }

        /// <summary>
        /// 选中节点事件，节点发出
        /// </summary>
        /// <param name="_nodeItem"></param>
        /// <param name="_isSelected"></param>
        private void SelectNodeAction(NodeItem _nodeItem, bool _isSelected)
        {
            ChangeNodeSelectStateAction?.Invoke(_nodeItem, _isSelected);
        }




        #region 外部调用点击节点方法
        /// <summary>
        /// 外部调用
        /// </summary>
        /// <param name="allSelectObjs"></param>
        public void ClickNodeByObjects(GameObject[] allSelectObjs)
        {
            UnselectNodes();

            if (allSelectObjs != null)
            {
                for (int i = 0; i < allSelectObjs.Length; i++)
                {
                    selectedNodes.Add(allNodesInfo.Find((p) => { return p.nodeParam == GameObjectIDHelper.GetID(allSelectObjs[i]); }));
                    OpenParentNode(selectedNodes[i]);
                }
            }

            RefreshSelectNodeState();

            RefreshCanShowItem();

            RefreshNodesObjs();

            if (selectedNodes.Count > 0 && scrollViewAutoValue && !inSideClick)
            {
                ScrollViewAutoChangeValue(canShowNodes.FindIndex((p) => { return p.nodeParam == selectedNodes[0].nodeParam; }));
            }

            inSideClick = false;
        }

        /// <summary>
        /// 外部调用
        /// </summary>
        public void ClickNodeInfo(NodeItemSerializable nodeItemSerializable)
        {
            OpenParentNode(nodeItemSerializable);

            RefreshCanShowItem();

            RefreshNodesObjs();

            for (int i = 0; i < allNodeObjs.Count; i++)
            {
                NodeItem tempNodeItem = allNodeObjs[i].GetComponent<NodeItem>();
                if (tempNodeItem.nodeInfo.nodeParam == nodeItemSerializable.nodeParam)
                {
                    if (!tempNodeItem.isStatic)
                    {
                        if (!isClickIconOpen)
                        {
                            nodeItemSerializable.isOpen = !nodeItemSerializable.isOpen;
                            tempNodeItem.Open = nodeItemSerializable.isOpen;
                        }
                    }
                    MouseClickAction(tempNodeItem);
                    break;
                }
            }
        }

        #endregion

        void OpenParentNode(NodeItemSerializable nodeItemSerializable)
        {
            if (nodeItemSerializable.parentNode != null)
            {
                nodeItemSerializable.parentNode.isOpen = true;
                OpenParentNode(nodeItemSerializable.parentNode);
            }
        }

        /// <summary>
        /// 自动定位slider
        /// </summary>
        /// <param name="selectIndex"></param>
        private void ScrollViewAutoChangeValue(int selectIndex)
        {
            curShowObjLastIndex = Mathf.Clamp(selectIndex + maxShowNodeObjsCount / 2, 0, canShowNodes.Count);
            SetScrollBarValue();

            ScrollBarChange(scrollbar.value);
        }
    }
}