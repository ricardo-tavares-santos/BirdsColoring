// Optimized Mobile Painter - Unitycoder.com
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_4_6_FEATURES
using UnityEngine.EventSystems;
#endif

namespace unitycoder_MobilePaint
{
	// list of drawmodes
	public enum DrawMode
	{
		Default,
		CustomBrush,
		FloodFill,
		Pattern
	}

	public class MobilePaint : MonoBehaviour
	{
		Camera paintCamera;
		public bool enableMouse = true;
		public bool enableTouch = false;
		public LayerMask paintLayerMask;
		public bool createCanvasMesh = true; // default canvas is full screen quad, if disabled existing mesh is used
		public bool connectBrushStokes = true; // if brush moves too fast, then connect them with line. NOTE! Disable this if you are painting to custom mesh

		//	*** Default settings ***
		public Color32 paintColor = new Color32 (255, 0, 0, 255);
		public float resolutionScaler = 1.0f; // 1 means screen resolution, 0.5f means half the screen resolution
		public int brushSize; // default brush size
		public int brushSizeMin = 1; // default min brush size
		public int brushSizeMax = 64; // default max brush size
		public bool useAdditiveColors = true; // true = alpha adds up slowly, false = 1 click will instantly set alpha to brush or paint color alpha value
		public float brushAlphaStrength = 0.5f; // multiplier to soften brush additive alpha, 0.1f is nice & smooth, 1 = faster

//		public bool DontPaintOverBlack = true; // so that outlines are reserved when painting

//		public int drawMode = 0; // drawing modes: 0 = draw (default), 1 = custom brush, 2 = floodfill
		public DrawMode drawMode = DrawMode.Default; // drawing modes: 0 = Default, 1 = custom brush, 2 = floodfill
		public bool useLockArea = false; // locking mask: only paint in area of the color that your click first
		public bool useMaskLayerOnly = false; // if true, only check pixels from mask layer, not from the painted texture
		public bool useThreshold = false;
		public byte paintThreshold = 128; // 0 = only exact match, 255 = match anything

		//private bool lockMaskCreated=false; //is lockmask already created for this click, not used yet
		private byte[] lockMaskPixels; // locking mask pixels


		public bool canDrawOnBlack = true; // to stop filling on mask black lines, FIXME: not working if its not pure black..


		//public bool drawAfterFill = true; // TODO: return to drawing mode after first fill?

		public Vector2 canvasSizeAdjust = new Vector2 (-32, 0); // this means, "ScreenResolution.xy+screenSizeAdjust.xy" (use only minus values, to add un-drawable border on right or bottom)

		public string targetTexture = "_MainTex"; // target texture for this material shader (usually _MainTex)
		public FilterMode filterMode = FilterMode.Point;

		// canvas clear color
		public Color32 clearColor = new Color32 (255, 255, 255, 255);
		
		// for using texture on canvas
		public bool useMaskImage = false;
		public Texture2D maskTex;
		
		// for using custom brushes
		public bool useCustomBrushes = true;
		public Texture2D[] customBrushes;
		public bool useCustomBrushAlpha = true; // true = use alpha from brush, false = use alpha from current paint color
		public int selectedBrush = 0; // currently selected brush index

		//private Color[] customBrushPixels;
		private byte[] customBrushBytes;
		private int customBrushWidth;
		private int customBrushHeight;
		private int customBrushWidthHalf;
//		private int customBrushHeightHalf;
		private int texWidthMinusCustomBrushWidth;
		private int texHeightMinusCustomBrushHeight;

		// PATTERNS

		public Texture2D[] customPatterns;
		private byte[] patternBrushBytes;
		private int customPatternWidth;
		private int customPatternHeight;
		public int selectedPattern = 0;



		// UNDO
		private byte[] undoPixels; // undo buffer
		//private List<byte[]> undoPixels; // undo buffer(s)
		public bool undoEnabled = false;

		// new UI
		#if ENABLE_4_6_FEATURES
		public GameObject userInterface;
		public bool hideUIWhilePainting=true;
		private bool isUIVisible=true;
		#endif

		// for old GUIScaling
		private float scaleAdjust = 1f;
		private const float BASE_WIDTH = 800;
		private const float BASE_HEIGHT = 480;


		//	*** private variables, no need to touch ***
		private byte[] pixels; // byte array for texture painting, this is the image that we paint into.
		private byte[] maskPixels; // byte array for mask texture
		private byte[] clearPixels; // byte array for clearing texture

		private Texture2D tex; // texture that we paint into (it gets updated from pixels[] array when painted)
		//private Texture2D maskTex; // texture used as a overlay mask
		private int texWidth;
		private int texHeight;
		private Touch touch; // touch reference
		private Camera cam; // main camera reference
		private RaycastHit hit;
		private bool wentOutside = false;
		private bool usingClearingImage = false; // did we have initial texture as maintexture, then use it as clear pixels array

		private Vector2 pixelUV; // with mouse
		private Vector2 pixelUVOld; // with mouse

		private Vector2[] pixelUVs; // mobiles
		private Vector2[] pixelUVOlds; // mobiles

		[HideInInspector]
		public bool
			textureNeedsUpdate = false; // if we have modified texture

		// for checking if UI element is clicked, then dont paint under it
		#if ENABLE_4_6_FEATURES
		EventSystem eventSystem;
		#endif
		public bool useNewUI = false;
		public GameObject hintTexture;
		public UITexture nailTexture;

		void Awake ()
		{

			//brushSize = (int)(Screen.height * 0.3f);

			// reference to 4.6 canvas
			#if ENABLE_4_6_FEATURES
			GameObject go = GameObject.Find("EventSystem");
			if (go!=null) {
				eventSystem = go.GetComponent<UnityEngine.EventSystems.EventSystem>();
				if (eventSystem==null) useNewUI=false;
			}else{
				useNewUI=false;
			}
			#else
			useNewUI = false;
			#endif

			paintCamera = FindObjectOfType<Camera> ();


			maskTex = Resources.Load("Drawings/New/"+GameManager.Instance.imageIndex) as Texture2D;


			InitializeEverything ();
		//	ReadCurrentCustomBrush();
			nailTexture.mainTexture = GetComponent<Renderer> ().material.mainTexture;
		}

		void Start ()
		{
			GameManager.Instance.mobilePaint = this;
			GameObject.FindGameObjectWithTag("PaintCamera").GetComponent<Camera>().orthographicSize = 1f;
		}

//		public void AssignNailTexture()
//		{
//			Debug.Log ("assignnewTexture");
//			// create new texture
//			tex = null;
//			tex = new Texture2D (texWidth, texHeight, TextureFormat.RGBA32, false);
//			GetComponent<Renderer> ().material.SetTexture (targetTexture, tex);
//			// init pixels array
//			pixels = new byte[texWidth * texHeight * 4];
//			ClearImage ();
//			Invoke ("ChangeNailTexture",0.01f);
//		}

		void ChangeNailTexture(){
			nailTexture.mainTexture = GetComponent<Renderer> ().material.mainTexture;
		}

