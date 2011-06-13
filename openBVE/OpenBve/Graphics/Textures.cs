using System;
using System.Drawing;
using Tao.OpenGl;

namespace OpenBve {
	/// <summary>Provides functions for dealing with textures.</summary>
	internal static partial class Textures {
		
		// --- members ---
		
		/// <summary>Holds all currently registered textures.</summary>
		private static Texture[] RegisteredTextures = new Texture[16];
		
		/// <summary>The number of currently registered textures.</summary>
		private static int RegisteredTexturesCount = 0;
		
		
		// --- initialize / deinitialize ---
		
		/// <summary>Initializes the texture component. A call to Deinitialize must be made when terminating the program.</summary>
		internal static void Initialize() {
		}
		
		/// <summary>Deinitializes the texture component.</summary>
		internal static void Deinitialize() {
			UnloadAllTextures();
		}
		
		
		// --- register texture ---

		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="file">The path to the file that contains the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		internal static bool RegisterTexture(string file, out Texture handle) {
			return RegisterTexture(new OpenBveApi.Path.FileReference(file), null, out handle);
		}
		
		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="file">The path to the file that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		internal static bool RegisterTexture(string file, OpenBveApi.Textures.TextureParameters parameters, out Texture handle) {
			return RegisterTexture(new OpenBveApi.Path.FileReference(file), parameters, out handle);
		}
		
		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		internal static bool RegisterTexture(OpenBveApi.Path.PathReference path, OpenBveApi.Textures.TextureParameters parameters, out Texture handle) {
			/*
			 * Check if the texture is already registered.
			 * If so, return the existing handle.
			 * */
			for (int i = 0; i < RegisteredTexturesCount; i++) {
				PathSource source = RegisteredTextures[i].Source as PathSource;
				if (source != null && source.Path == path && source.Parameters == parameters) {
					handle = RegisteredTextures[i];
					return true;
				}
			}
			/*
			 * Register the texture and return the newly created handle.
			 * */
			if (RegisteredTextures.Length == RegisteredTexturesCount) {
				Array.Resize<Texture>(ref RegisteredTextures, RegisteredTextures.Length << 1);
			}
			RegisteredTextures[RegisteredTexturesCount] = new Texture(path, parameters);
			RegisteredTexturesCount++;
			handle = RegisteredTextures[RegisteredTexturesCount - 1];
			return true;
		}
		
		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		internal static bool RegisterTexture(OpenBveApi.Textures.Texture texture, out Texture handle) {
			/*
			 * Register the texture and return the newly created handle.
			 * */
			if (RegisteredTextures.Length == RegisteredTexturesCount) {
				Array.Resize<Texture>(ref RegisteredTextures, RegisteredTextures.Length << 1);
			}
			RegisteredTextures[RegisteredTexturesCount] = new Texture(texture);
			RegisteredTexturesCount++;
			handle = RegisteredTextures[RegisteredTexturesCount - 1];
			return true;
		}
		
		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="bitmap">The bitmap that contains the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		/// <remarks>Be sure not to dispose of the bitmap after calling this function.</remarks>
		internal static bool RegisterTexture(Bitmap bitmap, out Texture handle) {
			/*
			 * Register the texture and return the newly created handle.
			 * */
			if (RegisteredTextures.Length == RegisteredTexturesCount) {
				Array.Resize<Texture>(ref RegisteredTextures, RegisteredTextures.Length << 1);
			}
			RegisteredTextures[RegisteredTexturesCount] = new Texture(bitmap);
			RegisteredTexturesCount++;
			handle = RegisteredTextures[RegisteredTexturesCount - 1];
			return true;
		}

		
		// --- load texture ---
		
