using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointNavigationManager : MonoBehaviour
{
	#region Properties
	
	public List<Waypoint> AllWaypoints {
		get { return this.waypoints; }
	}
	
	public NodeGraph WaypointNodeGraph {
		get { return this.nodeGraph; }
	}

	public GameObject TheGround = null;

	#endregion
	
	#region Public static methods
	
	static WaypointNavigationManager theInstance = null;
	
	public static WaypointNavigationManager GetInstance ()
	{
		if (theInstance == null) {
			// Create a new monobehaviour instance here
			GameObject go = GameObject.Find ("Navigation");
			if (go == null) {
				go = new GameObject ("Navigation");
			}
			WaypointNavigationManager newWPManager = go.GetComponent<WaypointNavigationManager>();
			if(newWPManager == null)
			{
				Debug.LogError("No instance of " + theInstance.GetType ().ToString () + " found!");
			}
			newWPManager.Init();
		}
			
		return theInstance;
	}
	
	#endregion
	
	#region Construction
	void Start ()
	{
		Init();
	}
	
	private void Init()
	{
		if (theInstance != null && theInstance != this) {
			Debug.Log ("Multiple instances of " + this.GetType ().ToString () + ". Destroying instance residing on gameobject '" + gameObject.name + "'. Original resides on '" + theInstance.transform.name + "'");
			Destroy (this);
		} else {
			theInstance = this;
		}
		
		// Gather all nodes
		this.waypoints = ScriptAssistant.GetAllInstancesWithType<Waypoint> ();
		
		// Create graph
		this.nodeGraph = new NodeGraph();
		this.nodeGraph.CustomEdgeWeightFunction = this.CalculateEdgeWeight;
		
		// First add nodes
		foreach(var waypoint in this.waypoints)
		{
			NavigationNode navNode = new NavigationNode(waypoint.WorldPosition);
			this.nodeGraph.AddNode(navNode);
			this.nodeTranslationTable[waypoint] = navNode;
			this.waypointTranslationTable[navNode] = waypoint;
		}
		// Then create edges
		foreach(var waypoint in this.waypoints)
		{
			foreach(var neighbour in waypoint.Neighbours)
			{
				try 
				{
					this.nodeGraph.CreateEdge(this.nodeTranslationTable[waypoint], this.nodeTranslationTable[neighbour]);
				} 
				catch (UnityException e)
				{
					Debug.LogException(e);
				}
			}
		}
	}
	
	#endregion
	
	#region Public methods
	
	public Waypoint FindClosestWaypoint(Vector3 worldPosition)
	{
		Waypoint closest = null;
		float closestSqrDistance = float.MaxValue;
		foreach(Waypoint wp in this.waypoints)
		{
			float tmpDistance = Vector3.SqrMagnitude(worldPosition - wp.transform.position);
		
			if(tmpDistance < closestSqrDistance)
			{
				closest = wp;
				closestSqrDistance = tmpDistance;
			}
		}
		
		return closest;
	}

	/// <summary>
	/// Finds the shortest path between start and destination. 
	/// </summary>
	/// <returns>A list of waypoints that produce the shortest path from start to destination. Null if no path could be resolved.</returns>
	/// <param name="start">Start.</param>
	/// <param name="destination">Destination.</param>
	public List<Waypoint> FindShortestPathBetween(Waypoint start, Waypoint destination)
	{
		var nodePath = this.FindShortestPath(this.nodeTranslationTable[start], this.nodeTranslationTable[destination]);
		if(nodePath == null) return null;

		List<Waypoint> waypointPath = new List<Waypoint>(); 
		for(int i = nodePath.Count-1; i > -1; i--)
		{
			waypointPath.Add(this.waypointTranslationTable[nodePath[i]]);
		}
		
		return waypointPath;
	}
	
	public void FindShortestPathBetweenUsingUpdate(Waypoint start, Waypoint destination)
	{
		this.FindShortestPathUsingpdate(this.nodeTranslationTable[start], this.nodeTranslationTable[destination]);
	}
	
	#endregion
	
	#region Operations
	
	private List<NavigationNode> FindShortestPath(NavigationNode start, NavigationNode destination)
	{
		PriorityQueue<NavigationNode> openSet = new PriorityQueue<NavigationNode>();
		PriorityQueue<NavigationNode> closedSet = new PriorityQueue<NavigationNode>();
		Dictionary<NavigationNode, float> accumulatedWeight = new Dictionary<NavigationNode, float>();
		
		openSet.Enqueue(start, CalculateEdgeWeight(start, destination));
		accumulatedWeight[start] = 0f;
		
		bool arrived = false;
		while(!arrived)
		{
			// Investigate the next node with the lowest weight
			PriorityQueue<NavigationNode>.PriorityQueueItem currentNode = null;
			if(!openSet.TryDequeueLowest(out currentNode))
			{
				// Algorithm could not reach the final node.
				// This probably means that there is not path between requested start & destination nodes.
				return null;
			}

			if(currentNode.Item.Equals(destination))
			{
				arrived = true;
			}
			else
			{
				// Evaluate all neighbours
				foreach(var edge in currentNode.Item.Edges)
				{
					NavigationNode neighbourNode = edge.NodeA;
					if(currentNode.Item.Equals(edge.NodeA)) neighbourNode = edge.NodeB;
					float nodeWeight = accumulatedWeight[currentNode.Item] + edge.EdgeWeight; 
					float heuristicWeight = CalculateEdgeWeight(neighbourNode, destination);
					
					if(!closedSet.allItems.Contains(neighbourNode))
					{
						if(accumulatedWeight.ContainsKey(neighbourNode))
						{
							if(nodeWeight < accumulatedWeight[neighbourNode])
								accumulatedWeight[neighbourNode] = nodeWeight;
						} else accumulatedWeight[neighbourNode] = nodeWeight;
						
						// Store node in "TODO" list
						openSet.Enqueue(neighbourNode, nodeWeight + heuristicWeight);
					}
				}
			}
			// Current node has already been visited, make sure we dont visit it again
			closedSet.Enqueue(currentNode.Item, currentNode.PriorityWeight);
		}
		
		// All nodes between start and destination has been weighed. 
		// Now its time to find a path!
		List<NavigationNode> result = new List<NavigationNode>();
		bool pathComplete = false;
		NavigationNode indexNode = destination;
		while(!pathComplete)
		{
			result.Add(indexNode);
			if(indexNode.Equals(start))
			{
				pathComplete = true;
			} 
			else
			{
				// Find cheapest neighbour
				NavigationNode cheapestNeighbour = null;
				foreach(var edge in indexNode.Edges)
				{
					NavigationNode neighbourNode = edge.NodeA;
					if(indexNode.Equals(edge.NodeA)) neighbourNode = edge.NodeB;
					
					if(accumulatedWeight.ContainsKey(neighbourNode))
					{
						if(cheapestNeighbour == null) cheapestNeighbour = neighbourNode;
						
						if(accumulatedWeight[cheapestNeighbour] > accumulatedWeight[neighbourNode])
							cheapestNeighbour = neighbourNode;
					}
				}
				
				indexNode = cheapestNeighbour;
			}
		}
		
		return result;
	}
	
	private float CalculateEdgeWeight(NavigationNode a, NavigationNode b)
	{
		return Vector3.Magnitude(a.WorldPosition - b.WorldPosition);
	}
	
	#endregion
	
	#region FindPath In Update with visual indication // TODO: Remove region
	
	private PriorityQueue<NavigationNode> openSetInUpdate = null;
	private PriorityQueue<NavigationNode> closedSetInUpdate = null;
	private NavigationNode destinationNodeInUpdate = null;
	private bool arrivedInUpdate = true;
	private System.DateTime iterationStartTime;
	private void FindShortestPathUsingpdate(NavigationNode start, NavigationNode destination)
	{	
		destinationNodeInUpdate = destination;
		openSetInUpdate = new PriorityQueue<NavigationNode>();
		closedSetInUpdate = new PriorityQueue<NavigationNode>();
		
		openSetInUpdate.Enqueue(start, CalculateEdgeWeight(start, destination));
		
		iterationStartTime = System.DateTime.Now;
		
		// Start the algorithm
		arrivedInUpdate = false;
	}
	
	void Update()
	{
		if(!arrivedInUpdate && (System.DateTime.Now - iterationStartTime) > System.TimeSpan.FromMilliseconds(300))
		{
			iterationStartTime = System.DateTime.Now;
			var currentNode = openSetInUpdate.DequeueLowest();
			this.waypointTranslationTable[currentNode.Item].IsCurrent = true;
			
			if(currentNode.Item.Equals(destinationNodeInUpdate))
			{
				arrivedInUpdate = true;
				Debug.Log("SUCCE!!!!!!!1 Faaaaaan vad bra!!!1");
				this.waypointTranslationTable[currentNode.Item].IsCurrent = true;
			}
			else
			{
				foreach(var edge in currentNode.Item.Edges)
				{
					NavigationNode neighbourNode = edge.NodeA;
					if(currentNode.Item.Equals(edge.NodeA)) neighbourNode = edge.NodeB;
					float nodeWeight = currentNode.PriorityWeight 
						- CalculateEdgeWeight(currentNode.Item, destinationNodeInUpdate)
						+ edge.EdgeWeight 
						+ CalculateEdgeWeight(neighbourNode, destinationNodeInUpdate);
					
					if(!closedSetInUpdate.allItems.Contains(neighbourNode))
						openSetInUpdate.Enqueue(neighbourNode, nodeWeight);
				}
			}
			// Current node has already been visited, make sure we dont visit it again
			closedSetInUpdate.Enqueue(currentNode.Item, currentNode.PriorityWeight);
		}	
	}
	
	#endregion
	
	#region Editor events
		
	void OnDrawGizmos()
	{
		/*
		if(this.closedSetInUpdate != null && arrivedInUpdate == false)
		{
			foreach(var node in this.openSetInUpdate.allItems)	
				this.waypointTranslationTable[node].IsOpen = true;
			foreach(var node in this.closedSetInUpdate.allItems)
			{
				//this.waypointTranslationTable[node].IsClosed = true;
				foreach(var edge in node.Edges)
					Gizmos.DrawLine(edge.NodeA.WorldPosition, edge.NodeB.WorldPosition);
			}
		}
		*/
		if(this.nodeGraph != null && this.nodeGraph.AllEdges != null)
		{
			foreach(var edge in this.nodeGraph.AllEdges)
			{
				Gizmos.DrawLine(edge.NodeA.WorldPosition, edge.NodeB.WorldPosition);
			}
		}
		
	}
	
	void OnGUI()
	{
		if(this.closedSetInUpdate != null && arrivedInUpdate == false)
		{
			foreach(var node in this.closedSetInUpdate.PriorityItems)	
			{
				Rect r = new Rect(0,0,100,100);
				var scrPoint = Camera.current.WorldToScreenPoint(this.waypointTranslationTable[node.Item].WorldPosition);
				r.x = scrPoint.x;
				r.y = Screen.height - scrPoint.y;
				GUIStyle gs = new GUIStyle();
				gs.normal.textColor = Color.black;
				GUI.Label(r, string.Format("{0:f1}", (node.PriorityWeight - CalculateEdgeWeight(destinationNodeInUpdate, node.Item))), gs);
			}
		}
	}
	#endregion
	
	#region Data
	
	private Dictionary<Waypoint, NavigationNode> nodeTranslationTable = new Dictionary<Waypoint, NavigationNode>(); 
	private Dictionary<NavigationNode, Waypoint> waypointTranslationTable = new Dictionary<NavigationNode, Waypoint>(); 
	private List<Waypoint> waypoints;
	private NodeGraph nodeGraph;
	
	#endregion
} 

