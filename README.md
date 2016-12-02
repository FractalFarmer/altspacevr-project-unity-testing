## AltspaceVR Engineering Project - Unity Testing - Response from Shawn Kunkel ##

##### "QA should be utilized not only as an effective means of risk management, but as a vehicle for the transformation of it's own historical bottleneck into increased team velocity" #####

### Overview ###

Included is my full response to the task you have assigned me in the [AltspaceVR project unity testing](https://github.com/AltspaceVR/altspacevr-project-unity-testing) GitHub repository.

You may find my [response](https://github.com/FractalFarmer/altspacevr-project-unity-testing) in my public Git repo.

### Sections ###
- Testing Strategy for the TANKS! game
- The tests, and code change overview
- Known issues
- Other

### Testing Strategy for the TANKS! game ###

Multiplayer online gaming has a lot of moving parts. In the case of TANKS!, we have a pretty standard list:

- Platforms / Environments
- Web
- App layer / REST
- Unity tests
- Security
- Networking
- Graphics
- Physics
- Player sync
- Servers / Simulators
- Communication
- Media
- Audio
- etc.

If we're starting from scratch, QA should be involved from the beginning in the project's tasks. QA would need to do an audit of systems, architecture and initial features/functionality. Also taken into account would be new features and milestones, high risk areas, high priority areas and ROI on the automation of a feature. I believe that the early involvement of members of the development team with QA is important, so when considering framework, harness, test plans, tech, etc., is critical.  

As for methodology, I'm in favor of following Agile SDLC / Continuous Deployment practices, emphasizing robust automation. 

To that end, the following are some of what I consider priorities  regarding practices and processes:

- Develop and follow standards
- Establish baselines
- Code Reviews (QA included)
- Pair programming
- Debug builds for automated testing
- Code tags for documentation generation
- Involve QA from conception Dev tasks (app, tech upgrade, feature, service, etc.)
- QA involves Dev from conception of QA projects (possibly including an initial 'surge' from Dev as foundations are laid)
- Unit tests for all new qualified code sections, including QA
- Involve SDK and beta test community, provide them with low-overhead tools (e.g. public facing JIRA instance or ways to test their own stuff) and processes (bug reporting, feature requests, suggestions, feedback). Have regular meetings with them.
- Acceptance and release criteria
- Continuous deployment/distribution through automated testing
- Establish high-level tests at design time
- Defect classifications
- Test summaries and reporting
- Demo everything
- Create tests as you go
- Modular test suites
- Take initiative, but get Product Owner approval for big stuff
- If someone has time, jot / share testing ideas for their area
- Emphasize 'shift-left' testing
- Have a rollback plan in place for all types of upgrade (even an automated rollback for highest Severity defects if a post-deploy test fails)

Above, I mention standards and baselines. I think that standards are important for reduction of risk and improving efficiency, as long as they make sense and don't get in the way. ROI, again. Some areas where I think both of these apply to this project, to one degree or another, are as follows:

- Models
- Coding
- Scene construction
- Devices / platforms
- Graphics
- Inputs
- Audio formats
- Action constraints (like weapon fire and move rates)
- Agent limits per scene
- Tech version alignment for dev (Unity, .NET, plus other tools and packages)
- Scorekeeping system
- Camera FPS
- Simlator FPS
- Simulator time dilation
- Network Latency
- Gold data sets for test
- Behavior ranges for gameObjects (A rough idea based on the Tanks: Moving -> Stationary, Shooting -> Not Shooting, Turning -> Not Turning, Weapon damage radius, etc) 
- Physics / collisions
- Media (in-scene web, audio, video, etc)
- Messaging

As for automated testing, there are a lot of tools and methods available. Unity's test suite, WebDriver, Angular / node, Apache, etc., are all well suited to the technology of this game. 

The above could also be used for gameplay testing and game engine validation.

I'm fond of triggering automation via the CI system, and the following are some thoughts on some of the requirements of that approach:

- Parameterized launch / preprocessor directives / metadata
- Appium / other virtual app testing
- Test doubles - performance tests eg.
- Stripped of debug items upon production build and verify build is clean
- CI system test result reporting

I think that the standard battery of Acceptance, Bug / Regression, Unit and Integration tests would fit just fine.

To round all of that out, data collection and analysis is critical. Some areas that I would explore are, but aren't limited to:

- Tracebacks
- Pending features
- Deployment / update schedules
- External defect reports
- Internal defect reports
- Defect rates per feature / dev / etc
- Automated test results
- User feedback and UX tests
- Data analysis tools


### The tests, with code change overview ###

All mofified assets are in the _Completed-Assets section.

Modified files:

- TankHealth.cs
- TankMovement.cs (autonomous movement culled from [here](http://wiki.unity3d.com/index.php/Wander))
- CameraControl.cs (If game object is in camera view from [here](http://answers.unity3d.com/answers/1031557/view.html))

####TankDamageTest####
#####Overview
A tank receives damage when hit by a shell.

#####Files(s) and objects
TankHealth.cs

CompleteTestTank prefab

Assert component

#####Summary
This test verifies whether a tank has lost health when impacted by a shell. The scene loads, and a stationary tank is hit by a shell that falls from overhead.

#####Modifications
I used the 'CompleteTestTank' prefab for this test.

I modified TankHealth.cs by exposing the private variable, 'm_CurrentHealth' to public. I am polling it and comparing it through the Assertion Component to an expected value of 100. The test passes if when OnTriggerEnter, the health is any value below 100.

####FPSCorrectnessTest####
#####Overview	 
The frame-rate of the game does not drop below 60 FPS for more than 100 milliseconds at a time.

#####Files(s) and objects
CameraControl.cs

CompleteTank prefab

CameraRig

Assert component connected to the CameraRig.

#####Summary
A one millisecond sample of the framerate. 

I actually wrote two tests for this. One, "FPSCorrectnessTestFAIL", expects to fail if the test framerate is below 60 (passing). The other, "FPSCorrectnessTestPASS" that expects to pass when it meets the expected framerate of 50.

#####Files(s) and objects
TankHealth.cs

CompleteTestTank prefab

Assert component

#####Modifications
I used the 'CompleteTank' prefab for this test.

I modified the CameraControl.cs file in a few areas.

First, I added these 4 public variables:

		/* [HideInInspector] */ public float m_SampleRate = 1.0f;  // framerate poll rate.
		/* [HideInInspector] */ public float m_CurrentSceneFPS = 0.0000f;  // The current FPS.
		/* [HideInInspector] */ public float m_ExpectedSceneFPS = 0.0500f;  // Expected FPS.
		/* [HideInInspector] */ public bool m_TargetsInView = false; // This is true when all of the targets are visible. If even one falls off, it reports false.

I wrote two functions, which I call from FixedUpdate. Their usage is simple:

			// Get the framerate and update m_CurrentSceneFPS
			GetFPS ();

			// Parse the targets list and verify that the targets are in view. 
			VerifyTargetsInView ();

GetFPS ()

		// Assign current FPS to current FPS variable. Made this a public function so that the FPS may be exposed to other scripts for testing & dev.
		// My preference is be to stick this in a separate library.
		public void GetFPS ()
		{
			m_CurrentSceneFPS = m_SampleRate/Time.deltaTime; // Get the current framerate and assign it to public float 'm_CurrentSceneFPS' for use in testing.
		}

VerifyTargetsInView ()

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

####TankVisibilityTest####
#####Overview
Verify that two tank targets remain in the camera view area while navigating randomly.
 
#####Files(s) and objects
TankMovement.cs

CameraControl.cs

CompleteTestTank_Test prefab

Assert component

#####Summary
Two tanks randomly navigate the LevelArt for ten seconds. During this time, the camera is supposed to adjust it's size, and move to fit the two tanks within it's view area. I have added modifications to the camera (view area polling and failure reporting) and tanks ('m_TestMode = true' switches to autonomous movement) to achieve this.

#####Modifications to TankMovement.cs

######Variable declarations

		// Autonamous movement - added 
		/* [HideInInspector] */ public bool m_TestMode = true; // if true, then autonamous behavior is implemented in favor of manual control.
		/* [HideInInspector] */ public float directionChangeInterval = 0.75f; // How often the tank changes direction
		/* [HideInInspector] */ public float maxHeadingChange = 90; // Maximum degrees per turn

		CharacterController controller; // The Character Controller for the autonamous movement.
		float heading; // heading for autonamous movement.

######Persistent conditional
I use this in many functions to defer to manual if not in test mode. I will skip explaining the functions that only have one 'if' statement with no other changes. I'd probably use #if/#endif instead. I know, it's a hack. I'd prefer to pull this all out into it's own library.

			if (m_TestMode == false) // if not in test mode and manual control is used

######Awake ()

Initialize the controller (Not the ideal spot for that, because it is always activated even when not needed) or set the Rigidbody if the input is manual.

		private void Awake ()
		{
			controller = GetComponent<CharacterController> (); // Set the bot controller - should be in else statement

			if (m_TestMode == false) // if not in test mode and manual control is used
			{
				m_Rigidbody = GetComponent<Rigidbody> (); // Map to the object's rigidbody component for controller input
			} 
		}

######Start ()
Initialize manual input or random movement.

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

######FixedUpdate ()
Move () called for both manual or random input. Turn () called for only manual.

		private void FixedUpdate () 
		{
			Move ();

			if (m_TestMode == false)  // if not in test mode and manual control is used
			{
				Turn ();
			} 
		}
######Move ()
Applies movement to the tank depending on manual or autonomous (m_TestMode flag) mode.

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
######NewHeading ()
An Enumerator that calls the new heading routine for autonomous movement. From Wander.cs

		/// Repeatedly calls the function that calculates a new direction to move toward. 
		IEnumerator NewHeading()
		{
			while (true)
			{
				NewHeadingRoutine(); // set new heading
				yield return new WaitForSeconds(directionChangeInterval); 
			}
		}

######NewHeadingRoutine ()
Calculates a new heading for autonomous movement. From Wander.cs

		/// Calculates a new direction to move toward.
		void NewHeadingRoutine()
		{
			var floor = Mathf.Clamp(heading - maxHeadingChange, 0, 360); // keeps minimun
			var ceil = Mathf.Clamp(heading + maxHeadingChange, 0, 360);
			heading = Random.Range(floor, ceil);
		}

#####Modifications to CameraControl.cs
I combined some existing target checks with onscreen checks found in the Unity forums to create the following function.

######VerifyTargetsInView ()
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

### Known Issues ###
I know that in this state, the code is something of a kludge. Had this been a proper project, it'd be cleaner and more modular. I left public variables exposed so you can play with them, as well as to test my tests. The tank autonomous movement doesnt use the Rigidbody, so it climbs up walls and other things. I have an audiolistener warning that doesn't make sense. I have active audiolisteners in all scenes.

### Other ###
######What would I have done, had I more time:
- Break off my code changes into their own QA libraries and wrappers. (e.g. Camera driving utility that moves, zooms, handles camera types, polls framrates and 'knows' what it is seeing)
- Build on those proposed libraries by building those wrappers around the behavior ranges listed above.
- AI that overrides the user inputs, instead of the way I did it, which causes them to ride up on colliders.
- Better functions and naming
- Test tagging
- Automation tool-driven parameterized launch of tests (for example, from command line, Jenkins, etc)
- Proper reporting of test results