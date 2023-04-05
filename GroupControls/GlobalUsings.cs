global using System;
global using System.CodeDom;
global using System.Collections;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.ComponentModel.Design;
global using System.Drawing;
global using System.Reflection;
global using System.Runtime.InteropServices;
global using System.Windows.Forms;
global using System.Windows.Forms.Layout;
#if NETFRAMEWORK
global using CodeDomSerializer = System.ComponentModel.Design.Serialization.CodeDomSerializer;
//global using CollectionEditor = System.ComponentModel.Design.CollectionEditor;
#else
global using CodeDomSerializer = Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;
//global using CollectionEditor = Microsoft.DotNet.DesignTools.Editors.CollectionEditor;
#endif