/// <summary>
/// Edge weight function.
/// </summary>
public delegate float EdgeWeightFunction(NavigationNode a, NavigationNode b);

/// <summary>
/// Maintains a collection of edges & nodes. Edges can be added and removed without storing duplicates. The collection knows which edges 
/// are connected to which nodes.
/// </summary>
public class NodeGraph
{
	
	#region Properties
	
	public List<NavigationEdge> AllEdges {
		get { return this.allEdges; }
	}
	
	public List<NavigationNode> AllNodes
	{
		get { return this.allNodes; }
	}
	
	/// <summary>
	/// Gets or sets a custom edge weight function. The standard function used is the distance between NodeA & NodeB.
	/// </summary>
	/// <value>
	/// The custom edge weight function.
	/// </value>
	public EdgeWeightFunction CustomEdgeWeightFunction {
		get;
		set;
	}
	
	#endregion
	
	#region Construction
	
	public NodeGraph ()
	{
		this.allEdges = new List<NavigationEdge> ();
		this.allNodes = new List<NavigationNode> ();
	}
	
	#endregion
	
	#region Public methods
	
	public void AddNode(NavigationNode newNode)
	{
		this.allNodes.Add(newNode);	
	}
	
	/// <summary>
	/// Removes a node and all connected edges
	/// </summary>
	/// <param name='node'>
	/// Node.
	/// </param>
	public void RemoveNode(NavigationNode node)
	{
		if(node.Edges != null)
		{
			foreach(var edge in node.Edges)
			{
				this.allEdges.Remove(edge);
			}
		}
		this.allNodes.Remove(node);
	}
	
