using UnityEngine;

namespace Complete
{
    public class CameraControl : MonoBehaviour
    {
        public float m_DampTime = 0.2f;                 // Approximate time for the camera to refocus.
        public float m_ScreenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge.
        public float m_MinSize = 6.5f;                  // The smallest orthographic size the camera can be.
		[HideInInspector] public Transform[] m_Targets; // All the targets the camera needs to encompass. 

		/* [HideInInspector] */ public float m_SampleRate = 1.0f;  // framerate poll rate.
		/* [HideInInspector] */ public float m_CurrentSceneFPS = 0.0000f;  // The current FPS.
		/* [HideInInspector] */ public float m_ExpectedSceneFPS = 0.0500f;  // Expected FPS.
		/* [HideInInspector] */ public bool m_TargetsInView = false; // This is true when all of the targets are visible. If one falls off, it reports false.
		/* [HideInInspector] */ public bool m_AllTargetsInView = true; // This is set false any time m_TargetsInView is false. It is polled at the end if the test by the Assert component. Should be an array.
		/* [HideInInspector] */ public static bool m_TargetsExpectedInView = true; //If we expect the targets to always be in the camera view

        private Camera m_Camera;                        // Used for referencing the camera.
        private float m_ZoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
        private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position.
        private Vector3 m_DesiredPosition;              // The position the camera is moving towards.


        private void Awake ()
        {
            m_Camera = GetComponentInChildren<Camera> ();
        }


        private void FixedUpdate ()
        {
			// Get the framerate and update m_CurrentSceneFPS
			GetFPS ();

			// Parse the targets list and verify that the targets are in view. 
			VerifyTargetsInView ();

			// Move the camera.
            Move ();

            // Change the size of the camera based.
            Zoom ();
        }


        private void Move ()
        {
            // Find the average position of the targets.
            FindAveragePosition ();

            // Smoothly transition to that position.
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }


        private void FindAveragePosition ()
        {
            Vector3 averagePos = new Vector3 ();
            int numTargets = 0;

            // Go through all the targets and add their positions together.
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // If the target isn't active, go on to the next one.
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                // Add to the average and increment the number of targets in the average.
                averagePos += m_Targets[i].position;
                numTargets++;
            }

            // If there are targets divide the sum of the positions by the number of them to find the average.
            if (numTargets > 0)
                averagePos /= numTargets;

            // Keep the same y value.
            averagePos.y = transform.position.y;

            // The desired position is the average position;
            m_DesiredPosition = averagePos;
        }


        private void Zoom ()
        {
            // Find the required size based on the desired position and smoothly transition to that size.
            float requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp (m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }


        private float FindRequiredSize ()
        {
            // Find the position the camera rig is moving towards in its local space.
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // Start the camera's size calculation at zero.
            float size = 0f;

            // Go through all the targets...
            for (int i = 0; i < m_Targets.Length; i++)
            {
                // ... and if they aren't active continue on to the next target.
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                // Otherwise, find the position of the target in the camera's local space.
                Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

                // Find the position of the target from the desired position of the camera's local space.
                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

                // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                // Choose the largest out of the current size and the calculated size based on the tank being to the left or right of the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }

            // Add the edge buffer to the size.
            size += m_ScreenEdgeBuffer;

            // Make sure the camera's size isn't below the minimum.
            size = Mathf.Max (size, m_MinSize);

            return size;
        }

		// Assign current FPS to current FPS variable. Made this a public function so that the FPS may be exposed to other scripts for testing & dev.
		// My preference is be to stick this in a separate library.
		public void GetFPS ()
		{
			m_CurrentSceneFPS = m_SampleRate/Time.deltaTime; // Get the current framerate and assign it to public float 'm_CurrentSceneFPS' for use in testing.
		}

		// Parse the targets list and verify that the targets are in view. 
		public void VerifyTargetsInView ()
		{
			for (int i = 0; i < m_Targets.Length; i++) 
			{
				// If they aren't active continue on to the next target.
				if (!m_Targets[i].gameObject.activeSelf) 
					continue;

				// The code from the Unity3d Answers forum that I used: http://answers.unity3d.com/answers/1031557/view.html
				Vector3 screenpoint = m_Camera.WorldToViewportPoint(m_Targets [i].position); // Converts target position to camera space
				m_TargetsInView = screenpoint.z > 0 && screenpoint.x > 0 && screenpoint.x < 1 && screenpoint.y > 0 && screenpoint.y < 1; // sets the public boolean 'm_TargetsInView' to true or false to access during testing

				if (m_TargetsInView == false) // If any of the targets are out of range
				{
					m_AllTargetsInView = false; // Set the polling flag to false
				}
			}
		}

        public void SetStartPositionAndSize ()
        {
            // Find the desired position.
            FindAveragePosition ();

            // Set the camera's position to the desired position without damping.
            transform.position = m_DesiredPosition;

            // Find and set the required size of the camera.
            m_Camera.orthographicSize = FindRequiredSize ();
        }
    }
}