		public void InitializeEverything ()
		{
			
			if (enableMouse && enableTouch)
				Debug.LogWarning ("You have enabled both Mouse & Touch, that can cause problems. Also UI hiding won't work..");
			
			// calculate scaling ratio for different screen resolutions
			float _baseHeightInverted = 1.0f / BASE_HEIGHT;
			float ratio = (781 * _baseHeightInverted) * scaleAdjust;
			canvasSizeAdjust *= ratio;
			
			
			// WARNING: fixed maximum amount of touches, is set to 20 here. Not sure if some device supports more?
			pixelUVs = new Vector2[20];
			pixelUVOlds = new Vector2[20];
			
			// adjust cam size
			paintCamera.orthographicSize = 781 / 2;
			
			
			if (createCanvasMesh) {
				CreateFullScreenQuad ();
			} else { // using existing mesh
				if (connectBrushStokes)
					Debug.LogWarning ("Custom mesh used, but connectBrushStokes is enabled, it can cause problems on the mesh borders wrapping");
				
				if (GetComponent<MeshCollider> () == null)
					Debug.LogError ("MeshCollider is missing, won't be able to raycast to canvas object");
				if (GetComponent<MeshFilter> () == null || GetComponent<MeshFilter> ().sharedMesh == null)
					Debug.LogWarning ("Mesh or MeshFilter is missing, won't be able to see the canvas object");
			}
			
			// create texture
			if (useMaskImage) {
				// check if its assigned
				if (maskTex == null) {
					Debug.LogWarning ("maskImage is not assigned. Setting 'useMaskImage' to false");
					useMaskImage = false;
				} else {
					// Check if we have correct material to use mask image (layer)
					if (GetComponent<Renderer> ().material.name.StartsWith ("CanvasWithAlpha") || GetComponent<Renderer> ().material.name.StartsWith ("CanvasDefault")) {
						// FIXME: this is bit annoying to compare material names..
						Debug.LogWarning ("CanvasWithAlpha and CanvasDefault materials do not support using MaskImage (layer). Disabling 'useMaskImage'");
						Debug.LogWarning ("CanvasWithAlpha and CanvasDefault materials do not support using MaskImage (layer). Disabling 'useMaskLayerOnly'");
						useMaskLayerOnly = false;
						
						useMaskImage = false;
						maskTex = null;
					} else {
						// takes texture size from maskTex, NOTE: if its square, image will look stretched
						
						// TODO: should have option for scaling/padding to match screensize
						
						texWidth = maskTex.width;
						texHeight = maskTex.height;
						GetComponent<Renderer> ().material.SetTexture ("_MaskTex", maskTex);
					}
				}
				
			} else {	// no mask texture
				// calculate texture size from screen size
				texWidth = (int)(633 * resolutionScaler + canvasSizeAdjust.x);
				texHeight = (int)(781 * resolutionScaler + canvasSizeAdjust.y);
			}
			
			
			// TODO: check if target texture exists
			if (!GetComponent<Renderer> ().material.HasProperty (targetTexture))
				Debug.LogError ("Fatal error: Current shader doesn't have a property: '" + targetTexture + "'");

			// we have no texture set for canvas
//			if (renderer.material.mainTexture==null)
			if (GetComponent<Renderer> ().material.GetTexture (targetTexture) == null) {
				// create new texture
				tex = new Texture2D (texWidth, texHeight, TextureFormat.RGBA32, false);
				GetComponent<Renderer> ().material.SetTexture (targetTexture, tex);
				
				// init pixels array
				pixels = new byte[texWidth * texHeight * 4];
				
			} else { // we have canvas texture, then use that as clearing texture
				
				usingClearingImage = true;
				
				texWidth = GetComponent<Renderer> ().material.GetTexture (targetTexture).width;
				texHeight = GetComponent<Renderer> ().material.GetTexture (targetTexture).height;
				
				// init pixels array
				pixels = new byte[texWidth * texHeight * 4];
				
				tex = new Texture2D (texWidth, texHeight, TextureFormat.RGBA32, false);
				
				// we keep current maintex and read it as "clear pixels array"
				ReadClearingImage ();
				
				GetComponent<Renderer> ().material.SetTexture (targetTexture, tex);
			}
			
			
			ClearImage ();
			
			// set texture modes
			tex.filterMode = filterMode;
			tex.wrapMode = TextureWrapMode.Clamp;
			//tex.wrapMode = TextureWrapMode.Repeat;
			
			if (useMaskImage) {
				ReadMaskImage ();
			}
			
			// undo system
			if (undoEnabled) {
				undoPixels = new byte[texWidth * texHeight * 4];
				System.Array.Copy (pixels, undoPixels, pixels.Length);
			}
			
			// locking mask enabled
			if (useLockArea) {
				lockMaskPixels = new byte[texWidth * texHeight * 4];
			}

			// TEST pattern painter
			if (customPatterns != null && customPatterns.Length > 0)
				ReadCurrentCustomPattern ();


		} // InitializeEverything





		// *** MAINLOOP ***
		void Update ()
		{
			if (enableMouse) {

				MousePaint ();
			}
			if (enableTouch)
				TouchPaint ();

			UpdateTexture ();
		}

		void MousePaint ()
		{
			// TEST: Undo key for desktop
			if (Input.GetKeyDown ("u")) {
				DoUndo ();
			}

			// mouse is over UI element? then dont paint
			#if ENABLE_4_6_FEATURES
			if (useNewUI && eventSystem.IsPointerOverGameObject()) return;
			#endif


			if (Input.GetMouseButtonDown (0)) {
			
				#if ENABLE_4_6_FEATURES
				if (hideUIWhilePainting && isUIVisible) HideUI();
				#endif
			
				// when starting, grab undo buffer first
				if (undoEnabled) {

					System.Array.Copy (pixels, undoPixels, pixels.Length);
				}

				if (useLockArea) {
					if (!Physics.Raycast (paintCamera.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, paintLayerMask))
						return;
					CreateAreaLockMask ((int)(hit.textureCoord.x * texWidth), (int)(hit.textureCoord.y * texHeight));
				}
			}

			if (Input.GetMouseButton (0)) {
				// Only if we hit something, then we continue
				if (!Physics.Raycast (paintCamera.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, paintLayerMask)) {
					wentOutside = true;
//					Debug.Log("mouseclicked");
					return;
				}

				pixelUVOld = pixelUV; // take previous value, so can compare them
				pixelUV = hit.textureCoord;
				pixelUV.x *= texWidth;
				pixelUV.y *= texHeight;

				if (wentOutside) {
					pixelUVOld = pixelUV;
					wentOutside = false;
				}

				// lets paint where we hit
				switch (drawMode) {
				case DrawMode.Default: // drawing
					DrawCircle ((int)pixelUV.x, (int)pixelUV.y);
					break;

				case DrawMode.Pattern: // draw with pattern
					DrawPatternCircle ((int)pixelUV.x, (int)pixelUV.y);
					break;

				case DrawMode.CustomBrush: // custom brush
					DrawCustomBrush ((int)pixelUV.x, (int)pixelUV.y);
					break;

				case DrawMode.FloodFill: // floodfill
					if (pixelUVOld == pixelUV)
						break;
					if (useThreshold) {
						if (useMaskLayerOnly) {
							FloodFillMaskOnlyWithThreshold ((int)pixelUV.x, (int)pixelUV.y);
						} else {
							FloodFillWithTreshold ((int)pixelUV.x, (int)pixelUV.y);
						}
					} else {
						if (useMaskLayerOnly) {
							FloodFillMaskOnly ((int)pixelUV.x, (int)pixelUV.y);
						} else {

							FloodFill ((int)pixelUV.x, (int)pixelUV.y);
						}
					}
					break;

				default: // unknown mode
					// Debug.LogWarning("Unknown drawing mode:"+drawMode);
					break;
				}

				textureNeedsUpdate = true;
			}

			if (Input.GetMouseButtonDown (0)) {
				// take this position as start position
				if (!Physics.Raycast (paintCamera.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, paintLayerMask))
					return;

				pixelUVOld = pixelUV;
			}


			// check distance from previous drawing point and connect them with DrawLine
			if (connectBrushStokes && Vector2.Distance (pixelUV, pixelUVOld) > brushSize) {
				switch (drawMode) {
				case DrawMode.Default: // drawing
					DrawLine (pixelUVOld, pixelUV);
					break;

				case DrawMode.CustomBrush:
					DrawLineWithBrush (pixelUVOld, pixelUV);
					break;

				case DrawMode.Pattern:
					DrawLineWithPattern (pixelUVOld, pixelUV);
					break;

				default: // other modes
					break;
				}
				pixelUVOld = pixelUV;
				textureNeedsUpdate = true;
			}

			// show UI after releasing button
			#if ENABLE_4_6_FEATURES
			if (Input.GetMouseButtonUp(0))
			{
				if (hideUIWhilePainting && !isUIVisible) ShowUI();
			}
			#endif


		}

