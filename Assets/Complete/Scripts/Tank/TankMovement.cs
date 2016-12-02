using UnityEngine;
using System.Collections;

namespace Complete
{
	public class TankMovement : MonoBehaviour 
	{
		public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
		public float m_Speed = 12f;                 // How fast the tank moves forward and back.
		public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
		public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
		public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
		public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

		private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
		private string m_TurnAxisName;              // The name of the input axis for turning.
		private Rigidbody m_Rigidbody;              // Reference used to move the tank.
		private float m_MovementInputValue;         // The current value of the movement input.
		private float m_TurnInputValue;             // The current value of the turn input.
		private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.

		// Autonamous movement - added 
		/* [HideInInspector] */ public bool m_TestMode = true; // if true, then autonamous behavior is implemented in favor of manual control.
		/* [HideInInspector] */ public float directionChangeInterval = 0.75f; // How often the tank changes direction
		/* [HideInInspector] */ public float maxHeadingChange = 90; // Maximum degrees per turn

		CharacterController controller; // The controller for the autonamous movement.
		float heading; // heading for autonamous movement.

		private void Awake ()
		{
			controller = GetComponent<CharacterController> (); // Set the bot controller - should be in else statement

			if (m_TestMode == false) // if not in test mode and manual control is used
			{
				m_Rigidbody = GetComponent<Rigidbody> (); // Map to the object's rigidbody component for controller input
			} 
		}


		private void OnEnable ()
		{
			if (m_TestMode == false) // if not in test mode and manual control is used
			{			
				// When the tank is turned on, make sure it's not kinematic.
				m_Rigidbody.isKinematic = false;

				// Also reset the input values.
				m_MovementInputValue = 0f;
				m_TurnInputValue = 0f;
			}
		}


		private void OnDisable ()
		{
			if (m_TestMode == false) // if not in test mode and manual control is used
			{
				// When the tank is turned off, set it to kinematic so it stops moving.
				m_Rigidbody.isKinematic = true;
			}
		}


		private void Start ()
		{
			if (m_TestMode == false) // if not in test mode and manual control is used 
			{			
				// The axes names are based on player number.
				m_MovementAxisName = "Vertical" + m_PlayerNumber;
				m_TurnAxisName = "Horizontal" + m_PlayerNumber;

				// Store the original pitch of the audio source.
				m_OriginalPitch = m_MovementAudio.pitch;
			}
			else
			{
				// Set random initial rotation for autonamous unit
				heading = Random.Range(45, 270); 
				transform.eulerAngles = new Vector3(0, heading, 0);
				StartCoroutine(NewHeading());
			}
		}


		private void Update ()
		{
			if (m_TestMode == false) // if not in test mode and manual control is used
			{  
				// Store the value of both input axes.
				m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
				m_TurnInputValue = Input.GetAxis (m_TurnAxisName);
				EngineAudio ();
			} 
		}


		private void EngineAudio ()
		{
			// If there is no input (the tank is stationary)...
			if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f) {
				// ... and if the audio source is currently playing the driving clip...
				if (m_MovementAudio.clip == m_EngineDriving) {
					// ... change the clip to idling and play it.
					m_MovementAudio.clip = m_EngineIdling;
					m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
					m_MovementAudio.Play ();
				}
			} else {
				// Otherwise if the tank is moving and if the idling clip is currently playing...
				if (m_MovementAudio.clip == m_EngineIdling) {
					// ... change the clip to driving and play.
					m_MovementAudio.clip = m_EngineDriving;
					m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
					m_MovementAudio.Play (); 
				}
			}
		}


		private void FixedUpdate () 
		{
			Move ();

			if (m_TestMode == false)  // if not in test mode and manual control is used
			{
				Turn ();
			} 
		}


		private void Move ()
		{
			if (m_TestMode == false) // if not in test mode and manual control is used
			{  
				// Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
				Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
				// Apply this movement to the rigidbody's position.
				m_Rigidbody.MovePosition (m_Rigidbody.position + movement);
			} 
			else 
			{
				// Random movement from the Wander.cs script
				transform.eulerAngles = new Vector3(0, heading, 0);
				var forward = transform.TransformDirection (Vector3.forward);
				controller.SimpleMove (forward * m_Speed);
			}
		}


		private void Turn ()
		{
			// Determine the number of degrees to be turned based on the input, speed and time between frames.
			float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

			// Make this into a rotation in the y axis.
			Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

			// Apply this rotation to the rigidbody's rotation.
			m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
		}


		/// Repeatedly calls the function that calculates a new direction to move toward. 
		IEnumerator NewHeading()
		{
			while (true)
			{
				NewHeadingRoutine(); // set new heading
				yield return new WaitForSeconds(directionChangeInterval); 
			}
		}


		/// Calculates a new direction to move toward.
		void NewHeadingRoutine()
		{
			var floor = Mathf.Clamp(heading - maxHeadingChange, 0, 360); // keeps minimun
			var ceil = Mathf.Clamp(heading + maxHeadingChange, 0, 360);
			heading = Random.Range(floor, ceil);
		}
	}
}