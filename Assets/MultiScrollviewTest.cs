﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiScrollviewTest : MonoBehaviour
{
    [SerializeField]
    TableScrollViewer    viewer;

    List<object>         viewerList = new List<object>();

    void Awake()
    {
        for (int i = 0; i < 100; i++)
        {
           viewerList.Add(i);
        }

        viewer?.Initialize();
        viewer?.SetTable(viewerList);
        viewer?.OnSelect.AddListener(OnSelectVertical);
        viewer?.OnKeyDown.AddListener(OnKeyDown);
    }

    static string[] items = { "ITEM", "A", "B", "C" };

    public void OnSelectVertical(List<object> table, int itemIndex, int subIndex, bool isCancel)
    {
        int row = (int)table[itemIndex];
        Debug.Log($"multi selected: {row+1} - {items[subIndex+1]}");
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
        else
        if (Input.GetKeyDown(KeyCode.LeftArrow) == true)
        {
            args.Flag = TableScrollViewer.eKeyMoveFlag.Left;
        }
        else
        if (Input.GetKeyDown(KeyCode.RightArrow) == true)
        {
            args.Flag = TableScrollViewer.eKeyMoveFlag.Right;
        }
    }
}
