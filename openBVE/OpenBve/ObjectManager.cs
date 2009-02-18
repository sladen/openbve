using System;

namespace OpenBve {
    internal static class ObjectManager {

        // unified objects
        internal abstract class UnifiedObject { }

        // static objects
        internal class StaticObject : UnifiedObject {
            internal World.Mesh[] Meshes;
            internal int RendererIndex;
            internal float StartingDistance;
            internal float EndingDistance;
            internal byte Dynamic;
        }
        internal static StaticObject[] Objects = new StaticObject[16];
        internal static int ObjectsUsed;
        internal static int[] ObjectsSortedByStart = new int[] { };
        internal static int[] ObjectsSortedByEnd = new int[] { };
        internal static int ObjectsSortedByStartPointer = 0;
        internal static int ObjectsSortedByEndPointer = 0;
        internal static double LastUpdatedTrackPosition = 0.0;

        // animated objects
        internal class Damping {
            internal bool DirectMode;
            internal double NaturalFrequency;
            internal double NaturalTime;
            internal double DampingRatio;
            internal double NaturalDampingFrequency;
            internal double OriginalAngle;
            internal double OriginalDerivative;
            internal double TargetAngle;
            internal double CurrentAngle;
            internal double CurrentDerivative;
            internal double CurrentValue;
            internal double LastUpdated;
            internal double CurrentTimeDelta;
            internal int CurrentTicks;
            internal int CurrentCounter;
            internal Damping(double NaturalFrequency, double DampingRatio) {
                this.DirectMode = true;
                this.NaturalFrequency = NaturalFrequency;
                this.NaturalTime = 1.0 / NaturalFrequency;
                this.DampingRatio = DampingRatio;
                if (DampingRatio < 1.0) {
                    this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(1.0 - DampingRatio * DampingRatio);
                } else if (DampingRatio == 1.0) {
                    this.NaturalDampingFrequency = NaturalFrequency;
                } else {
                    this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(DampingRatio * DampingRatio - 1.0);
                }
                this.OriginalAngle = 0.0;
                this.OriginalDerivative = 0.0;
                this.TargetAngle = 0.0;
                this.CurrentAngle = 0.0;
                this.CurrentDerivative = 0.0;
                this.CurrentValue = 1.0;
                this.LastUpdated = Game.SecondsSinceMidnight;
                this.CurrentTimeDelta = 0.0;
                this.CurrentTicks = 0;
                this.CurrentCounter = 0;
            }
            internal Damping Clone() {
                return (Damping)this.MemberwiseClone();
            }
        }
        internal struct AnimatedObjectState {
            internal World.Vector3D Position;
            internal ObjectManager.StaticObject Object;
        }
        internal class AnimatedObject {
            internal AnimatedObjectState[] States;
            internal FunctionScripts.FunctionScript StateFunction;
            internal int CurrentState;
            internal World.Vector3D TranslateXDirection;
            internal World.Vector3D TranslateYDirection;
            internal World.Vector3D TranslateZDirection;
            internal FunctionScripts.FunctionScript TranslateXFunction;
            internal FunctionScripts.FunctionScript TranslateYFunction;
            internal FunctionScripts.FunctionScript TranslateZFunction;
            internal World.Vector3D RotateXDirection;
            internal World.Vector3D RotateYDirection;
            internal World.Vector3D RotateZDirection;
            internal FunctionScripts.FunctionScript RotateXFunction;
            internal FunctionScripts.FunctionScript RotateYFunction;
            internal FunctionScripts.FunctionScript RotateZFunction;
            internal Damping RotateXDamping;
            internal Damping RotateYDamping;
            internal Damping RotateZDamping;
            internal World.Vector2D TextureShiftXDirection;
            internal World.Vector2D TextureShiftYDirection;
            internal FunctionScripts.FunctionScript TextureShiftXFunction;
            internal FunctionScripts.FunctionScript TextureShiftYFunction;
            internal bool LEDClockwiseWinding;
            internal double LEDMaximumAngle;
            internal FunctionScripts.FunctionScript LEDFunction;
            internal double RefreshRate;
            internal double TimeNextUpdating;
            internal int ObjectIndex;
            internal bool IsFreeOfFunctions() {
                if (this.StateFunction != null) return false;
                if (this.TranslateXFunction != null | this.TranslateYFunction != null | this.TranslateZFunction != null) return false;
                if (this.RotateXFunction != null | this.RotateYFunction != null | this.RotateZFunction != null) return false;
                if (this.TextureShiftXFunction != null | this.TextureShiftYFunction != null) return false;
                if (this.LEDFunction != null) return false;
                return true;
            }
            internal AnimatedObject Clone() {
                AnimatedObject Result = new AnimatedObject();
                Result.States = new AnimatedObjectState[this.States.Length];
                for (int i = 0; i < this.States.Length; i++) {
                    Result.States[i].Position = this.States[i].Position;
                    Result.States[i].Object = CloneObject(this.States[i].Object);
                }
                Result.StateFunction = this.StateFunction == null ? null : this.StateFunction.Clone();
                Result.CurrentState = this.CurrentState;
                Result.TranslateZDirection = this.TranslateZDirection;
                Result.TranslateYDirection = this.TranslateYDirection;
                Result.TranslateXDirection = this.TranslateXDirection;
                Result.TranslateXFunction = this.TranslateXFunction == null ? null : this.TranslateXFunction.Clone();
                Result.TranslateYFunction = this.TranslateYFunction == null ? null : this.TranslateYFunction.Clone();
                Result.TranslateZFunction = this.TranslateZFunction == null ? null : this.TranslateZFunction.Clone();
                Result.RotateXDirection = this.RotateXDirection;
                Result.RotateYDirection = this.RotateYDirection;
                Result.RotateZDirection = this.RotateZDirection;
                Result.RotateXFunction = this.RotateXFunction == null ? null : this.RotateXFunction.Clone();
                Result.RotateXDamping = this.RotateXDamping == null ? null : this.RotateXDamping.Clone();
                Result.RotateYFunction = this.RotateYFunction == null ? null : this.RotateYFunction.Clone();
                Result.RotateYDamping = this.RotateYDamping == null ? null : this.RotateYDamping.Clone();
                Result.RotateZFunction = this.RotateZFunction == null ? null : this.RotateZFunction.Clone();
                Result.RotateZDamping = this.RotateZDamping == null ? null : this.RotateZDamping.Clone();
                Result.TextureShiftXDirection = this.TextureShiftXDirection;
                Result.TextureShiftYDirection = this.TextureShiftYDirection;
                Result.TextureShiftXFunction = this.TextureShiftXFunction == null ? null : this.TextureShiftXFunction.Clone();
                Result.TextureShiftYFunction = this.TextureShiftYFunction == null ? null : this.TextureShiftYFunction.Clone();
                Result.LEDClockwiseWinding = this.LEDClockwiseWinding;
                Result.LEDMaximumAngle = this.LEDMaximumAngle;
                Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
                Result.RefreshRate = this.RefreshRate;
                Result.TimeNextUpdating = 0.0;
                Result.ObjectIndex = -1;
                return Result;
            }
        }
        internal class AnimatedObjectCollection : UnifiedObject {
            internal AnimatedObject[] Objects;
        }
        internal enum VisibilityChangeMode { Hide, DontChange, Show }

