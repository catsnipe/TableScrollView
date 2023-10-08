using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]

public partial class TableScrollViewer : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    /// <summary>
    /// スクロールする向き
    /// </summary>
    public enum eOrientation
    {
        Vertical,
        Horizontal,
    }
    /// <summary>
    /// キー移動を促すフラグ
    /// </summary>
    public enum eKeyMoveFlag
    {
        /// <summary>
        /// 移動なし
        /// </summary>
        None,
        /// <summary>
        /// 選択
        /// </summary>
        Select,
        /// <summary>
        /// 選択キャンセル
        /// </summary>
        Cancel,
        /// <summary>
        /// 上移動
        /// </summary>
        Up,
        /// <summary>
        /// 下移動
        /// </summary>
        Down,
        /// <summary>
        /// 左移動
        /// </summary>
        Left,
        /// <summary>
        /// 右移動
        /// </summary>
        Right,
        /// <summary>
        /// 上にページ移動
        /// </summary>
        PageUp,
        /// <summary>
        /// 下にページ移動
        /// </summary>
        PageDown,
        /// <summary>
        /// 左にページ移動
        /// </summary>
        PageLeft,
        /// <summary>
        /// 右にページ移動
        /// </summary>
        PageRight,
    }
    /// <summary>
    /// KeyDown EventArgs
    /// </summary>
    public class KeyDownArgs
    {
        public eKeyMoveFlag Flag;
    }
    /// <summary>
    /// SetSelectedIndex した際のポジションスクロール方法
    /// </summary>
    public enum ePositionMoveMode
    {
        /// <summary>
        /// 1フレームで該当ポジションに移動
        /// </summary>
        OneFrame,
        /// <summary>
        /// スクロールしながら該当ポジションに移動
        /// </summary>
        ScrollMove,
        /// <summary>
        /// ポジション移動しない
        /// </summary>
        DontMove,
    }

    /// <summary>
    /// キー入力が必要になった時に呼ばれるイベント
    /// </summary>
    [Serializable]
    public class OnKeyDownEvent : UnityEvent<KeyDownArgs> {}

    /// <summary>
    /// 選択、またはキャンセルされた時に呼ばれるイベント
    /// object[] table, int itemIndex, int subIndex, bool isCancel
    /// 
    /// (table) テーブル
    /// (itemIndex) 選択されている行
    /// (subIndex) 選択されている列（行のサブアイテム）
    /// (isCancel) キャンセルボタンが押された場合 true
    /// </summary>
    [Serializable]
    public class OnSelectEvent : UnityEvent<List<object>, int, int, bool> {}

    /// <summary>
    /// カーソルが移動した時に発生するイベント
    /// object[] table, int itemIndex, int subIndex, bool userInput
    /// 
    /// (table) テーブル
    /// (itemIndex) 選択されている行
    /// (subIndex) 選択されている列（行のサブアイテム）
    /// (userInput) ユーザー選択で変化した場合 true、SetSelectedIndex() の場合 false
    /// </summary>
    [Serializable]
    public class OnCursorMoveEvent : UnityEvent<List<object>, int, int, bool> {}

    /// <summary>
    /// Node Prefab
    /// </summary>
    [SerializeField]
    public TableNodeElement
                          SourceNode = null;
    /// <summary>
    /// スクロールする向き
    /// </summary>
    [SerializeField]
    public eOrientation   Orientation = eOrientation.Vertical;
    /// <summary>
    /// Vertical Layout Group の Padding.Top 同様
    /// </summary>
    [SerializeField, Space(10)]
    public float          PaddingTop = 0;
    /// <summary>
    /// Vertical Layout Group の Padding.Bottom 同様
    /// </summary>
    [SerializeField]
    public float          PaddingBottom = 0;
    /// <summary>
    /// Vertical Layout Group の Spacing 同様
    /// </summary>
    [SerializeField]
    public float          Spacing = 0;
    /// <summary>
    /// スクロールスピード
    /// </summary>
    [SerializeField, Space(10), Range(0.01f, 1f)]
    public float          ScrollTime = 0.5f;
    /// <summary>
    /// ページスクロールする際に移動する項目量
    /// </summary>
    [SerializeField, Range(1, 1000)]
    public int            SkipIndexByPageScroll = 10;
    /// <summary>
    /// スクロールバーの自動フェードアウト
    /// </summary>
    [SerializeField, Space(10), Tooltip("スクロールバーの自動フェードアウト")]
    public bool           ScrollbarAutoFadeout = true;
    /// <summary>
    /// ドラッグ移動した際、自動的に項目が定位置に吸着する
    /// </summary>
    [SerializeField, Tooltip("ドラッグ移動した際、自動的に項目が定位置に吸着する")]
    public bool           AdsorptionTarget = true;
    /// <summary>
    /// true では一度フォーカスを移してから、選択させるようにする。false だと即選択する
    /// </summary>
    [SerializeField, Tooltip("true では一度フォーカスを移してから、選択させるようにする。false だと即選択する")]
    public bool           SelectAfterFocus = false;
    /// <summary>
    /// マウスカーソルが項目の上に来ると、自動的にフォーカスする
    /// マウスとタップを同じ操作にしたい場合、false にしておいたほうが安全
    /// </summary>
    [SerializeField, Tooltip("マウスカーソルが項目の上に来ると、自動的にフォーカスする")]
    public bool           EasyFocusForMouse = false;
    /// <summary>
    /// 選択後、自動的にビューの選択を禁止する。元に戻すには InputEnabled() を呼び出す
    /// </summary>
    [SerializeField, Tooltip("選択後、自動的にビューの選択を禁止する。元に戻すには InputEnabled() を呼び出す")]
    bool                  DisabledAfterSelect = false;

    /// <summary>
    /// OnClick
    /// </summary>
    [SerializeField, Header("Event")]
    public OnSelectEvent  OnSelect = null;
    /// <summary>
    /// キー入力確認イベント
    /// </summary>
    [SerializeField]
    public OnKeyDownEvent OnKeyDown = null;
    /// <summary>
    /// サウンド要求イベント
    /// </summary>
    [SerializeField]
    public OnCursorMoveEvent
                          OnCursorMove = null;
    
