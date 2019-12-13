using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mystery.Graphing;

namespace Mystery.Graphing
{
    [CustomEditor(typeof(DebugGraphRenderer), true)]
    public class GraphRendererEditor : IGraphConsoleRendererEditor
    {
    }
}