		void TouchPaint ()
		{
			int i = 0;

			while (i < Input.touchCount) {
				hintTexture.SetActive (false);
				touch = Input.GetTouch (i);
				#if ENABLE_4_6_FEATURES
				if (useNewUI && eventSystem.IsPointerOverGameObject(touch.fingerId)) return;
				#endif
				i++;
			}

			i = 0;
			// loop until all touches are processed
			while (i < Input.touchCount) {

				touch = Input.GetTouch (i);
				if (touch.phase == TouchPhase.Began) {
					#if ENABLE_4_6_FEATURES
					if (hideUIWhilePainting && isUIVisible) HideUI();
					#endif

					// when starting, grab undo buffer first
					if (undoEnabled) {
						System.Array.Copy (pixels, undoPixels, pixels.Length);
					}

					if (useLockArea) {
						if (!Physics.Raycast (paintCamera.ScreenPointToRay (touch.position), out hit, Mathf.Infinity, paintLayerMask)) {
							wentOutside = true;
							return;
						}

						/*
						pixelUV = hit.textureCoord;
						pixelUV.x *= texWidth;
						pixelUV.y *= texHeight;
						if (wentOutside) {pixelUVOld = pixelUV;wentOutside=false;}
						CreateAreaLockMask((int)pixelUV.x, (int)pixelUV.y);
						*/

						pixelUVs [touch.fingerId] = hit.textureCoord;
						pixelUVs [touch.fingerId].x *= texWidth;
						pixelUVs [touch.fingerId].y *= texHeight;
						if (wentOutside) {
							pixelUVOlds [touch.fingerId] = pixelUVs [touch.fingerId];
							wentOutside = false;
						}
						CreateAreaLockMask ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
					}
				}
				// check state
				if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began) {

					// do raycast on touch position
					if (Physics.Raycast (paintCamera.ScreenPointToRay (touch.position), out hit, Mathf.Infinity, paintLayerMask)) {
						// take previous value, so can compare them
						pixelUVOlds [touch.fingerId] = pixelUVs [touch.fingerId];
						// get hit texture coordinate
						pixelUVs [touch.fingerId] = hit.textureCoord;
						pixelUVs [touch.fingerId].x *= texWidth;
						pixelUVs [touch.fingerId].y *= texHeight;
						// paint where we hit
						switch (drawMode) {
						case DrawMode.Default:
							DrawCircle ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
							break;

						case DrawMode.CustomBrush:
							DrawCustomBrush ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
							break;

						case DrawMode.Pattern:
							DrawPatternCircle ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
							break;

						case DrawMode.FloodFill:
							if (useThreshold) {
								if (useMaskLayerOnly) {
									FloodFillMaskOnlyWithThreshold ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
								} else {
									FloodFillWithTreshold ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
								}
							} else {
								if (useMaskLayerOnly) {
									FloodFillMaskOnly ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
								} else {
									
									FloodFill ((int)pixelUVs [touch.fingerId].x, (int)pixelUVs [touch.fingerId].y);
								}
							}
							break;


						default:
							// unknown mode
							break;
						}
						// set flag that texture needs to be applied
						textureNeedsUpdate = true;
					}
				}
				// if we just touched screen, set this finger id texture paint start position to that place
				if (touch.phase == TouchPhase.Began) {
					pixelUVOlds [touch.fingerId] = pixelUVs [touch.fingerId];
				}
				// check distance from previous drawing point
				if (connectBrushStokes && Vector2.Distance (pixelUVs [touch.fingerId], pixelUVOlds [touch.fingerId]) > brushSize) {
					switch (drawMode) {
					case DrawMode.Default:
						DrawLine (pixelUVOlds [touch.fingerId], pixelUVs [touch.fingerId]);
						break;

					case DrawMode.CustomBrush:
						DrawLineWithBrush (pixelUVOlds [touch.fingerId], pixelUVs [touch.fingerId]);
						break;
						
					case DrawMode.Pattern:
						DrawLineWithPattern (pixelUVOlds [touch.fingerId], pixelUVs [touch.fingerId]);
						break;

					default:
						// unknown mode, set back to 0?
						break;
					}
					textureNeedsUpdate = true;

					pixelUVOlds [touch.fingerId] = pixelUVs [touch.fingerId];

				}
				// loop all touches
				i++;
			}

			#if ENABLE_4_6_FEATURES
			if (Input.touchCount==0)
			{
				if (hideUIWhilePainting && !isUIVisible) ShowUI();
			}
			#endif

		}

		public void HideUI ()
		{
			if (!useNewUI)
				return;
//			isUIVisible = false;
//			userInterface.SetActive (isUIVisible);
		}

		public void ShowUI ()
		{
			if (!useNewUI)
				return;
//			isUIVisible = true;
//			userInterface.SetActive (isUIVisible);
		}

		void UpdateTexture ()
		{
			if (textureNeedsUpdate) {
				textureNeedsUpdate = false;
				tex.LoadRawTextureData (pixels);
				tex.Apply (false);
			}
		}

		void CreateAreaLockMask (int x, int y)
		{
			if (useThreshold) {
				if (useMaskLayerOnly) {
					LockAreaFillWithThresholdMaskOnly (x, y);
				} else {
					LockMaskFillWithThreshold (x, y);
				}
			} else { // no threshold
				if (useMaskLayerOnly) {
					LockAreaFillMaskOnly (x, y);
				} else {
					LockAreaFill (x, y);
				}
			}
			//lockMaskCreated = true; // not used yet
		}

		// main painting function, http://stackoverflow.com/a/24453110
		public void DrawCircle (int x, int y)
		{
			// clamp brush inside texture
			if (createCanvasMesh) { // TEMPORARY FIX: with a custom sphere mesh, small gap in paint at the end, so must disable clamp on most custom meshes
				//x = PaintTools.ClampBrushInt(x,brushSize,texWidth-brushSize);
				//y = PaintTools.ClampBrushInt(y,brushSize,texHeight-brushSize);
			}

			if (!canDrawOnBlack) {
//				if (pixels[(texWidth*y+x)*4]==0 && pixels[(texWidth*y+x)*4+1]==0 && pixels[(texWidth*y+x)*4+2]==0 && pixels[(texWidth*y+x)*4+3]!=0) return;
			}

			int pixel = 0;

			// draw fast circle: 
			int r2 = brushSize * brushSize;
			int area = r2 << 2;
			int rr = brushSize << 1;
			for (int i = 0; i < area; i++) {
				int tx = (i % rr) - brushSize;
				int ty = (i / rr) - brushSize;
				if (tx * tx + ty * ty < r2) {
					if (x + tx < 0 || y + ty < 0 || x + tx >= texWidth || y + ty >= texHeight)
						continue; // temporary fix for corner painting


					pixel = (texWidth * (y + ty) + x + tx) * 4;
					//pixel = ( texWidth*( (y+ty) % texHeight )+ (x+tx) % texWidth )*4;

					if (useAdditiveColors) {
						// additive over white also
						if (!useLockArea || (useLockArea && lockMaskPixels [pixel] == 1)) {
							pixels [pixel] = (byte)Mathf.Lerp (pixels [pixel], paintColor.r, paintColor.a / 255f * brushAlphaStrength);
							pixels [pixel + 1] = (byte)Mathf.Lerp (pixels [pixel + 1], paintColor.g, paintColor.a / 255f * brushAlphaStrength);
							pixels [pixel + 2] = (byte)Mathf.Lerp (pixels [pixel + 2], paintColor.b, paintColor.a / 255f * brushAlphaStrength);
							pixels [pixel + 3] = (byte)Mathf.Lerp (pixels [pixel + 3], paintColor.a, paintColor.a / 255 * brushAlphaStrength);
						}

					} else { // no additive, just paint my colors

						if (!useLockArea || (useLockArea && lockMaskPixels [pixel] == 1)) {
							pixels [pixel] = paintColor.r;
							pixels [pixel + 1] = paintColor.g;
							pixels [pixel + 2] = paintColor.b;
							pixels [pixel + 3] = paintColor.a;
						}

					} // if additive
				} // if in circle
			} // for area
		} // DrawCircle()

