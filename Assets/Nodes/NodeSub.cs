using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeSub : TableSubNodeElement
{
    [SerializeField]
    Image                 Frame = null;
    [SerializeField]
    TextMeshProUGUI       Text = null;
    
    public override void onEffectFocus(bool focus, bool isAnimation)
    {
        if (focus == true)
        {
            Frame.color = new Color(0,0,0,1);
            Text.color = new Color(1,1,1,1);
        }
        else
        {
            Frame.color = new Color(0,0,0,0.33f);
            Text.color = new Color(0,0,0,1);
        }
    }

    /// <summary>
    /// 行の表示更新通知があった場合、ここで表示を更新する
    /// </summary>
    public override void onEffectChange(int itemIndex, int subIndex)
    {
        string abc  = "ABC";
        Text.SetText($"{itemIndex+1}-{abc[subIndex]}");

    }

    /// <summary>
    /// クリック通知があった場合の表示エフェクトをここに記述する
    /// </summary>
    public override void onEffectClick()
    {
    }

}
