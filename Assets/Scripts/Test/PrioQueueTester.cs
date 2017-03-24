using UnityEngine;
using System.Collections;

public class PrioQueueTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PriorityQueue<string> pQueue = new PriorityQueue<string>();
		pQueue.Enqueue("abb", 15f);
		pQueue.Enqueue("astra", 28f);
		pQueue.Enqueue("ssab", 3f);
		pQueue.Enqueue("volvo", 5.5f);
		pQueue.Enqueue("astra", 500f);
		pQueue.Enqueue("scania", 38f);
		pQueue.Enqueue("lundin mining", 301f);
		pQueue.Enqueue("ericsson", 0.1f);
		Debug.Log("pQueue Count: "+pQueue.allItems.Count);
		var anItemHi = pQueue.DequeueHighest();
		Debug.Log("Dequeue high : "+anItemHi.PriorityWeight + " | " + anItemHi.Item);
		var anItemLo = pQueue.DequeueLowest();
		Debug.Log("Dequeue low : "+anItemLo.PriorityWeight + " | " + anItemLo.Item);
		
		Debug.Log("List :");
		var all = pQueue.allItems;
		
		foreach(var item in all)
		{
			Debug.Log(item);
		}
		
		Debug.Log("PeekHigh : "+pQueue.PeekHighest().PriorityWeight+ " | "+ pQueue.PeekHighest().Item);
		Debug.Log("PeekLow : "+pQueue.PeekLowest().PriorityWeight+ " | "+ pQueue.PeekLowest().Item);
		
		Debug.Log("Subset :");
		var subset = pQueue.SubSet("volvo", "astra");
		foreach(var item in subset)
		{
			Debug.Log(item);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
