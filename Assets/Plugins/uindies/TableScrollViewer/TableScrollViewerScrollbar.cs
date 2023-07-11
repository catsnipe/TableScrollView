using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public partial class TableScrollViewer : MonoBehaviour
{
    Scrollbar       scrollbar;
    CanvasGroup     cgroup_scrollbar;
    Vector2         prePosition;

    bool            isScrollbarAutoFadeOut = false;
    
    Coroutine       co_scrollbarOn;

    void initScrollbar()
    {
        if (scrollRect.verticalScrollbar != null)
        {
            scrollbar = scrollRect.verticalScrollbar;
        }
        else
        if (scrollRect.horizontalScrollbar != null)
        {
            scrollbar = scrollRect.horizontalScrollbar;
        }
        
        if (scrollbar == null)
        {
            return;
        }

        // これをしておかないと、スクロールバーをマウスで動かした後のキー入力で勝手にポジションが上下し、ハマる…
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        scrollbar.navigation = nav;

        cgroup_scrollbar = safeGetCanvasGroup(scrollbar);
        prePosition = new Vector2();
        prePosition.x = scrollRect.content.transform.localPosition.x;
        prePosition.y = scrollRect.content.transform.localPosition.y;

        if (ScrollbarAutoFadeout == true)
        {
            setScrollbarAlpha(0);
        }
    }

    /// <summary>
    /// update
    /// </summary>
    void updateScrollbar()
    {
        if (scrollbar == null)
        {
            return;
        }

        scrollbarAutoFadeOut(ScrollbarAutoFadeout);

        if (isScrollbarAutoFadeOut == false)
        {
            return;
        }

//Debug.Log($"{scrollRect.content.transform.localPosition.x} {scrollRect.content.transform.localPosition.y}");
        // キー、マウスホイール
        if (prePosition.x != scrollRect.content.transform.localPosition.x ||
            prePosition.y != scrollRect.content.transform.localPosition.y)
        {
            dispOnScrollbar();
        }
    }
    
    /// <summary>
    /// スクロールバー、自動フェードアウト機能の有効、無効
    /// </summary>
    /// <param name="enabled">true..有効、false..無効</param>
    void scrollbarAutoFadeOut(bool enabled)
    {
        if (isScrollbarAutoFadeOut == enabled)
        {
            return;
        }

        isScrollbarAutoFadeOut = enabled;

        if (enabled == true)
        {
            setScrollbarAlpha(0);
        }
        else
        {
            setScrollbarAlpha(1);
        }
    }

    /// <summary>
    /// スクロールバーを一秒表示
    /// </summary>
    void dispOnScrollbar()
    {
        if (isScrollbarAutoFadeOut == false)
        {
            return;
        }

        if (co_scrollbarOn != null)
        {
            return;
        }
        co_scrollbarOn = StartCoroutine(scrollbarOn());
    }

    /// <summary>
    /// On/Off animation
    /// </summary>
    IEnumerator scrollbarOn()
    {
        if (cgroup_scrollbar == null)
        {
            yield break;
        }

        float a    = cgroup_scrollbar.alpha;
        float time = Time.time;

        // On
        while (true)
        {
            float t = (Time.time - time) * 5;
            t = Mathf.Clamp01(t);

            setScrollbarAlpha(a + (1-a) * t);

            if (t >= 1)
            {
                break;
            }

            yield return null;
        }

        while (prePosition.x != scrollRect.content.transform.localPosition.x ||
               prePosition.y != scrollRect.content.transform.localPosition.y)
        {
            prePosition.x = scrollRect.content.transform.localPosition.x;
            prePosition.y = scrollRect.content.transform.localPosition.y;
            yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(1);

        time = Time.time;

        // Off
        while (true)
        {
            float t = (Time.time - time) * 5;
            t = Mathf.Clamp01(t);

            setScrollbarAlpha(1 * (1 - t));

            if (t >= 1)
            {
                break;
            }

            yield return null;
        }

        co_scrollbarOn = null;
    }

    /// <summary>
    /// CanvasGroup があればそれを使う、なければ作成する
    /// </summary>
    /// <param name="bar">アタッチするスクロールバー</param>
    CanvasGroup safeGetCanvasGroup(Scrollbar bar)
    {
        CanvasGroup group = bar.gameObject.GetComponentInChildren<CanvasGroup>();
        if (group != null)
        {
            return group;
        }
        return bar.gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// set alpha
    /// </summary>
    void setScrollbarAlpha(float a)
    {
        if (cgroup_scrollbar != null)
        {
            cgroup_scrollbar.alpha = a;
        }
    }
}
