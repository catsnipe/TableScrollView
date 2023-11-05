using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeHorizontal : TableNodeElement
{
    [SerializeField]
    Image              Icon = null;
    [SerializeField]
    Image              Focus = null;
    [SerializeField]
    Sprite[]           IconSprites = null;

    /// <summary>
    /// ���������R�[�������
    /// </summary>
    public override void onInitialize()
    {
    }

    /// <summary>
    /// �t�H�[�J�X ON/OFF �̕\���������ɋL�q����
    /// </summary>
    public override void onEffectFocus(bool focus, bool isAnimation)
    {
        Focus.color = new Color(0,0,0, focus == true ? 0.5f : 0.1f);
    }

    /// <summary>
    /// �s�̕\���X�V�ʒm���������ꍇ�A�����ŕ\�����X�V����
    /// </summary>
    public override void onEffectChange(int itemIndex)
    {
        int no = (int)table[itemIndex];

        Icon.sprite = IconSprites[no % IconSprites.Length];

        this.name = "Line: " + (no+1).ToString("00");
    }
}