	public bool CreateEdge (NavigationNode nodeA, NavigationNode nodeB)
	{
		// Check if it already exists
		var edgesForNode = nodeA.Edges;
		if (edgesForNode != null) {
			foreach (var edge in edgesForNode) {
				// Only need to check for B since edgesForNode only shows edges where A is involved
				if (edge.ContainsNode (nodeB)) 
					return false;
			}
		}
		
		// Create the edge
		NavigationEdge bleedingEdge = CustomEdgeWeightFunction == null 
			? new NavigationEdge (nodeA, nodeB) : new NavigationEdge(nodeA, nodeB, CustomEdgeWeightFunction);
		
		// Store it
		this.allEdges.Add (bleedingEdge);
		nodeA.Edges.Add (bleedingEdge);
		nodeB.Edges.Add (bleedingEdge);
		
		return true;
	}
	
	/// <summary>
	/// Removes the edge between Node A & B.
	/// </summary>
	/// <param name='a'>
	/// A.
	/// </param>
	/// <param name='b'>
	/// B.
	/// </param>
	public void RemoveEdge(NavigationNode a, NavigationNode b)
	{
		NavigationEdge navEdge = null;
		var edges = a.Edges;
		if(edges != null)
		{
			foreach(var ed in edges)
			{
				if(ed.ContainsNode(b))
				{
					navEdge = ed;
					break;
				}
			}
		}
		
		if(navEdge != null) RemoveEdge(navEdge);
	}
	
