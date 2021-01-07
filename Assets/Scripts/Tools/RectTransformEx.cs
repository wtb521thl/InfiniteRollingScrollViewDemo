using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class RectTransformEx
{
    public static RectTransform GetRectTransform(this Transform rectTrans)
    {
        RectTransform selfRect = rectTrans as RectTransform;
        return selfRect;
    }


    /// <summary>
    /// 获取UI世界坐标中心点
    /// </summary>
    public static Vector3 GetCenter(this RectTransform selfTrans)
    {
        Vector2 tempLocalPos = selfTrans.rect.center;
        Matrix4x4 mat = selfTrans.localToWorldMatrix;
        return mat.MultiplyPoint(tempLocalPos);
    }
    /// <summary>
    /// 获取ui世界坐标尺寸
    /// </summary>
    /// <param name="selfTrans"></param>
    /// <returns></returns>
    public static Vector2 GetSize(this RectTransform selfTrans)
    {
        Vector2 size;
        Vector3[] corners = new Vector3[4];
        selfTrans.GetCornersWorld(corners);
        size = corners[2] - corners[0];
        return size;
    }

    /// <summary>
    /// 获取ui世界坐标尺寸
    /// </summary>
    /// <param name="selfTrans"></param>
    /// <returns></returns>
    public static Vector2 GetLocalSize(this RectTransform selfTrans)
    {
        Vector2 size;
        Vector3[] corners = new Vector3[4];
        selfTrans.GetLocalCorners(corners);
        size = corners[2] - corners[0];
        return size;
    }

    public static void GetCornersLocal(this RectTransform selfTrans, Vector3[] fourCornersArray)
    {
        if (fourCornersArray == null || fourCornersArray.Length < 4)
        {
            Debug.LogError("Calling GetLocalCorners with an array that is null or has less than 4 elements.");
            return;
        }

        Rect tmpRect = selfTrans.rect;
        float x0 = tmpRect.x;
        float y0 = tmpRect.y;
        float x1 = tmpRect.xMax;
        float y1 = tmpRect.yMax;

        fourCornersArray[0] = new Vector3(x0, y0, 0f);
        fourCornersArray[1] = new Vector3(x0, y1, 0f);
        fourCornersArray[2] = new Vector3(x1, y1, 0f);
        fourCornersArray[3] = new Vector3(x1, y0, 0f);
    }

    public static void GetCornersWorld(this RectTransform selfTrans, Vector3[] fourCornersArray)
    {
        if (fourCornersArray == null || fourCornersArray.Length < 4)
        {
            Debug.LogError("Calling GetWorldCorners with an array that is null or has less than 4 elements.");
            return;
        }

        selfTrans.GetCornersLocal(fourCornersArray);

        Matrix4x4 mat = selfTrans.localToWorldMatrix;
        for (int i = 0; i < 4; i++)
            fourCornersArray[i] = mat.MultiplyPoint(fourCornersArray[i]);
    }

}
