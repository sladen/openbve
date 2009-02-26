using System;
using Tao.OpenAl;
using Tao.Sdl;

namespace OpenBve {
    internal static class SoundManager {

        // sound buffers
        private class SoundBuffer {
            internal string FileName;
            internal double Duration;
            internal int OpenAlBufferIndex;
            internal float Radius;
        }
        private static SoundBuffer[] SoundBuffers = new SoundBuffer[16];

        // sound sources
        internal class SoundSource {
            internal World.Vector3D Position;
            internal float[] OpenAlPosition;
            internal float[] OpenAlVelocity;
            internal int OpenAlSourceIndex;
            internal int SoundBufferIndex;
            internal double Radius;
            internal float Pitch;
            internal float Gain;
            internal bool Looped;
            internal bool Suppressed;
            internal bool FinishedPlaying;
            internal bool HasHandle;
            internal int TrainIndex;
            internal int CarIndex;
        }
        internal static SoundSource[] SoundSources = new SoundSource[16];

        // listener
        private static float[] ListenerPosition = new float[] { 0.0f, 0.0f, 0.0f };
        private static float[] ListenerVelocity = new float[] { 0.0f, 0.0f, 0.0f };
        private static float[] ListenerOrientation = new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f };
        private static double InternalTimer = 0.0;

        // misc
        internal static double OuterRadiusFactor = 8.0;
        private static double OuterRadiusFactorMinimum = 2.0;
        private static double OuterRadiusFactorMaximum = 8.0;
        private static double OuterRadiusSpeed = 0.0;
        private static int SoundsQueriedPlaying = 0;
        private static int SoundsActuallyPlaying = 0;

        // initialize
        internal static void Initialize() {
            Sdl.SDL_Init(Sdl.SDL_INIT_AUDIO);
            SdlMixer.Mix_OpenAudio(44100, (short)SdlMixer.MIX_DEFAULT_FORMAT, 1, 512);
            Alut.alutInit();
            Al.alSpeedOfSound(343.0f);
            switch (Interface.CurrentOptions.SoundRange) {
                case Interface.SoundRange.Low:
                    OuterRadiusFactorMinimum = 2.0;
                    OuterRadiusFactorMaximum = 8.0;
                    break;

                case Interface.SoundRange.Medium:
                    OuterRadiusFactorMinimum = 4.0;
                    OuterRadiusFactorMaximum = 16.0;
                    break;
                case Interface.SoundRange.High:
                    OuterRadiusFactorMinimum = 8.0;
                    OuterRadiusFactorMaximum = 32.0;
                    break;
            }
            OuterRadiusFactor = OuterRadiusFactorMaximum;
        }

        // deinitialize
        internal static void Deinitialize() {
            SoundManager.StopAllSounds(true);
            SoundManager.UnuseAllSoundsBuffers();
            SdlMixer.Mix_CloseAudio();
            Alut.alutExit();
        }