		/// <summary>Loads the specified texture into OpenGL if not already loaded.</summary>
		/// <param name="handle">The handle to the registered texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		internal static bool LoadTexture(Texture handle) {
			if (handle.Loaded) {
				return true;
			} else if (handle.Ignore) {
				return false;
			} else {
				OpenBveApi.Textures.Texture texture;
				if (handle.Source.GetTexture(out texture)) {
					if (texture.BitsPerPixel == 24 | texture.BitsPerPixel == 32) {
						int[] names = new int[1];
						Gl.glGenTextures(1, names);
						int error = Gl.glGetError();
						if (error != 0) {
							int zzz = 0; // TODO //
						}
						Gl.glBindTexture(Gl.GL_TEXTURE_2D, names[0]);
						error = Gl.glGetError();
						if (error != 0) {
							int zzz = 0; // TODO //
						}
						handle.OpenGlTextureName = names[0];
						handle.Width = texture.Width;
						handle.Height = texture.Height;
						handle.Transparency = texture.GetTransparencyType();
						switch (Interface.CurrentOptions.Interpolation) {
							case Interface.InterpolationMode.NearestNeighbor:
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
								break;
							case Interface.InterpolationMode.Bilinear:
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
								break;
							case Interface.InterpolationMode.NearestNeighborMipmapped:
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
								break;
							case Interface.InterpolationMode.BilinearMipmapped:
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
								break;
							case Interface.InterpolationMode.TrilinearMipmapped:
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
								break;
							default:
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
								Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
								break;
						}
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
						if (Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.AnisotropicFiltering) {
							Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, Interface.CurrentOptions.AnisotropicFilteringLevel);
						}
						if (texture.BitsPerPixel == 24) {
							if (Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.NearestNeighbor | Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.Bilinear) {
								Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, texture.Width, texture.Height, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, texture.Bytes);
							} else {
								Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGB, texture.Width, texture.Height, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, texture.Bytes);
							}
						} else if (handle.Transparency == OpenBveApi.Textures.TextureTransparencyType.Opaque) {
							byte[] oldBytes = texture.Bytes;
							byte[] newBytes = new byte[3 * texture.Width * texture.Height];
							int j = 0;
							for (int i = 0; i < oldBytes.Length; i += 4) {
								newBytes[j + 0] = oldBytes[i + 0];
								newBytes[j + 1] = oldBytes[i + 1];
								newBytes[j + 2] = oldBytes[i + 2];
								j += 3;
							}
							if (Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.NearestNeighbor | Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.Bilinear) {
								Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, texture.Width, texture.Height, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, newBytes);
							} else {
								Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGB, texture.Width, texture.Height, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, newBytes);
							}
						} else {
							if (Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.NearestNeighbor | Interface.CurrentOptions.Interpolation == Interface.InterpolationMode.Bilinear) {
								Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, texture.Width, texture.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, texture.Bytes);
							} else {
								Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, texture.Width, texture.Height, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, texture.Bytes);
							}
						}
						handle.Loaded = true;
						return true;
					}
				}
			}
			handle.Ignore = true;
			return false;
		}
		
		/// <summary>Loads all registered textures.</summary>
		internal static void LoadAllTextures() {
			for (int i = 0; i < RegisteredTexturesCount; i++) {
				LoadTexture(RegisteredTextures[i]);
			}
		}

		
		// --- unload texture ---
		
		/// <summary>Unloads the specified texture from OpenGL if loaded.</summary>
		/// <param name="handle">The handle to the registered texture.</param>
		internal static void UnloadTexture(Texture handle) {
			if (handle.Loaded) {
				Gl.glDeleteTextures(1, new int[] { handle.OpenGlTextureName });
				handle.Loaded = false;
			}
			handle.Ignore = false;
		}

		/// <summary>Unloads all registered textures.</summary>
		internal static void UnloadAllTextures() {
			for (int i = 0; i < RegisteredTexturesCount; i++) {
				UnloadTexture(RegisteredTextures[i]);
			}
		}
		
		
		// --- statistics ---
		
		/// <summary>Gets the number of registered textures.</summary>
		/// <returns>The number of registered textures.</returns>
		internal static int GetNumberOfRegisteredTextures() {
			return RegisteredTexturesCount;
		}

		/// <summary>Gets the number of loaded textures.</summary>
		/// <returns>The number of loaded textures.</returns>
		internal static int GetNumberOfLoadedTextures() {
			int count = 0;
			for (int i = 0; i < RegisteredTexturesCount; i++) {
				if (RegisteredTextures[i].Loaded) {
					count++;
				}
			}
			return count;
		}

	}
}