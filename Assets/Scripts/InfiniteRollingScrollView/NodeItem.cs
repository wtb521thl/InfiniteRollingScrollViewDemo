using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Tianbo.Wang
{
    public class NodeItem : MonoBehaviour
    {
        [HideInInspector]
        public RectTransform selfTrans;
        public GameObject selectImage;
        public GameObject hoverImage;
        public Vector3 closeRotAngle = new Vector3(0, 0, 90);
        public Vector3 openRotAngle = Vector3.zero;

        public Image iconImage;

        public NodeItemSerializable nodeInfo;

        /// <summary>
        /// 当前节点是否打开
        /// </summary>
        private bool open = false;

        bool isSelected = false;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                ChangeSelectColor(isSelected);
            }
        }

        public bool Open
        {
            get
            {
                return open;
            }
            set
            {
                open = value;
                ChangeState();
            }
        }

        bool isInited = false;

        /// <summary>
        /// 是否是静态的，可以点击，但是没有打开关闭的动作和动画
        /// </summary>
        public bool isStatic = false;

        public Action<NodeItem> MouseEnterAction;

        public Action<NodeItem> MouseExitAction;

        public Action<NodeItem, bool> SelectNodeAction;

        public Action<NodeItem> MouseClickAction;

        private void Awake()
        {
            selfTrans = GetComponent<RectTransform>();
            EventTriggerListener.Get(gameObject).onClick += ItemClick;
            EventTriggerListener.Get(gameObject).onEnter += ItemEnter;
            EventTriggerListener.Get(gameObject).onExit += ItemExit;

            ItemExit(gameObject);

        }

        private void ItemEnter(GameObject go)
        {
            hoverImage.SetActive(true);
            MouseEnterAction?.Invoke(this);
        }
        private void ItemExit(GameObject go)
        {
            hoverImage.SetActive(false);
            MouseExitAction?.Invoke(this);
        }
        /// <summary>
        /// 初始化时候必须调用
        /// </summary>
        /// <param name="isOpen">是否打开状态</param>
        /// <param name="isStatic">是否是静态的，不可以点击</param>
        public void Init(bool isParent, bool _isStatic = false)
        {

            isStatic = _isStatic;
            if (!isStatic)
            {
                iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, isParent ? 1 : 0);
                if (!isInited)
                {
                    iconImage.rectTransform.localEulerAngles = closeRotAngle;
                }
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }

            isInited = true;
        }


        void ChangeSelectColor(bool _isSelecter)
        {
            selectImage.SetActive(_isSelecter);
            SelectNodeAction?.Invoke(this, _isSelecter);
        }


        private void ItemClick(GameObject go)
        {
            Open = !Open;
            nodeInfo.isOpen = Open;
            MouseClickAction?.Invoke(this);
        }

        void ChangeState()
        {
            if (!isStatic)
            {
                if (Open)
                {
                    if (iconImage.gameObject.activeInHierarchy)
                    {
                        iconImage.rectTransform.DOLocalRotate(openRotAngle, 0.3f);
                    }
                    else
                    {
                        iconImage.rectTransform.localEulerAngles = openRotAngle;
                    }
                }
                else
                {
                    if (iconImage.gameObject.activeInHierarchy)
                    {
                        iconImage.rectTransform.DOLocalRotate(closeRotAngle, 0.3f);
                    }
                    else
                    {
                        iconImage.rectTransform.localEulerAngles = closeRotAngle;
                    }
                }
            }
        }
    }

    [Serializable]
    public class NodeItemSerializable
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string nodeName;

        /// <summary>
        /// 节点级别
        /// </summary>
        public int nodeLevel;

        /// <summary>
        /// 父节点名字
        /// </summary>
        public string parentNodeName;

        /// <summary>
        /// 节点地址名称
        /// </summary>
        public string nodeParam;

        /// <summary>
        /// 是否处于打开
        /// </summary>
        public bool isOpen = false;

        /// <summary>
        /// 是否处于选中
        /// </summary>
        public bool isSelected = false;

        /// <summary>
        /// 父节点
        /// </summary>
        public NodeItemSerializable parentNode;

        /// <summary>
        /// 子节点
        /// </summary>
        [NonSerialized]
        public List<NodeItemSerializable> childNodes = new List<NodeItemSerializable>();

        public NodeItemSerializable()
        {

        }

        public NodeItemSerializable(string _nodeName, string _itemParentName, int _nodeLevel, string _nodeParam = "")
        {
            nodeName = _nodeName;
            nodeParam = _nodeParam;
            parentNodeName = _itemParentName;
            nodeLevel = _nodeLevel;
        }
    }
    [Serializable]
    public class NodeItemSerializableInfo
    {
        public List<NodeItemSerializable> nodeItemSerializables = new List<NodeItemSerializable>();

        public void Add(string _nodeName, string _itemParentName, int _nodeLevel, string _nodeParam = "")
        {
            nodeItemSerializables.Add(new NodeItemSerializable(_nodeName, _itemParentName, _nodeLevel, _nodeParam));
        }
        public void Add(NodeItemSerializable tempItem)
        {
            nodeItemSerializables.Add(tempItem);
        }

        public void Clear()
        {
            nodeItemSerializables.Clear();
        }
    }

}