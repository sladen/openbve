using System;
using System.Windows.Forms;
using Tao.OpenAl;

namespace OpenBve {
	internal static partial class Sounds {
		
		// --- members ---
		
		/// <summary>The current OpenAL device.</summary>
		private static IntPtr OpenAlDevice = IntPtr.Zero;
		
		/// <summary>The current OpenAL context.</summary>
		private static IntPtr OpenAlContext = IntPtr.Zero;
		
		/// <summary>A list of all sound buffers.</summary>
		private static SoundBuffer[] Buffers = new SoundBuffer[16];
		
		/// <summary>The number of sound buffers.</summary>
		private static int BufferCount = 0;
		
		/// <summary>A list of all sound sources.</summary>
		private static SoundSource[] Sources = new SoundSource[16];
		
		/// <summary>The number of sound sources.</summary>
		private static int SourceCount = 0;
		
		/// <summary>The auditory threshold in pascal. Sounds with pressures below this value are not audible.</summary>
		internal static double AuditoryThreshold = 0.00002;
		
		internal static double ThresholdOfPain = 0.356;
		
		/// <summary>The gain threshold. Sounds with gains below this value are not played.</summary>
		internal static double GainThreshold = 0.0001;
		
		/// <summary>The global volume factor.</summary>
		internal static double GlobalVolume = 1.0;

		/// <summary>Whether all sounds are mute.</summary>
		internal static bool GlobalMute = false;

		
		// --- initialization and deinitialization ---
		