//[SerializeField]
//TextMeshProUGUI text;

    class NodeGroup
    {
        public GameObject       Object;
        public RectTransform    Rect;
        public TableNodeElement Node;
    }

    class RowDisplay
    {
        public float    Position;
        public float    Size;
        public float    LastPosition;
    }

    /// <summary>
    /// 現在選択中のリスト番号
    /// </summary>
    public int          SelectedIndex
    {
        get
        {
            if (reserveSelectedIndex >= 0)
            {
                return reserveSelectedIndex;
            }
            return selectedIndex;
        }
        private set
        {
            selectedIndex = value;
        }
    }
    int                 selectedIndex = -1;
    /// <summary>
    /// 選択された行のデータ
    /// </summary>
    public object       SelectedRow
    {
        get
        {
            if (SelectedIndex < 0 || SelectedIndex >= table.Count)
            {
                return null;
            }
            return table[SelectedIndex];
        }
    }
    /// <summary>
    /// 選択されたテーブルノード
    /// </summary>
    public TableNodeElement SelectedTableNode
    {
        get
        {
            if (reserveSelectedIndex >= 0)
            {
                Debug.LogWarning("SetSelectedIndex() 直後のテーブルノードは古い可能性があります.");
            }
            return selectedNodeGroup?.Node;
        }
    }
    NodeGroup           selectedNodeGroup;
    /// <summary>
    /// テーブル最大行数
    /// </summary>
    public int          ItemCount
    {
        get; private set;
    }

    /// <summary>
    /// 
    /// </summary>
    int                 selectedSubIndex = TableNodeElement.SUBINDEX_ROOT;
    /// <summary>
    /// 次に選択されるリスト番号（予約）
    /// </summary>
    int                 reserveSelectedIndex;
    /// <summary>
    /// 一瞬でスクロールポジションを移動させるなら true
    /// 実際には ForceSelectedIndex() から呼ばれる
    /// </summary>
    ePositionMoveMode   positionMoveMode;

    /// <summary>
    /// CanvasGroup
    /// </summary>
    public CanvasGroup  CanvasGroup
    {
        get; private set;
    }
    CanvasGroup[]       parentGroups;

    /// <summary>
    /// 本体
    /// </summary>
    ScrollRect          scrollRect;
    RectTransform       scrollRectTransform;
    /// <summary>
    /// 初期サイズ
    /// </summary>
    Vector2             viewWH;
    /// <summary>
    /// 全ての表示ノード。表示分のみ確保
    /// </summary>
    List<NodeGroup>     nodeGroups;
    Dictionary<TableNodeElement, NodeGroup>
                        nodeSearch;
    Dictionary<int, NodeGroup>
                        nodeIndex;

    /// <summary>
    /// キー情報
    /// </summary>
    KeyDownArgs         keyDownArgs;

    /// <summary>
    /// 全行の表示位置など
    /// </summary>
    List<RowDisplay>    rowDisplays;


    /// <summary>
    /// Vertical Layout Group の Padding.Top/Bottom と同じ値
    /// </summary>
    float               paddingTop;
    float               paddingBottom;
    /// <summary>
    /// Vertical Layout Group の Spacing
    /// </summary>
    float               nodeSpace;
    /// <summary>
    /// 画面外で余分に確保しておくノード数。基本は 0 で問題ないが、ウィンドウ可変でノード増加の可能性がある時に設定する
    /// </summary>
    int                 nodeExtraNumber;
    /// <summary>
    /// テーブル
    /// </summary>
    List<object>        table;
    /// <summary>
    /// 表示中の先頭行
    /// </summary>
    int                 itemStart;
    /// <summary>
    /// テーブル変更用のテンポラリバッファ
    /// </summary>
    List<object>        changeTable;
    int                 changeSelectedIndex;

    // キー移動の目標ポジションと、現在のポジション
    float               targetNormPos;
    float               currentNormPos;
    float               timeNormPos;

    // 吸着イベント
    Coroutine           co_autoTarget;

    // Content（全てのスクロールエリア）の高さ
    float               contentSize;
    // ScrollView（表示画面分のスクロールエリア）の高さ
    float               scrollSize;

    bool                focusIsAnimation = false;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_nodeExtraNumber">画面外で余分に確保しておくノード数。基本は 0 で問題ないが、ウィンドウ可変でノード増加の可能性がある時に設定する</param>
    public void Initialize(int _nodeExtraNumber = 0)
    {
        if (SourceNode == null)
        {
            Debug.LogError("SourceNode is not found. Please set by inspector.");
            return;
        }
        if (CanvasGroup != null)
        {
            return;
        }

        scrollRect          = GetComponent<ScrollRect>();
        scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        CanvasGroup         = GetComponent<CanvasGroup>();
        parentGroups        = GetComponentsInParent<CanvasGroup>();

        nodeExtraNumber     = _nodeExtraNumber;

        RectTransform rect = SourceNode.GetComponent<RectTransform>();
        if (Orientation == eOrientation.Vertical)
        {
            scrollRect.content.anchorMin = new Vector2(0, 1);
            scrollRect.content.anchorMax = new Vector2(1, 1);
        }
        else
        {
            scrollRect.content.anchorMin = new Vector2(0, 0);
            scrollRect.content.anchorMax = new Vector2(0, 1);
        }

        viewWH = new Vector2(rectGetWidth(scrollRectTransform), rectGetHeight(scrollRectTransform));
        reserveSelectedIndex = -1;
        positionMoveMode     = ePositionMoveMode.OneFrame;

        initScrollbar();

        // event
        scrollRect.onValueChanged.AddListener(onValueChanged);
    }

    /// <summary>
    /// テーブルリセット
    /// </summary>
    public void ResetTable()
    {
        SetTable((List<object>)null);
    }

    /// <summary>
    /// 表示するテーブルの設定
    /// </summary>
    public void SetTable(object[] _table)
    {
        SetTable(_table.ToList());
    }

    /// <summary>
    /// 表示するテーブルの設定
    /// </summary>
    public void SetTable(IList _table)
    {
        SetTable(_table.Cast<object>().ToList());
    }

    /// <summary>
    /// 表示するテーブルの設定
    /// </summary>
    public void SetTable(List<object> _table)
    {
        if (SourceNode == null)
        {
            Debug.LogError("SourceNode is not found. Please set by inspector.");
            return;
        }
        if (CanvasGroup == null)
        {
            Initialize();
        }

        paddingTop    = PaddingTop;
        paddingBottom = PaddingBottom;
        nodeSpace     = Spacing;

        if (table != null)
        {
            for (int i = 0; i < nodeGroups.Count; i++)
            {
                NodeGroup group  = nodeGroups[i];
                Destroy(group.Object);
            }
            nodeGroups = null;
            nodeSearch = null;
            nodeIndex  = null;
        }

        table             = _table;
        ItemCount         = table == null ? 0 : table.Count;
        if (selectedIndex >= ItemCount)
        {
            selectedIndex = ItemCount-1;
        }
        if (Orientation == eOrientation.Vertical)
        {
            scrollSize    = rectGetHeight(scrollRectTransform);
        }
        else
        {
            scrollSize    = rectGetWidth(scrollRectTransform);
        }

        rowDisplays = new List<RowDisplay>();

        float position = 0;
        float sizeMin  = scrollSize;

        if (table == null || table.Count == 0)
        {
            //
        }
        else
        {
            // リスト全ての表示位置を計算
            if (Orientation == eOrientation.Vertical)
            {
                position = SourceNode.GetCustomHeight(table, 0) / 2;
                sizeMin  = SourceNode.GetCustomHeight(table, 0);
                rowDisplays.Add(
                    new RowDisplay()
                    {
                        Position = position,
                        Size = sizeMin,
                        LastPosition = position + sizeMin / 2
                    }
                );
            }
            else
            {
                position = 0;
                sizeMin  = SourceNode.GetCustomWidth(table, 0);
                rowDisplays.Add(
                    new RowDisplay()
                    {
                        Position = position,
                        Size = sizeMin,
                        LastPosition = position + sizeMin
                    }
                );
            }

            for (int i = 1; i < table.Count; i++)
            {
                float size0;
                float size1;

                if (Orientation == eOrientation.Vertical)
                {
                    size0 = SourceNode.GetCustomHeight(table, i-1);
                    size1 = SourceNode.GetCustomHeight(table, i);
                }
                else
                {
                    size0 = SourceNode.GetCustomWidth(table, i-1);
                    size1 = SourceNode.GetCustomWidth(table, i); //  + nodeSpace;
                }

                if (sizeMin > size1)
                {
                    sizeMin = size1;
                }

                if (Orientation == eOrientation.Vertical)
                {
                    position += (size0 + size1) / 2 + nodeSpace;
                }
                else
                {
                    position += size0 + nodeSpace;
                }

                float lastPosition;

                if (Orientation == eOrientation.Vertical)
                {
                    lastPosition = position + size1 / 2;
                }
                else
                {
                    lastPosition = position + size1;
                }

                rowDisplays.Add(
                    new RowDisplay()
                    {
                        Position = position,
                        Size = size1,
                        LastPosition = lastPosition
                    }
                );
            }
        }

        contentSize       = paddingTop + paddingBottom;
        if (rowDisplays.Count > 0)
        {
            contentSize += rowDisplays[rowDisplays.Count-1].LastPosition;
        }

        nodeGroups        = new List<NodeGroup>();
        nodeSearch        = new Dictionary<TableNodeElement, NodeGroup>();
        selectedNodeGroup = null;
        selectedSubIndex  = TableNodeElement.SUBINDEX_ROOT;

        keyDownArgs       = new KeyDownArgs();

        int viewMax = (int)(scrollSize / sizeMin);
        int nodeMax = viewMax + 2 + nodeExtraNumber;
        if (nodeMax > ItemCount)
        {
            nodeMax = ItemCount;
        }

        for (int i = 0; i < nodeMax; i++)
        {
            TableNodeElement obj = Instantiate(SourceNode, scrollRect.content.transform);
            obj.Initialize();
            NodeGroup group = new NodeGroup();
            group.Object    = obj.gameObject;
            group.Node      = obj;
            group.Rect      = obj.GetComponent<RectTransform>();
            
            if (group.Node == null)
            {
                Debug.LogError("ScrollViewerNode を継承した Node クラスが SourceNode に存在しません.");
            }

            group.Node.SetEvent(nodeEnter, nodeClick);
            group.Node.SetViewAndTable(this, table);

            nodeGroups.Add(group);
            nodeSearch.Add(group.Node, group);
        }

        if (Orientation == eOrientation.Vertical)
        {
            rectSetHeight(scrollRect.content, contentSize);
        }
        else
        {
            rectSetWidth(scrollRect.content, contentSize);
        }

        scrollRect.content.transform.localPosition = Vector3.zero;
        viewerScroll(new Vector2(0, 1), true);
    }

    /// <summary>
    /// カーソルを指定行まで移動させる
    /// </summary>
    /// <param name="selindex">指定行数</param>
    /// <param name="_positionMove">指定行数までどのようにスクロールするか</param>
    public void SetSelectedIndex(object row, ePositionMoveMode _positionMove = ePositionMoveMode.OneFrame)
    {
        int index = table.FindIndex( (a) => a == row );
        SetSelectedIndex(index, _positionMove);
    }

    /// <summary>
    /// カーソルを指定行まで移動させる
    /// </summary>
    /// <param name="selindex">指定行数</param>
    /// <param name="_positionMove">指定行数までどのようにスクロールするか</param>
    public void SetSelectedIndex(int selindex, ePositionMoveMode _positionMove = ePositionMoveMode.OneFrame)
    {
        if (checkUsable() == false)
        {
            return;
        }

        if (selindex >= 0)
        {
            selindex = Mathf.Clamp(selindex, 0, table.Count-1);
            reserveSelectedIndex = selindex;
            positionMoveMode     = _positionMove;
            selectedSubIndex     = -1;
        }
    }
    
    /// <summary>
    /// カーソルを指定行数移動させる
    /// </summary>
    /// <param name="amount">増減行数</param>
    public void AddSelectedIndex(int amount, ePositionMoveMode _positionMove = ePositionMoveMode.OneFrame)
    {
        amount += SelectedIndex;
        amount  = Mathf.Clamp(amount, 0, table.Count-1);
        SetSelectedIndex(amount, _positionMove);
    }

    /// <summary>
    /// TableSubNodeElement のポジション設定 
    /// </summary>
    /// <param name="subIndex"></param>
    public void SetSubIndex(int subIndex)
    {
        if (selectedNodeGroup != null)
        {
            NodeGroup group = selectedNodeGroup;
            if (group.Node.GetSubIndexMax() == 0)
            {
                subIndex = TableNodeElement.SUBINDEX_ROOT;
            }
            else
            {
                subIndex = Mathf.Clamp(subIndex, 0, group.Node.GetSubIndexMax()-1);
            }

            selectedSubIndex = subIndex;

            group.Node.SetSubIndex(subIndex);
            group.Node.SetFocus(true, false);
        }
    }

    /// <summary>
    /// TableSubNodeElement のポジション増減
    /// </summary>
    /// <param name="amount"></param>
    public void AddSubIndex(int amount)
    {
        if (selectedNodeGroup != null)
        {
            NodeGroup group = selectedNodeGroup;
            SetSubIndex(group.Node.GetSubIndex() + amount);
        }
    }

    /// <summary>
    /// SetSelectedIndex() でカーソルを動かしたか確認
    /// </summary>
    /// <returns>true .. SetSelectedIndex() でカーソルを動かした</returns>
    public bool CheckCallSetSelectedIndex()
    {
        return reserveSelectedIndex >= 0;
    }

    /// <summary>
    /// 表示内容を更新する
    /// </summary>
    public void Refresh(bool forceRefresh = false)
    {
        if (checkUsable() == false)
        {
            return;
        }
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group  = nodeGroups[i];
            if (forceRefresh == true || group.Rect.gameObject.activeInHierarchy == true)
            {
                group.Node.Refresh();
            }
        }
    }

    /// <summary>
    /// 表示内容を更新する（一行分）
    /// </summary>
    public void Refresh(int index, bool forceRefresh = false)
    {
        if (checkUsable() == false)
        {
            return;
        }
        if (nodeIndex.ContainsKey(index) == false)
        {
            return;
        }

        NodeGroup group = nodeIndex[index];
        if (forceRefresh == true || group.Rect.gameObject.activeInHierarchy == true)
        {
            group.Node.Refresh();
        }
    }

    /// <summary>
    /// 表示内容を更新する（一行分）
    /// </summary>
    public void Refresh(object obj, bool forceRefresh = false)
    {
        if (checkUsable() == false)
        {
            return;
        }

        foreach (var pair in nodeIndex)
        {
            NodeGroup group = pair.Value;
            if (group.Node.GetItem() == obj)
            {
                if (forceRefresh == true || group.Rect.gameObject.activeInHierarchy == true)
                {
                    group.Node.Refresh();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// テーブルの変更（追加、削除）を開始します
    /// </summary>
    public void BeginUpdateTable()
    {
        // 変更用テーブル情報を作成
        if (table == null)
        {
            Debug.LogError("table is null. cannot update.");
            return;
        }
        changeTable         = new List<object>(table);
        changeSelectedIndex = selectedIndex;
    }

    /// <summary>
    /// テーブルの変更（追加、削除）を完了します
    /// </summary>
    public void EndUpdateTable()
    {
        SetTable(changeTable);
        SetSelectedIndex(changeSelectedIndex);

        // 変更用テーブル情報をクリア
        changeTable         = null;
        changeSelectedIndex = -1;
    }

    /// <summary>
    /// テーブル行を追加します
    /// </summary>
    public void InsertRow(int index, object row)
    {
        if (changeTable == null)
        {
            Debug.LogError("no modify table. You need to call BeginTableModify().");
            return;
        }
        if (table.Count > 0 && table[0].GetType() != row.GetType())
        {
            Debug.LogError("does not match Table.");
            return;
        }

        index = index < 0 ? 0 : (index >= table.Count ? table.Count-1 : index);

        changeTable.Insert(index, row);
        changeSelectedIndex = index;
    }

    /// <summary>
    /// テーブル行を追加します
    /// </summary>
    public void AddRow(object row)
    {
        if (changeTable == null)
        {
            Debug.LogError("no modify table. You need to call BeginTableModify().");
            return;
        }
        if (table.Count > 0 && table[0].GetType() != row.GetType())
        {
            Debug.LogError("does not match Table.");
            return;
        }

        changeTable.Add(row);
        changeSelectedIndex = changeTable.Count-1;
    }

    /// <summary>
    /// テーブル行を削除します
    /// </summary>
    public void RemoveRow(object row)
    {
        if (changeTable == null || changeTable.Count == 0)
        {
            return;
        }
        
        // 削除する行を検索
        int index = changeTable.FindIndex( (a) => a == row );

        changeTable.RemoveAt(index);

        if (index < changeSelectedIndex)
        {
            // カーソル位置より上が消された場合、カーソルも１つ上に移動する
            changeSelectedIndex -= 1;
        }
        else
        if (changeSelectedIndex >= changeTable.Count-1)
        {
            // 最終行をオーバーしていた場合、最終行に合わせる
            changeSelectedIndex = changeTable.Count-1;
        }
    }

    /// <summary>
    /// 入力許可、禁止
    /// </summary>
    /// <param name="enabled">true..許可、false..禁止</param>
    public void InputEnabled(bool enabled)
    {
        CanvasGroup.blocksRaycasts = enabled;
    }

    /// <summary>
    /// スクロール感度の設定。0 だとすぐ止まり、1 だと止まらない
    /// </summary>
    /// <param name="rate">スクロール感度</param>
    public void SetDecelerationRate(float rate)
    {
        rate = Mathf.Clamp(rate, 0, 1);
        if (scrollRect != null)
        {
            scrollRect.decelerationRate = rate;
        }
    }

    /// <summary>
    /// パッドコントロールの更新
    /// </summary>
    void Update()
    {
        if (checkUsable() == false)
        {
            return;
        }
        int selIndex = selectedIndex;

        keyDownArgs.Flag = eKeyMoveFlag.None;

        if (CheckBlockRaycasts() == true)
        {
            OnKeyDown?.Invoke(keyDownArgs);
        }

        if (keyDownArgs.Flag == eKeyMoveFlag.Select)
        {
            performSelect();
        }
        else
        if (keyDownArgs.Flag == eKeyMoveFlag.Cancel)
        {
            select(selectedIndex, TableNodeElement.SUBINDEX_ROOT, true);
        }
        else
        {
            eKeyMoveFlag[] keys = new eKeyMoveFlag[6];
            if (Orientation == eOrientation.Vertical)
            {
                keys[0] = eKeyMoveFlag.Up;
                keys[1] = eKeyMoveFlag.Down;
                keys[2] = eKeyMoveFlag.PageUp;
                keys[3] = eKeyMoveFlag.PageDown;
                keys[4] = eKeyMoveFlag.Left;
                keys[5] = eKeyMoveFlag.Right;
            }
            else
            {
                keys[0] = eKeyMoveFlag.Left;
                keys[1] = eKeyMoveFlag.Right;
                keys[2] = eKeyMoveFlag.PageLeft;
                keys[3] = eKeyMoveFlag.PageRight;
                keys[4] = eKeyMoveFlag.Up;
                keys[5] = eKeyMoveFlag.Down;
            }

            if (reserveSelectedIndex >= 0)
            {
                selIndex = reserveSelectedIndex;
            }
            else
            if (keyDownArgs.Flag == keys[0])
            {
                if (--selIndex < 0)
                {
                    selIndex = 0;
                }
            }
            else
            if (keyDownArgs.Flag == keys[1])
            {
                if (++selIndex >= ItemCount)
                {
                    selIndex = ItemCount-1;
                }
            }
            else
            if (keyDownArgs.Flag == keys[2])
            {
                selIndex -= SkipIndexByPageScroll;
                if (selIndex < 0)
                {
                    selIndex = 0;
                }
            }
            else
            if (keyDownArgs.Flag == keys[3])
            {
                selIndex += SkipIndexByPageScroll;
                if (selIndex >= ItemCount)
                {
                    selIndex = ItemCount-1;
                }
            }
            else
            if (keyDownArgs.Flag == keys[4])
            {
                AddSubIndex(-1);
            }
            else
            if (keyDownArgs.Flag == keys[5])
            {
                AddSubIndex(1);
            }
        }

        if (selIndex != selectedIndex || reserveSelectedIndex >= 0)
        {
            if (Orientation == eOrientation.Vertical)
            {
                currentNormPos = scrollRect.verticalNormalizedPosition;
            }
            else
            {
                currentNormPos = scrollRect.horizontalNormalizedPosition;
            }
            targetNormPos = getTargetNormalizedPosition(selIndex);

            selectedIndex = selIndex;
            setFocus(selectedIndex);

            // SetSelectedIndex Mode
            var move = positionMoveMode;

            if (reserveSelectedIndex == -1)
            {
                // 通常の選択の場合
                move = ePositionMoveMode.ScrollMove;
            }

            if (move == ePositionMoveMode.DontMove)
            {
                // no operation
            }
            else
            if (move == ePositionMoveMode.ScrollMove)
            {
                OnCursorMove?.Invoke(table, selectedIndex, selectedSubIndex, true);
                if (SelectAfterFocus == false)
                {
                    OnSelect?.Invoke(table, selectedIndex, selectedSubIndex, false);
                }

                focusIsAnimation = true;
                timeNormPos = Time.time;
            }
            else
            if (move == ePositionMoveMode.OneFrame)
            {
                OnCursorMove?.Invoke(table, selectedIndex, selectedSubIndex, false);
                if (SelectAfterFocus == false)
                {
                    OnSelect?.Invoke(table, selectedIndex, selectedSubIndex, false);
                }

                timeNormPos = Time.time - ScrollTime;
            }
        }
        reserveSelectedIndex  = -1;

        // ターゲットに向かって減速移動
        if (timeNormPos > 0)
        {
            float t = Mathf.Clamp(Time.time - timeNormPos, 0, ScrollTime);
            float pos;

            if (t >= ScrollTime)
            {
                pos = targetNormPos;
                timeNormPos = 0;
            }
            else
            {
                pos = currentNormPos + (targetNormPos - currentNormPos) * cubicOut(t / ScrollTime);
            }

            if (Orientation == eOrientation.Vertical)
            {

                scrollRect.verticalNormalizedPosition = pos;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = pos;
            }
        }

        updateScrollbar();

        if (Orientation == eOrientation.Vertical)
        {
            scrollSize = rectGetHeight(scrollRectTransform);
        }
        else
        {
            scrollSize = rectGetWidth(scrollRectTransform);
        }
//viewerScroll(spos, false);
    }

    /// <summary>
    /// OnBeginDrag
    /// </summary>
    public void OnBeginDrag(PointerEventData data)
    {
        if (checkUsable() == false)
        {
            return;
        }
        // ユーザーがドラッグを開始したので、吸着は停止する
        if (co_autoTarget != null)
        {
            StopCoroutine(co_autoTarget);
        }
    }

    /// <summary>
    /// OnEndDrag
    /// </summary>
    public void OnEndDrag(PointerEventData data)
    {
        if (checkUsable() == false)
        {
            return;
        }
        // 不要なはずだが、一応
        if (co_autoTarget != null)
        {
            StopCoroutine(co_autoTarget);
        }

        // ドラッグが完了したら、自動吸着コルーチンを開始する
        if (AdsorptionTarget == true)
        {
            co_autoTarget = StartCoroutine(autoTarget());
        }
    }

    /// <summary>
    /// 親オブジェクトも含めて、blockRaycasts の確認
    /// </summary>
    public bool CheckBlockRaycasts()
    {
        if (checkUsable() == false)
        {
            return false;
        }
        foreach (CanvasGroup group in parentGroups)
        {
            if (group.blocksRaycasts == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 吸着メイン
    /// </summary>
    IEnumerator autoTarget()
    {
        while (true)
        {
            Vector2 velocity = scrollRect.velocity;
            float   v;
            // ベロシティが一定以下になるまで待つ
            if (Orientation == eOrientation.Vertical)
            {
                v = velocity.y;
            }
            else
            {
                v = velocity.x;
            }
            if (Math.Abs(v) < 750)
            {
                break;
            }
            yield return null;
        }

        int visibleNodeCount = 0;
        foreach (var node in nodeGroups)
        {
            if (node.Object.activeSelf == true)
            {
                visibleNodeCount++;
            }
        }

        int   sel0 = itemStart + visibleNodeCount/2;
        int   sel1 = itemStart + visibleNodeCount/2 - 1;
        if (sel0 < 0)
        {
            yield break;
        }
        if (sel1 < 0)
        {
            sel1 = sel0;
        }
        float tgt0 = getTargetNormalizedPosition(sel0);
        float tgt1 = getTargetNormalizedPosition(sel1);

        float pos;
        if (Orientation == eOrientation.Vertical)
        {
            pos = scrollRect.verticalNormalizedPosition;
        }
        else
        {
            pos = scrollRect.horizontalNormalizedPosition;
        }

        float targetPos;

        if (pos < 0)
        {
            targetPos = 0;
        }
        else
        if (pos > 1)
        {
            targetPos = 1;
        }
        else
        // より近い方に吸着する
        if (Math.Abs(tgt0 - pos) < Math.Abs(tgt1 - pos))
        {
            targetPos = tgt0;
        }
        else
        {
            targetPos = tgt1;
        }

        float time = Time.time;

        while (true)
        {
            float t = Time.time - time;
            float p;

            if (t >= ScrollTime)
            {
                p = targetPos;
            }
            else
            {
                p = pos + (targetPos - pos) * cubicOut(t / ScrollTime);
            }

            if (Orientation == eOrientation.Vertical)
            {
                scrollRect.verticalNormalizedPosition = p;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = p;
            }

            if (t >= ScrollTime)
            {
                break;
            }
            yield return null;
        }

        co_autoTarget = null;
    }

//Vector2 spos;
    /// <summary>
    /// スクロールされるとコール
    /// </summary>
    void onValueChanged(Vector2 pos)
    {
//spos = pos;
        viewerScroll(pos, false);
    }

    int findIndexOfNextLargerNumber(float n, List<RowDisplay> list)
    {
        int left = 0;
        int right = list.Count - 1;
        int result = 0;

        while (left <= right)
        {
            int mid = left + ((right - left) / 2);

            if (list[mid].LastPosition <= n)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
                result = mid;
            }
        }

        return result;
    }

    void viewerScroll(Vector2 pos, bool initialize)
    {
        if (checkUsable() == false)
        {
            return;
        }

        // 現在の Content 上の表示位置から、一番上に表示されるべき行番号を得る
        float top;
        int   itemIndex;

        if (Orientation == eOrientation.Vertical)
        {
            top =  scrollRect.content.transform.localPosition.y;
        }
        else
        {
            top = -scrollRect.content.transform.localPosition.x;
        }

        itemIndex = findIndexOfNextLargerNumber(top, rowDisplays);

        if (nodeIndex == null)
        {
            nodeIndex = new Dictionary<int, NodeGroup>();

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                NodeGroup group  = nodeGroups[i];
                int       rindex = itemIndex + i;

                redrawNode(group, rindex);
                nodeIndex.Add(rindex, group);
            }
        }
        else
        {
            var nindex = new Dictionary<int, NodeGroup>();

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                int rindex = itemIndex + i;
                if (nodeIndex.ContainsKey(rindex) == true)
                {
                    nindex.Add(rindex, nodeIndex[rindex]);
                    nodeIndex.Remove(rindex);
                }
                else
                {
                    nindex.Add(rindex, null);
                }
            }

//DDisp.Log($"{spos.y} {top} {rectGetWidth(scrollRectTransform)} {rectGetHeight(scrollRectTransform)}");
            var blankGroup = new List<NodeGroup>();
            foreach (var pair in nodeIndex)
            {
                blankGroup.Add(pair.Value);
            }

            for (int i = 0; i < nodeGroups.Count; i++)
            {
                int rindex = itemIndex + i;
                if (nindex[rindex] != null)
                {
                    // そのまま
                }
                else
                {
                    nindex[rindex] = blankGroup[0];
                    blankGroup.RemoveAt(0);

                    redrawNode(nindex[rindex], rindex);
                }

                var group = nindex[rindex];

                if (Orientation == eOrientation.Vertical)
                {
                    float y = rectGetY(group.Rect) + top;
                    float nodeSizeHalf = rectGetHeight(group.Rect) / 2;
//DDisp.Log($"{rindex} {group.Node.GetItemIndex()} {group.Object.name} {y <= -rectGetHeight(scrollRectTransform) - nodeSizeHalf} {y >= nodeSizeHalf} {y} {nodeSizeHalf} {-rectGetHeight(scrollRectTransform) - nodeSizeHalf}");
                    if (y <= -rectGetHeight(scrollRectTransform) - nodeSizeHalf || y >= nodeSizeHalf)
                    {
                        group.Object.SetActive(false);
//DDisp.Log($"kieta {group.Object.name}");
                    }
                    else
                    {
                        group.Object.SetActive(true);
                    }
                }
                else
                {
                    float x = rectGetX(group.Rect) - top;
                    float nodeSizeHalf = rectGetWidth(group.Rect) / 2;
//DDisp.Log($"{rindex} {group.Node.GetItemIndex()} {group.Object.name} {x <= -nodeSizeHalf} {x >= nodeSizeHalf} {x} {nodeSizeHalf} {rectGetWidth(scrollRectTransform) + nodeSizeHalf}");

                    if (x <= -nodeSizeHalf || x >= rectGetWidth(scrollRectTransform) + nodeSizeHalf)
                    {
                        group.Object.SetActive(false);
//DDisp.Log($"kieta {group.Object.name}");
                    }
                    else
                    {
                        group.Object.SetActive(true);
                    }
                }
            }

            nodeIndex = nindex;
        }
        
        if (initialize == false)
        {
            setFocus(selectedIndex);
        }
        else
        {
            setFocus(-1);
        }

        itemStart = itemIndex;
    }

    /// <summary>
    /// ノードグループを再描画
    /// </summary>
    /// <param name="group">ノードグループ</param>
    /// <param name="rindex">テーブルの先頭から何行目か</param>
    void redrawNode(NodeGroup group, int rindex)
    {
        if (rindex < 0 || rindex >= ItemCount)
        {
            // out of range
        }
        else
        {
            group.Object.SetActive(true);
            group.Node.SetItemIndex(rindex);

            if (Orientation == eOrientation.Vertical)
            {
                rectSetY(group.Rect, -(getPos(rindex)));
            }
            else
            {
                rectSetX(group.Rect,   getPos(rindex) - group.Rect.rect.x);
            }
        }
    }

    /// <summary>
    /// マウスカーソルが選択項目上に入った時にコール
    /// </summary>
    void nodeEnter(TableNodeElement searchkey)
    {
        if (timeNormPos > 0)
        {
            // キーで選択（移動）中はマウスイベントを禁止
            return;
        }

        NodeGroup search = null;
        if (searchkey != null && nodeSearch.ContainsKey(searchkey) == true)
        {
            search = nodeSearch[searchkey];
        }

        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group = nodeGroups[i];
            if (group == search)
            {
                if (selectedIndex != group.Node.GetItemIndex())
                {
                    selectedIndex = group.Node.GetItemIndex();
                    OnCursorMove?.Invoke(table, selectedIndex, selectedSubIndex, true);
                }

                selectedNodeGroup = group;
                group.Node.SetFocus(true, true);
            }
            else
            {
                group.Node.SetFocus(false);
            }
        }

    }
    
    /// <summary>
    /// 選択された時にコール
    /// </summary>
    void nodeClick(TableNodeElement node)
    {
        select(node.GetItemIndex(), node.GetSubIndex(), false);
    }

    /// <summary>
    /// 選択（or キャンセル）
    /// </summary>
    /// <param name="itemIndex">選択された行</param>
    /// <param name="subIndex"></param>
    /// <param name="isCancel">キャンセルであれば true</param>
    void select(int itemIndex, int subIndex, bool isCancel)
    {
        selectedSubIndex = subIndex;
        OnSelect?.Invoke(table, itemIndex, subIndex, isCancel);

        if (DisabledAfterSelect == true)
        {
            CanvasGroup.blocksRaycasts = false;
        }
    }
    
    /// <summary>
    /// フォーカス設定
    /// </summary>
    void setFocus(int selIndex)
    {
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group = nodeGroups[i];

            if (selIndex >= 0 && selIndex == group.Node.GetItemIndex())
            {
                selectedNodeGroup = group;

                group.Node.SetSubIndex(selectedSubIndex);
                group.Node.SetFocus(true, focusIsAnimation);
                focusIsAnimation = false;
            }
            else
            {
                group.Node.SetFocus(false);
            }
        }
    }

    /// <summary>
    /// 項目選択
    /// </summary>
    void performSelect()
    {
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup group = nodeGroups[i];

            if (group.Node.GetFocus() == true)
            {
                group.Node.PerformClick(group.Node.GetSubIndex());
                break;
            }
        }
    }

    /// <summary>
    /// 使用するための最低限の設定があるかチェック
    /// </summary>
    /// <returns>true..使用可能</returns>
    bool checkUsable()
    {
        if (CanvasGroup == null)
        {
            return false;
        }
        if (SourceNode == null)
        {
            return false;
        }
        if (table == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 行番号から、項目が Content のどのポジションにあるべきか計算し、返す
    /// </summary>
    float getPos(int no)
    {
        if (no < 0 || no >= rowDisplays.Count)
        {
            return paddingTop;
        }
        else
        {
            return paddingTop + rowDisplays[no].Position;
        }
    }

    /// <summary>
    /// 指定された項目番号の NormalizedPosition を算出する
    /// </summary>
    /// <param name="selIndex">項目番号</param>
    /// <returns>Target Normalized Position</returns>
    float getTargetNormalizedPosition(int selIndex)
    {
        float contentCenter;
        
        if (Orientation == eOrientation.Vertical)
        {
            contentCenter = contentSize - getPos(selIndex); // - nodeSize * 0.5f;
        }
        else
        {
            var rowdisp = rowDisplays[selIndex];
            contentCenter = contentSize - getPos(selIndex) - rowdisp.Size / 2; // - nodeSize * 0.5f;
        }
        float scrollCenter  = scrollSize * 0.5f;

        if (Orientation == eOrientation.Vertical)
        {
            return Mathf.Clamp01((contentCenter - scrollCenter) / (contentSize - scrollSize));
        }
        else
        {
            return 1 - Mathf.Clamp01((contentCenter - scrollCenter) / (contentSize - scrollSize));
        }
    }
    
    /// <summary>
    /// rect control
    /// </summary>
    float rectGetWidth(RectTransform rect)
    {
        return rect.rect.size.x;
    }
    float rectGetHeight(RectTransform rect)
    {
        return rect.rect.size.y;
    }
    void rectSetWidth(RectTransform rect, float width)
    {
        var size = rect.sizeDelta;
        size.x = width;
        rect.sizeDelta = size;
    }
    void rectSetHeight(RectTransform rect, float height)
    {
        var size = rect.sizeDelta;
        size.y = height;
        rect.sizeDelta = size;
    }
    void rectSetX(RectTransform rect, float x)
    {
        Vector3 trans = rect.gameObject.transform.localPosition;
        trans.x = x;
        rect.gameObject.transform.localPosition = trans;
    }
    void rectSetY(RectTransform rect, float y)
    {
        Vector3 trans = rect.gameObject.transform.localPosition;
        trans.y = y;
        rect.gameObject.transform.localPosition = trans;
    }
    float rectGetX(RectTransform rect)
    {
        return rect.gameObject.transform.localPosition.x;
    }
    float rectGetY(RectTransform rect)
    {
        return rect.gameObject.transform.localPosition.y;
    }

    float cubicOut(float t)
    {
        t -= 1;
        float v = t * t * t + 1;
        return v;
    }

}