        internal static void InitializeAnimatedObject(ref AnimatedObject Object, int StateIndex, bool Overlay) {
            int i = Object.ObjectIndex;
            Renderer.HideObject(i);
            int t = StateIndex;
            if (t >= 0) {
                int n = Object.States[t].Object.Meshes.Length;
                ObjectManager.Objects[i].Meshes = new World.Mesh[n];
                for (int j = 0; j < n; j++) {
                    int m = Object.States[t].Object.Meshes[j].Vertices.Length;
                    ObjectManager.Objects[i].Meshes[j].Vertices = new World.Vertex[m];
                    for (int k = 0; k < m; k++) {
                        ObjectManager.Objects[i].Meshes[j].Vertices[k] = Object.States[t].Object.Meshes[j].Vertices[k];
                    }
                    m = Object.States[t].Object.Meshes[j].Faces.Length;
                    ObjectManager.Objects[i].Meshes[j].Faces = new World.MeshFace[m];
                    for (int k = 0; k < m; k++) {
                        ObjectManager.Objects[i].Meshes[j].Faces[k].Flags = Object.States[t].Object.Meshes[j].Faces[k].Flags;
                        ObjectManager.Objects[i].Meshes[j].Faces[k].Material = Object.States[t].Object.Meshes[j].Faces[k].Material;
                        int o = Object.States[t].Object.Meshes[j].Faces[k].Vertices.Length;
                        ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices = new World.MeshFaceVertex[o];
                        for (int h = 0; h < o; h++) {
                            ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h] = Object.States[t].Object.Meshes[j].Faces[k].Vertices[h];
                        }
                    }
                    ObjectManager.Objects[i].Meshes[j].Materials = Object.States[t].Object.Meshes[j].Materials;
                }
            } else {
                ObjectManager.Objects[i].Meshes = new World.Mesh[] { };
            }
            Object.CurrentState = StateIndex;
            Renderer.ShowObject(i, Overlay);
        }

