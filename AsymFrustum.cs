/// <summary>
/// Asym frustum.
/// based on http://paulbourke.net/stereographics/stereorender/
/// and http://answers.unity3d.com/questions/165443/asymmetric-view-frusta-selective-region-rendering.html
/// </summary>
using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class AsymFrustum : MonoBehaviour
{

	public GameObject virtualWindow;

    /// <summary>
    /// Screen/window to virtual world width (in units. I suggest using meters)
    /// </summary>
    public float width;
    /// <summary>
	/// Screen/window to virtual world height (in units. I suggest using meters)
    /// </summary>
    public float height;
    /// <summary>
	/// The maximum height the camera can have (up axis in local coordinates from  the virtualWindow) (in units. I suggest using meters)
    /// </summary>
    public float maxHeight = 2000.0f;
    float windowWidth;
    float windowHeight;



	public bool verbose = false;

	/// <summary>
	/// Called when this Component gets initialized
	/// </summary>
    void Start()
    {
    }

	/// <summary>
	/// Late Update. Hopefully by now the head position got updated by whatever you use as input here.
	/// </summary>
    void LateUpdate()
    {
		windowWidth = width;
		windowHeight = height;
		
		// gets the local position of this camera depending on the virtual screen
		Vector3 localPos = virtualWindow.transform.InverseTransformPoint (transform.position);

		setAsymmetricFrustum(GetComponent<Camera>(), localPos,GetComponent<Camera>().nearClipPlane);

    }
    /// <summary>
    /// Sets the asymmetric Frustum for the given virtual Window (at pos 0,0,0 )
    /// and the camera passed
    /// </summary>
    /// <param name="cam">Camera to get the asymmetric frustum for</param>
    /// <param name="pos">Position of the camera. Usually cam.transform.position</param>
    /// <param name="nearDist">Near clipping plane, usually cam.nearClipPlane</param>
    public void setAsymmetricFrustum(Camera cam,Vector3 pos, float nearDist)
    {

        // Focal length = orthogonal distance to image plane
		Vector3 newpos = pos;
		//newpos.Scale (new Vector3 (1, 1, 1));
        float focal = 1;
        
		newpos = new Vector3 (newpos.x, newpos.z, newpos.y);
		if (verbose) 
		{
			Debug.Log (newpos.x+";"+newpos.y+";"+newpos.z);
		}

		focal = Mathf.Clamp(newpos.z,0.001f, maxHeight);

        // Ratio for intercept theorem
        float ratio = focal / nearDist;

        // Compute size for focal
        float imageLeft = (-windowWidth / 2.0f) - newpos.x;
        float imageRight = (windowWidth / 2.0f) - newpos.x;
        float imageTop = (windowHeight / 2.0f) - newpos.y;
        float imageBottom = (-windowHeight / 2.0f) - newpos.y;

        // Intercept theorem
        float nearLeft = imageLeft / ratio;
        float nearRight = imageRight / ratio;
		float nearTop = imageTop / ratio;
		float nearBottom = imageBottom / ratio;

        Matrix4x4 m = PerspectiveOffCenter(nearLeft, nearRight, nearBottom, nearTop, cam.nearClipPlane, cam.farClipPlane);
        cam.projectionMatrix = m;
    }


   
    /// <summary>
	/// Set an off-center projection, where perspective's vanishing
	/// point is not necessarily in the center of the screen.
	/// left/right/top/bottom define near plane size, i.e.
	/// how offset are corners of camera's near plane.
	/// Tweak the values and you can see camera's frustum change.
    /// </summary>
    /// <returns>The off center.</returns>
    /// <param name="left">Left.</param>
    /// <param name="right">Right.</param>
    /// <param name="bottom">Bottom.</param>
    /// <param name="top">Top.</param>
    /// <param name="near">Near.</param>
    /// <param name="far">Far.</param>
    Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = (2.0f * near) / (right - left);
        float y = (2.0f * near) / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0f * far * near) / (far - near);
        float e = -1.0f;

        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;
        return m;
    }
    /// <summary>
    /// Draws gizmos in the Edit window.
    /// </summary>
    public virtual void OnDrawGizmos()
    {
		Gizmos.DrawLine (GetComponent<Camera>().transform.position, GetComponent<Camera>().transform.position+GetComponent<Camera>().transform.up * 10);		
		Gizmos.color = Color.green;
		Gizmos.DrawLine(virtualWindow.transform.position, virtualWindow.transform.position + virtualWindow.transform.up);
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(virtualWindow.transform.position - virtualWindow.transform.forward * 0.5f * windowHeight, virtualWindow.transform.position + virtualWindow.transform.forward * 0.5f * windowHeight);
		
		Gizmos.color = Color.red;
		Gizmos.DrawLine(virtualWindow.transform.position - virtualWindow.transform.right * 0.5f * windowWidth, virtualWindow.transform.position + virtualWindow.transform.right * 0.5f * windowWidth);
		Gizmos.color = Color.white;
		Vector3 leftBottom = virtualWindow.transform.position - virtualWindow.transform.right * 0.5f * windowWidth - virtualWindow.transform.forward * 0.5f * windowHeight;
		Vector3 leftTop = virtualWindow.transform.position - virtualWindow.transform.right * 0.5f * windowWidth + virtualWindow.transform.forward * 0.5f * windowHeight;
		Vector3 rightBottom = virtualWindow.transform.position + virtualWindow.transform.right * 0.5f * windowWidth - virtualWindow.transform.forward * 0.5f * windowHeight;
		Vector3 rightTop = virtualWindow.transform.position + virtualWindow.transform.right * 0.5f * windowWidth + virtualWindow.transform.forward * 0.5f * windowHeight;
		
		Gizmos.DrawLine(leftBottom,leftTop);
		Gizmos.DrawLine(leftTop,rightTop);
		Gizmos.DrawLine(rightTop,rightBottom);
		Gizmos.DrawLine(rightBottom,leftBottom);		
		Gizmos.color = Color.grey;
		Gizmos.DrawLine(transform.position,leftTop);
		Gizmos.DrawLine(transform.position,rightTop);
		Gizmos.DrawLine(transform.position,rightBottom);
		Gizmos.DrawLine(transform.position,leftBottom);	

	}
}