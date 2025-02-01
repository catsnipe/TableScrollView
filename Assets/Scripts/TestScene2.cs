using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TestScene2 : MonoBehaviour
{
    [SerializeField]
    TableScrollViewer   viewer;

    public class Row
    {
        public int No;
        public string PlaceName;
    }

    List<Row> rows = new List<Row>()
    {
        new Row() { No = 1, PlaceName = "Amsterdam" },
        new Row() { No = 2, PlaceName = "Bangkok" },
        new Row() { No = 3, PlaceName = "Chicago" },
        new Row() { No = 4, PlaceName = "Dubai" },
        new Row() { No = 5, PlaceName = "Edinburgh" },
        new Row() { No = 6, PlaceName = "Frankfurt" },
        new Row() { No = 7, PlaceName = "Geneva" },
        new Row() { No = 8, PlaceName = "Hong Kong" },
        new Row() { No = 9, PlaceName = "Istanbul" },
        new Row() { No = 10, PlaceName = "Washington, D.C." },
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        viewer?.Initialize();
        viewer?.SetTable(rows);
        viewer?.OnSelect.AddListener(OnSelectVertical);
        viewer?.OnKeyDown.AddListener(OnKeyDown);
    }

    public void OnSelectVertical(List<object> table, int itemIndex, int subIndex, bool isCancel)
    {
        Row row = (Row)table[itemIndex];

        if (subIndex < 0)
        {
            Debug.Log($"selected: {row.No} {row.PlaceName}");
        }
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
