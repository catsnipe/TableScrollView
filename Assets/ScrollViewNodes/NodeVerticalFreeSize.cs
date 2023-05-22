﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeVerticalFreeSize : TableNodeElement
{
    [SerializeField]
    TextMeshProUGUI    No = null;
    [SerializeField]
    TextMeshProUGUI    Desc = null;
    [SerializeField]
    Image              Icon = null;
    [SerializeField]
    Image              Focus = null;
    [SerializeField]
    string[]           Descriptions = null;
    [SerializeField]
    Sprite[]           IconSprites = null;

    RectTransform      focusRect;

    /// <summary>
    /// 初期化時コールされる
    /// </summary>
    public override void onInitialize()
    {
        focusRect = Focus.GetComponent<RectTransform>();
    }

    /// <summary>
    /// フォーカス ON/OFF の表示をここに記述する
    /// </summary>
    public override void onEffectFocus(bool focus, bool isAnimation)
    {
        Focus.color = new Color(0,0,0, focus == true ? 0.2f : 0.1f);
    }

    /// <summary>
    /// 行の表示更新通知があった場合、ここで表示を更新する
    /// </summary>
    public override void onEffectChange(int itemIndex)
    {
        int no = (int)table[itemIndex];

        No.SetText("Line: " + (no+1).ToString("00"));
        Desc.SetText(Descriptions[no % Descriptions.Length]);
        Icon.sprite = IconSprites[no % IconSprites.Length];

        float height = GetCustomHeight(table, itemIndex);

        RectSetHeight(focusRect, height-10);
        RectSetHeight(NodeRect, height);

        this.name = No.text;
    }

    public override float GetCustomHeight(List<object> tbl, int itemIndex)
    {
        int no = (int)tbl[itemIndex];

        if ((no % 3) == 0)
        {
            return 200;
        }
        else
        if ((no % 3) == 1)
        {
            return 500;
        }
        else
        {
            return 300;
        }
    }

}
