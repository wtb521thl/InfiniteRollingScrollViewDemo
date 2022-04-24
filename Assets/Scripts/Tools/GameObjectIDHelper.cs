using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Tianbo.Wang
{
    public class GameObjectIDHelper
    {

        static Dictionary<GameObject, string> dic = new Dictionary<GameObject, string>();
        static Dictionary<string, int> idCount = new Dictionary<string, int>();
        static Transform[] ts;
        public static void InitDic(GameObject root, Action<GameObject> AddIdAction)
        {
            ts = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < ts.Length; i++)
            {
                AddObjID(ts[i].gameObject, root.transform.parent, AddIdAction);

            }
        }

        static string AddObjID(GameObject tempObj, Transform rootParent, Action<GameObject> AddIDAction)
        {

            int childCount = 0;
            for (int j = 0; j < tempObj.transform.childCount; j++)
            {
                if (tempObj.transform.GetChild(j).name == "_border" || tempObj.transform.GetChild(j).name == "_collider" || tempObj.transform.GetChild(j).name == "[enc]")
                {
                    continue;
                }
                childCount += 1;
            }
            string tempObjID = tempObj.name + "||" + (tempObj.transform.parent == rootParent ? "null" : tempObj.transform.parent.name.Split(new string[] { "||" }, StringSplitOptions.None)[0]) + "||" + childCount;

            if (!idCount.ContainsKey(tempObjID))
            {
                idCount.Add(tempObjID, 0);
            }
            idCount[tempObjID] += 1;

            tempObjID = tempObjID + "||" + idCount[tempObjID];

            dic.Add(tempObj, tempObjID);

            //tempObj.name = dic[tempObj];
            AddIDAction?.Invoke(tempObj);
            return tempObjID;
        }

        public static void RemoveObjID(GameObject tempObj)
        {
            string[] tempIdSplit = GetID(tempObj).Split(new string[] { "||" }, StringSplitOptions.None);
            string tempID = "";
            for (int i = 0; i < tempIdSplit.Length - 1; i++)
            {
                if (i == 0)
                {
                    tempID += tempIdSplit[i];
                }
                else
                {
                    tempID += "||" + tempIdSplit[i];
                }
            }
            if (idCount.ContainsKey(tempID))
            {
                idCount[tempID] -= 1;
                if (idCount[tempID] < 0)
                {
                    idCount.Remove(tempID);
                }
            }
            if (dic.ContainsKey(tempObj))
            {
                dic.Remove(tempObj);
            }
        }

        public static void RemoveObjID(string tempObjID)
        {
            string[] tempIdSplit = tempObjID.Split(new string[] { "||" }, StringSplitOptions.None);
            string tempID = "";
            for (int i = 0; i < tempIdSplit.Length - 1; i++)
            {
                if (i == 0)
                {
                    tempID += tempIdSplit[i];
                }
                else
                {
                    tempID += "||" + tempIdSplit[i];
                }
            }
            if (idCount.ContainsKey(tempID))
            {
                idCount[tempID] -= 1;
                if (idCount[tempID] < 0)
                {
                    idCount.Remove(tempID);
                }
            }
            List<KeyValuePair<GameObject, string>> tempList = dic.ToList();
            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i].Value == tempObjID)
                {
                    tempList.Remove(tempList[i]);
                    break;
                }
            }
            dic.Clear();
            for (int i = 0; i < tempList.Count; i++)
            {
                dic.Add(tempList[i].Key, tempList[i].Value);
            }
        }



        /// <summary>
        /// 找到所有ID相同的物体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<GameObject> GetAllObjsById(string id, int findTimes = 0)
        {
            List<GameObject> objs = new List<GameObject>();
            bool isFind = false;
            foreach (var item in dic)
            {
                if (item.Value == id)
                {
                    objs.Add(item.Key);
                    isFind = true;
                }
            }
            if (findTimes == 0 && !isFind)
            {
                findTimes += 1;
                string[] allSplitStrs = id.Split(new string[] { "||" }, StringSplitOptions.None);
                if (allSplitStrs.Length >= 4)
                {
                    objs.AddRange(GetAllObjsById(allSplitStrs[0] + "1" + "||" + allSplitStrs[1].ToString() + "||" + allSplitStrs[2] + "||" + allSplitStrs[3], findTimes));
                }
            }
            if (findTimes == 1 && !isFind)
            {
                findTimes += 1;
                string[] allSplitStrs = id.Split(new string[] { "||" }, StringSplitOptions.None);
                if (allSplitStrs.Length >= 4)
                {
                    objs.AddRange(GetAllObjsById(allSplitStrs[0] + "||" + allSplitStrs[1].ToString() + "1" + "||" + allSplitStrs[2] + "||" + allSplitStrs[3], findTimes));
                }
            }
            if (findTimes == 2 && !isFind)
            {
                findTimes += 1;
                string[] allSplitStrs = id.Split(new string[] { "||" }, StringSplitOptions.None);
                if (allSplitStrs.Length >= 4)
                {
                    objs.AddRange(GetAllObjsById(allSplitStrs[0] + "1" + "||" + allSplitStrs[1].ToString() + "1" + "||" + allSplitStrs[2] + "||" + allSplitStrs[3], findTimes));
                }
            }
            return objs;
        }

        public static void ClearDic()
        {
            dic.Clear();

            idCount.Clear();

        }

        public static string GetID(GameObject target)
        {
            if (dic.ContainsKey(target))
            {
                return dic[target];
            }
            return target.name;
        }




    }
}