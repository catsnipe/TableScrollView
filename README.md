# TableScrollView
UI / ScrollView extension class for unity

![image](https://www.create-forever.games/wp-content/uploads/2021/02/image-36.png)

## requirement
unity2019 or later  

## features
* Graphics acceleration. (contents of 10000 lines is also possible)
* Supports horizontal and vertical scroll views.
* Supports keyboard input. (LRUD / PageScroll / Confirm / Cancel)
* Rows can be added / deleted while the table is displayed. (01/03/2022)
### (jp)
* 10000行あっても処理落ちしないようグラフィックの高速化を図っています
* 横、または縦のスクロールビューを作成できます
* キーボード入力とマウス（タッチ）入力が同居できるよう設計されています
* テーブル表示中に行の追加・削除が行えるよう改善しました (2022/01/03)
## usage
1. Copy to 'Assets/'.  
  **TableNodeElement.cs**  
  **TableScrollViewer.cs**  
  **TableScrollViewerScrollbar.cs**  
  **TableSubNodeElement.cs**  
2. Create content node(TableNodeElement)  
3. Attach TableScrollViewer  

see more detail (japanese): https://www.create-forever.games/table-scroll-viewer/