        // update
        internal static void Update(double TimeElapsed) {
            // listener
            double vx = World.CameraTrackFollower.WorldDirection.X * World.CameraSpeed;
            double vy = World.CameraTrackFollower.WorldDirection.Y * World.CameraSpeed;
            double vz = World.CameraTrackFollower.WorldDirection.Z * World.CameraSpeed;
            if (World.CameraMode == World.CameraViewMode.Interior) {
                ListenerVelocity[0] = 0.0f;
                ListenerVelocity[1] = 0.0f;
                ListenerVelocity[2] = 0.0f;
            } else {
                ListenerVelocity[0] = (float)vx;
                ListenerVelocity[1] = (float)vy;
                ListenerVelocity[2] = (float)vz;
            }
            ListenerOrientation[0] = (float)World.AbsoluteCameraDirection.X;
            ListenerOrientation[1] = (float)World.AbsoluteCameraDirection.Y;
            ListenerOrientation[2] = (float)World.AbsoluteCameraDirection.Z;
            ListenerOrientation[3] = (float)-World.AbsoluteCameraUp.X;
            ListenerOrientation[4] = (float)-World.AbsoluteCameraUp.Y;
            ListenerOrientation[5] = (float)-World.AbsoluteCameraUp.Z;
            Al.alListenerfv(Al.AL_POSITION, ListenerPosition);
            Al.alListenerfv(Al.AL_VELOCITY, ListenerVelocity);
            Al.alListenerfv(Al.AL_ORIENTATION, ListenerOrientation);
            double cx = World.AbsoluteCameraPosition.X;
            double cy = World.AbsoluteCameraPosition.Y;
            double cz = World.AbsoluteCameraPosition.Z;
            // outer radius
            int n = Interface.CurrentOptions.SoundNumber - 3;
            if (SoundsActuallyPlaying >= n) {
                OuterRadiusSpeed -= TimeElapsed;
                if (OuterRadiusSpeed < -1.0) OuterRadiusSpeed = -1.0;
            } else if (SoundsQueriedPlaying < n) {
                OuterRadiusSpeed += TimeElapsed;
                if (OuterRadiusSpeed > 1.0) OuterRadiusSpeed = 1.0;
            } else {
                OuterRadiusSpeed -= (double)Math.Sign(OuterRadiusSpeed) * TimeElapsed;
                if (OuterRadiusSpeed * OuterRadiusSpeed <= TimeElapsed * TimeElapsed) {
                    OuterRadiusSpeed = 0.0;
                }
            }
            OuterRadiusFactor += OuterRadiusSpeed * TimeElapsed;
            if (OuterRadiusFactor < OuterRadiusFactorMinimum) {
                OuterRadiusFactor = OuterRadiusFactorMinimum;
            } else if (OuterRadiusFactor > OuterRadiusFactorMaximum) {
                OuterRadiusFactor = OuterRadiusFactorMaximum;
            }
            // sources
            SoundsQueriedPlaying = 0;
            SoundsActuallyPlaying = 0;
            for (int i = 0; i < SoundSources.Length; i++) {
                if (SoundSources[i] != null && !SoundSources[i].FinishedPlaying) {
                    double rx = SoundSources[i].Position.X;
                    double ry = SoundSources[i].Position.Y;
                    double rz = SoundSources[i].Position.Z;
                    int t = SoundSources[i].TrainIndex;
                    double px, py, pz;
                    if (t >= 0) {
                        int c = SoundSources[i].CarIndex;
                        double tx, ty, tz;
                        TrainManager.CreateWorldCoordinates(TrainManager.Trains[t], c, rx, ry, rz, out px, out py, out pz, out tx, out ty, out tz);
                        px -= cx; py -= cy; pz -= cz;
                        double sp = TrainManager.Trains[t].Specs.CurrentAverageSpeed;
                        if (World.CameraMode != World.CameraViewMode.Interior) {
                            SoundSources[i].OpenAlVelocity[0] = (float)(tx * sp);
                            SoundSources[i].OpenAlVelocity[1] = (float)(ty * sp);
                            SoundSources[i].OpenAlVelocity[2] = (float)(tz * sp);
                        } else {
                            SoundSources[i].OpenAlVelocity[0] = (float)(tx * sp - vx);
                            SoundSources[i].OpenAlVelocity[1] = (float)(ty * sp - vy);
                            SoundSources[i].OpenAlVelocity[2] = (float)(tz * sp - vz);
                        }
                    } else {
                        px = rx - cx; py = ry - cy; pz = rz - cz;
                        if (World.CameraMode != World.CameraViewMode.Interior) {
                            SoundSources[i].OpenAlVelocity[0] = 0.0f;
                            SoundSources[i].OpenAlVelocity[1] = 0.0f;
                            SoundSources[i].OpenAlVelocity[2] = 0.0f;
                        } else {
                            SoundSources[i].OpenAlVelocity[0] = (float)-vx;
                            SoundSources[i].OpenAlVelocity[1] = (float)-vy;
                            SoundSources[i].OpenAlVelocity[2] = (float)-vz;
                        }
                    }
                    double dt = px * px + py * py + pz * pz;
                    double rt = SoundSources[i].Radius * SoundSources[i].Radius;
                    bool update = false, play = false;
                    double gainfactor = 1.0;
                    int j = SoundSources[i].OpenAlSourceIndex;
                    if (dt <= rt) {
                        update = true;
                        if (SoundSources[i].Suppressed) {
                            play = true;
                            SoundSources[i].Suppressed = false;
                        }
                    } else {
                        double st = OuterRadiusFactor * OuterRadiusFactor * rt;
                        if (dt <= st) {
                            gainfactor = (dt - rt) / (st - rt);
                            gainfactor = 2.0 / (gainfactor + 1.0) - 1.0;
                            update = true;
                            if (SoundSources[i].Suppressed) {
                                play = true;
                                SoundSources[i].Suppressed = false;
                            }
                        } else {
                            if (dt <= OuterRadiusFactorMaximum * rt) {
                                SoundsQueriedPlaying++;
                            }
                            if (!SoundSources[i].Suppressed) {
                                if (SoundSources[i].Looped) {
                                    if (j >= 0) {
                                        Al.alSourceStop(j);
                                        Al.alDeleteSources(1, ref j);
                                    }
                                    SoundSources[i].OpenAlSourceIndex = -1;
                                    SoundSources[i].Suppressed = true;
                                } else {
                                    StopSound(i, false);
                                }
                            } else if (!SoundSources[i].Looped) {
                                StopSound(i, false);
                            } continue;
                        }
                    }
                    if (update) {
                        SoundsQueriedPlaying++;
                        SoundsActuallyPlaying++;
                    }
                    if (play) {
                        if (SoundSources[i].SoundBufferIndex >= 0) {
                            UseSoundBuffer(SoundSources[i].SoundBufferIndex);
                            if (SoundBuffers[SoundSources[i].SoundBufferIndex].OpenAlBufferIndex >= 0) {
                                Al.alGetError();
                                Al.alGenSources(1, out j);
                                int err = Al.alGetError();
                                if (err == Al.AL_NO_ERROR) {
                                    SoundSources[i].OpenAlSourceIndex = j;
                                    Al.alSourcei(j, Al.AL_BUFFER, SoundBuffers[SoundSources[i].SoundBufferIndex].OpenAlBufferIndex);
                                } else {
                                    SoundSources[i].Suppressed = true;
                                    continue;
                                }
                            } else {
                                StopSound(i, false);
                                continue;
                            }
                        } else {
                            StopSound(i, false);
                            continue;
                        }
                    }
                    if (update) {
                        if (play || IsPlaying(i)) {
                            SoundSources[i].OpenAlPosition[0] = (float)px;
                            SoundSources[i].OpenAlPosition[1] = (float)py;
                            SoundSources[i].OpenAlPosition[2] = (float)pz;
                            Al.alSourcefv(j, Al.AL_POSITION, SoundSources[i].OpenAlPosition);
                            Al.alSourcefv(j, Al.AL_VELOCITY, SoundSources[i].OpenAlVelocity);
                            Al.alSourcef(j, Al.AL_PITCH, SoundSources[i].Pitch);
                            float g = SoundSources[i].Gain * SoundSources[i].Gain * (float)gainfactor;
                            if (g > 1.0f) g = 1.0f;
                            Al.alSourcef(j, Al.AL_GAIN, g);
                        } else {
                            StopSound(i, false);
                            continue;
                        }
                    }
                    if (play) {
                        Al.alSourcei(j, Al.AL_LOOPING, SoundSources[i].Looped ? Al.AL_TRUE : Al.AL_FALSE);
                        Al.alSourcef(j, Al.AL_REFERENCE_DISTANCE, SoundBuffers[SoundSources[i].SoundBufferIndex].Radius);
                        Al.alSourcePlay(j);
                    }
                }
            }
            // infrequent updates
            InternalTimer += TimeElapsed;
            if (InternalTimer > 1.0) {
                InternalTimer -= 1.0;
                double Elevation = World.AbsoluteCameraPosition.Y + Game.RouteInitialElevation;
                double AirTemperature = Game.GetAirTemperature(Elevation);
                double AirPressure = Game.GetAirPressure(Elevation, AirTemperature);
                double SpeedOfSound = Game.GetSpeedOfSound(AirPressure, AirTemperature);
                Al.alSpeedOfSound((float)SpeedOfSound);
            }
        }

