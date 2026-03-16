﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 为生成脚本统一设置标签，缺失时给出一次性警告。
/// </summary>
public static class TagUtility
{
    private static readonly HashSet<string> missingTags = new HashSet<string>();

    public static bool TryAssignTag(GameObject target, string tagName)
    {
        if (target == null || string.IsNullOrWhiteSpace(tagName))
        {
            return false;
        }

        try
        {
            target.tag = tagName;
            return true;
        }
        catch (UnityException)
        {
            if (missingTags.Add(tagName))
            {
                Debug.LogWarning($"[TagUtility] Tag \"{tagName}\" 尚未在 Tags & Layers 面板中定义，已保留 {target.name} 的默认标签。");
            }
            return false;
        }
    }
}
