using UnityEngine;

namespace pfc.DomeTools {

	[ExecuteInEditMode]
	public class ShiftedPerspective : MonoBehaviour
	{
		public Transform screenQuad;
		
		private float n = 0.05f;
		private float f = 1000;
			
		private float sr = 1;
		private float sl = -1;
		private float st = 1;
		private float sb = -1;
		private float dist = 1;
		
		private Matrix4x4 proj = Matrix4x4.identity;
		private Vector3 diff;
		private Camera cam;

		private void Update() 
		{
			if (!cam) cam = GetComponent<Camera>();
			if (!cam) return;
			if (!screenQuad) return;

			var screenWidthDM = screenQuad.lossyScale.x;
			var screenHeightDM = screenQuad.lossyScale.y;

			sr = -screenWidthDM / 2;
			sl = -sr;
			st = -screenHeightDM / 2;
			sb = -st;

			n = cam.nearClipPlane;
			f = cam.farClipPlane;

			if (screenQuad)
			{
				diff = screenQuad.InverseTransformPoint(transform.position);
				diff.Scale(screenQuad.lossyScale);
			}
			else
				diff = transform.position;
			
			dist = diff.z;

			transform.rotation = screenQuad.rotation;
			
			// Convert from screen to near plane
			float r = (sr + diff.x) * n / dist;
			float l = (sl + diff.x) * n / dist;
			float t = (st + diff.y) * n / dist;
			float b = (sb + diff.y) * n / dist;		
			
			// Construct projection matrix
			proj[0,0] = 2*n/(r-l);
			proj[0,1] = 0;
			proj[0,2] = (r+l)/(r-l);
			proj[0,3] = 0;
			proj[1,0] = 0;
			proj[1,1] = 2*n/(t-b);
			proj[1,2] = (t+b)/(t-b);
			proj[1,3] = 0;
			proj[2,0] = 0;
			proj[2,1] = 0;
			proj[2,2] = -(f+n)/(f-n);
			proj[2,3] = -2*f*n/(f-n);
			proj[3,0] = 0;
			proj[3,1] = 0;
			proj[3,2] = -1;
			proj[3,3] = 0;
			
			var diagonal = screenWidthDM / 2;
			cam.aspect = 1;
			cam.fieldOfView = Mathf.Atan2(diagonal, Mathf.Abs(dist)) * 2 * Mathf.Rad2Deg;

			cam.projectionMatrix = proj;
		}

		public Color gizmoColor = new Color(1,1,1,0.5f);

		private void OnDrawGizmosSelected()
		{
			if(!cam) return;
			
			Gizmos.color = gizmoColor;
			Vector3[] frustumCorners = new Vector3[4];
			cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

			for (int i = 0; i < 4; i++)
			{
				var worldSpaceCorner = cam.transform.TransformVector(frustumCorners[i]);
				Gizmos.DrawLine(cam.transform.position, worldSpaceCorner);
			}
		}
	}
}