        // use sound buffer
        private static int UseSoundBuffer(int SoundBufferIndex) {
            if (SoundBufferIndex >= 0) {
                if (SoundBuffers[SoundBufferIndex].OpenAlBufferIndex == -1) {
                    // load via alut
                    SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = Alut.alutCreateBufferFromFile(SoundBuffers[SoundBufferIndex].FileName);
                    if (SoundBuffers[SoundBufferIndex].OpenAlBufferIndex != 0) {
                        // detect stereo
                        int c; Al.alGetBufferi(SoundBuffers[SoundBufferIndex].OpenAlBufferIndex, Al.AL_CHANNELS, out c);
                        if (c == 1) {
                            // valid mono
                            try {
                                // determine duration
                                int bits; Al.alGetBufferiv(SoundBuffers[SoundBufferIndex].OpenAlBufferIndex, Al.AL_BITS, out bits);
                                int channels; Al.alGetBufferiv(SoundBuffers[SoundBufferIndex].OpenAlBufferIndex, Al.AL_CHANNELS, out channels);
                                int frequency; Al.alGetBufferiv(SoundBuffers[SoundBufferIndex].OpenAlBufferIndex, Al.AL_FREQUENCY, out frequency);
                                int size; Al.alGetBufferiv(SoundBuffers[SoundBufferIndex].OpenAlBufferIndex, Al.AL_SIZE, out size);
                                if (bits != 0 & frequency != 0) {
                                    SoundBuffers[SoundBufferIndex].Duration = 8.0 * (double)size / (double)(bits * frequency);
                                } else {
                                    // guess duration
                                    System.IO.FileInfo f = new System.IO.FileInfo(SoundBuffers[SoundBufferIndex].FileName);
                                    SoundBuffers[SoundBufferIndex].Duration = 0.0000113378684807256 * (double)f.Length;
                                }
                            } catch {
                                // guess duration
                                System.IO.FileInfo f = new System.IO.FileInfo(SoundBuffers[SoundBufferIndex].FileName);
                                SoundBuffers[SoundBufferIndex].Duration = 0.0000113378684807256 * (double)f.Length;
                            }
                        } else {
                            // must not be stereo
                            Al.alDeleteBuffers(1, ref SoundBuffers[SoundBufferIndex].OpenAlBufferIndex);
                            SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = 0;
                        }
                    }
                    if (SoundBuffers[SoundBufferIndex].OpenAlBufferIndex == 0) {
                        // load via sdl
                        IntPtr h = SdlMixer.Mix_LoadWAV(SoundBuffers[SoundBufferIndex].FileName);
                        if (h != IntPtr.Zero) {
                            // convert sdl to openal
                            SdlMixer.Mix_Chunk chu = (SdlMixer.Mix_Chunk)System.Runtime.InteropServices.Marshal.PtrToStructure(h, typeof(SdlMixer.Mix_Chunk));
                            if (chu.abuf != IntPtr.Zero) {
                                // determine duration
                                double dur = (double)chu.alen * 8.0 / (double)(44100 * 16 * 1);
                                // convert data
                                byte[] dat = new byte[chu.alen];
                                System.Runtime.InteropServices.Marshal.Copy(chu.abuf, dat, 0, chu.alen);
                                SdlMixer.Mix_FreeChunk(h);
                                int i; Al.alGenBuffers(1, out i);
                                Al.alBufferData(i, Al.AL_FORMAT_MONO16, dat, dat.Length, 44100);
                                SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = i;
                                SoundBuffers[SoundBufferIndex].Duration = dur;
                            } else {
                                // no usable sound
                                SdlMixer.Mix_FreeChunk(h);
                                SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = -1;
                            }
                        } else {
                            // no usable sound
                            SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = -1;
                        }
                    }
                }
                return SoundBuffers[SoundBufferIndex].OpenAlBufferIndex;
            } else {
                return -1;
            }
        }