        internal static void UpdateAnimatedObject(ref AnimatedObject Object, TrainManager.Train Train, int SectionIndex, double TrackPosition, World.Vector3D Position, World.Vector3D Direction, World.Vector3D Up, World.Vector3D Side, bool Overlay, VisibilityChangeMode Visibility, double TimeElapsed) {
            int s = Object.CurrentState;
            int i = Object.ObjectIndex;
            // state change
            if (Object.StateFunction != null) {
                double sd = Object.StateFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                int si = (int)Math.Round(sd);
                int sn = Object.States.Length;
                if (si < 0 | si >= sn) si = -1;
                if (s != si) {
                    InitializeAnimatedObject(ref Object, si, Overlay);
                    s = si;
                }
            }
            // translation
            if (Object.TranslateXFunction != null) {
                double x = Object.TranslateXFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                double rx = Object.TranslateXDirection.X, ry = Object.TranslateXDirection.Y, rz = Object.TranslateXDirection.Z;
                World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
                Position.X += x * rx;
                Position.Y += x * ry;
                Position.Z += x * rz;
            }
            if (Object.TranslateYFunction != null) {
                double y = Object.TranslateYFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                double rx = Object.TranslateYDirection.X, ry = Object.TranslateYDirection.Y, rz = Object.TranslateYDirection.Z;
                World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
                Position.X += y * rx;
                Position.Y += y * ry;
                Position.Z += y * rz;
            }
            if (Object.TranslateZFunction != null) {
                double z = Object.TranslateZFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                double rx = Object.TranslateZDirection.X, ry = Object.TranslateZDirection.Y, rz = Object.TranslateZDirection.Z;
                World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
                Position.X += z * rx;
                Position.Y += z * ry;
                Position.Z += z * rz;
            }
            // rotation
            bool rotateX = Object.RotateXFunction != null;
            bool rotateY = Object.RotateYFunction != null;
            bool rotateZ = Object.RotateZFunction != null;
            double cosX, sinX;
            if (rotateX) {
                double a = Object.RotateXFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                ObjectManager.UpdateDamping(ref Object.RotateXDamping, TimeElapsed, ref a);
                cosX = Math.Cos(a);
                sinX = Math.Sin(a);
            } else {
                cosX = 0.0; sinX = 0.0;
            }
            double cosY, sinY;
            if (rotateY) {
                double a = Object.RotateYFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                ObjectManager.UpdateDamping(ref Object.RotateYDamping, TimeElapsed, ref a);
                cosY = Math.Cos(a);
                sinY = Math.Sin(a);
            } else {
                cosY = 0.0; sinY = 0.0;
            }
            double cosZ, sinZ;
            if (rotateZ) {
                double a = Object.RotateZFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                ObjectManager.UpdateDamping(ref Object.RotateZDamping, TimeElapsed, ref a);
                cosZ = Math.Cos(a);
                sinZ = Math.Sin(a);
            } else {
                cosZ = 0.0; sinZ = 0.0;
            }
            // texture shift
            bool shiftx = Object.TextureShiftXFunction != null;
            bool shifty = Object.TextureShiftYFunction != null;
            if (shiftx | shifty) {
                for (int j = 0; j < ObjectManager.Objects[i].Meshes.Length; j++) {
                    for (int k = 0; k < ObjectManager.Objects[i].Meshes[j].Vertices.Length; k++) {
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].TextureCoordinates = Object.States[s].Object.Meshes[j].Vertices[k].TextureCoordinates;
                    }
                }
                if (shiftx) {
                    double x = Object.TextureShiftXFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                    x -= Math.Floor(x);
                    for (int j = 0; j < ObjectManager.Objects[i].Meshes.Length; j++) {
                        for (int k = 0; k < ObjectManager.Objects[i].Meshes[j].Vertices.Length; k++) {
                            ObjectManager.Objects[i].Meshes[j].Vertices[k].TextureCoordinates.X += (float)(x * Object.TextureShiftXDirection.X);
                            ObjectManager.Objects[i].Meshes[j].Vertices[k].TextureCoordinates.Y += (float)(x * Object.TextureShiftXDirection.Y);
                        }
                    }
                }
                if (shifty) {
                    double y = Object.TextureShiftYFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
                    y -= Math.Floor(y);
                    for (int j = 0; j < ObjectManager.Objects[i].Meshes.Length; j++) {
                        for (int k = 0; k < ObjectManager.Objects[i].Meshes[j].Vertices.Length; k++) {
                            ObjectManager.Objects[i].Meshes[j].Vertices[k].TextureCoordinates.X += (float)(y * Object.TextureShiftYDirection.X);
                            ObjectManager.Objects[i].Meshes[j].Vertices[k].TextureCoordinates.Y += (float)(y * Object.TextureShiftYDirection.Y);
                        }
                    }
                }
            }
            // led
            bool led = Object.LEDFunction != null;
            double ledangle;
            if (led) {
                ledangle = Object.LEDFunction.Perform(Train, Position, TrackPosition, SectionIndex, TimeElapsed);
            } else {
                ledangle = 0.0;
            }
            // determine position for each polygon
            int m = ObjectManager.Objects[i].Meshes.Length;
            for (int j = 0; j < m; j++) {
                /// initialize vertices
                int n = Object.States[s].Object.Meshes[j].Vertices.Length;
                for (int k = 0; k < n; k++) {
                    ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates = Object.States[s].Object.Meshes[j].Vertices[k].Coordinates;
                }
                // led
                if (led) {
                    double max = Object.LEDMaximumAngle;
                    double pos1 = 0.0;
                    double pos2 = 1.0;
                    switch (j) {
                        case 0:
                            if (ledangle <= -0.5 * Math.PI) {
                                pos1 = 0.5;
                            } else if (ledangle >= 0.5 * Math.PI) {
                                pos1 = 1.0;
                            } else {
                                pos1 = 0.5 * Math.Tan(ledangle - 2.0 * Math.PI) + 0.5;
                                if (pos1 < 0.5) pos1 = 0.5;
                                else if (pos1 > 1.0) pos1 = 1.0;
                            }
                            if (max <= -0.5 * Math.PI) {
                                pos2 = 0.5;
                            } else if (max >= 0.5 * Math.PI) {
                                pos2 = 1.0;
                            } else {
                                pos2 = 0.5 * Math.Tan(max - 2.0 * Math.PI) + 0.5;
                                if (pos2 < 0.5) pos2 = 0.5;
                                else if (pos2 > 1.0) pos2 = 1.0;
                            }
                            break;
                        case 1:
                            if (ledangle <= 0.0) {
                                pos1 = 0.0;
                            } else if (ledangle >= Math.PI) {
                                pos1 = 1.0;
                            } else {
                                pos1 = 0.5 * Math.Tan(ledangle - 0.5 * Math.PI) + 0.5;
                                if (pos1 < 0.0) pos1 = 0.0;
                                else if (pos1 > 1.0) pos1 = 1.0;
                            }
                            if (max <= 0.0) {
                                pos2 = 0.0;
                            } else if (max >= Math.PI) {
                                pos2 = 1.0;
                            } else {
                                pos2 = 0.5 * Math.Tan(max - 0.5 * Math.PI) + 0.5;
                                if (pos2 < 0.0) pos2 = 0.0;
                                else if (pos2 > 1.0) pos2 = 1.0;
                            }
                            break;
                        case 2:
                            if (ledangle <= 0.5 * Math.PI) {
                                pos1 = 0.0;
                            } else if (ledangle >= 1.5 * Math.PI) {
                                pos1 = 1.0;
                            } else {
                                pos1 = 0.5 * Math.Tan(ledangle - Math.PI) + 0.5;
                                if (pos1 < 0.0) pos1 = 0.0;
                                else if (pos1 > 1.0) pos1 = 1.0;
                            }
                            if (max <= 0.5 * Math.PI) {
                                pos2 = 0.0;
                            } else if (max >= 1.5 * Math.PI) {
                                pos2 = 1.0;
                            } else {
                                pos2 = 0.5 * Math.Tan(max - Math.PI) + 0.5;
                                if (pos2 < 0.0) pos2 = 0.0;
                                else if (pos2 > 1.0) pos2 = 1.0;
                            }
                            break;
                        case 3:
                            if (ledangle <= Math.PI) {
                                pos1 = 0.0;
                            } else if (ledangle >= 2.0 * Math.PI) {
                                pos1 = 1.0;
                            } else {
                                pos1 = 0.5 * Math.Tan(ledangle - 1.5 * Math.PI) + 0.5;
                                if (pos1 < 0.0) pos1 = 0.0;
                                else if (pos1 > 1.0) pos1 = 1.0;
                            }
                            if (max <= Math.PI) {
                                pos2 = 0.0;
                            } else if (max >= 2.0 * Math.PI) {
                                pos2 = 1.0;
                            } else {
                                pos2 = 0.5 * Math.Tan(max - 1.5 * Math.PI) + 0.5;
                                if (pos2 < 0.0) pos2 = 0.0;
                                else if (pos2 > 1.0) pos2 = 1.0;
                            }
                            break;
                        case 4:
                            if (ledangle <= 1.5 * Math.PI) {
                                pos1 = 0.0;
                            } else if (ledangle >= 2.5 * Math.PI) {
                                pos1 = 0.5;
                            } else {
                                pos1 = 0.5 * Math.Tan(ledangle - 2.0 * Math.PI) + 0.5;
                                if (pos1 < 0.0) pos1 = 0.0;
                                else if (pos1 > 0.5) pos1 = 0.5;
                            }
                            if (max <= 1.5 * Math.PI) {
                                pos2 = 0.0;
                            } else if (max >= 2.5 * Math.PI) {
                                pos2 = 0.5;
                            } else {
                                pos2 = 0.5 * Math.Tan(max - 2.0 * Math.PI) + 0.5;
                                if (pos2 < 0.0) pos2 = 0.0;
                                else if (pos2 > 0.5) pos2 = 0.5;
                            }
                            break;
                    }
                    double cpos1 = 1.0 - pos1;
                    double cpos2 = 1.0 - pos2;
                    double x0 = Object.States[s].Object.Meshes[j].Vertices[1].Coordinates.X;
                    double y0 = Object.States[s].Object.Meshes[j].Vertices[1].Coordinates.Y;
                    double z0 = Object.States[s].Object.Meshes[j].Vertices[1].Coordinates.Z;
                    double x1 = Object.States[s].Object.Meshes[j].Vertices[2].Coordinates.X;
                    double y1 = Object.States[s].Object.Meshes[j].Vertices[2].Coordinates.Y;
                    double z1 = Object.States[s].Object.Meshes[j].Vertices[2].Coordinates.Z;
                    if (Object.LEDClockwiseWinding) {
                        ObjectManager.Objects[i].Meshes[j].Vertices[1].Coordinates.X = x0 * cpos1 + x1 * pos1;
                        ObjectManager.Objects[i].Meshes[j].Vertices[1].Coordinates.Y = y0 * cpos1 + y1 * pos1;
                        ObjectManager.Objects[i].Meshes[j].Vertices[1].Coordinates.Z = z0 * cpos1 + z1 * pos1;
                        ObjectManager.Objects[i].Meshes[j].Vertices[2].Coordinates.X = x0 * cpos2 + x1 * pos2;
                        ObjectManager.Objects[i].Meshes[j].Vertices[2].Coordinates.Y = y0 * cpos2 + y1 * pos2;
                        ObjectManager.Objects[i].Meshes[j].Vertices[2].Coordinates.Z = z0 * cpos2 + z1 * pos2;
                    } else {
                        ObjectManager.Objects[i].Meshes[j].Vertices[1].Coordinates.X = x0 * cpos2 + x1 * pos2;
                        ObjectManager.Objects[i].Meshes[j].Vertices[1].Coordinates.Y = y0 * cpos2 + y1 * pos2;
                        ObjectManager.Objects[i].Meshes[j].Vertices[1].Coordinates.Z = z0 * cpos2 + z1 * pos2;
                        ObjectManager.Objects[i].Meshes[j].Vertices[2].Coordinates.X = x0 * cpos1 + x1 * pos1;
                        ObjectManager.Objects[i].Meshes[j].Vertices[2].Coordinates.Y = y0 * cpos1 + y1 * pos1;
                        ObjectManager.Objects[i].Meshes[j].Vertices[2].Coordinates.Z = z0 * cpos1 + z1 * pos1;
                    }
                }
                // update vertices
                for (int k = 0; k < n; k++) {
                    // rotate
                    if (rotateX) {
                        World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z, Object.RotateXDirection.X, Object.RotateXDirection.Y, Object.RotateXDirection.Z, cosX, sinX);
                    }
                    if (rotateY) {
                        World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z, Object.RotateYDirection.X, Object.RotateYDirection.Y, Object.RotateYDirection.Z, cosY, sinY);
                    }
                    if (rotateZ) {
                        World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z, Object.RotateZDirection.X, Object.RotateZDirection.Y, Object.RotateZDirection.Z, cosZ, sinZ);
                    }
                    // translate
                    if (Overlay) {
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X += Object.States[s].Position.X - Position.X;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y += Object.States[s].Position.Y - Position.Y;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z += Object.States[s].Position.Z - Position.Z;
                        World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z, World.AbsoluteCameraDirection.X, World.AbsoluteCameraDirection.Y, World.AbsoluteCameraDirection.Z, World.AbsoluteCameraUp.X, World.AbsoluteCameraUp.Y, World.AbsoluteCameraUp.Z, World.AbsoluteCameraSide.X, World.AbsoluteCameraSide.Y, World.AbsoluteCameraSide.Z);
                        double dx = -Math.Tan(World.CameraCurrentAlignment.Yaw) - World.CameraCurrentAlignment.TrackOffset.X;
                        double dy = -Math.Tan(World.CameraCurrentAlignment.Pitch) - World.CameraCurrentAlignment.TrackOffset.Y;
                        double dz = -World.CameraCurrentAlignment.TrackOffset.Z;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X += World.AbsoluteCameraPosition.X + dx * World.AbsoluteCameraSide.X + dy * World.AbsoluteCameraUp.X + dz * World.AbsoluteCameraDirection.X;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y += World.AbsoluteCameraPosition.Y + dx * World.AbsoluteCameraSide.Y + dy * World.AbsoluteCameraUp.Y + dz * World.AbsoluteCameraDirection.Y;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z += World.AbsoluteCameraPosition.Z + dx * World.AbsoluteCameraSide.Z + dy * World.AbsoluteCameraUp.Z + dz * World.AbsoluteCameraDirection.Z;
                    } else {
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X += Object.States[s].Position.X;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y += Object.States[s].Position.Y;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z += Object.States[s].Position.Z;
                        World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.X += Position.X;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Y += Position.Y;
                        ObjectManager.Objects[i].Meshes[j].Vertices[k].Coordinates.Z += Position.Z;
                    }
                }
                /// update normals
                for (int k = 0; k < Object.States[s].Object.Meshes[j].Faces.Length; k++) {
                    for (int h = 0; h < Object.States[s].Object.Meshes[j].Faces[k].Vertices.Length; h++) {
                        ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal = Object.States[s].Object.Meshes[j].Faces[k].Vertices[h].Normal;
                        if (rotateX) {
                            World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Z, Object.RotateXDirection.X, Object.RotateXDirection.Y, Object.RotateXDirection.Z, cosX, sinX);
                        }
                        if (rotateY) {
                            World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Z, Object.RotateYDirection.X, Object.RotateYDirection.Y, Object.RotateYDirection.Z, cosY, sinY);
                        }
                        if (rotateZ) {
                            World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Z, Object.RotateZDirection.X, Object.RotateZDirection.Y, Object.RotateZDirection.Z, cosZ, sinZ);
                        }
                        World.Rotate(ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Meshes[j].Faces[k].Vertices[h].Normal.Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
                    }
                }
                World.CreateNormals(ref ObjectManager.Objects[i].Meshes[j], false);
                // visibility changed
                if (Visibility == VisibilityChangeMode.Hide) {
                    Renderer.HideObject(i);
                } else if (Visibility == VisibilityChangeMode.Show) {
                    Renderer.ShowObject(i, Overlay);
                }
            }
        }
        internal static void UpdateDamping(ref Damping Damping, double TimeElapsed, ref double Angle) {
            if (Damping != null) {
                if (Damping.DirectMode) {
                    /// direct mode
                    bool immediate = Math.Abs(Angle - Damping.CurrentAngle) > 0.1;
                    if (immediate | Damping.CurrentAngle == Angle) {
                        Damping.CurrentTicks++;
                        if (immediate | Damping.CurrentTicks >= 3) {
                            Damping.DirectMode = false;
                            Damping.OriginalAngle = Damping.CurrentAngle;
                            Damping.TargetAngle = Angle;
                            Damping.CurrentValue = 0.0;
                            Damping.OriginalDerivative = 0.0;
                            Damping.CurrentDerivative = 0.0;
                            Damping.CurrentTimeDelta = 0.0;
                            Damping.CurrentTicks = 0;
                        }
                    } else {
                        Damping.CurrentAngle = Angle;
                        Damping.CurrentTicks = 0;
                    }
                } else {
                    /// damping mode
                    if (Angle != Damping.TargetAngle) {
                        Damping.CurrentCounter++;
                    } else {
                        Damping.CurrentCounter >>= 1;
                    }
                    if (Damping.CurrentCounter >= 3 && Math.Abs(Angle - Damping.CurrentAngle) < 0.1 & Math.Abs(Damping.CurrentDerivative) < 0.01) {
                        Damping.DirectMode = true;
                        Damping.CurrentAngle = Angle;
                        Damping.CurrentTicks = 0;
                    } else {
                        /// update target angle
                        double nf = Damping.NaturalFrequency;
                        double dr = Damping.DampingRatio;
                        double ndf = Damping.NaturalDampingFrequency;
                        if (Damping.CurrentTicks >= 3 && (Damping.CurrentTimeDelta > Damping.NaturalTime || Math.Abs(Angle - Damping.TargetAngle) > 0.1)) {
                            double a = Damping.TargetAngle - Damping.OriginalAngle;
                            Damping.OriginalAngle = Damping.CurrentAngle;
                            Damping.TargetAngle = Angle;
                            double b = Damping.TargetAngle - Damping.OriginalAngle;
                            double r = b == 0.0 ? 1.0 : a / b;
                            Damping.CurrentTimeDelta = 0.0;
                            Damping.OriginalDerivative = Damping.CurrentDerivative * r;
                            Damping.CurrentTicks = 0;
                        }
                        /// update variables
                        {
                            double t = Damping.CurrentTimeDelta;
                            double b;
                            if (nf == 0.0) {
                                b = 1.0;
                            } else if (dr == 0.0) {
                                b = Math.Cos(nf * t) + Damping.OriginalDerivative * Math.Sin(nf * t) / nf;
                            } else if (dr < 1.0) {
                                double n = (Damping.OriginalDerivative + nf * dr) / ndf;
                                b = Math.Exp(-dr * nf * t) * (Math.Cos(ndf * t) + n * Math.Sin(ndf * t));
                            } else if (dr == 1.0) {
                                b = Math.Exp(-nf * t);
                            } else {
                                double n = (Damping.OriginalDerivative + nf * dr) / ndf;
                                b = Math.Exp(-dr * nf * t) * (Math.Cosh(ndf * t) + n * Math.Sinh(ndf * t));
                            }
                            if (TimeElapsed >= 0.001) {
                                Damping.CurrentDerivative = (b - Damping.CurrentValue) / TimeElapsed;
                            }
                            Damping.CurrentValue = b;
                            Angle = Damping.TargetAngle * (1.0 - b) + Damping.OriginalAngle * b;
                            Damping.CurrentAngle = Angle;
                            Damping.CurrentTimeDelta += TimeElapsed;
                            Damping.CurrentTicks++;
                        }
                    }
                }
            }
        }

        // animated world object
        internal class AnimatedWorldObject {
            internal World.Vector3D Position;
            internal double TrackPosition;
            internal World.Vector3D Direction;
            internal World.Vector3D Up;
            internal World.Vector3D Side;
            internal AnimatedObject Object;
            internal int SectionIndex;
            internal double Radius;
            internal bool Visible;
        }
        internal static AnimatedWorldObject[] AnimatedWorldObjects = new AnimatedWorldObject[4];
        internal static int AnimatedWorldObjectsUsed = 0;
        internal static void CreateAnimatedWorldObjects(AnimatedObject[] Prototypes, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness, bool DuplicateMaterials) {
            bool[] free = new bool[Prototypes.Length];
            bool anyfree = false;
            for (int i = 0; i < Prototypes.Length; i++) {
                free[i] = Prototypes[i].IsFreeOfFunctions();
                if (free[i]) anyfree = true;
            }
            if (anyfree) {
                for (int i = 0; i < Prototypes.Length; i++) {
                    if (Prototypes[i].States.Length != 0) {
                        if (free[i]) {
                            World.Vector3D p = Position;
                            World.Vector3D s = BaseTransformation.X;
                            World.Vector3D u = BaseTransformation.Y;
                            World.Vector3D d = BaseTransformation.Z;
                            p.X += Prototypes[i].States[0].Position.X * s.X + Prototypes[i].States[0].Position.Y * u.X + Prototypes[i].States[0].Position.Z * d.X;
                            p.Y += Prototypes[i].States[0].Position.X * s.Y + Prototypes[i].States[0].Position.Y * u.Y + Prototypes[i].States[0].Position.Z * d.Y;
                            p.Z += Prototypes[i].States[0].Position.X * s.Z + Prototypes[i].States[0].Position.Y * u.Z + Prototypes[i].States[0].Position.Z * d.Z;
                            CreateStaticObject(Prototypes[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, TrackPosition, Brightness, DuplicateMaterials);
                        } else {
                            CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition);
                        }
                    }
                }
            } else {
                for (int i = 0; i < Prototypes.Length; i++) {
                    if (Prototypes[i].States.Length != 0) {
                        CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition);
                    }
                }
            }
        }
        internal static int CreateAnimatedWorldObject(AnimatedObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, double TrackPosition) {
            int a = AnimatedWorldObjectsUsed;
            if (a >= AnimatedWorldObjects.Length) {
                Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
            }
            World.Transformation FinalTransformation = new World.Transformation(BaseTransformation, AuxTransformation);
            AnimatedWorldObjects[a] = new AnimatedWorldObject();
            AnimatedWorldObjects[a].Position = Position;
            AnimatedWorldObjects[a].Direction = FinalTransformation.Z;
            AnimatedWorldObjects[a].Up = FinalTransformation.Y;
            AnimatedWorldObjects[a].Side = FinalTransformation.X;
            AnimatedWorldObjects[a].Object = Prototype.Clone();
            AnimatedWorldObjects[a].Object.ObjectIndex = CreateDynamicObject();
            AnimatedWorldObjects[a].SectionIndex = SectionIndex;
            AnimatedWorldObjects[a].TrackPosition = TrackPosition;
            double r = 0.0;
            for (int i = 0; i < Prototype.States.Length; i++) {
                for (int j = 0; j < Prototype.States[i].Object.Meshes.Length; j++) {
                    for (int k = 0; k < Prototype.States[i].Object.Meshes[j].Vertices.Length; k++) {
                        double x = Prototype.States[i].Object.Meshes[j].Vertices[k].Coordinates.X;
                        double y = Prototype.States[i].Object.Meshes[j].Vertices[k].Coordinates.Y;
                        double z = Prototype.States[i].Object.Meshes[j].Vertices[k].Coordinates.Z;
                        double t = x * x + y * y + z * z;
                        if (t > r) r = t;
                    }
                }
            }
            AnimatedWorldObjects[a].Radius = Math.Sqrt(r);
            AnimatedWorldObjects[a].Visible = true;
            InitializeAnimatedObject(ref AnimatedWorldObjects[a].Object, 0, false);
            AnimatedWorldObjectsUsed++;
            return a;
        }
        internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate) {
            for (int i = 0; i < AnimatedWorldObjectsUsed; i++) {
                double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
                double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius;
                double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius;
                double ta = World.CameraTrackFollower.TrackPosition - World.BackgroundImageDistance - World.ExtraViewingDistance;
                double tb = World.CameraTrackFollower.TrackPosition + World.BackgroundImageDistance + World.ExtraViewingDistance;
                bool v = pb >= ta & pa <= tb;
                if (v) {
                    if (Game.SecondsSinceMidnight >= AnimatedWorldObjects[i].Object.TimeNextUpdating | ForceUpdate) {
                        AnimatedWorldObjects[i].Object.TimeNextUpdating = Game.SecondsSinceMidnight + AnimatedWorldObjects[i].Object.RefreshRate;
                        TrainManager.Train t;
                        if (TrainManager.PlayerTrain >= 0) {
                            t = TrainManager.Trains[TrainManager.PlayerTrain];
                        } else {
                            t = null;
                        }
                        UpdateAnimatedObject(ref AnimatedWorldObjects[i].Object, t, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, VisibilityChangeMode.DontChange, TimeElapsed);
                    }
                    if (!AnimatedWorldObjects[i].Visible) {
                        Renderer.ShowObject(AnimatedWorldObjects[i].Object.ObjectIndex, false);
                        AnimatedWorldObjects[i].Visible = true;
                    }
                } else if (AnimatedWorldObjects[i].Visible) {
                    Renderer.HideObject(AnimatedWorldObjects[i].Object.ObjectIndex);
                    AnimatedWorldObjects[i].Visible = false;
                    if (ForceUpdate) {
                        AnimatedWorldObjects[i].Object.TimeNextUpdating = Game.SecondsSinceMidnight + AnimatedWorldObjects[i].Object.RefreshRate;
                    }
                } else if (ForceUpdate) {
                    AnimatedWorldObjects[i].Object.TimeNextUpdating = Game.SecondsSinceMidnight + AnimatedWorldObjects[i].Object.RefreshRate;
                }
            }
        }

        // load object
        internal enum ObjectLoadMode { Normal, DontAllowUnloadOfTextures, PreloadTextures }
        internal static UnifiedObject LoadObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeat) {
#if !DEBUG
            try {
#endif
            if (!System.IO.Path.HasExtension(FileName)) {
                while (true) {
                    string f;
                    f = Interface.GetCorrectedFileName(FileName + ".x");
                    if (System.IO.File.Exists(f)) {
                        FileName = f;
                        break;
                    }
                    f = Interface.GetCorrectedFileName(FileName + ".csv");
                    if (System.IO.File.Exists(f)) {
                        FileName = f;
                        break;
                    }
                    f = Interface.GetCorrectedFileName(FileName + ".b3d");
                    if (System.IO.File.Exists(f)) {
                        FileName = f;
                        break;
                    }
                    break;
                }
            }
            UnifiedObject Result;
            switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
                case ".csv":
                case ".b3d":
                    Result = BveCsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeat);
                    break;
                case ".x":
                    Result = BveXObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeat);
                    break;
                case ".animated":
                    Result = BveAnimatedObjectParser.ReadObject(FileName, Encoding, LoadMode);
                    break;
                default:
                    Interface.AddMessage(Interface.MessageType.Error, false, "The file extension is not supported in " + FileName);
                    return null;
            }
            OptimizeObject(Result, PreserveVertices);
            return Result;
