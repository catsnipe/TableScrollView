﻿// Copyright (c) catsnipe
// Released under the MIT license

// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions:
   
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TableNodeElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    TableSubNodeElement[]    SubNodes = null;

    Action<TableNodeElement, bool>
                             eventEnter;
    Action<TableNodeElement, bool>
                             eventClick;

    /// <summary>
    /// Root Sub-index
    /// </summary>
    public const int         SUBINDEX_ROOT = -1;

    /// <summary>
    /// Root RectTriangle
    /// </summary>
    public RectTransform  NodeRect
    {
        get
        {
            if (nodeRect == null)
            {
                nodeRect = GetComponent<RectTransform>();
            }
            return nodeRect;
        }
    }
    RectTransform nodeRect;

    public List<object>      table;
    TableScrollViewer        viewer;
    int                      itemIndex = -1;
    int                      subIndex  = SUBINDEX_ROOT;
    bool                     initFocus = false;
    bool                     focused;
    bool                     subIndexChanged;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        onInitialize();

        enabled = true;

        TableSubNodeElement[] nodeSubElements = this.GetComponentsInChildren<TableSubNodeElement>();
        if (nodeSubElements != null)
        {
            foreach (var element in nodeSubElements)
            {
                element.Initialize();
            }
        }
    }

    /// <summary>
    /// set focus
    /// </summary>
    /// <param name="focus">on..true, off.false</param>
    public void SetFocus(bool focus, bool isAnimation = true)
    {
        if (focus == false)
        {
            if (subIndex != SUBINDEX_ROOT)
            {
                SetSubIndex(SUBINDEX_ROOT);
            }
        }
        else
        {
            if (itemIndex < 0)
            {
                return;
            }
            if (subnodesIsNullOrEmpty() == false && subIndex == SUBINDEX_ROOT)
            {
                subIndex = 0;
                subIndexChanged = true;
            }
        }

        if (subIndexChanged == true)
        {
            setSubNodeFocus(focus, isAnimation);
            subIndexChanged = false;
        }

        if (initFocus == true && focus == focused)
        {
            return;
        }
//Debug.Log($"focus:{itemIndex} {focus}");
        // フォーカス ON/OFF の時の表示をここでカスタマイズする

        onEffectFocus(focus, isAnimation);

        initFocus = true;
        focused   = focus;
    }

    void setSubNodeFocus(bool focus, bool isAnimation)
    {
        for (int i = 0; i < SubNodes.Length; i++)
        {
            if (focus == true && i == subIndex)
            {
                SubNodes[i].SetFocus(focus, isAnimation);
            }
            else
            {
                SubNodes[i].SetFocus(false, isAnimation);
            }
        }
    }

    bool subnodesIsNullOrEmpty()
    {
        if (SubNodes == null || SubNodes.Length == 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// check focus
    /// </summary>
    public bool CheckFocus()
    {
        return focused;
    }

    /// <summary>
    /// Mouse enter
    /// </summary>
    public void SetEvent(Action<TableNodeElement, bool> _eventEnter, Action<TableNodeElement, bool> _eventClick)
    {
        eventEnter = _eventEnter;
        eventClick = _eventClick;
    }

    /// <summary>
    /// set viewer and Table(Data List): 表示するリストの設定
    /// </summary>
    public void SetViewAndTable(TableScrollViewer _viewer, List<object> _table)
    {
        viewer = _viewer;
        table  = _table;

        if (SubNodes != null)
        {
            for (int i = 0; i < SubNodes.Length; i++)
            {
                SubNodes[i].SetSubNode(this, i);
            }
        }
    }

    /// <summary>
    /// set item index: リストの行を設定
    /// </summary>
    public void SetItemIndex(int index)
    {
        if (itemIndex == index)
        {
            return;
        }
        if (index >= table.Count)
        {
            Debug.LogError($"指定行のデータは存在しません. '{index}'");
            return;
        }
        itemIndex = index;
        initFocus = false;

        // 行が変更された場合、それに伴う表示をここでカスタマイズする
        Refresh();
    }
    
    /// <summary>
    /// 表示の更新
    /// </summary>
    public void Refresh()
    {
        onEffectChange(itemIndex);

        if (SubNodes != null)
        {
            for (int i = 0; i < SubNodes.Length; i++)
            {
                SubNodes[i].onEffectChange(itemIndex, i);
            }
        }
    }

    /// <summary>
    /// get row index
    /// </summary>
    public int GetItemIndex()
    {
        return itemIndex;
    }

    /// <summary>
    /// get item
    /// </summary>
    public object GetItem()
    {
        return table[itemIndex];
    }

    /// <summary>
    /// get sub index
    /// </summary>
    public int GetSubIndex()
    {
        return subIndex;
    }

    public int GetSubIndexMax()
    {
        return SubNodes.Length;
    }

    /// <summary>
    /// set sub index
    /// </summary>
    public void SetSubIndex(int sindex)
    {
        if (sindex != subIndex)
        {
            subIndex        = SubNodes == null ? SUBINDEX_ROOT : Mathf.Clamp(sindex, SUBINDEX_ROOT, SubNodes.Length-1);
            subIndexChanged = true;
        }
    }

    /// <summary>
    /// 項目の選択を発生する
    /// </summary>
    public void PerformClick(int sindex)
    {
        performClick(sindex, false);
    }

    void performClick(int sindex, bool click)
    {
        SetSubIndex(sindex);

        if (click == false ||
            viewer.CheckTouchEnable() == true)
        {
            onEffectClick();
            if (subnodesIsNullOrEmpty() == false && subIndex != SUBINDEX_ROOT)
            {
                SubNodes[subIndex].onEffectClick();
            }
        }
        eventClick?.Invoke(this, click);
    }

    public virtual float GetCustomWidth(List<object> tbl, int itemIndex)
    {
        return RectGetWidth(NodeRect);
    }

    public virtual float GetCustomHeight(List<object> tbl, int itemIndex)
    {
        return RectGetHeight(NodeRect);
    }

    public void OnPointerEnter(int sindex)
    {
#if UNITY_STANDALONE
        if (viewer?.EasyFocusForMouse == true)
        {
            SetSubIndex(sindex);
            //subIndex = sindex;
            //setSubNodeFocus(focused, true);

            eventEnter?.Invoke(this, true);
        }
#endif
    }

    public void OnPointerClick(int sindex)
    {
        SetSubIndex(sindex);

        if (viewer == null)
        {
            return;
        }
        if (viewer.SelectAfterFocus == true)
        {
            // 1回目フォーカス、２回目選択
            if (focused == false)
            {
                eventEnter?.Invoke(this, true);
            }
            else
            {
                performClick(sindex, true);
            }
        }
        else
        {
            // フォーカスと選択を同時に
            eventEnter?.Invoke(this, true);

            performClick(sindex, true);
        }
    }

    protected float RectGetWidth(RectTransform rect)
    {
        return rect.rect.size.x;
    }

    protected float RectGetHeight(RectTransform rect)
    {
        return rect.rect.size.y;
    }

    protected void RectSetWidth(RectTransform rect, float width)
    {
        var size = rect.sizeDelta;
        size.x = width;
        rect.sizeDelta = size;
    }

    protected void RectSetHeight(RectTransform rect, float height)
    {
        var size = rect.sizeDelta;
        size.y = height;
        rect.sizeDelta = size;
    }

    /// <summary>
    /// Mouse event
    /// </summary>
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (subnodesIsNullOrEmpty() == true)
        {
            OnPointerEnter(SUBINDEX_ROOT);
        }
    }
    
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        OnPointerClick(SUBINDEX_ROOT);
    }

    /// <summary>
    /// 初期化時コールされる
    /// </summary>
    public virtual void onInitialize()
    {
    }

    /// <summary>
    /// フォーカス ON/OFF の表示エフェクトをここに記述する
    /// </summary>
    public virtual void onEffectFocus(bool focus, bool initialize)
    {
    }

    /// <summary>
    /// 行の表示更新通知があった場合、ここで表示を更新する
    /// </summary>
    public virtual void onEffectChange(int itemIndex)
    {
    }

    /// <summary>
    /// クリック通知があった場合の表示エフェクトをここに記述する
    /// </summary>
    public virtual void onEffectClick()
    {
    }
}
