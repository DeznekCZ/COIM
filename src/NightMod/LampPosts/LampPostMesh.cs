using System.Collections.Generic;
using UnityEngine;

namespace NightMod.LampPosts;

/// <summary>
/// Builds the lamp post 3D model procedurally, so the mod ships no asset bundle. The model is a
/// thin vertical pole with a boxy lamp head on top, and a small panel tucked under the head - the
/// "lens" the light shines from.
///
/// <para>The mesh has two submeshes so the head and pole can be matte while the lens is emissive:
/// submesh 0 is the pole and head, submesh 1 is the lens panel. The model is built once and
/// shared by every placed lamp post, and its origin sits at the foot of the pole so the post
/// stands on the terrain surface it is placed on.</para>
/// </summary>
internal static class LampPostMesh {

	/// <summary>Total height of the post, in Unity units (≈ metres in COI's convention).</summary>
	public const float Height = 6.5f;

	/// <summary>Submesh index of the matte pole and head.</summary>
	public const int MatteSubmesh = 0;

	/// <summary>Submesh index of the emissive lens panel under the head.</summary>
	public const int LensSubmesh = 1;

	/// <summary>Edge length of the square pole cross-section.</summary>
	private const float PoleWidth = 0.18f;

	/// <summary>Half-width of the boxy lamp head sitting on top of the pole.</summary>
	private const float HeadHalfWidth = 0.5f;

	/// <summary>Vertical thickness of the lamp head.</summary>
	private const float HeadThickness = 0.36f;

	/// <summary>Half-width of the emissive lens panel under the head.</summary>
	private const float LensHalfWidth = 0.32f;

	/// <summary>Vertical thickness of the lens panel.</summary>
	private const float LensThickness = 0.1f;

	private static Mesh s_shared;

	/// <summary>
	/// The shared lamp post mesh, built on first use. Every lamp post prefab references this one
	/// instance.
	/// </summary>
	public static Mesh Shared {
		get {
			if (s_shared == null) {
				s_shared = build();
			}
			return s_shared;
		}
	}

	/// <summary>
	/// Local-space position of the centre of the lens underside, relative to the pole foot. The
	/// night light system places a spot light here and aims it straight down.
	/// </summary>
	public static Vector3 LensLocalPosition =>
		new Vector3(0f, Height - HeadThickness - LensThickness, 0f);

	private static Mesh build() {
		List<Vector3> verts = new List<Vector3>();
		List<int> matteTris = new List<int>();
		List<int> lensTris = new List<int>();

		float poleHalf = PoleWidth * 0.5f;
		float headBottom = Height - HeadThickness;

		// The pole: a tall thin box from the ground to the underside of the head.
		addBox(verts, matteTris,
			new Vector3(-poleHalf, 0f, -poleHalf),
			new Vector3(poleHalf, headBottom, poleHalf));

		// The lamp head: a wider, flat box on top of the pole.
		addBox(verts, matteTris,
			new Vector3(-HeadHalfWidth, headBottom, -HeadHalfWidth),
			new Vector3(HeadHalfWidth, Height, HeadHalfWidth));

		// The lens: a thin emissive panel hanging just under the head (its top edge tucks slightly
		// up into the head so there is no z-fighting gap). This is the glowing face the light cone
		// appears to come from.
		addBox(verts, lensTris,
			new Vector3(-LensHalfWidth, headBottom - LensThickness, -LensHalfWidth),
			new Vector3(LensHalfWidth, headBottom + 0.02f, LensHalfWidth));

		Mesh mesh = new Mesh { name = "NightMod_LampPost" };
		mesh.SetVertices(verts);
		mesh.subMeshCount = 2;
		mesh.SetTriangles(matteTris, MatteSubmesh);
		mesh.SetTriangles(lensTris, LensSubmesh);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	/// <summary>Appends an axis-aligned box spanning <paramref name="min"/>..<paramref name="max"/>.</summary>
	private static void addBox(List<Vector3> verts, List<int> tris, Vector3 min, Vector3 max) {
		// The eight box corners.
		Vector3 c0 = new Vector3(min.x, min.y, min.z);
		Vector3 c1 = new Vector3(max.x, min.y, min.z);
		Vector3 c2 = new Vector3(max.x, min.y, max.z);
		Vector3 c3 = new Vector3(min.x, min.y, max.z);
		Vector3 c4 = new Vector3(min.x, max.y, min.z);
		Vector3 c5 = new Vector3(max.x, max.y, min.z);
		Vector3 c6 = new Vector3(max.x, max.y, max.z);
		Vector3 c7 = new Vector3(min.x, max.y, max.z);

		// Each face gets its own four vertices so RecalculateNormals produces a flat per-face
		// normal instead of averaging across the box edges (which rounds the shading). Winding is
		// clockwise so the faces point outward in Unity's left-handed space.
		addFace(verts, tris, c0, c1, c2, c3); // bottom (-Y)
		addFace(verts, tris, c7, c6, c5, c4); // top    (+Y)
		addFace(verts, tris, c4, c5, c1, c0); // -Z
		addFace(verts, tris, c6, c7, c3, c2); // +Z
		addFace(verts, tris, c7, c4, c0, c3); // -X
		addFace(verts, tris, c5, c6, c2, c1); // +X
	}

	/// <summary>Appends one quad as four fresh vertices and two triangles.</summary>
	private static void addFace(List<Vector3> verts, List<int> tris, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
		int b = verts.Count;
		verts.Add(p0);
		verts.Add(p1);
		verts.Add(p2);
		verts.Add(p3);
		tris.Add(b);
		tris.Add(b + 1);
		tris.Add(b + 2);
		tris.Add(b);
		tris.Add(b + 2);
		tris.Add(b + 3);
	}
}