#if !DEBUG
            } catch (Exception ex) {
                Interface.AddMessage(Interface.MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
                return null;
            }
#endif
        }
        internal static StaticObject LoadStaticObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeat) {
#if !DEBUG
            try {
#endif
            if (!System.IO.Path.HasExtension(FileName)) {
                while (true) {
                    string f;
                    f = Interface.GetCorrectedFileName(FileName + ".x");
                    if (System.IO.File.Exists(f)) {
                        FileName = f;
                        break;
                    }
                    f = Interface.GetCorrectedFileName(FileName + ".csv");
                    if (System.IO.File.Exists(f)) {
                        FileName = f;
                        break;
                    }
                    f = Interface.GetCorrectedFileName(FileName + ".b3d");
                    if (System.IO.File.Exists(f)) {
                        FileName = f;
                        break;
                    }
                    break;
                }
            }
            StaticObject Result;
            switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
                case ".csv":
                case ".b3d":
                    Result = BveCsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeat);
                    break;
                case ".x":
                    Result = BveXObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeat);
                    break;
                default:
                    Interface.AddMessage(Interface.MessageType.Error, false, "The file extension is not supported in " + FileName);
                    return null;
            }
            OptimizeObject(Result, PreserveVertices);
            return Result;
#if !DEBUG
            } catch (Exception ex) {
                Interface.AddMessage(Interface.MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
                return null;
            }
#endif
        }

        // optimize object
        internal static void OptimizeObject(UnifiedObject Prototype, bool PreserveVertices) {
            if (Prototype is StaticObject) {
                StaticObject s = (StaticObject)Prototype;
                OptimizeObject(s, PreserveVertices);
            } else if (Prototype is AnimatedObjectCollection) {
                AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
                for (int i = 0; i < a.Objects.Length; i++) {
                    for (int j = 0; j < a.Objects[i].States.Length; j++) {
                        OptimizeObject(a.Objects[i].States[j].Object, PreserveVertices);
                    }
                }
            }
        }
        internal static void OptimizeObject(StaticObject Prototype, bool PreserveVertices) {
            if (Prototype == null) return;
            if (Prototype.Meshes.Length == 0) return;
            // merge meshes
            if (Prototype.Meshes.Length >= 2) {
                int va = 0, fa = 0, ma = 0;
                for (int i = 1; i < Prototype.Meshes.Length; i++) {
                    va += Prototype.Meshes[i].Vertices.Length;
                    ma += Prototype.Meshes[i].Materials.Length;
                    fa += Prototype.Meshes[i].Faces.Length;
                }
                int v = Prototype.Meshes[0].Vertices.Length;
                int m = Prototype.Meshes[0].Materials.Length;
                int f = Prototype.Meshes[0].Faces.Length;
                Array.Resize<World.Vertex>(ref Prototype.Meshes[0].Vertices, v + va);
                Array.Resize<World.MeshMaterial>(ref Prototype.Meshes[0].Materials, m + ma);
                Array.Resize<World.MeshFace>(ref Prototype.Meshes[0].Faces, f + fa);
                for (int i = 1; i < Prototype.Meshes.Length; i++) {
                    for (int j = 0; j < Prototype.Meshes[i].Vertices.Length; j++) {
                        Prototype.Meshes[0].Vertices[v + j] = Prototype.Meshes[i].Vertices[j];
                    }
                    for (int j = 0; j < Prototype.Meshes[i].Materials.Length; j++) {
                        Prototype.Meshes[0].Materials[m + j] = Prototype.Meshes[i].Materials[j];
                    }
                    for (int j = 0; j < Prototype.Meshes[i].Faces.Length; j++) {
                        Prototype.Meshes[0].Faces[f + j] = Prototype.Meshes[i].Faces[j];
                        Prototype.Meshes[0].Faces[f + j].Material += (ushort)m;
                        for (int k = 0; k < Prototype.Meshes[i].Faces[j].Vertices.Length; k++) {
                            Prototype.Meshes[0].Faces[f + j].Vertices[k].Index += (ushort)v;
                        }
                    }
                    v += Prototype.Meshes[i].Vertices.Length;
                    m += Prototype.Meshes[i].Materials.Length;
                    f += Prototype.Meshes[i].Faces.Length;
                }
                Array.Resize<World.Mesh>(ref Prototype.Meshes, 1);
            }
            // materials
            if (Prototype.Meshes[0].Materials.Length >= 1) {
                /// merge
                int m = Prototype.Meshes[0].Materials.Length;
                for (int i = m - 1; i >= 1; i--) {
                    for (int j = i - 1; j >= 0; j--) {
                        if (World.MeshMaterial.Equals(Prototype.Meshes[0].Materials[i], Prototype.Meshes[0].Materials[j])) {
                            for (int k = i; k < m - 1; k++) {
                                Prototype.Meshes[0].Materials[k] = Prototype.Meshes[0].Materials[k + 1];
                            }
                            for (int k = 0; k < Prototype.Meshes[0].Faces.Length; k++) {
                                int a = (int)Prototype.Meshes[0].Faces[k].Material;
                                if (a == i) {
                                    Prototype.Meshes[0].Faces[k].Material = (ushort)j;
                                } else if (a > i) {
                                    Prototype.Meshes[0].Faces[k].Material--;
                                }
                            }
                            m--;
                            break;
                        }
                    }
                }
                /// eliminate unsed
                for (int i = m - 1; i >= 0; i--) {
                    int j; for (j = 0; j < Prototype.Meshes[0].Faces.Length; j++) {
                        if ((int)Prototype.Meshes[0].Faces[j].Material == i) break;
                    } if (j == Prototype.Meshes[0].Faces.Length) {
                        for (int k = i; k < m - 1; k++) {
                            Prototype.Meshes[0].Materials[k] = Prototype.Meshes[0].Materials[k + 1];
                        }
                        for (int k = 0; k < Prototype.Meshes[0].Faces.Length; k++) {
                            int a = (int)Prototype.Meshes[0].Faces[k].Material;
                            if (a > i) {
                                Prototype.Meshes[0].Faces[k].Material--;
                            }
                        } m--;
                    }
                }
                if (m != Prototype.Meshes[0].Materials.Length) {
                    Array.Resize<World.MeshMaterial>(ref Prototype.Meshes[0].Materials, m);
                }
            }
            // vertices
            if (Prototype.Meshes[0].Vertices.Length >= 1 & !PreserveVertices) {
                /// merge
                int v = Prototype.Meshes[0].Vertices.Length;
                for (int i = v - 1; i >= 1; i--) {
                    for (int j = i - 1; j >= 0; j--) {
                        if (World.Vertex.Equals(Prototype.Meshes[0].Vertices[i], Prototype.Meshes[0].Vertices[j])) {
                            for (int k = i; k < v - 1; k++) {
                                Prototype.Meshes[0].Vertices[k] = Prototype.Meshes[0].Vertices[k + 1];
                            }
                            for (int k = 0; k < Prototype.Meshes[0].Faces.Length; k++) {
                                for (int h = 0; h < Prototype.Meshes[0].Faces[k].Vertices.Length; h++) {
                                    int a = (int)Prototype.Meshes[0].Faces[k].Vertices[h].Index;
                                    if (a == i) {
                                        Prototype.Meshes[0].Faces[k].Vertices[h].Index = (ushort)j;
                                    } else if (a > i) {
                                        Prototype.Meshes[0].Faces[k].Vertices[h].Index--;
                                    }
                                }
                            }
                            v--;
                            break;
                        }
                    }
                }
                /// eliminate unused
                for (int i = v - 1; i >= 0; i--) {
                    int j; for (j = 0; j < Prototype.Meshes[0].Faces.Length; j++) {
                        int k; for (k = 0; k < Prototype.Meshes[0].Faces[j].Vertices.Length; k++) {
                            if ((int)Prototype.Meshes[0].Faces[j].Vertices[k].Index == i) break;
                        } if (k != Prototype.Meshes[0].Faces[j].Vertices.Length) break;
                    } if (j == Prototype.Meshes[0].Faces.Length) {
                        for (int k = i; k < v - 1; k++) {
                            Prototype.Meshes[0].Vertices[k] = Prototype.Meshes[0].Vertices[k + 1];
                        }
                        for (int k = 0; k < Prototype.Meshes[0].Faces.Length; k++) {
                            for (int h = 0; h < Prototype.Meshes[0].Faces[k].Vertices.Length; h++) {
                                int a = (int)Prototype.Meshes[0].Faces[k].Vertices[h].Index;
                                if (a > i) {
                                    Prototype.Meshes[0].Faces[k].Vertices[h].Index--;
                                }
                            }
                        } v--;
                    }
                }
                if (v != Prototype.Meshes[0].Vertices.Length) {
                    Array.Resize<World.Vertex>(ref Prototype.Meshes[0].Vertices, v);
                }
            }
        }

        // create object
        internal static void CreateObject(UnifiedObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double TrackPosition) {
            CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, TrackPosition, 1.0, false);
        }
        internal static void CreateObject(UnifiedObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness, bool DuplicateMaterials) {
            if (Prototype is StaticObject) {
                StaticObject s = (StaticObject)Prototype;
                CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, TrackPosition, Brightness, DuplicateMaterials);
            } else if (Prototype is AnimatedObjectCollection) {
                AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
                CreateAnimatedWorldObjects(a.Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, TrackPosition, Brightness, DuplicateMaterials);
            }
        }

        // create static object
        internal static int CreateStaticObject(StaticObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double TrackPosition) {
            return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, TrackPosition, 1.0, false);
        }
        internal static int CreateStaticObject(StaticObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness, bool DuplicateMaterials) {
            int a = ObjectsUsed;
            if (a >= Objects.Length) {
                Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
            }
            ApplyStaticObjectData(ref Objects[a], Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, TrackPosition, Brightness, DuplicateMaterials);
            ObjectsUsed++;
            return a;
        }
        internal static void ApplyStaticObjectData(ref StaticObject Object, StaticObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness, bool DuplicateMaterials) {
            Object = new StaticObject();
            Object.StartingDistance = float.MaxValue;
            Object.EndingDistance = float.MinValue;
            Object.Meshes = new World.Mesh[Prototype.Meshes.Length];
            bool brightnesschange = Brightness != 1.0;
            // meshes
            for (int i = 0; i < Prototype.Meshes.Length; i++) {
                // vertices
                Object.Meshes[i].Vertices = new World.Vertex[Prototype.Meshes[i].Vertices.Length];
                for (int j = 0; j < Prototype.Meshes[i].Vertices.Length; j++) {
                    Object.Meshes[i].Vertices[j] = Prototype.Meshes[i].Vertices[j];
                    if (AccurateObjectDisposal) {
                        World.Rotate(ref Object.Meshes[i].Vertices[j].Coordinates.X, ref Object.Meshes[i].Vertices[j].Coordinates.Y, ref Object.Meshes[i].Vertices[j].Coordinates.Z, AuxTransformation);
                        if (Object.Meshes[i].Vertices[j].Coordinates.Z < Object.StartingDistance) {
                            Object.StartingDistance = (float)Object.Meshes[i].Vertices[j].Coordinates.Z;
                        }
                        if (Object.Meshes[i].Vertices[j].Coordinates.Z > Object.EndingDistance) {
                            Object.EndingDistance = (float)Object.Meshes[i].Vertices[j].Coordinates.Z;
                        }
                        Object.Meshes[i].Vertices[j].Coordinates = Prototype.Meshes[i].Vertices[j].Coordinates;
                    }
                    World.Rotate(ref Object.Meshes[i].Vertices[j].Coordinates.X, ref Object.Meshes[i].Vertices[j].Coordinates.Y, ref Object.Meshes[i].Vertices[j].Coordinates.Z, AuxTransformation);
                    World.Rotate(ref Object.Meshes[i].Vertices[j].Coordinates.X, ref Object.Meshes[i].Vertices[j].Coordinates.Y, ref Object.Meshes[i].Vertices[j].Coordinates.Z, BaseTransformation);
                    Object.Meshes[i].Vertices[j].Coordinates.X += Position.X;
                    Object.Meshes[i].Vertices[j].Coordinates.Y += Position.Y;
                    Object.Meshes[i].Vertices[j].Coordinates.Z += Position.Z;
                }
                // faces
                Object.Meshes[i].Faces = new World.MeshFace[Prototype.Meshes[i].Faces.Length];
                for (int j = 0; j < Prototype.Meshes[i].Faces.Length; j++) {
                    Object.Meshes[i].Faces[j].Flags = Prototype.Meshes[i].Faces[j].Flags;
                    Object.Meshes[i].Faces[j].Material = Prototype.Meshes[i].Faces[j].Material;
                    Object.Meshes[i].Faces[j].Vertices = new World.MeshFaceVertex[Prototype.Meshes[i].Faces[j].Vertices.Length];
                    for (int k = 0; k < Prototype.Meshes[i].Faces[j].Vertices.Length; k++) {
                        Object.Meshes[i].Faces[j].Vertices[k] = Prototype.Meshes[i].Faces[j].Vertices[k];
                        double nx = Object.Meshes[i].Faces[j].Vertices[k].Normal.X;
                        double ny = Object.Meshes[i].Faces[j].Vertices[k].Normal.Y;
                        double nz = Object.Meshes[i].Faces[j].Vertices[k].Normal.Z;
                        if (nx * nx + ny * ny + nz * nz != 0.0) {
                            World.Rotate(ref Object.Meshes[i].Faces[j].Vertices[k].Normal.X, ref Object.Meshes[i].Faces[j].Vertices[k].Normal.Y, ref Object.Meshes[i].Faces[j].Vertices[k].Normal.Z, AuxTransformation);
                            World.Rotate(ref Object.Meshes[i].Faces[j].Vertices[k].Normal.X, ref Object.Meshes[i].Faces[j].Vertices[k].Normal.Y, ref Object.Meshes[i].Faces[j].Vertices[k].Normal.Z, BaseTransformation);
                        }
                    }
                }
                World.CreateNormals(ref Object.Meshes[i], false);
                // materials
                if (brightnesschange) {
                    Object.Meshes[i].Materials = new World.MeshMaterial[Prototype.Meshes[i].Materials.Length];
                    for (int j = 0; j < Prototype.Meshes[i].Materials.Length; j++) {
                        Object.Meshes[i].Materials[j] = Prototype.Meshes[i].Materials[j];
                        Object.Meshes[i].Materials[j].Color.R = (byte)Math.Round((double)Prototype.Meshes[i].Materials[j].Color.R * Brightness);
                        Object.Meshes[i].Materials[j].Color.G = (byte)Math.Round((double)Prototype.Meshes[i].Materials[j].Color.G * Brightness);
                        Object.Meshes[i].Materials[j].Color.B = (byte)Math.Round((double)Prototype.Meshes[i].Materials[j].Color.B * Brightness);
                    }
                } else if (DuplicateMaterials) {
                    Object.Meshes[i].Materials = new World.MeshMaterial[Prototype.Meshes[i].Materials.Length];
                    for (int j = 0; j < Prototype.Meshes[i].Materials.Length; j++) {
                        Object.Meshes[i].Materials[j] = Prototype.Meshes[i].Materials[j];
                    }
                } else {
                    Object.Meshes[i].Materials = Prototype.Meshes[i].Materials;
                }
            }
            if (AccurateObjectDisposal) {
                Object.StartingDistance += (float)TrackPosition;
                Object.EndingDistance += (float)TrackPosition;
            } else {
                Object.StartingDistance = (float)StartingDistance;
                Object.EndingDistance = (float)EndingDistance;
            }
        }

        // create dynamic object
        internal static int CreateDynamicObject() {
            int a = ObjectsUsed;
            if (a >= Objects.Length) {
                Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
            }
            Objects[a] = new StaticObject();
            Objects[a].Dynamic = 1;
            ObjectsUsed++;
            return a;
        }

        // clone object
        internal static StaticObject CloneObject(StaticObject Prototype) {
            return CloneObject(Prototype, -1, -1);
        }
        internal static StaticObject CloneObject(StaticObject Prototype, int DaytimeTextureIndex, int NighttimeTextureIndex) {
            StaticObject Result = new StaticObject();
            Result.StartingDistance = Prototype.StartingDistance;
            Result.EndingDistance = Prototype.EndingDistance;
            Result.Meshes = new World.Mesh[Prototype.Meshes.Length];
            for (int i = 0; i < Prototype.Meshes.Length; i++) {
                // vertices
                Result.Meshes[i].Vertices = new World.Vertex[Prototype.Meshes[i].Vertices.Length];
                for (int j = 0; j < Prototype.Meshes[i].Vertices.Length; j++) {
                    Result.Meshes[i].Vertices[j] = Prototype.Meshes[i].Vertices[j];
                }
                // faces
                Result.Meshes[i].Faces = new World.MeshFace[Prototype.Meshes[i].Faces.Length];
                for (int j = 0; j < Prototype.Meshes[i].Faces.Length; j++) {
                    Result.Meshes[i].Faces[j].Flags = Prototype.Meshes[i].Faces[j].Flags;
                    Result.Meshes[i].Faces[j].Material = Prototype.Meshes[i].Faces[j].Material;
                    Result.Meshes[i].Faces[j].Vertices = new World.MeshFaceVertex[Prototype.Meshes[i].Faces[j].Vertices.Length];
                    for (int k = 0; k < Prototype.Meshes[i].Faces[j].Vertices.Length; k++) {
                        Result.Meshes[i].Faces[j].Vertices[k] = Prototype.Meshes[i].Faces[j].Vertices[k];
                    }
                }
                // materials
                if (DaytimeTextureIndex >= 0 | NighttimeTextureIndex != 0) {
                    Result.Meshes[i].Materials = new World.MeshMaterial[Prototype.Meshes[i].Materials.Length];
                    for (int j = 0; j < Prototype.Meshes[i].Materials.Length; j++) {
                        Result.Meshes[i].Materials[j] = Prototype.Meshes[i].Materials[j];
                        if (DaytimeTextureIndex >= 0) {
                            Result.Meshes[i].Materials[j].DaytimeTextureIndex = DaytimeTextureIndex;
                        }
                        if (NighttimeTextureIndex >= 0) {
                            Result.Meshes[i].Materials[j].NighttimeTextureIndex = NighttimeTextureIndex;
                        }
                    }
                } else {
                    Result.Meshes[i].Materials = Prototype.Meshes[i].Materials;
                }
            }
            return Result;
        }

        // finish creating objects
        internal static void FinishCreatingObjects() {
            Array.Resize<StaticObject>(ref Objects, ObjectsUsed);
            Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjectsUsed);
        }

        // initialize visibility
        internal static void InitializeVisibility() {
            // sort objects
            ObjectsSortedByStart = new int[ObjectsUsed];
            ObjectsSortedByEnd = new int[ObjectsUsed];
            double[] a = new double[ObjectsUsed];
            double[] b = new double[ObjectsUsed];
            int n = 0;
            for (int i = 0; i < ObjectsUsed; i++) {
                if (Objects[i].Dynamic == 0) {
                    ObjectsSortedByStart[n] = i;
                    ObjectsSortedByEnd[n] = i;
                    a[n] = Objects[i].StartingDistance;
                    b[n] = Objects[i].EndingDistance;
                    n++;
                }
            }
            Array.Resize<int>(ref ObjectsSortedByStart, n);
            Array.Resize<int>(ref ObjectsSortedByEnd, n);
            Array.Resize<double>(ref a, n);
            Array.Resize<double>(ref b, n);
            Array.Sort<double, int>(a, ObjectsSortedByStart);
            Array.Sort<double, int>(b, ObjectsSortedByEnd);
            ObjectsSortedByStartPointer = 0;
            ObjectsSortedByEndPointer = 0;
            // initial visiblity
            double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.TrackOffset.Z;
            for (int i = 0; i < ObjectsUsed; i++) {
                if (Objects[i].StartingDistance <= p + World.ForwardViewingDistance & Objects[i].EndingDistance >= p - World.BackwardViewingDistance) {
                    Renderer.ShowObject(i, false);
                }
            }
        }

        // update visibility
        internal static void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged) {
            if (ViewingDistanceChanged) {
                UpdateVisibility(TrackPosition);
                UpdateVisibility(TrackPosition - 0.001);
                UpdateVisibility(TrackPosition + 0.001);
                UpdateVisibility(TrackPosition);
            } else {
                UpdateVisibility(TrackPosition);
            }
        }
        internal static void UpdateVisibility(double TrackPosition) {
            double d = TrackPosition - LastUpdatedTrackPosition;
            int n = ObjectsSortedByStart.Length;
            double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.TrackOffset.Z;
            if (d < 0.0) {
                if (ObjectsSortedByStartPointer >= n) ObjectsSortedByStartPointer = n - 1;
                if (ObjectsSortedByEndPointer >= n) ObjectsSortedByEndPointer = n - 1;
                // introduce
                while (ObjectsSortedByEndPointer >= 0) {
                    int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
                    if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance) {
                        Renderer.ShowObject(o, false);
                        ObjectsSortedByEndPointer--;
                    } else break;
                }
                // dispose
                while (ObjectsSortedByStartPointer >= 0) {
                    int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
                    if (Objects[o].StartingDistance > p + World.ForwardViewingDistance) {
                        Renderer.HideObject(o);
                        ObjectsSortedByStartPointer--;
                    } else break;
                }
            } else if (d > 0.0) {
                if (ObjectsSortedByStartPointer < 0) ObjectsSortedByStartPointer = 0;
                if (ObjectsSortedByEndPointer < 0) ObjectsSortedByEndPointer = 0;
                // introduce
                while (ObjectsSortedByStartPointer < n) {
                    int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
                    if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance) {
                        Renderer.ShowObject(o, false);
                        ObjectsSortedByStartPointer++;
                    } else break;
                }
                // dispose
                while (ObjectsSortedByEndPointer < n) {
                    int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
                    if (Objects[o].EndingDistance < p - World.BackwardViewingDistance) {
                        Renderer.HideObject(o);
                        ObjectsSortedByEndPointer++;
                    } else break;
                }
            }
            LastUpdatedTrackPosition = TrackPosition;
        }

    }
}