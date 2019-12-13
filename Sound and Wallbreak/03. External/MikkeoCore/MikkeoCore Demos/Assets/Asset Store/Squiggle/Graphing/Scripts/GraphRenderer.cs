using UnityEngine;
using System.Collections;
using Mystery.Graphing;
using System.Collections.Generic;

namespace Mystery.Graphing
{
    public class GraphRenderer : IGraphConsoleRenderer
    {
        //TODO: Make Graph Console Asset? (needs to be serialable)
        public GraphConsole GraphConsole;

        protected override IGraphConsole LoadGraph()
        {
            return GraphConsole;
        }
    }
}