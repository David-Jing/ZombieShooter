using UnityEngine;
using System.Collections;
using Mystery.Graphing;
using System.Collections.Generic;

namespace Mystery.Graphing
{
    public class DebugGraphRenderer : IGraphConsoleRenderer
    {
        public string GraphName = typeof(double).Name;

        protected override IGraphConsole LoadGraph()
        {
            foreach (IGraphConsole console in DebugGraph.GetGraphEnumerator())
            {
                if (console.Name == GraphName)
                    return console;
            }
            return null;
        }
    }
}