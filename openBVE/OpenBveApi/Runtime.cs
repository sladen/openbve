using System;

namespace OpenBveApi.Runtime {

	// --- load ---
	
	/// <summary>Represents properties supplied to the plugin on loading.</summary>
	public class LoadProperties {
		// members
		/// <summary>The absolute path to the plugin file.</summary>
		private string MyPluginFile;
		/// <summary>The absolute path to the train folder.</summary>
		private string MyTrainFolder;
		/// <summary>The array of panel variables.</summary>
		private int[] MyPanel;
		/// <summary>The array of sound instructions.</summary>
		private int[] MySound;
		/// <summary>The reason why the plugin failed loading.</summary>
		private string MyReason;
		// properties
		/// <summary>Gets the absolute path to the plugin file.</summary>
		public string PluginFile {
			get {
				return this.MyPluginFile;
			}
		}
		/// <summary>Gets the absolute path to the train folder.</summary>
		public string TrainFolder {
			get {
				return this.MyTrainFolder;
			}
		}
		/// <summary>Gets or sets the array of panel variables.</summary>
		public int[] Panel {
			get {
				return this.MyPanel;
			}
			set {
				this.MyPanel = value;
			}
		}
		/// <summary>Gets or sets the array of sound instructions.</summary>
		public int[] Sound {
			get {
				return this.MySound;
			}
			set {
				this.MySound = value;
			}
		}
		/// <summary>Gets or sets the reason why the plugin failed loading.</summary>
		public string Reason {
			get {
				return this.MyReason;
			}
			set {
				this.MyReason = value;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="pluginFile">The absolute path to the plugin file.</param>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="reason">The reason why the plugin failed loading.</param>
		public LoadProperties(string pluginFile, string trainFolder, string reason) {
			this.MyPluginFile = pluginFile;
			this.MyTrainFolder = trainFolder;
			this.MyReason = reason;
		}
	}
	
	
	// --- set vehicle specs ---
	
	/// <summary>Represents the type of brake the train uses.</summary>
	public enum BrakeTypes : int {
		/// <summary>The train uses the electromagnetic straight air brake. The numerical value of this constant is 0.</summary>
		ElectromagneticStraightAirBrake = 0,
		/// <summary>The train uses the analog/digital electro-pneumatic air brake without a brake pipe (electric command brake). The numerical value of this constant is 1.</summary>
		ElectricCommandBrake = 1,
		/// <summary>The train uses the automatic air brake with partial release. The numerical value of this constant is 2.</summary>
		AutomaticAirBrake = 2
	}
	
	/// <summary>Represents the specification of the train.</summary>
	public class VehicleSpecs {
		// members
		/// <summary>The number of power notches the train has.</summary>
		private int MyPowerNotches;
		/// <summary>The type of brake the train uses.</summary>
		private BrakeTypes MyBrakeType;
		/// <summary>Whether the train has a hold brake.</summary>
		private bool MyHasHoldBrake;
		/// <summary>The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		private int MyBrakeNotches;
		/// <summary>The number of cars the train has.</summary>
		private int MyCars;
		// properties
		/// <summary>Gets the number of power notches the train has.</summary>
		public int PowerNotches {
			get {
				return this.MyPowerNotches;
			}
		}
		/// <summary>Gets the type of brake the train uses.</summary>
		public BrakeTypes BrakeType {
			get {
				return this.MyBrakeType;
			}
		}
		/// <summary>Gets the number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		public int BrakeNotches {
			get {
				return this.MyBrakeNotches;
			}
		}
		/// <summary>Gets whether the train has a hold brake.</summary>
		public bool HasHoldBrake {
			get {
				return this.MyHasHoldBrake;
			}
		}
		/// <summary>Gets the index of the brake notch that corresponds to B1 or LAP.</summary>
		/// <remarks>For trains without a hold brake, this returns 1. For trains with a hold brake, this returns 2.</remarks>
		public int AtsNotch {
			get {
				if (this.HasHoldBrake) {
					return 2;
				} else {
					return 1;
				}
			}
		}
		/// <summary>Gets the index of the brake notch that corresponds to 70% of the available brake notches.</summary>
		public int B67Notch {
			get {
				return (int)Math.Round(0.7 * this.MyBrakeNotches);
			}
		}
		/// <summary>Gets the number of cars the train has.</summary>
		public int Cars {
			get {
				return this.MyCars;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="powerNotches">The number of power notches the train has.</param>
		/// <param name="brakeType">The type of brake the train uses.</param>
		/// <param name="brakeNotches">The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</param>
		/// <param name="hasHoldBrake">Whether the train has a hold brake.</param>
		/// <param name="cars">The number of cars the train has.</param>
		public VehicleSpecs(int powerNotches, BrakeTypes brakeType, int brakeNotches, bool hasHoldBrake, int cars) {
			this.MyPowerNotches = powerNotches;
			this.MyBrakeType = brakeType;
			this.MyBrakeNotches = brakeNotches;
			this.MyHasHoldBrake = hasHoldBrake;
			this.MyCars = cars;
		}
	}
	
	
	// --- initialize ---
	
	/// <summary>Represents the mode in which the plugin should initialize.</summary>
	public enum InitializationModes : int {
		/// <summary>The safety system should be enabled. The train has its service brakes applied. The numerical value of this constant is -1.</summary>
		OnService = -1,
		/// <summary>The safety system should be enabled. The train has its emergency brakes applied. The numerical value of this constant is 0.</summary>
		OnEmergency = 0,
		/// <summary>The safety system should be disabled. The train has its emergency brakes applied. The numerical value of this constant is 1.</summary>
		OffEmergency = 1
	}

	
	// --- elapse ---
	
	/// <summary>Represents a speed.</summary>
	public class Speed {
		// members
		/// <summary>The speed in meters per second.</summary>
		private double MyValue;
		// properties
		/// <summary>Gets the speed in meters per second.</summary>
		public double MetersPerSecond {
			get {
				return this.MyValue;
			}
		}
		/// <summary>Gets the speed in kilometes per hour.</summary>
		public double KilometersPerHour {
			get {
				return 3.6 * this.MyValue;
			}
		}
		/// <summary>Gets the speed in miles per hour.</summary>
		public double MilesPerHour {
			get {
				return 2.236936 * this.MyValue;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The speed in meters per second.</param>
		public Speed(double value) {
			this.MyValue = value;
		}
	}
	
	/// <summary>Represents a time.</summary>
	public class Time {
		// members
		/// <summary>The time in seconds.</summary>
		private double MyValue;
		// properties
		/// <summary>Gets the time in seconds.</summary>
		public double Seconds {
			get {
				return this.MyValue;
			}
		}
		/// <summary>Gets the time in milliseconds.</summary>
		public double Milliseconds {
			get {
				return 1000.0 * this.MyValue;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="value">The time in seconds.</param>
		public Time(double value) {
			this.MyValue = value;
		}
	}
	
	/// <summary>Represents the current state of the train.</summary>
	public class VehicleState {
		// members
		/// <summary>The current location of the train, in meters.</summary>
		private double MyLocation;
		/// <summary>The current speed of the train.</summary>
		private Speed MySpeed;
		/// <summary>The current absolute time.</summary>
		private Time MyTotalTime;
		/// <summary>The elapsed time since the last call to Elapse.</summary>
		private Time MyElapsedTime;
		/// <summary>The current pressure in the brake cylinder, in pascal.</summary>
		private double MyBcPressure;
		/// <summary>The current pressure in the main reservoir, in pascal.</summary>
		private double MyMrPressure;
		/// <summary>The current pressure in the emergency reservoir, in pascal.</summary>
		private double MyErPressure;
		/// <summary>The current pressure in the brake pipe, in pascal.</summary>
		private double MyBpPressure;
		/// <summary>The current pressure in the straight air pipe, in pascal.</summary>
		private double MySapPressure;
		// properties
		/// <summary>Gets the current location of the train, in meters.</summary>
		public double Location {
			get {
				return this.MyLocation;
			}
		}
		/// <summary>Gets the current speed of the train.</summary>
		public Speed Speed {
			get {
				return this.MySpeed;
			}
		}
		/// <summary>Gets the absolute time.</summary>
		public Time TotalTime {
			get {
				return this.MyTotalTime;
			}
		}
		/// <summary>Gets the time that elapsed since the last call to Elapse.</summary>
		public Time ElapsedTime {
			get {
				return this.MyElapsedTime;
			}
		}
		/// <summary>Gets the current pressure in the brake cylinder, in pascal.</summary>
		public double BcPressure {
			get {
				return this.MyBcPressure;
			}
		}
		/// <summary>Gets the current pressure in the main reservoir, in pascal.</summary>
		public double MrPressure {
			get {
				return this.MyMrPressure;
			}
		}
		/// <summary>Gets the current pressure in the emergency reservoir, in pascal.</summary>
		public double ErPressure {
			get {
				return this.MyErPressure;
			}
		}
		/// <summary>Gets the current pressure in the brake pipe, in pascal.</summary>
		public double BpPressure {
			get {
				return this.MyBpPressure;
			}
		}
		/// <summary>Gets the current pressure in the straight air pipe, in pascal.</summary>
		public double SapPressure {
			get {
				return this.MySapPressure;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">The current location of the train, in meters.</param>
		/// <param name="speed">The current speed of the train.</param>
		/// <param name="totalTime">The current absolute time.</param>
		/// <param name="elapsedTime">The elapsed time since the last call to Elapse.</param>
		/// <param name="bcPressure">The current pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The current pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The current pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The current pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The current pressure in the straight air pipe, in pascal.</param>
		public VehicleState(double location, Speed speed, Time totalTime, Time elapsedTime, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure) {
			this.MyLocation = location;
			this.MySpeed = speed;
			this.MyTotalTime = totalTime;
			this.MyElapsedTime = elapsedTime;
			this.MyBcPressure = bcPressure;
			this.MyMrPressure = mrPressure;
			this.MyErPressure = erPressure;
			this.MyBpPressure = bpPressure;
			this.MySapPressure = sapPressure;
		}
	}
	
	/// <summary>Represents an instruction for the const speed system.</summary>
	public enum ConstSpeedInstructions : int {
		/// <summary>The const speed system should continue operation in the user-specified way.</summary>
		Continue = 0,
		/// <summary>The const speed system should be forced on.</summary>
		Enable = 1,
		/// <summary>The const speed system should be forced off.</summary>
		Disable = 2
	}
	
	/// <summary>Represents the handles of the cab.</summary>
	public class Handles {
		// members
		/// <summary>The current reverser position.</summary>
		private int MyReverser;
		/// <summary>The current power notch.</summary>
		private int MyPowerNotch;
		/// <summary>The current brake notch.</summary>
		private int MyBrakeNotch;
		/// <summary>The instructions for the const speed system.</summary>
		private ConstSpeedInstructions MyConstSpeed;
		// properties
		/// <summary>Gets or sets the current reverser position.</summary>
		public int Reverser {
			get {
				return this.MyReverser;
			}
			set {
				this.MyReverser = value;
			}
		}
		/// <summary>Gets or sets the current power notch.</summary>
		public int PowerNotch {
			get {
				return this.MyPowerNotch;
			}
			set {
				this.MyPowerNotch = value;
			}
		}
		/// <summary>Gets or sets the current brake notch.</summary>
		public int BrakeNotch {
			get {
				return this.MyBrakeNotch;
			}
			set {
				this.MyBrakeNotch = value;
			}
		}
		/// <summary>Gets or sets the current instruction for the const speed system.</summary>
		public ConstSpeedInstructions ConstSpeed {
			get {
				return this.MyConstSpeed;
			}
			set {
				this.MyConstSpeed = value;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// <param name="constSpeed">The current instruction for the const speed system.</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, ConstSpeedInstructions constSpeed) {
			this.MyReverser = reverser;
			this.MyPowerNotch = powerNotch;
			this.MyBrakeNotch = brakeNotch;
			this.MyConstSpeed = constSpeed;
		}
	}
	
	/// <summary>Provides constants and functions related to sound instructions.</summary>
	public static class SoundInstructions {
		// constants
		/// <summary>Instructs the sound to stop. The numerical value of this constant is -10000.</summary>
		public const int Stop = -10000;
		/// <summary>Instructs the sound to play in a loop. The numerical value of this constant is 0.</summary>
		public const int PlayLooping = 0;
		/// <summary>Instructs the sound to play once. The numerical value of this constant is 1.</summary>
		public const int PlayOnce = 1;
		/// <summary>Instructs the sound to continue. The numerical value of this constant is 2.</summary>
		public const int Continue = 2;
		// functions
		/// <summary>Gets the sound instruction to change the volume of an already playing sound from a specified volume.</summary>
		/// <param name="volume">The volume between 0.0 and 1.0, where 0.0 represents 0% volume, and 1.0 represents 100% volume.</param>
		/// <returns>The sound instruction corresponding to changing the volume of a sound that is already playing.</returns>
		/// <remarks>If no sound is already playing, a new sound will start playing in a loop with the specified volume.</remarks>
		public static int FromVolume(double volume) {
			if (volume < 0.0) {
				volume = 0.0;
			} else if (volume > 1.0) {
				volume = 1.0;
			}
			return (int)Math.Round(-9999.0 + 9998.0 * volume);
		}
	}
	
	
	// --- key down / key up ---
	
	/// <summary>Represents a virtual key.</summary>
	public enum VirtualKeys : int {
		/// <summary>The virtual S key. The default assignment is Space. The numerical value of this constant is 0.</summary>
		S = 0,
		/// <summary>The virtual A1 key. The default assignment is Insert. The numerical value of this constant is 1.</summary>
		A1 = 1,
		/// <summary>The virtual A2 key. The default assignment is Delete. The numerical value of this constant is 2.</summary>
		A2 = 2,
		/// <summary>The virtual B1 key. The default assignment is Home. The numerical value of this constant is 3.</summary>
		B1 = 3,
		/// <summary>The virtual B2 key. The default assignment is End. The numerical value of this constant is 4.</summary>
		B2 = 4,
		/// <summary>The virtual C1 key. The default assignment is Page Up. The numerical value of this constant is 5.</summary>
		C1 = 5,
		/// <summary>The virtual C2 key. The default assignment is Page Down. The numerical value of this constant is 6.</summary>
		C2 = 6,
		/// <summary>The virtual D key. The default assignment is 2. The numerical value of this constant is 7.</summary>
		D = 7,
		/// <summary>The virtual E key. The default assignment is 3. The numerical value of this constant is 8.</summary>
		E = 8,
		/// <summary>The virtual F key. The default assignment is 4. The numerical value of this constant is 9.</summary>
		F = 9,
		/// <summary>The virtual G key. The default assignment is 5. The numerical value of this constant is 10.</summary>
		G = 10,
		/// <summary>The virtual H key. The default assignment is 6. The numerical value of this constant is 11.</summary>
		H = 11,
		/// <summary>The virtual I key. The default assignment is 7. The numerical value of this constant is 12.</summary>
		I = 12,
		/// <summary>The virtual J key. The default assignment is 8. The numerical value of this constant is 13.</summary>
		J = 13,
		/// <summary>The virtual K key. The default assignment is 9. The numerical value of this constant is 14.</summary>
		K = 14,
		/// <summary>The virtual L key. The default assignment is 0. The numerical value of this constant is 15.</summary>
		L = 15
	}
	
	
	// --- horn blow ---
	
	/// <summary>Represents the type of horn.</summary>
	public enum HornTypes : int {
		/// <summary>The primary horn. The numerical value of this constant is 0.</summary>
		Primary = 1,
		/// <summary>The secondary horn. The numerical value of this constant is 1.</summary>
		Secondary = 2,
		/// <summary>The music horn. The numerical value of this constant is 2.</summary>
		Music = 3
	}
	
	
	// --- door open / door close ---
	
	/// <summary>Represents the state of the doors.</summary>
	public enum DoorStates : int {
		/// <summary>No door is open.</summary>
		None = 0,
		/// <summary>The left doors are open.</summary>
		Left = 1,
		/// <summary>The right doors are open.</summary>
		Right = 2,
		/// <summary>All doors are open.</summary>
		Both = 3
	}
	
	
	// --- set signal ---
	
	/// <summary>Represents information about a signal or section.</summary>
	public class SignalData {
		// members
		/// <summary>The aspect of the signal or section.</summary>
		private int MyAspect;
		/// <summary>The distance to the signal or section.</summary>
		private double MyDistance;
		// properties
		/// <summary>Gets the aspect of the signal or section.</summary>
		public int Aspect {
			get {
				return this.MyAspect;
			}
		}
		/// <summary>Gets the distance to the signal or section.</summary>
		public double Distance {
			get {
				return this.MyDistance;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="aspect">The aspect of the signal or section.</param>
		/// <param name="distance">The distance to the signal or section.</param>
		public SignalData(int aspect, double distance) {
			this.MyAspect = aspect;
			this.MyDistance = distance;
		}
	}
	
	
	// --- set beacon ---
	
	/// <summary>Represents data trasmitted by a beacon.</summary>
	public class BeaconData {
		// members
		/// <summary>The type of beacon.</summary>
		private int MyType;
		/// <summary>Optional data the beacon transmits.</summary>
		private int MyOptional;
		/// <summary>The section the beacon is attached to.</summary>
		private SignalData MySignal;
		// properties
		/// <summary>Gets the type of beacon.</summary>
		public int Type {
			get {
				return this.MyType;
			}
		}
		/// <summary>Gets optional data the beacon transmits.</summary>
		public int Optional {
			get {
				return this.MyOptional;
			}
		}
		/// <summary>Gets the section the beacon is attached to.</summary>
		public SignalData Signal {
			get {
				return this.MySignal;
			}
		}
		// constructors
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="type">The type of beacon.</param>
		/// <param name="optional">Optional data the beacon transmits.</param>
		/// <param name="signal">The section the beacon is attached to.</param>
		public BeaconData(int type, int optional, SignalData signal) {
			this.MyType = type;
			this.MyOptional = optional;
			this.MySignal = signal;
		}
	}
	
	
	// --- plugin interface ---
	
	/// <summary>Represents the interface to be implemented by the plugin.</summary>
	public interface IRuntime {

		/// <summary>Is called when the plugin is loaded.</summary>
		/// <param name="properties">The properties supplied to the plugin on loading.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		/// <remarks>If the plugin was not loaded successfully, the plugin should set the Reason property to supply the reason of failure.</remarks>
		bool Load(LoadProperties properties);
		
		/// <summary>Is called when the plugin is unloaded.</summary>
		void Unload();
		
		/// <summary>Is called after loading to inform the plugin about the specifications of the train.</summary>
		/// <param name="specs">The specifications of the train.</param>
		void SetVehicleSpecs(VehicleSpecs specs);
		
		/// <summary>Is called when the plugin should initialize or reinitialize.</summary>
		/// <param name="mode">The mode of initialization.</param>
		void Initialize(InitializationModes mode);
		
		/// <summary>Is called every frame.</summary>
		/// <param name="state">The current state of the train.</param>
		/// <param name="handles">The handles of the safety system.</param>
		/// <param name="panel">The array of panel variables the plugin initialized in the Load call.</param>
		/// <param name="sound">The array of sound instructions the plugin initialized in the Load call.</param>
		/// <param name="message">The message the plugin passes to the host application for debugging purposes, or a null reference.</param>
		/// <remarks>Set members of the Handles argument to overwrite the driver's settings.</remarks>
		void Elapse(VehicleState state, Handles handles, int[] panel, int[] sound, out string message);
		
		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		void SetReverser(int reverser);
		
		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		void SetPower(int powerNotch);
		
		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		void SetBrake(int brakeNotch);
		
		/// <summary>Is called when a virtual key is pressed.</summary>
		/// <param name="key">The virtual key that was pressed.</param>
		void KeyDown(VirtualKeys key);
		
		/// <summary>Is called when a virtual key is released.</summary>
		/// <param name="key">The virtual key that was released.</param>
		void KeyUp(VirtualKeys key);
		
		/// <summary>Is called when a horn is played or when the music horn is stopped.</summary>
		/// <param name="type">The type of horn.</param>
		void HornBlow(HornTypes type);
		
		/// <summary>Is called when the state of the doors changes.</summary>
		/// <param name="oldState">The old state of the doors.</param>
		/// <param name="newState">The new state of the doors.</param>
		void DoorChange(DoorStates oldState, DoorStates newState);
		
		/// <summary>Is called when the aspect in the current section changes.</summary>
		/// <param name="data">The signal data.</param>
		void SetSignal(SignalData data);
		
		/// <summary>Is called when the train passes a beacon.</summary>
		/// <param name="data">The beacon data.</param>
		void SetBeacon(BeaconData data);
		
	}
	
}