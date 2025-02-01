using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeScene2 : TableNodeElement
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
    Sprite[]           IconSprites = null;

    /// <summary>
    /// 初期化時コールされる
    /// </summary>
    public override void onInitialize()
    {
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
        var row = (TestScene2.Row)table[itemIndex];

        No.SetText("Line: " + row.No.ToString("00"));
        Desc.SetText(row.PlaceName);
        Icon.sprite = IconSprites[row.No % IconSprites.Length];

        this.name = No.text;
    }

}