		public void DrawPatternCircle (int x, int y)
		{
			// clamp brush inside texture
			if (createCanvasMesh) { // TEMPORARY FIX: with a custom sphere mesh, small gap in paint at the end, so must disable clamp on most custom meshes
				//x = PaintTools.ClampBrushInt(x,brushSize,texWidth-brushSize);
				//y = PaintTools.ClampBrushInt(y,brushSize,texHeight-brushSize);
			}
			
			if (!canDrawOnBlack) {
//				if (pixels[(texWidth*y+x)*4]==0 && pixels[(texWidth*y+x)*4+1]==0 && pixels[(texWidth*y+x)*4+2]==0 && pixels[(texWidth*y+x)*4+3]!=0) return;
			}
			
			int pixel = 0;
			
			// draw fast circle: 
			int r2 = brushSize * brushSize;
			int area = r2 << 2;
			int rr = brushSize << 1;

			for (int i = 0; i < area; i++) {
				int tx = (i % rr) - brushSize;
				int ty = (i / rr) - brushSize;

				if (tx * tx + ty * ty < r2) {
					if (x + tx < 0 || y + ty < 0 || x + tx >= texWidth || y + ty >= texHeight)
						continue; // temporary fix for corner painting

					pixel = (texWidth * (y + ty) + x + tx) * 4; // << 2

					if (useAdditiveColors) {
						// additive over white also
						if (!useLockArea || (useLockArea && lockMaskPixels [pixel] == 1)) {

							// TODO: take pattern texture as paint color
							/*
							Color32 patternColor = new Color(x,y,0,1);

							pixels[pixel] = (byte)Mathf.Lerp(pixels[pixel],patternColor.r,patternColor.a/255f*brushAlphaStrength);
							pixels[pixel+1] = (byte)Mathf.Lerp(pixels[pixel+1],patternColor.g,patternColor.a/255f*brushAlphaStrength);
							pixels[pixel+2] = (byte)Mathf.Lerp(pixels[pixel+2],patternColor.b,patternColor.a/255f*brushAlphaStrength);
							pixels[pixel+3] = (byte)Mathf.Lerp(pixels[pixel+3],patternColor.a,patternColor.a/255*brushAlphaStrength);
							*/
						}
						
					} else { // no additive, just paint my colors
						
						if (!useLockArea || (useLockArea && lockMaskPixels [pixel] == 1)) {
							// TODO: pattern dynamic scalar value?

							float yy = Mathf.Repeat (y + ty, customPatternWidth);
							float xx = Mathf.Repeat (x + tx, customPatternWidth);
							int pixel2 = (int)Mathf.Repeat ((customPatternWidth * xx + yy) * 4, patternBrushBytes.Length);

							pixels [pixel] = patternBrushBytes [pixel2];
							pixels [pixel + 1] = patternBrushBytes [pixel2 + 1];
							pixels [pixel + 2] = patternBrushBytes [pixel2 + 2];
							pixels [pixel + 3] = patternBrushBytes [pixel2 + 3];
						}
						
					} // if additive
				} // if in circle
			} // for area
		} // DrawPatternCircle()


		
		// actual custom brush painting function
		void DrawCustomBrush (int px, int py)
		{

			// TODO: this function needs comments/info..

			// get position where we paint
			int startX = (int)(px - customBrushWidthHalf);
			int startY = (int)(py - customBrushWidthHalf);
			
			if (startX < 0) {
				startX = 0;
			} else {
				if (startX + customBrushWidth >= texWidth)
					startX = texWidthMinusCustomBrushWidth;
			}
			
			if (startY < 1) {  // TODO: temporary fix, 1 instead of 0
				startY = 1;
			} else {
				if (startY + customBrushHeight >= texHeight)
					startY = texHeightMinusCustomBrushHeight;
			}
			
			// could use this for speed (but then its box shaped..)
			//System.Array.Copy(splatPixByte,0,data,4*(startY*startX),splatPixByte.Length);
			
			
			int pixel = (texWidth * startY + startX) * 4;

			int brushPixel = 0;

			//
			for (int y = 0; y < customBrushHeight; y++) {
				for (int x = 0; x < customBrushWidth; x++) {
					//brushColor = (customBrushPixels[x*customBrushWidth+y]);
					brushPixel = (customBrushWidth * (y) + x) * 4;

					// brush alpha is over 0 in this pixel?
					if (customBrushBytes [brushPixel + 3] > 0) {

						// take alpha from brush?
						if (useCustomBrushAlpha) {
							if (useAdditiveColors) {

								// additive over white also
								pixels [pixel] = (byte)Mathf.Lerp (pixels [pixel], customBrushBytes [brushPixel], customBrushBytes [brushPixel + 3] * brushAlphaStrength);
								pixels [pixel + 1] = (byte)Mathf.Lerp (pixels [pixel + 1], customBrushBytes [brushPixel + 1], customBrushBytes [brushPixel + 3] * brushAlphaStrength);
								pixels [pixel + 2] = (byte)Mathf.Lerp (pixels [pixel + 2], customBrushBytes [brushPixel + 2], customBrushBytes [brushPixel + 3] * brushAlphaStrength);
								pixels [pixel + 3] = (byte)Mathf.Lerp (pixels [pixel + 3], customBrushBytes [brushPixel + 3], customBrushBytes [brushPixel + 3] * brushAlphaStrength);

							} else { // no additive colors

								pixels [pixel] = customBrushBytes [brushPixel];
								pixels [pixel + 1] = customBrushBytes [brushPixel + 1];
								pixels [pixel + 2] = customBrushBytes [brushPixel + 2];
								pixels [pixel + 3] = customBrushBytes [brushPixel + 3];
							}

						} else { // use paint color alpha

							if (useAdditiveColors) {
								pixels [pixel] = (byte)Mathf.Lerp (pixels [pixel], customBrushBytes [brushPixel], paintColor.a / 255f * brushAlphaStrength);
								pixels [pixel + 1] = (byte)Mathf.Lerp (pixels [pixel + 1], customBrushBytes [brushPixel + 1], paintColor.a / 255f * brushAlphaStrength);
								pixels [pixel + 2] = (byte)Mathf.Lerp (pixels [pixel + 2], customBrushBytes [brushPixel + 2], paintColor.a / 255f * brushAlphaStrength);
								pixels [pixel + 3] = (byte)Mathf.Lerp (pixels [pixel + 3], customBrushBytes [brushPixel + 3], paintColor.a / 255f * brushAlphaStrength);

							} else { // no additive colors

								pixels [pixel] = customBrushBytes [brushPixel];
								pixels [pixel + 1] = customBrushBytes [brushPixel + 1];
								pixels [pixel + 2] = customBrushBytes [brushPixel + 2];
								pixels [pixel + 3] = customBrushBytes [brushPixel + 3];
							}
						}

					} // if alpha>0

					pixel += 4;
					
				} // for x

				pixel = (texWidth * (startY == 0 ? 1 : startY + y) + startX + 1) * 4;
			} // for y
		} // DrawCustomBrush

