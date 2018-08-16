using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP{
	public abstract class GOAP_Sensor {
		#region Private variables
		private GOAP_Memory _Memory;
		private GOAP_Agent _Agent;
		#endregion

		#region private variables
		protected float WaitSeconds = 0.5f;
		private float CurrentWait;
		#endregion

		#region Public voids
		public void UpdateSensor(){
			CurrentWait -= 1 * Time.deltaTime;

			if (CurrentWait <= 0) {
				CurrentWait = WaitSeconds;
				SkipUpdate ();
			}

			Update (); // call update void
		}
		#endregion

		#region abstracts

		#endregion

		#region Virtuals
		protected virtual void Awake(){}
		protected virtual void Update (){}
		protected virtual void SkipUpdate(){}
		protected virtual void OnDestroy(){}
		#endregion

		#region inputs
		public void Initialize(GOAP_Agent Agent){
			this._Memory = Agent.Memory;
			this._Agent = Agent;
			Awake ();
		}
		public void Destroy(){
			OnDestroy ();
		}
		#endregion

		#region Get / Set
		public GOAP_Memory Memory{
			get{
				return _Memory;
			}
		}
		public GOAP_Agent Agent{
			get{
				return _Agent;
			}
		}
		#endregion
	}
}
