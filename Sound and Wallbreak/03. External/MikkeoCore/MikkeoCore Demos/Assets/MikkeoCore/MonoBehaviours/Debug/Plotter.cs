/*
 * Use of this requires Squiggle to be installed:
 * 
 * https://www.assetstore.unity3d.com/en/#!/content/21970
 * 
 */ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mikkeo.Extensions;

public sealed class Plotter : Singleton<Plotter> {
	public List<string> labelKeywords = new List<string> () { 
		//"facing",
		//"inverse",
		"screenPadding",
		//"control",
		//"ExplosionForce",
		//"speed",
		//"track speed",
		//"stuck",
		//"thrust"
		//"velocity"
		//"thrust percent" 
		//"ang"
	};

	void Awake () {
	}

	public void Plot (string label, float value) {

		if (IsPlotting(label)) { }
        //Need to install Squiggle to get this working again
        //			DebugGraph.Log (label, value);
    }

	public void MultiPlot (string label, Color colour, float value) {
		if (IsPlotting(label)) { }
        //Need to install Squiggle to get this working again
        //			DebugGraph.MultiLog (label, colour, value);
    }

    bool IsPlotting (string label) {
				
		bool _isPlotting = false;
		string lowerLabel = label.ToLower ();

		for (int i = 0; i < labelKeywords.Count; i++) {
			string lowerKeyword = labelKeywords [i].ToLower ();
			if (lowerLabel.Contains (lowerKeyword)) {
				_isPlotting = true;
			}
		}

		return _isPlotting;
	}
}