	/// <summary>
	/// Removes an edge.
	/// </summary>
	/// <param name='edge'>
	/// NavigationEdge to be removed.
	/// </param>
	public void RemoveEdge (NavigationEdge edge)
	{
		edge.NodeA.Edges.Remove(edge);
		edge.NodeB.Edges.Remove(edge);
		
		if (this.allEdges.Contains (edge))
			this.allEdges.Remove (edge);
	}
	
	public List<NavigationEdge> GetEdgesInvolvingNode (NavigationNode node)
	{
		List<NavigationEdge> result = node.Edges; 
		
		return result;
	}
	
	#endregion
	
	#region Data
	
	private List<NavigationEdge> allEdges = null;
	private List<NavigationNode> allNodes = null;
	
	#endregion
}
/*
/// <summary>
/// Navigation path. A list of NavigationNodes
/// </summary>
public class NavigationPath : Stack<NavigationNode>
{
	#region Properties
	
	public float TotalWeight {
		get { return this.totalWeight; }
	}
	
	#endregion
	
	#region Construction
	
	public NavigationPath ()
	{
	}
	
	#endregion
	
	#region Overrides
	
	public NavigationNode Peek()
	{
		NavigationNode result = base.Peek();
		
		return result;
	}
	
	public NavigationNode Pop()
	{
		NavigationNode result = base.Pop();
		if(base.Count > 0)
		{
			var lastNode = base.Peek();
			foreach(var edge in lastNode.Edges)
			{
				if(edge.ContainsNode(result))
				{
					totalWeight -= edge.EdgeWeight;
				}
			}
		}
		return result;
	}
	
	public void Push(NavigationNode node)
	{
		var lastNode = base.Peek();
		foreach(var edge in lastNode.Edges)
		{
			if(edge.ContainsNode(node))
			{
				totalWeight += edge.EdgeWeight;
			}
		}
		
		base.Push(node);
	}
	
	#endregion
	
	#region Data
	
	float totalWeight = 0f;
	
	#endregion
}
 */
