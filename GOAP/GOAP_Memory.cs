using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP{
	public class GOAP_Memory {
		#region variables
		private GOAP_State _State = new GOAP_State();
		private GOAP_Sensor[] Sensors;
		#endregion

		#region Get / Set
		public GOAP_State State{
			get{
				return _State; // return current state
			}
			set{
				_State = value; // set state to be value
			}
		}
		#endregion

		#region Input voids
		public void DestroyMemory(){
			for (int i = 0; i < Sensors.Length; i++) {Sensors [i].Destroy ();} // destroy sensors
		}
		public void UpdateMemory(){
			if (Sensors != null) { // update sensors if there are any
				for (int i = 0; i < Sensors.Length; i++) {
					Sensors [i].UpdateSensor ();
				}
			}
		}
		public void SetSensors(GOAP_Sensor[] Sens,GOAP_Agent Agent){
			Sensors = Sens;

			if (Sens != null) { // initialize sensors if there are any
				for (int i = 0; i < Sensors.Length; i++) {
					Sensors [i].Initialize (Agent);
				}
			}
		}
		#endregion
	}
}
