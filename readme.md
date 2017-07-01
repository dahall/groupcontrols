**Project Description**
WinForms controls that display multiple sub-controls without creating a unique window handle for each child. Instead each child is drawn using the default renderers and its space and status are managed by the parent. Currently there are the following controls:
* RadioButtonList (similar the ASP.NET control)
* CheckBoxList

![](Home_GroupControls.jpg)

In the Source Code, you will find an example project. Of note along with the controls are two generic classes. The first is a clone of List<T> called EventedList<T>. It has all the same methods, but adds events on all changes to the list or its items. The second is a SparseArray<T> that behaves the same as a List<T>, but is built on top of a Dictionary<T> so that you can address items that have not been added and have non-sequential indexes. Both classes can be found in the source code.