        // unuse sound buffer
        private static void UnuseSoundBuffer(int SoundBufferIndex) {
            if (SoundBuffers[SoundBufferIndex].OpenAlBufferIndex >= 0) {
                Al.alDeleteBuffers(1, ref SoundBuffers[SoundBufferIndex].OpenAlBufferIndex);
                SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = -1;
            }
        }
        private static void UnuseAllSoundsBuffers() {
            for (int i = 0; i < SoundBuffers.Length; i++) {
                if (SoundBuffers[i] != null) {
                    UnuseSoundBuffer(i);
                }
            }
        }

        // load sound
        internal static int LoadSound(string FileName, double Radius) {
            int i;
            for (i = 0; i < SoundBuffers.Length; i++) {
                if (SoundBuffers[i] != null && string.Compare(SoundBuffers[i].FileName, FileName, StringComparison.OrdinalIgnoreCase) == 0 & SoundBuffers[i].Radius == Radius) {
                    return i;
                }
            }
            for (i = 0; i < SoundBuffers.Length; i++) {
                if (SoundBuffers[i] == null) break;
            }
            if (i == SoundBuffers.Length) {
                Array.Resize<SoundBuffer>(ref SoundBuffers, SoundBuffers.Length << 1);
            }
            SoundBuffers[i] = new SoundBuffer();
            SoundBuffers[i].FileName = FileName;
            SoundBuffers[i].OpenAlBufferIndex = -1;
            SoundBuffers[i].Radius = (float)Radius;
            return i;
        }

