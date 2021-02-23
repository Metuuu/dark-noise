using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rule {

	List<string> restrictions = new List<string>();

	public Rule(List<string> reasonsNotAllowed = null) {
		if (reasonsNotAllowed != null) {
			reasonsNotAllowed.ForEach(DontAllow);
		}
	}

	public Rule(string reasonNotAllowed = null) {
		if (reasonNotAllowed != null) {
			DontAllow(reasonNotAllowed);
		}
	}

	public bool IsAllowed {
		get { return restrictions.Count == 0; }
	}

	public void DontAllow(string reason) {
		if (!restrictions.Contains(reason)) {
			restrictions.Add(reason);
		}
	}

	public void Allow(string reasonNotAllowed) {
		restrictions.Remove(reasonNotAllowed);
	}

}
