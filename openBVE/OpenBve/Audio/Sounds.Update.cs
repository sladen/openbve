using System;
using Tao.OpenAl;

namespace OpenBve {
	internal static partial class Sounds {
		
		/// <summary>Updates the sound component. Should be called every frame.</summary>
		internal static void Update() {
			/*
			 * Set up the listener
			 * */
			OpenBveApi.Math.Vector3 listenerPosition = World.AbsoluteCameraPosition.GetAPIStructure();
			OpenBveApi.Math.Orientation3 listenerOrientation = new OpenBveApi.Math.Orientation3(World.AbsoluteCameraSide.GetAPIStructure(), World.AbsoluteCameraUp.GetAPIStructure(), World.AbsoluteCameraDirection.GetAPIStructure());
			OpenBveApi.Math.Vector3 listenerVelocity;
			if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior) {
				TrainManager.Car car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
				OpenBveApi.Math.Vector3 diff = car.FrontAxle.Follower.WorldPosition.GetAPIStructure() - car.RearAxle.Follower.WorldPosition.GetAPIStructure();
				listenerVelocity = car.Specs.CurrentSpeed * OpenBveApi.Math.Vector3.Normalize(diff) + World.CameraAlignmentSpeed.Position.GetAPIStructure();
			} else {
				listenerVelocity = World.CameraAlignmentSpeed.Position.GetAPIStructure();
			}
			Al.alListener3f(Al.AL_POSITION, 0.0f, 0.0f, 0.0f);
			Al.alListener3f(Al.AL_VELOCITY, (float)listenerVelocity.X, (float)listenerVelocity.Y, (float)listenerVelocity.Z);
			Al.alListenerfv(Al.AL_ORIENTATION, new float[] { (float)listenerOrientation.Z.X, (float)listenerOrientation.Z.Y, (float)listenerOrientation.Z.Z, -(float)listenerOrientation.Y.X, -(float)listenerOrientation.Y.Y, -(float)listenerOrientation.Y.Z });
			/*
			 * Set up the atmospheric attributes
			 * */
			double elevation = World.AbsoluteCameraPosition.Y + Game.RouteInitialElevation;
			double airTemperature = Game.GetAirTemperature(elevation);
			double airPressure = Game.GetAirPressure(elevation, airTemperature);
			double airDensity = Game.GetAirDensity(airPressure, airTemperature);
			double speedOfSound = Game.GetSpeedOfSound(airPressure, airTemperature);
			try {
				Al.alSpeedOfSound((float)speedOfSound);
			} catch { }
			/*
			 * Update the sound sources
			 * */
			for (int i = 0; i < SourceCount; i++) {
				if (Sources[i].State == SoundSourceState.StopPending) {
					/*
					 * The sound is still playing but is to be stopped.
					 * Stop the sound, then remove it from the list of
					 * sound sources.
					 * */
					Al.alDeleteSources(1, ref Sources[i].OpenAlSourceName);
					Sources[i].State = SoundSourceState.Stopped;
					Sources[i].OpenAlSourceName = 0;
					Sources[i] = Sources[SourceCount - 1];
					SourceCount--;
					i--;
				} else if (Sources[i].State == SoundSourceState.Stopped) {
					/*
					 * The sound was already stopped. Remove it from
					 * the list of sound sources.
					 * */
					Sources[i] = Sources[SourceCount - 1];
					SourceCount--;
					i--;
				} else if (GlobalMute) {
					/*
					 * The sound is played or about to be played, but
					 * the global mute option is enabled. Stop the sound
					 * sound if necessary, then remove it from the list
					 * of sound sources if the sound is not looping.
					 * */
					if (Sources[i].State == SoundSourceState.Playing) {
						Al.alDeleteSources(1, ref Sources[i].OpenAlSourceName);
						Sources[i].State = SoundSourceState.PlayPending;
						Sources[i].OpenAlSourceName = 0;
					}
					if (!Sources[i].Looped) {
						Sources[i].State = SoundSourceState.Stopped;
						Sources[i].OpenAlSourceName = 0;
						Sources[i] = Sources[SourceCount - 1];
						SourceCount--;
						i--;
					}
				} else {
					/*
					 * The sound is to be played or is already playing.
					 * Calculate the sound pressure of the source according
					 * to atmospheric attributes and distance to listener,
					 * then compute the gain according to the sound pressure.
					 * */
					OpenBveApi.Math.Vector3 position;
					OpenBveApi.Math.Vector3 velocity;
					if (Sources[i].Train != null) {
						OpenBveApi.Math.Vector3 direction;
						TrainManager.CreateWorldCoordinates(Sources[i].Train, Sources[i].Car, Sources[i].Position.X, Sources[i].Position.Y, Sources[i].Position.Z, out position.X, out position.Y, out position.Z, out direction.X, out direction.Y, out direction.Z);
						velocity = Sources[i].Train.Cars[Sources[i].Car].Specs.CurrentSpeed * direction;
					} else {
						position = Sources[i].Position;
						velocity = OpenBveApi.Math.Vector3.Null;
					}
					OpenBveApi.Math.Vector3 positionDifference = position - listenerPosition;
					double distance = positionDifference.Norm();
					double soundPower = Sources[i].Power;
					double soundPressure = Math.Sqrt(airDensity * soundPower / (4.0 * Math.PI * distance * distance * speedOfSound));
					double gain;
					if (soundPressure > AuditoryThreshold) {
						gain = GlobalVolume * Sources[i].Volume * Math.Log(soundPressure / AuditoryThreshold) / Math.Log(ThresholdOfPain / AuditoryThreshold);
					} else {
						gain = 0.0;
					}
					if (gain <= GainThreshold) {
						/*
						 * If the gain is too low to be audible, stop the sound.
						 * If the sound is not looping, stop it if necessary,
						 * then remove it from the list of sound sources.
						 * */
						if (Sources[i].State == SoundSourceState.Playing) {
							Al.alDeleteSources(1, ref Sources[i].OpenAlSourceName);
							Sources[i].State = SoundSourceState.PlayPending;
							Sources[i].OpenAlSourceName = 0;
						}
						if (!Sources[i].Looped) {
							Sources[i].State = SoundSourceState.Stopped;
							Sources[i].OpenAlSourceName = 0;
							Sources[i] = Sources[SourceCount - 1];
							SourceCount--;
							i--;
						}
					} else {
						/*
						 * Play the sound and update position, velocity, pitch and gain.
						 * For non-looping sounds, check if the sound is still playing.
						 * */
						if (Sources[i].State != SoundSourceState.Playing) {
							LoadBuffer(Sources[i].Buffer);
							if (Sources[i].Buffer.Loaded) {
								Al.alGenSources(1, out Sources[i].OpenAlSourceName);
								Al.alSourcei(Sources[i].OpenAlSourceName, Al.AL_BUFFER, Sources[i].Buffer.OpenAlBufferName);
							} else {
								/*
								 * We cannot play the sound because
								 * the buffer could not be loaded.
								 * */
								Sources[i].State = SoundSourceState.Stopped;
								continue;
							}
						}
						Al.alSource3f(Sources[i].OpenAlSourceName, Al.AL_POSITION, (float)positionDifference.X, (float)positionDifference.Y, (float)positionDifference.Z);
						Al.alSource3f(Sources[i].OpenAlSourceName, Al.AL_VELOCITY, (float)velocity.X, (float)velocity.Y, (float)velocity.Z);
						Al.alSourcef(Sources[i].OpenAlSourceName, Al.AL_PITCH, (float)Sources[i].Pitch);
						Al.alSourcef(Sources[i].OpenAlSourceName, Al.AL_GAIN, (float)gain);
						if (Sources[i].State != SoundSourceState.Playing) {
							Al.alSourcei(Sources[i].OpenAlSourceName, Al.AL_LOOPING, Sources[i].Looped ? Al.AL_TRUE : Al.AL_FALSE);
							Al.alSourcePlay(Sources[i].OpenAlSourceName);
							Sources[i].State = SoundSourceState.Playing;
						}
						if (!Sources[i].Looped) {
							int state;
							Al.alGetSourcei(Sources[i].OpenAlSourceName, Al.AL_SOURCE_STATE, out state);
							if (state != Al.AL_INITIAL & state != Al.AL_PLAYING) {
								/*
								 * The sound is not playing any longer.
								 * Remove it from the list of sound sources.
								 * */
								Al.alDeleteSources(1, ref Sources[i].OpenAlSourceName);
								Sources[i].State = SoundSourceState.Stopped;
								Sources[i].OpenAlSourceName = 0;
								Sources[i] = Sources[SourceCount - 1];
								SourceCount--;
								i--;
							}
						}
					}
				}
			}
		}
		
	}
}