		void FloodFillMaskOnly (int x, int y)
		{
			// get canvas hit color
			byte hitColorR = maskPixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = maskPixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = maskPixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = maskPixels [((texWidth * (y) + x) * 4) + 3];
			
			// early exit if its same color already
			//if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA) return;

			if (!canDrawOnBlack) {
				if (hitColorA == 0)
					return;
			}

			
			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;

			lockMaskPixels = new byte[texWidth * texHeight * 4];

			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
					if (lockMaskPixels [pixel] == 0
						&& maskPixels [pixel + 0] == hitColorR 
						&& maskPixels [pixel + 1] == hitColorG 
						&& maskPixels [pixel + 2] == hitColorB 
						&& maskPixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0
						&& maskPixels [pixel + 0] == hitColorR 
						&& maskPixels [pixel + 1] == hitColorG 
						&& maskPixels [pixel + 2] == hitColorB 
						&& maskPixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0
						&& maskPixels [pixel + 0] == hitColorR 
						&& maskPixels [pixel + 1] == hitColorG 
						&& maskPixels [pixel + 2] == hitColorB 
						&& maskPixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0
						&& maskPixels [pixel + 0] == hitColorR 
						&& maskPixels [pixel + 1] == hitColorG 
						&& maskPixels [pixel + 2] == hitColorB 
						&& maskPixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
			}
		} // floodfill