		/// <summary>Initializes audio. A call to SDL_Init must have been made before calling this function. A call to Deinitialize must be made when terminating the program.</summary>
		/// <returns>Whether initializing audio was successful.</returns>
		internal static bool Initialize() {
			Deinitialize();
			OpenAlDevice = Alc.alcOpenDevice(null);
			if (OpenAlDevice != IntPtr.Zero) {
				OpenAlContext = Alc.alcCreateContext(OpenAlDevice, IntPtr.Zero);
				if (OpenAlContext != IntPtr.Zero) {
					Alc.alcMakeContextCurrent(OpenAlContext);
					try {
						Al.alSpeedOfSound(343.0f);
					} catch {
						MessageBox.Show("OpenAL 1.1 is required. You seem to have OpenAL 1.0.", "openBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					Al.alDistanceModel(Al.AL_NONE);
					return true;
				} else {
					Alc.alcCloseDevice(OpenAlDevice);
					OpenAlDevice = IntPtr.Zero;
					MessageBox.Show("The OpenAL context could not be created.", "openBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return false;
				}
			} else {
				OpenAlContext = IntPtr.Zero;
				MessageBox.Show("The OpenAL sound device could not be opened.", "openBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
		}
		
		/// <summary>Deinitializes audio.</summary>
		internal static void Deinitialize() {
			StopAllSounds();
			UnloadAllBuffers();
			if (OpenAlContext != IntPtr.Zero) {
				Alc.alcMakeContextCurrent(IntPtr.Zero);
				Alc.alcDestroyContext(OpenAlContext);
				OpenAlContext = IntPtr.Zero;
			}
			if (OpenAlDevice != IntPtr.Zero) {
				Alc.alcCloseDevice(OpenAlDevice);
				OpenAlDevice = IntPtr.Zero;
			}
		}
		
		
		// --- registering buffers ---
		
		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="file">The path to the sound file.</param>
		/// <returns>The handle to the sound buffer.</returns>
		internal static SoundBuffer RegisterBuffer(string file) {
			return RegisterBuffer(new OpenBveApi.Path.FileReference(file));
		}
		
		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="path">The path to the sound.</param>
		/// <returns>The handle to the sound buffer.</returns>
		internal static SoundBuffer RegisterBuffer(OpenBveApi.Path.PathReference path) {
			for (int i = 0; i < BufferCount; i++) {
				if (Buffers[i].Origin is PathOrigin) {
					if (((PathOrigin)Buffers[i].Origin).Path == path) {
						return Buffers[i];
					}
				}
			}
			if (Buffers.Length == BufferCount) {
				Array.Resize<SoundBuffer>(ref Buffers, Buffers.Length << 1);
			}
			Buffers[BufferCount] = new SoundBuffer(path);
			BufferCount++;
			return Buffers[BufferCount - 1];
		}

		/// <summary>Registers a sound buffer and returns a handle to the buffer.</summary>
		/// <param name="data">The raw sound data.</param>
		/// <returns>The handle to the sound buffer.</returns>
		internal static SoundBuffer RegisterBuffer(OpenBveApi.Sounds.Sound data) {
			if (Buffers.Length == BufferCount) {
				Array.Resize<SoundBuffer>(ref Buffers, Buffers.Length << 1);
			}
			Buffers[BufferCount] = new SoundBuffer(data);
			BufferCount++;
			return Buffers[BufferCount - 1];
		}

		
		// --- loading buffers ---
		
		/// <summary>Loads the specified sound buffer.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <returns>Whether loading the buffer was successful.</returns>
		internal static bool LoadBuffer(SoundBuffer buffer) {
			if (buffer.Loaded) {
				return true;
			} else if (buffer.Ignore) {
				return false;
			} else {
				OpenBveApi.Sounds.Sound sound;
				if (buffer.Origin.GetSound(out sound)) {
					if (sound.BitsPerSample == 8 | sound.BitsPerSample == 16) {
						Al.alGenBuffers(1, out buffer.OpenAlBufferName);
						int format = sound.BitsPerSample == 8 ? Al.AL_FORMAT_MONO8 : Al.AL_FORMAT_MONO16;
						Al.alBufferData(buffer.OpenAlBufferName, format, sound.Bytes[0], sound.Bytes[0].Length, sound.SampleRate);
						buffer.Duration = sound.Duration;
						buffer.Loaded = true;
						return true;
					}
				}
			}
			buffer.Ignore = true;
			return false;
		}
		
		/// <summary>Loads all sound buffers immediately.</summary>
		internal static void LoadAllBuffers() {
			for (int i = 0; i < BufferCount; i++) {
				LoadBuffer(Buffers[i]);
			}
		}
		
		
		// --- unloading buffers ---
		
		/// <summary>Unloads the specified sound buffer.</summary>
		/// <param name="buffer"></param>
		internal static void UnloadBuffer(SoundBuffer buffer) {
			if (buffer.Loaded) {
				Al.alDeleteBuffers(1, ref buffer.OpenAlBufferName);
				buffer.OpenAlBufferName = 0;
				buffer.Loaded = false;
				buffer.Ignore = false;
			}
		}
		
		/// <summary>Unloads all sound buffers immediately.</summary>
		internal static void UnloadAllBuffers() {
			for (int i = 0; i < BufferCount; i++) {
				UnloadBuffer(Buffers[i]);
			}
		}
		
		
		// --- play or stop sounds ---
		
		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="power">The sound power in watts.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal static SoundSource PlaySound(SoundBuffer buffer, double power, double pitch, double volume, OpenBveApi.Math.Vector3 position, bool looped) {
			if (Sources.Length == SourceCount) {
				Array.Resize<SoundSource>(ref Sources, Sources.Length << 1);
			}
			Sources[SourceCount] = new SoundSource(buffer, power, pitch, volume, position, null, 0, looped);
			SourceCount++;
			return Sources[SourceCount - 1];
		}
		
		/// <summary>Plays a sound.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <param name="power">The sound power in watts.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="position">The position. If a train and car are specified, the position is relative to the car, otherwise absolute.</param>
		/// <param name="train">The train the sound is attached to, or a null reference.</param>
		/// <param name="car">The car in the train the sound is attached to.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal static SoundSource PlaySound(SoundBuffer buffer, double power, double pitch, double volume, OpenBveApi.Math.Vector3 position, TrainManager.Train train, int car, bool looped) {
			if (Sources.Length == SourceCount) {
				Array.Resize<SoundSource>(ref Sources, Sources.Length << 1);
			}
			Sources[SourceCount] = new SoundSource(buffer, power, pitch, volume, position, train, car, looped);
			SourceCount++;
			return Sources[SourceCount - 1];
		}
		
		/// <summary>Stops the specified sound source.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		internal static void StopSound(SoundSource source) {
			if (source != null) {
				if (source.State == SoundSourceState.Playing) {
					Al.alDeleteSources(1, ref source.OpenAlSourceName);
					source.OpenAlSourceName = 0;
				}
				source.State = SoundSourceState.Stopped;
			}
		}
		
		/// <summary>Stops all sounds.</summary>
		internal static void StopAllSounds() {
			for (int i = 0; i < SourceCount; i++) {
				if (Sources[i].State == SoundSourceState.Playing) {
					Al.alDeleteSources(1, ref Sources[i].OpenAlSourceName);
					Sources[i].OpenAlSourceName = 0;
				}
				Sources[i].State = SoundSourceState.Stopped;
			}
		}
		
		/// <summary>Stops all sounds that are attached to the specified train.</summary>
		/// <param name="train">The train.</param>
		internal static void StopAllSounds(TrainManager.Train train) {
			for (int i = 0; i < SourceCount; i++) {
				if (Sources[i].Train == train) {
					if (Sources[i].State == SoundSourceState.Playing) {
						Al.alDeleteSources(1, ref Sources[i].OpenAlSourceName);
						Sources[i].OpenAlSourceName = 0;
					}
					Sources[i].State = SoundSourceState.Stopped;
				}
			}
		}
		
		
		// --- tests ---
		
		/// <summary>Checks whether the specified sound is playing or supposed to be playing.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		/// <returns>Whether the sound is playing or supposed to be playing.</returns>
		internal static bool IsPlaying(SoundSource source) {
			if (source != null) {
				if (source.State == SoundSourceState.PlayPending | source.State == SoundSourceState.Playing) {
					return true;
				}
			}
			return false;
		}

		/// <summary>Checks whether the specified sound is stopped or supposed to be stopped.</summary>
		/// <param name="source">The sound source, or a null reference.</param>
		/// <returns>Whether the sound is stopped or supposed to be stopped.</returns>
		internal static bool IsStopped(SoundSource source) {
			if (source != null) {
				if (source.State == SoundSourceState.StopPending | source.State == SoundSourceState.Stopped) {
					return true;
				}
			}
			return false;
		}
		
		/// <summary>Gets the duration of the specified sound buffer in seconds.</summary>
		/// <param name="buffer">The sound buffer.</param>
		/// <returns>The duration of the sound buffer in seconds, or zero if the buffer could not be loaded.</returns>
		internal static double GetDuration(SoundBuffer buffer) {
			LoadBuffer(buffer);
			return buffer.Duration;
		}
		
		
		// --- statistics ---
		
		/// <summary>Gets the number of registered sound buffers.</summary>
		/// <returns>The number of registered sound buffers.</returns>
		internal static int GetNumberOfRegisteredBuffers() {
			return BufferCount;
		}

		/// <summary>Gets the number of loaded sound buffers.</summary>
		/// <returns>The number of loaded sound buffers.</returns>
		internal static int GetNumberOfLoadedBuffers() {
			int count = 0;
			for (int i = 0; i < BufferCount; i++) {
				if (Buffers[i].Loaded) {
					count++;
				}
			}
			return count;
		}
		
		/// <summary>Gets the number of registered sound sources.</summary>
		/// <returns>The number of registered sound sources.</returns>
		internal static int GetNumberOfRegisteredSources() {
			return SourceCount;
		}

		/// <summary>Gets the number of playing sound sources.</summary>
		/// <returns>The number of playing sound sources.</returns>
		internal static int GetNumberOfPlayingSources() {
			int count = 0;
			for (int i = 0; i < SourceCount; i++) {
				if (Sources[i].State == SoundSourceState.Playing) {
					count++;
				}
			}
			return count;
		}

	}
}