/// <summary>
/// Navigation edge. Representation of a link between two <see cref="Waypoint"/>.
/// </summary>
/// <exception cref='UnityException'>
/// Is thrown when Node A & B are the same.
/// </exception>
public class NavigationEdge
{
	#region Properties
	
	public NavigationNode NodeA {
		get;
		set;
	}
	
	public NavigationNode NodeB {
		get;
		set;
	}
	
	/// <summary>
	/// Gets the edge weight.
	/// </summary>
	/// <value>
	/// The edge weight. This weight is NOT the distance between the nodes.
	/// </value>
	public float EdgeWeight {
		get { return this.weight; }
		set { this.weight = value; }
	}
	
	/// <summary>
	/// Gets the edge distance.
	/// </summary>
	/// <value>
	/// The distance between node A & B. This is a slower operation than using the weight.
	/// </value>
	public float EdgeDistance {
		get { return Mathf.Sqrt (this.weight); }
	}
	
	#endregion
	
	#region Construction
	
	public NavigationEdge (NavigationNode a, NavigationNode b)
	{
		Init(a, b);	
	}
	
	public NavigationEdge (NavigationNode a, NavigationNode b, EdgeWeightFunction customWeightCallback)
	{
		this.customWeightCallback = customWeightCallback;
		Init(a, b);
	}
	
	private void Init(NavigationNode a, NavigationNode b)
	{
		this.NodeA = a;
		this.NodeB = b;
		
		if (a == b)
			throw new UnityException ("Cannot create edge if node a & b are the same.");
		
		RecalculateWeight();
	}
	
	#endregion
	
	#region Public methods
	
	public bool ContainsNode (NavigationNode node)
	{
		return node == NodeA || node == NodeB;
	}
	
	/// <summary>
	/// Recalculates the weight. A custom weight function can be set using the constructor. 
	/// </summary>
	public void RecalculateWeight()
	{
		if(this.customWeightCallback != null)
		{
			this.EdgeWeight = this.customWeightCallback(NodeA, NodeB);
		}
		else
		{
			this.weight = Vector3.Distance (NodeA.WorldPosition, NodeB.WorldPosition);
		}
	}
	
	#endregion
	
	#region Data
	
	private float weight = 0f;
	private EdgeWeightFunction customWeightCallback = null;
	
	#endregion
}

public class NavigationNode
{
	#region Properties
	
	public List<NavigationEdge> Edges {
		get { return this.connectedEdges; }
	}
	
	public Vector3 WorldPosition {
		get;
		set;
	}
	
	#endregion
	
	#region Construction
	
	public NavigationNode (Vector3 worldPosition)
	{
		this.connectedEdges = new List<NavigationEdge>();
		this.WorldPosition = worldPosition;
	}
	
	#endregion
	
	#region Public methods
	
	#endregion
	
	#region Data
	
	private List<NavigationEdge> connectedEdges = null;
	
	#endregion
}