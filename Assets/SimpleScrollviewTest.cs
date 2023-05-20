using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleScrollviewTest : MonoBehaviour
{
    [SerializeField]
    TableScrollViewer    viewer;

    List<object>         viewerList = new List<object>();

    void Awake()
    {
        for (int i = 0; i < 1000; i++)
        {
           viewerList.Add(i);
        }

        viewer?.Initialize();
        viewer?.SetTable(viewerList);
        viewer?.OnSelect.AddListener(OnSelectVertical);
        viewer?.OnKeyDown.AddListener(OnKeyDown);
    }

    public void OnSelectVertical(List<object> table, int itemIndex, int subIndex, bool isCancel)
    {
        int row = (int)table[itemIndex];
        Debug.Log($"selected: {row}");
    }

    public void OnKeyDown(TableScrollViewer.KeyDownArgs args)
    {
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            args.Flag = TableScrollViewer.eKeyMoveFlag.Select;
        }
        else
        if (Input.GetKeyDown(KeyCode.UpArrow) == true)
        {
            args.Flag = TableScrollViewer.eKeyMoveFlag.Up;
        }
        else
        if (Input.GetKeyDown(KeyCode.DownArrow) == true)
        {
            args.Flag = TableScrollViewer.eKeyMoveFlag.Down;
        }
    }
}