        // get sound length
        internal static double GetSoundLength(int SoundBufferIndex) {
            UseSoundBuffer(SoundBufferIndex);
            return SoundBuffers[SoundBufferIndex].Duration;
        }

        // play sound
        internal enum Importance { DontCare, AlwaysPlay }
        internal static void PlaySound(ref int SoundSourceIndex, int SoundBufferIndex, World.Vector3D Position, Importance Important, bool Looped) {
            PlaySound(ref SoundSourceIndex, true, SoundBufferIndex, -1, -1, Position, Important, Looped, 1.0, 1.0);
        }
        internal static void PlaySound(int SoundBufferIndex, World.Vector3D Position, Importance Important, bool Looped) {
            int a = -1;
            PlaySound(ref a, false, SoundBufferIndex, -1, -1, Position, Important, Looped, 1.0, 1.0);
        }
        internal static void PlaySound(int SoundBufferIndex, int TrainIndex, int CarIndex, World.Vector3D Position, Importance Important, bool Looped) {
            int a = -1;
            PlaySound(ref a, false, SoundBufferIndex, TrainIndex, CarIndex, Position, Important, Looped, 1.0, 1.0);
        }
        internal static void PlaySound(ref int SoundSourceIndex, int SoundBufferIndex, int TrainIndex, int CarIndex, World.Vector3D Position, Importance Important, bool Looped) {
            PlaySound(ref SoundSourceIndex, true, SoundBufferIndex, TrainIndex, CarIndex, Position, Important, Looped, 1.0, 1.0);
        }
        internal static void PlaySound(int SoundBufferIndex, int TrainIndex, int CarIndex, World.Vector3D Position, Importance Important, bool Looped, double Pitch, double Gain) {
            int a = -1;
            PlaySound(ref a, false, SoundBufferIndex, TrainIndex, CarIndex, Position, Important, Looped, Pitch, Gain);
        }
        internal static void PlaySound(ref int SoundSourceIndex, int SoundBufferIndex, int TrainIndex, int CarIndex, World.Vector3D Position, Importance Important, bool Looped, double Pitch, double Gain) {
            PlaySound(ref SoundSourceIndex, true, SoundBufferIndex, TrainIndex, CarIndex, Position, Important, Looped, Pitch, Gain);
        }
        private static void PlaySound(ref int SoundSourceIndex, bool ReturnHandle, int SoundBufferIndex, int TrainIndex, int CarIndex, World.Vector3D Position, Importance Important, bool Looped, double Pitch, double Gain) {
            if (SoundSourceIndex >= 0) {
                StopSound(ref SoundSourceIndex);
            }
            if (Game.MinimalisticSimulation & Important == Importance.DontCare) {
                return;
            }
            if (SoundBufferIndex == -1) {
                return;
            }
            int i;
            for (i = 0; i < SoundSources.Length; i++) {
                if (SoundSources[i] == null) break;
            }
            if (i >= SoundSources.Length) {
                Array.Resize<SoundSource>(ref SoundSources, SoundSources.Length << 1);
            }
            SoundSources[i] = new SoundSource();
            SoundSources[i].Position = Position;
            SoundSources[i].OpenAlPosition = new float[] { 0.0f, 0.0f, 0.0f };
            SoundSources[i].OpenAlVelocity = new float[] { 0.0f, 0.0f, 0.0f };
            SoundSources[i].SoundBufferIndex = SoundBufferIndex;
            SoundSources[i].Radius = SoundBuffers[SoundBufferIndex].Radius;
            SoundSources[i].Pitch = (float)Pitch;
            SoundSources[i].Gain = (float)Gain;
            SoundSources[i].Looped = Looped;
            SoundSources[i].Suppressed = true;
            SoundSources[i].FinishedPlaying = false;
            SoundSources[i].TrainIndex = TrainIndex;
            SoundSources[i].CarIndex = CarIndex;
            SoundSources[i].OpenAlSourceIndex = -1;
            SoundSources[i].HasHandle = ReturnHandle;
            SoundSourceIndex = i;
        }