		// basic floodfill
		void FloodFill (int x, int y)
		{

			// get canvas hit color
			byte hitColorR = pixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = pixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = pixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = pixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0)
					return;
			}

			// early exit if its same color already
			if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA)
				return;

			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);

			int ptsx, ptsy;
			int pixel = 0;

			while (fillPointX.Count > 0) {

				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();

				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
					if (pixels [pixel + 0] == hitColorR 
						&& pixels [pixel + 1] == hitColorG 
						&& pixels [pixel + 2] == hitColorB 
						&& pixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						DrawPoint (pixel);
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (pixels [pixel + 0] == hitColorR 
						&& pixels [pixel + 1] == hitColorG 
						&& pixels [pixel + 2] == hitColorB 
						&& pixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (pixels [pixel + 0] == hitColorR 
						&& pixels [pixel + 1] == hitColorG 
						&& pixels [pixel + 2] == hitColorB 
						&& pixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (pixels [pixel + 0] == hitColorR 
						&& pixels [pixel + 1] == hitColorG 
						&& pixels [pixel + 2] == hitColorB 
						&& pixels [pixel + 3] == hitColorA) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						DrawPoint (pixel);
					}
				}
			}
		} // floodfill

		void FloodFillMaskOnlyWithThreshold (int x, int y)
		{
			//Debug.Log("hits");
			// get canvas hit color
			byte hitColorR = maskPixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = maskPixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = maskPixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = maskPixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorA != 0)
					return;
			}

			// early exit if outside threshold?
			//if (CompareThreshold(paintColor.r,hitColorR) && CompareThreshold(paintColor.g,hitColorG) && CompareThreshold(paintColor.b,hitColorB) && CompareThreshold(paintColor.a,hitColorA)) return;
			if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA)
				return;
			
			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;

			lockMaskPixels = new byte[texWidth * texHeight * 4];
			
			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (maskPixels [pixel + 0], hitColorR) 
						&& CompareThreshold (maskPixels [pixel + 1], hitColorG) 
						&& CompareThreshold (maskPixels [pixel + 2], hitColorB) 
						&& CompareThreshold (maskPixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (maskPixels [pixel + 0], hitColorR) 
						&& CompareThreshold (maskPixels [pixel + 1], hitColorG) 
						&& CompareThreshold (maskPixels [pixel + 2], hitColorB) 
						&& CompareThreshold (maskPixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (maskPixels [pixel + 0], hitColorR) 
						&& CompareThreshold (maskPixels [pixel + 1], hitColorG) 
						&& CompareThreshold (maskPixels [pixel + 2], hitColorB) 
						&& CompareThreshold (maskPixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (maskPixels [pixel + 0], hitColorR) 
						&& CompareThreshold (maskPixels [pixel + 1], hitColorG) 
						&& CompareThreshold (maskPixels [pixel + 2], hitColorB) 
						&& CompareThreshold (maskPixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
			}
		} // floodfillWithTreshold


		void FloodFillWithTreshold (int x, int y)
		{
			// get canvas hit color
			byte hitColorR = pixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = pixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = pixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = pixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0)
					return;
			}

			// early exit if outside threshold
			//if (CompareThreshold(paintColor.r,hitColorR) && CompareThreshold(paintColor.g,hitColorG) && CompareThreshold(paintColor.b,hitColorB) && CompareThreshold(paintColor.a,hitColorA)) return;
			if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA)
				return;

			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;

			lockMaskPixels = new byte[texWidth * texHeight * 4];

			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (pixels [pixel + 0], hitColorR) 
						&& CompareThreshold (pixels [pixel + 1], hitColorG) 
						&& CompareThreshold (pixels [pixel + 2], hitColorB) 
						&& CompareThreshold (pixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (pixels [pixel + 0], hitColorR) 
						&& CompareThreshold (pixels [pixel + 1], hitColorG) 
						&& CompareThreshold (pixels [pixel + 2], hitColorB) 
						&& CompareThreshold (pixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (pixels [pixel + 0], hitColorR) 
						&& CompareThreshold (pixels [pixel + 1], hitColorG) 
						&& CompareThreshold (pixels [pixel + 2], hitColorB) 
						&& CompareThreshold (pixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0
						&& CompareThreshold (pixels [pixel + 0], hitColorR) 
						&& CompareThreshold (pixels [pixel + 1], hitColorG) 
						&& CompareThreshold (pixels [pixel + 2], hitColorB) 
						&& CompareThreshold (pixels [pixel + 3], hitColorA)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						DrawPoint (pixel);
						lockMaskPixels [pixel] = 1;
					}
				}
			}
		} // floodfillWithTreshold


		void LockAreaFill (int x, int y)
		{

			byte hitColorR = pixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = pixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = pixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = pixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0)
					return;
			}

			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;

			lockMaskPixels = new byte[texWidth * texHeight * 4];

			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

					if (lockMaskPixels [pixel] == 0
						&& (pixels [pixel + 0] == hitColorR || pixels [pixel + 0] == paintColor.r) 
						&& (pixels [pixel + 1] == hitColorG || pixels [pixel + 1] == paintColor.g) 
						&& (pixels [pixel + 2] == hitColorB || pixels [pixel + 2] == paintColor.b) 
						&& (pixels [pixel + 3] == hitColorA || pixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0 
						&& (pixels [pixel + 0] == hitColorR || pixels [pixel + 0] == paintColor.r) 
						&& (pixels [pixel + 1] == hitColorG || pixels [pixel + 1] == paintColor.g) 
						&& (pixels [pixel + 2] == hitColorB || pixels [pixel + 2] == paintColor.b) 
						&& (pixels [pixel + 3] == hitColorA || pixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0 
						&& (pixels [pixel + 0] == hitColorR || pixels [pixel + 0] == paintColor.r) 
						&& (pixels [pixel + 1] == hitColorG || pixels [pixel + 1] == paintColor.g) 
						&& (pixels [pixel + 2] == hitColorB || pixels [pixel + 2] == paintColor.b) 
						&& (pixels [pixel + 3] == hitColorA || pixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0 
						&& (pixels [pixel + 0] == hitColorR || pixels [pixel + 0] == paintColor.r) 
						&& (pixels [pixel + 1] == hitColorG || pixels [pixel + 1] == paintColor.g) 
						&& (pixels [pixel + 2] == hitColorB || pixels [pixel + 2] == paintColor.b) 
						&& (pixels [pixel + 3] == hitColorA || pixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						lockMaskPixels [pixel] = 1;
					}
				}
			}
		} // LockAreaFill


		void LockAreaFillMaskOnly (int x, int y)
		{
			byte hitColorR = maskPixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = maskPixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = maskPixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = maskPixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0)
					return;
			}

			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;
			
			lockMaskPixels = new byte[texWidth * texHeight * 4];
			
			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
					
					if (lockMaskPixels [pixel] == 0
						&& (maskPixels [pixel + 0] == hitColorR || maskPixels [pixel + 0] == paintColor.r) 
						&& (maskPixels [pixel + 1] == hitColorG || maskPixels [pixel + 1] == paintColor.g) 
						&& (maskPixels [pixel + 2] == hitColorB || maskPixels [pixel + 2] == paintColor.b) 
						&& (maskPixels [pixel + 3] == hitColorA || maskPixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0 
						&& (maskPixels [pixel + 0] == hitColorR || maskPixels [pixel + 0] == paintColor.r) 
						&& (maskPixels [pixel + 1] == hitColorG || maskPixels [pixel + 1] == paintColor.g) 
						&& (maskPixels [pixel + 2] == hitColorB || maskPixels [pixel + 2] == paintColor.b) 
						&& (maskPixels [pixel + 3] == hitColorA || maskPixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0 
						&& (maskPixels [pixel + 0] == hitColorR || maskPixels [pixel + 0] == paintColor.r) 
						&& (maskPixels [pixel + 1] == hitColorG || maskPixels [pixel + 1] == paintColor.g) 
						&& (maskPixels [pixel + 2] == hitColorB || maskPixels [pixel + 2] == paintColor.b) 
						&& (maskPixels [pixel + 3] == hitColorA || maskPixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0 
						&& (maskPixels [pixel + 0] == hitColorR || maskPixels [pixel + 0] == paintColor.r) 
						&& (maskPixels [pixel + 1] == hitColorG || maskPixels [pixel + 1] == paintColor.g) 
						&& (maskPixels [pixel + 2] == hitColorB || maskPixels [pixel + 2] == paintColor.b) 
						&& (maskPixels [pixel + 3] == hitColorA || maskPixels [pixel + 3] == paintColor.a)) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						lockMaskPixels [pixel] = 1;
					}
				}
			}
		} // LockAreaFillMaskOnly


		// compares if two values are below threshold
		bool CompareThreshold (byte a, byte b)
		{
			//return Mathf.Abs(a-b)<=threshold;
			if (a < b) {
				a ^= b;
				b ^= a;
				a ^= b;
			} // http://lab.polygonal.de/?p=81
			return (a - b) <= paintThreshold;
		}

		// create locking mask floodfill, using threshold, checking pixels from mask only
		void LockAreaFillWithThresholdMaskOnly (int x, int y)
		{
//			Debug.Log("LockAreaFillWithThresholdMaskOnly");
			// get canvas color from this point
			byte hitColorR = maskPixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = maskPixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = maskPixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = maskPixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0)
					return;
			}

			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;
			
			lockMaskPixels = new byte[texWidth * texHeight * 4];
			
			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

					if (lockMaskPixels [pixel] == 0 // this pixel is not used yet
						&& (CompareThreshold (maskPixels [pixel + 0], hitColorR)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (maskPixels [pixel + 1], hitColorG)) 
						&& (CompareThreshold (maskPixels [pixel + 2], hitColorB)) 
						&& (CompareThreshold (maskPixels [pixel + 3], hitColorA))) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0 
						&& (CompareThreshold (maskPixels [pixel + 0], hitColorR)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (maskPixels [pixel + 1], hitColorG)) 
						&& (CompareThreshold (maskPixels [pixel + 2], hitColorB)) 
						&& (CompareThreshold (maskPixels [pixel + 3], hitColorA))) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0 
						&& (CompareThreshold (maskPixels [pixel + 0], hitColorR)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (maskPixels [pixel + 1], hitColorG)) 
						&& (CompareThreshold (maskPixels [pixel + 2], hitColorB)) 
						&& (CompareThreshold (maskPixels [pixel + 3], hitColorA))) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1; 
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0 
						&& (CompareThreshold (maskPixels [pixel + 0], hitColorR)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (maskPixels [pixel + 1], hitColorG)) 
						&& (CompareThreshold (maskPixels [pixel + 2], hitColorB))
						&& (CompareThreshold (maskPixels [pixel + 3], hitColorA))) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						lockMaskPixels [pixel] = 1; 
					}
				}
			}
		} // LockMaskFillWithTreshold



		// create locking mask floodfill, using threshold
		void LockMaskFillWithThreshold (int x, int y)
		{
//			Debug.Log("LockMaskFillWithTreshold");
			// get canvas color from this point
			byte hitColorR = pixels [((texWidth * (y) + x) * 4) + 0];
			byte hitColorG = pixels [((texWidth * (y) + x) * 4) + 1];
			byte hitColorB = pixels [((texWidth * (y) + x) * 4) + 2];
			byte hitColorA = pixels [((texWidth * (y) + x) * 4) + 3];

			if (!canDrawOnBlack) {
				if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0)
					return;
			}

			Queue<int> fillPointX = new Queue<int> ();
			Queue<int> fillPointY = new Queue<int> ();
			fillPointX.Enqueue (x);
			fillPointY.Enqueue (y);
			
			int ptsx, ptsy;
			int pixel = 0;
			
			lockMaskPixels = new byte[texWidth * texHeight * 4];
			
			while (fillPointX.Count > 0) {
				
				ptsx = fillPointX.Dequeue ();
				ptsy = fillPointY.Dequeue ();
				
				if (ptsy - 1 > -1) {
					pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
					
					if (lockMaskPixels [pixel] == 0 // this pixel is not used yet
						&& (CompareThreshold (pixels [pixel + 0], hitColorR) || CompareThreshold (pixels [pixel + 0], paintColor.r)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (pixels [pixel + 1], hitColorG) || CompareThreshold (pixels [pixel + 1], paintColor.g)) 
						&& (CompareThreshold (pixels [pixel + 2], hitColorB) || CompareThreshold (pixels [pixel + 2], paintColor.b)) 
						&& (CompareThreshold (pixels [pixel + 3], hitColorA) || CompareThreshold (pixels [pixel + 3], paintColor.a))) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy - 1);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx + 1 < texWidth) {
					pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
					if (lockMaskPixels [pixel] == 0 
						&& (CompareThreshold (pixels [pixel + 0], hitColorR) || CompareThreshold (pixels [pixel + 0], paintColor.r)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (pixels [pixel + 1], hitColorG) || CompareThreshold (pixels [pixel + 1], paintColor.g)) 
						&& (CompareThreshold (pixels [pixel + 2], hitColorB) || CompareThreshold (pixels [pixel + 2], paintColor.b)) 
						&& (CompareThreshold (pixels [pixel + 3], hitColorA) || CompareThreshold (pixels [pixel + 3], paintColor.a))) {
						fillPointX.Enqueue (ptsx + 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1;
					}
				}
				
				if (ptsx - 1 > -1) {
					pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
					if (lockMaskPixels [pixel] == 0 
						&& (CompareThreshold (pixels [pixel + 0], hitColorR) || CompareThreshold (pixels [pixel + 0], paintColor.r)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (pixels [pixel + 1], hitColorG) || CompareThreshold (pixels [pixel + 1], paintColor.g)) 
						&& (CompareThreshold (pixels [pixel + 2], hitColorB) || CompareThreshold (pixels [pixel + 2], paintColor.b)) 
						&& (CompareThreshold (pixels [pixel + 3], hitColorA) || CompareThreshold (pixels [pixel + 3], paintColor.a))) {
						fillPointX.Enqueue (ptsx - 1);
						fillPointY.Enqueue (ptsy);
						lockMaskPixels [pixel] = 1; 
					}
				}
				
				if (ptsy + 1 < texHeight) {
					pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
					if (lockMaskPixels [pixel] == 0 
						&& (CompareThreshold (pixels [pixel + 0], hitColorR) || CompareThreshold (pixels [pixel + 0], paintColor.r)) // if pixel is same as hit color OR same as paint color
						&& (CompareThreshold (pixels [pixel + 1], hitColorG) || CompareThreshold (pixels [pixel + 1], paintColor.g)) 
						&& (CompareThreshold (pixels [pixel + 2], hitColorB) || CompareThreshold (pixels [pixel + 2], paintColor.b)) 
						&& (CompareThreshold (pixels [pixel + 3], hitColorA) || CompareThreshold (pixels [pixel + 3], paintColor.a))) {
						fillPointX.Enqueue (ptsx);
						fillPointY.Enqueue (ptsy + 1);
						lockMaskPixels [pixel] = 1; 
					}
				}
			}
		} // LockMaskFillWithTreshold

		// get custom brush texture into custombrushpixels array, this needs to be called if custom brush is changed
		public void ReadCurrentCustomBrush ()
		{
			customBrushWidth = customBrushes [selectedBrush].width;
			customBrushHeight = customBrushes [selectedBrush].height;
			customBrushBytes = new byte[customBrushWidth * customBrushHeight * 4];
		
			int pixel = 0;
			for (int y = 0; y < customBrushHeight; y++) {
				for (int x = 0; x < customBrushWidth; x++) {
					// TODO: take colors from GetPixels
					Color brushPixel = customBrushes [selectedBrush].GetPixel (x, y);
					customBrushBytes [pixel] = (byte)(brushPixel.r * (GameManager.Instance.colors[GameManager.Instance.colorIndex].r*255));
					customBrushBytes [pixel + 1] = (byte)(brushPixel.g * (GameManager.Instance.colors[GameManager.Instance.colorIndex].g*255));
					customBrushBytes [pixel + 2] = (byte)(brushPixel.b * (GameManager.Instance.colors[GameManager.Instance.colorIndex].b*255));
					customBrushBytes [pixel + 3] = (byte)(brushPixel.a * 255);
					pixel += 4;
				}
			}
			// precalculate values
			customBrushWidthHalf = (int)(customBrushWidth * 0.5f);
			texWidthMinusCustomBrushWidth = texWidth - customBrushWidth;
			texHeightMinusCustomBrushHeight = texHeight - customBrushHeight;
		}


		// reads current texture pattern into pixel array
		public void ReadCurrentCustomPattern ()
		{
			if (customPatterns == null || customPatterns.Length == 0 || customPatterns [selectedPattern] == null) {
				Debug.LogError ("Problem: No custom patterns assigned on " + gameObject.name);
				return;
			}

			customPatternWidth = customPatterns [selectedPattern].width;
			customPatternHeight = customPatterns [selectedPattern].height;
			patternBrushBytes = new byte[customPatternWidth * customPatternHeight * 4];

			int pixel = 0;
			for (int x = 0; x < customPatternWidth; x++) {
				for (int y = 0; y < customPatternHeight; y++) {
					Color brushPixel = customPatterns [selectedPattern].GetPixel (x, y);


					patternBrushBytes [pixel] = (byte)(brushPixel.r * 255);
					patternBrushBytes [pixel + 1] = (byte)(brushPixel.g * 255);
					patternBrushBytes [pixel + 2] = (byte)(brushPixel.b * 255);
					patternBrushBytes [pixel + 3] = (byte)(brushPixel.a * 255);

					pixel += 4;
				}
			}
		}
		
		// draws single point to this pixel coordinate, with current paint color
		public void DrawPoint (int x, int y)
		{
			int pixel = (texWidth * y + x) * 4;
			pixels [pixel] = paintColor.r;
			pixels [pixel + 1] = paintColor.g;
			pixels [pixel + 2] = paintColor.b;
			pixels [pixel + 3] = paintColor.a;
		}


		// draws single point to this pixel array index, with current paint color
		public void DrawPoint (int pixel)
		{
			pixels [pixel] = paintColor.r;
			pixels [pixel + 1] = paintColor.g;
			pixels [pixel + 2] = paintColor.b;
			pixels [pixel + 3] = paintColor.a;
		}
		
		
		// draw line between 2 points (if moved too far/fast)
		// http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
		public void DrawLine (Vector2 start, Vector2 end)
		{
			int x0 = (int)start.x;
			int y0 = (int)start.y;
			int x1 = (int)end.x;
			int y1 = (int)end.y;
			int dx = Mathf.Abs (x1 - x0); // TODO: try these? http://stackoverflow.com/questions/6114099/fast-integer-abs-function
			int dy = Mathf.Abs (y1 - y0);
			int sx, sy;
			if (x0 < x1) {
				sx = 1;
			} else {
				sx = -1;
			}
			if (y0 < y1) {
				sy = 1;
			} else {
				sy = -1;
			}
			int err = dx - dy;
			bool loop = true;
			//			int minDistance=brushSize-1;
			int minDistance = (int)(brushSize >> 1); // divide by 2, you might want to set mindistance to smaller value, to avoid gaps between brushes when moving fast
			int pixelCount = 0;
			int e2;
			while (loop) {
				pixelCount++;
				if (pixelCount > minDistance) {
					pixelCount = 0;
					DrawCircle (x0, y0);
				}
				if ((x0 == x1) && (y0 == y1))
					loop = false;
				e2 = 2 * err;
				if (e2 > -dy) {
					err = err - dy;
					x0 = x0 + sx;
				}
				if (e2 < dx) {
					err = err + dx;
					y0 = y0 + sy;
				}
			}
		} // drawline

		void DrawLineWithBrush (Vector2 start, Vector2 end)
		{
			int x0 = (int)start.x;
			int y0 = (int)start.y;
			int x1 = (int)end.x;
			int y1 = (int)end.y;
			int dx = Mathf.Abs (x1 - x0); // TODO: try these? http://stackoverflow.com/questions/6114099/fast-integer-abs-function
			int dy = Mathf.Abs (y1 - y0);
			int sx, sy;
			if (x0 < x1) {
				sx = 1;
			} else {
				sx = -1;
			}
			if (y0 < y1) {
				sy = 1;
			} else {
				sy = -1;
			}
			int err = dx - dy;
			bool loop = true;
			//			int minDistance=brushSize-1;
			int minDistance = (int)(brushSize >> 1); // divide by 2, you might want to set mindistance to smaller value, to avoid gaps between brushes when moving fast
			int pixelCount = 0;
			int e2;
			while (loop) {
				pixelCount++;
				if (pixelCount > minDistance) {
					pixelCount = 0;
					DrawCustomBrush (x0, y0);
				}
				if ((x0 == x1) && (y0 == y1))
					loop = false;
				e2 = 2 * err;
				if (e2 > -dy) {
					err = err - dy;
					x0 = x0 + sx;
				}
				if (e2 < dx) {
					err = err + dx;
					y0 = y0 + sy;
				}
			}
		}

		void DrawLineWithPattern (Vector2 start, Vector2 end)
		{
			int x0 = (int)start.x;
			int y0 = (int)start.y;
			int x1 = (int)end.x;
			int y1 = (int)end.y;
			int dx = Mathf.Abs (x1 - x0); // TODO: try these? http://stackoverflow.com/questions/6114099/fast-integer-abs-function
			int dy = Mathf.Abs (y1 - y0);
			int sx, sy;
			if (x0 < x1) {
				sx = 1;
			} else {
				sx = -1;
			}
			if (y0 < y1) {
				sy = 1;
			} else {
				sy = -1;
			}
			int err = dx - dy;
			bool loop = true;
			//			int minDistance=brushSize-1;
			int minDistance = (int)(brushSize >> 1); // divide by 2, you might want to set mindistance to smaller value, to avoid gaps between brushes when moving fast
			int pixelCount = 0;
			int e2;
			while (loop) {
				pixelCount++;
				if (pixelCount > minDistance) {
					pixelCount = 0;
					DrawPatternCircle (x0, y0);
				}
				if ((x0 == x1) && (y0 == y1))
					loop = false;
				e2 = 2 * err;
				if (e2 > -dy) {
					err = err - dy;
					x0 = x0 + sx;
				}
				if (e2 < dx) {
					err = err + dx;
					y0 = y0 + sy;
				}
			}
		}

		/*
		// Bresenham line with custom width, not used yet, still broken
		// TODO: fix this.. http://members.chello.at/~easyfilter/bresenham.html
		void DrawLineWidth(Vector2 start, Vector2 end, float wd)
		//		void DrawLineWidth(int x0, int y0, int x1, int y1, float wd)
		{ 
			int x0=(int)start.x;
			int y0=(int)start.y;
			int x1=(int)end.x;
			int y1=(int)end.y;
			
			int dx = Mathf.Abs(x1-x0), sx = x0 < x1 ? 1 : -1; 
			int dy = Mathf.Abs(y1-y0), sy = y0 < y1 ? 1 : -1; 
			int err = dx-dy, e2, x2, y2;                          // error value e_xy 
			float ed = dx+dy == 0 ? 1 : Mathf.Sqrt((float)dx*dx+(float)dy*dy);
			
			for (wd = (wd+1)/2;;) // pixel loop
			{                                   
				DrawPoint(x0, y0);
				e2 = err;
				x2 = x0;
				if (2*e2 >= -dx) // x step
				{                                           
					for (e2 += dy, y2 = y0; e2 < ed*wd && (y1 != y2 || dx > dy); e2 += dx)
					{
						DrawPoint(x0, y2 += sy);
					}
					if (x0 == x1) break;
					e2 = err; err -= dy; x0 += sx; 
				} 
				
				if (2*e2 <= dy) // y step
				{                                            
					for (e2 = dx-e2; e2 < ed*wd && (x1 != x2 || dx < dy); e2 += dy)
					{
						DrawPoint(x2 += sx, y0);
					}
					if (y0 == y1) break;
					err += dx; y0 += sy; 
				}
			}
		} // DrawLineWidth
		*/


		// Basic undo function, copies original array (before drawing) into the image and applies it
		public void DoUndo ()
		{
			if (undoEnabled) {
				System.Array.Copy (undoPixels, pixels, undoPixels.Length);
				tex.LoadRawTextureData (undoPixels);
				tex.Apply (false);
			}
		}


		// init/clear image, this can be called outside this script also
		public void ClearImage ()
		{

			if (undoEnabled && undoPixels != null) {
				System.Array.Copy (pixels, undoPixels, pixels.Length);
			}


			if (usingClearingImage) {
				ClearImageWithImage ();

			} else {

				int pixel = 0;
				for (int y = 0; y < texHeight; y++) {
					for (int x = 0; x < texWidth; x++) {
						pixels [pixel] = clearColor.r;
						pixels [pixel + 1] = clearColor.g;
						pixels [pixel + 2] = clearColor.b;
						pixels [pixel + 3] = clearColor.a;
						pixel += 4;
					}
				}
				tex.LoadRawTextureData (pixels);
				tex.Apply (false);
			}
		} // clear image


		public void ClearImageWithImage ()
		{
			// fill pixels array with clearpixels array
			System.Array.Copy (clearPixels, 0, pixels, 0, clearPixels.Length);


			// just assign our clear image array into tex
			tex.LoadRawTextureData (clearPixels);
			tex.Apply (false);
		} // clear image


		public void ReadMaskImage ()
		{
			maskPixels = new byte[texWidth * texHeight * 4];

			int pixel = 0;
			for (int y = 0; y < texHeight; y++) {
				for (int x = 0; x < texWidth; x++) {
					Color c = maskTex.GetPixel (x, y);	
					maskPixels [pixel] = (byte)(c.r * 255);
					maskPixels [pixel + 1] = (byte)(c.g * 255);
					maskPixels [pixel + 2] = (byte)(c.b * 255);
					maskPixels [pixel + 3] = (byte)(c.a * 255);
					pixel += 4;
				}
			}

		}

		public void ReadClearingImage ()
		{
			clearPixels = new byte[texWidth * texHeight * 4];

			// get our current texture into tex
			tex.SetPixels32 (((Texture2D)GetComponent<Renderer> ().material.GetTexture (targetTexture)).GetPixels32 ());
			tex.Apply (false);

			int pixel = 0;
			for (int y = 0; y < texHeight; y++) {
				for (int x = 0; x < texWidth; x++) {
					// TODO: use readpixels32
					Color c = tex.GetPixel (x, y);
					
					clearPixels [pixel] = (byte)(c.r * 255);
					clearPixels [pixel + 1] = (byte)(c.g * 255);
					clearPixels [pixel + 2] = (byte)(c.b * 255);
					clearPixels [pixel + 3] = (byte)(c.a * 255);
					pixel += 4;
				}
			}
		}

		void CreateFullScreenQuad ()
		{
			cam = paintCamera;
			// create mesh plane, fits in camera view (with screensize adjust taken into consideration)
			Mesh go_Mesh = GetComponent<MeshFilter> ().mesh;
			go_Mesh.Clear ();
			go_Mesh.vertices = new [] {
				cam.ScreenToWorldPoint (new Vector3 (0, canvasSizeAdjust.y, cam.nearClipPlane + 0.1f)), // bottom left
				cam.ScreenToWorldPoint (new Vector3 (0, cam.pixelHeight + canvasSizeAdjust.y, cam.nearClipPlane + 0.1f)), // top left
				cam.ScreenToWorldPoint (new Vector3 (cam.pixelWidth + canvasSizeAdjust.x, cam.pixelHeight + canvasSizeAdjust.y, cam.nearClipPlane + 0.1f)), // top right
				cam.ScreenToWorldPoint (new Vector3 (cam.pixelWidth + canvasSizeAdjust.x, canvasSizeAdjust.y, cam.nearClipPlane + 0.1f)) // bottom right
			};
			go_Mesh.uv = new [] {
				new Vector2 (0, 0),
				new Vector2 (0, 1),
				new Vector2 (1, 1),
				new Vector2 (1, 0)
			};
			go_Mesh.triangles = new  [] {0, 1, 2, 0, 2, 3};

			// TODO: add option for this
			go_Mesh.RecalculateNormals ();

			// TODO: add option to calculate tangents
			go_Mesh.tangents = new [] {
				new Vector4 (1.0f, 0.0f, 0.0f, -1.0f),
				new Vector4 (1.0f, 0.0f, 0.0f, -1.0f),
				new Vector4 (1.0f, 0.0f, 0.0f, -1.0f),
				new Vector4 (1.0f, 0.0f, 0.0f, -1.0f)
			};


			// add mesh collider
			gameObject.AddComponent<MeshCollider> ();
		}

		public void SetBrushSize (int newSize)
		{
			// no validation is done, should be always bigger than 1
			Debug.Log (newSize);
			brushSize = newSize;
		}

		public void SetDrawModeBrush ()
		{
			drawMode = DrawMode.Default;
		}

		public void SetDrawModeFill ()
		{
			drawMode = DrawMode.FloodFill;
		}

		public void SetDrawModeShapes ()
		{
			drawMode = DrawMode.CustomBrush;
		}

		public void SetDrawModePattern ()
		{
			drawMode = DrawMode.Pattern;
		}

		// returns current image (later: including all layers) as Texture2D
		public Texture2D GetCanvasAsTexture ()
		{
			var image = new Texture2D ((int)(texWidth / resolutionScaler), (int)(texHeight / resolutionScaler), TextureFormat.RGBA32, false);

			// TODO: combine layers to single texture

			image.LoadRawTextureData (pixels);
			image.Apply (false);
			return image;
		}


		// returns screenshot as Texture2D
		public Texture2D GetScreenshot ()
		{
			HideUI ();
			paintCamera.Render (); // force render to hide UI
			var image = new Texture2D ((int)(texWidth / resolutionScaler), (int)(texHeight / resolutionScaler), TextureFormat.ARGB32, false);
			image.ReadPixels (new Rect (0, 0, image.width, image.height), 0, 0);
			image.Apply (false);
			ShowUI ();
			return image;
		}

	} // class
} // namespace
