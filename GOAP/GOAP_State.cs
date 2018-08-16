using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class GOAP_State {
	#region Private variables
	private Dictionary<string,PriorityValue> _Conditions = new Dictionary<string,PriorityValue>();
	#endregion

	#region Constructors
	public GOAP_State(){}
	public GOAP_State(GOAP_State reference){
		foreach (KeyValuePair<string,PriorityValue> pair in reference.Conditions) {
			Conditions [pair.Key] = pair.Value; // set value
		}
	}
	public GOAP_State(List<KeyValuePair<string,PriorityValue>> SortedList){
		foreach (var pair in SortedList) {
			if (!_Conditions.ContainsKey (pair.Key)) {
				_Conditions.Add (pair.Key,pair.Value); // doesnt already contain. Add to dictionary
			} else {
				break; // already containts. Stop adding, states are overlapping
			}
		}
	}
	#endregion

	#region Inputs
	public void Set(string Key,object value,int? Order = null,KeyValuePair<string,object>[] GenData = null){
		if (Order == null) {if (Conditions.ContainsKey (Key)) {Order = Conditions [Key].Order;} else {Order = 0;}} // check what order to use
		PriorityValue NewVal = new PriorityValue((int)Order,value);
		NewVal.SetGenData(GenData);
		Conditions [Key] = NewVal;

	}
	public void Set<T>(string Key, object value,int? Order = null,KeyValuePair<string,object>[] GenData = null){
		if (value != null && typeof(T).IsAssignableFrom (value.GetType ())) {
			Set (Key,(T)value,Order,GenData); // set
		}
	}
	public void Set<T>(string Key, GameObject GameObject,int? Order = null,KeyValuePair<string,object>[] GenData = null){
		if (GameObject != null) {
			T Comp = GameObject.GetComponent<T> ();
			if (Comp != null) {
				Set (Key, Comp,Order,GenData);
			}
		}
	}
	public void SetDynamic<T>(string Key, System.Func<object> DynamicValue,int? Order = null,KeyValuePair<string,object>[] GenData = null){
		Set (Key, (object)DynamicValue,Order,GenData);
	}

	public void SetOrder(string Key, int Order){
		if (Conditions.ContainsKey (Key)) {
			Conditions [Key].Order = Order;
		}
	}

	public object GetRaw(string Key){
		if (Conditions.ContainsKey (Key)) {
			return Conditions [Key].Value;
		}
		return null;
	}
	public object Get<T>(string Key){
		if (Conditions.ContainsKey (Key)) {
			object value = Conditions [Key].Value;
			value = CheckForDynamics (value); // check for dynamics
			return value;
		}
		return null;
	}

	public object GetGenData<T>(string Key, string GenKey){
		if (Conditions.ContainsKey (Key)) {
			if (Conditions [Key].GenData.ContainsKey (GenKey)) {
				object value = Conditions [Key].GenData [GenKey];
				value = CheckForDynamics (value); // check for dynamics
				return value;
			}
		}
		return null;
	}

	public int GetGenDataCount(string Key){
		if (Conditions.ContainsKey (Key)) {
			return Conditions [Key].GenData.Count;
		}
		return 0;
	}

	public T GetClass<T>(string Key) where T : class{
		object obj = Get<T>(Key);

		if (obj != null && typeof(T).IsAssignableFrom (obj.GetType ())) {
			return (T)obj;
		}

		return null;
	}
	public T GetGeneric<T>(string Key){
		object obj = Get<T>(Key);

		if (obj != null && typeof(T).IsAssignableFrom (obj.GetType ())) {
				return (T)obj;
		}

		return default(T);
	}

	public GOAP_State RemoveConditions(Dictionary<string,PriorityValue> CheckConditions){
		if (_Conditions.Count > 0 && CheckConditions.Count > 0) { // check if there are even items to remove
			foreach (KeyValuePair<string,PriorityValue> pair in CheckConditions) {
				if (_Conditions.ContainsKey (pair.Key)) { // if conditions contains key
					if (PairEquals (pair, new KeyValuePair<string, PriorityValue> (pair.Key, _Conditions [pair.Key]))) {
						_Conditions.Remove (pair.Key); // remove condition
					}
				}
			}
		}

		return this; // builder pattern
	}
	public void Clear(){
		_Conditions.Clear();
	}
	public bool RemoveSelfInOrderFromList(ref List<KeyValuePair<string,PriorityValue>> List){
		if (List.Count <= 0) {return true;}

		bool Success = false;
		for (int i = 0; i < List.Count;) {
			var Item = List [0];
			if (_Conditions.ContainsKey (Item.Key)) { // check if toremove contains key
				if (ValuesEqual(Item.Value,_Conditions[Item.Key])) { // check if values are the same
					List.RemoveAt(0); // remove first from list
					Success = true; // succeed
				} else {
					break;
				}
			} else {
				break;
			}
		}

		return Success;
	}

	public void ReplaceGenData(GOAP_State State){
		foreach (var Item in State.Conditions) {
			if (Conditions.ContainsKey (Item.Key)) {
				Conditions [Item.Key].SetGenData(State.Conditions[Item.Key].GenData);
			}
		}
	}

	public void PrintConditionTypes(string Extra){
		foreach (KeyValuePair<string,PriorityValue> pair in Conditions) {
			if (pair.Value.Value != null) {
				MonoBehaviour.print ("Type: "+pair.Value.Value.GetType()+ "Key: "+ pair.Key + ". Value: "+pair.Value.Value.ToString() + ". Extra: "+Extra);
			}
		}
	}

	public List<KeyValuePair<string,PriorityValue>> GetSortedList(bool Flipped = true){
		List<KeyValuePair<string,PriorityValue>> Keys = Conditions.ToList ();
		if (!Flipped) {
			Keys.Sort ((x, y) => x.Value.Order.CompareTo (y.Value.Order));
		} else {
			Keys.Sort ((x, y) => y.Value.Order.CompareTo (x.Value.Order));
		}
	
		return Keys;
	}

	public bool HasKeyMatch(GOAP_State Other){
		foreach(KeyValuePair<string,PriorityValue> pair in Other.Conditions){
			if (_Conditions.ContainsKey (pair.Key)) {
				return true; // is equal, has key match
			}
		}
		return false;
	}
	/// <summary>
	/// Determines wheter this state has a matching condition with the other state
	/// </summary>
	/// <returns><c>true</c> if this instance has match the specified Other; otherwise, <c>false</c>.</returns>
	/// <param name="Other">Other.</param>
	public bool HasMatch(GOAP_State Other){
		foreach(KeyValuePair<string,PriorityValue> pair in Other.Conditions){
			if (_Conditions.ContainsKey (pair.Key)) {
				if (PairEquals (pair, new KeyValuePair<string, PriorityValue> (pair.Key, _Conditions [pair.Key]))) {
					return true; // is equal, has match
				}
			}
		}
		return false;
	}
	#endregion

	#region Privates
	private bool ValuesEqual(PriorityValue a, PriorityValue b){
		object valueA = CheckForDynamics (a.Value); // check for dynamics
		object valueB = CheckForDynamics (b.Value); // check for dynamics
		
		if (valueA == null || valueB == null) {
			return valueA == valueB;
		} else {
			return valueA.Equals (valueB);
		}
	}
	private bool PairEquals(KeyValuePair<string,PriorityValue> a, KeyValuePair<string, PriorityValue> b){
		object valueA = CheckForDynamics (a.Value.Value); // check for dynamics
		object valueB = CheckForDynamics (b.Value.Value); // check for dynamics

		if (a.Key == b.Key) {
			if (valueA == null || valueB == null) {
				return valueA == valueB;
			} else {
				return valueA.Equals (valueB);
			}
		}
		return false;
	}
	#endregion

	#region Statics
	private static bool ValueMatch(object a, object b){
		object valueA = CheckForDynamics (a); // check for dynamics
		object valueB = CheckForDynamics (b); // check for dynamics

		return (valueA.GetHashCode () == valueB.GetHashCode ());
	}
	private static object CheckForDynamics(object val){
		if (IsDynamic(val)){
			System.Func<object> Dynamic = (System.Func<object>)val;
			val = Dynamic.Invoke ();
		}
		return val;
	}
	private static bool IsDynamic(object val){
		return (val != null && typeof(System.Func<object>).IsAssignableFrom (val.GetType ()));
	}
	#endregion

	#region Get / Set
	public Dictionary<string,PriorityValue> Conditions{
		get{
			return _Conditions;
		}
	}
	public int Count{
		get{
			return Conditions.Count;
		}
	}
	#endregion

	#region Statics
	public static GOAP_State operator +(GOAP_State a, GOAP_State b)
	{		
		GOAP_State result = new GOAP_State (a);
		foreach (var pair in b.Conditions) {
			result.Conditions [pair.Key] = pair.Value;
		}
		return result;
	}
	#endregion

	#region Inner classes
	public class PriorityValue{
		public int Order;
		public object Value;
		public Dictionary<string,object> GenData = new Dictionary<string, object>();

		public PriorityValue(int Order,object Value){
			this.Order = Order;
			this.Value = Value;
		}

		public void SetGenData(KeyValuePair<string,object>[] Data){
			if (Data != null) {
				for (int i = 0; i < Data.Length; i++) {
					if (GenData.ContainsKey (Data [i].Key)) {
						GenData [Data [i].Key] = Data [i].Value;
					} else {
						GenData.Add (Data [i].Key, Data [i].Value);
					}
				}
			}
		}
		public void SetGenData(Dictionary<string,object> Data){
			if (Data != null) {
				foreach (var item in Data) {
					if (GenData.ContainsKey (item.Key)) {
						GenData [item.Key] = item.Value;
					} else {
						GenData.Add (item.Key,item.Value);
					}
				}
			}
		}
	}
	#endregion
}