        // modulate sound
        internal static void ModulateSound(int SoundSourceIndex, double Pitch, double Gain) {
            if (SoundSourceIndex >= 0 && SoundSources[SoundSourceIndex] != null) {
                SoundSources[SoundSourceIndex].Pitch = (float)Pitch;
                SoundSources[SoundSourceIndex].Gain = (float)Gain;
            }
        }

        // stop sound
        private static void StopSound(int SoundSourceIndex, bool InvalidateHandle) {
            if (SoundSources[SoundSourceIndex].HasHandle & !InvalidateHandle) {
                SoundSources[SoundSourceIndex].FinishedPlaying = true;
            } else {
                StopSound(ref SoundSourceIndex);
            }
        }
        internal static void StopSound(ref int SoundSourceIndex) {
            if (SoundSourceIndex >= 0 && SoundSources[SoundSourceIndex] != null) {
                int i = SoundSources[SoundSourceIndex].OpenAlSourceIndex;
                if (i >= 0) {
                    Al.alSourceStop(i);
                    Al.alDeleteSources(1, ref i);
                }
                SoundSources[SoundSourceIndex] = null;
            }
            SoundSourceIndex = -1;
        }
        internal static void StopAllSounds(bool InvalidateHandles) {
            for (int i = 0; i < SoundSources.Length; i++) {
                if (SoundSources[i] != null) {
                    StopSound(i, InvalidateHandles);
                }
            }
        }
        internal static void StopAllSounds(int TrainIndex, bool InvalidateHandles) {
            for (int i = 0; i < SoundSources.Length; i++) {
                if (SoundSources[i] != null && SoundSources[i].TrainIndex == TrainIndex) {
                    StopSound(i, InvalidateHandles);
                }
            }
        }

        // is playing
        internal static bool IsPlaying(int SoundSourceIndex) {
            if (SoundSourceIndex >= 0 && SoundSources[SoundSourceIndex] != null) {
                if (SoundSources[SoundSourceIndex].Suppressed) {
                    return true;
                } else {
                    int i = SoundSources[SoundSourceIndex].OpenAlSourceIndex;
                    if (i >= 0) {
                        int state;
                        Al.alGetSourcei(i, Al.AL_SOURCE_STATE, out state);
                        return state == Al.AL_PLAYING;
                    } else {
                        return false;
                    }
                }
            } else {
                return false;
            }
        }

        // has finished playing
        internal static bool HasFinishedPlaying(int SoundSourceIndex) {
            if (SoundSourceIndex >= 0 && SoundSources[SoundSourceIndex] != null) {
                return SoundSources[SoundSourceIndex].FinishedPlaying;
            } else {
                return true;
            }
        }

    }
}