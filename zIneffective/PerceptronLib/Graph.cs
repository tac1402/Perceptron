// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System.IO;
using System.Collections.Generic;

namespace Tac.Perceptron
{
	public class Graph
	{
		public Dictionary<int, string> Nodes = new Dictionary<int, string>();
		public Dictionary<int, List<int>> Branches = new Dictionary<int, List<int>>();

		public void Save(string argName)
		{
			string file = "";
			foreach (var node in Nodes)
			{
				file += node.Key + " " + node.Value + "\n";
			}
			file += "#\n";
			foreach (var branch in Branches)
			{
				foreach (int b in branch.Value)
				{
					file += branch.Key.ToString() + " " + b.ToString() + "\n";
				}
			}
			File.WriteAllText(argName + ".tgf", file);
		}

		public void AddBranch(int argL1, int argL2, string argAdd)
		{
			if (Nodes.ContainsKey(argL1) == false)
			{
				Nodes.Add(argL1, argL1.ToString());
			}
			if (Nodes.ContainsKey(argL2) == false)
			{
				Nodes.Add(argL2, argL2.ToString());
			}
			Nodes[argL2] += argAdd;

			if (Branches.ContainsKey(argL1) == false)
			{ 
				Branches[argL1] = new List<int>();
			}
			Branches[argL1].Add(argL2);
		}

	}
}
