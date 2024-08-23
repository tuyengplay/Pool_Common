using System.Collections.Generic;
using UnityEngine;
namespace GameCore.Slicer2D
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D))]
    public class SlicedSprite : MonoBehaviour
    {
        public MeshRenderer MeshRenderer { get { return m_MeshRenderer; } }
        public Vector2 MinCoords { get { return m_MinCoords; } }
        public Vector2 MaxCoords { get { return m_MaxCoords; } }
        public Bounds SpriteBounds { get { return m_SpriteBounds; } }
        public int ParentInstanceID { get { return m_ParentInstanceID; } }
        public int CutsSinceParentObject { get { return m_CutsSinceParentObject; } }
        public bool Rotated { get { return m_Rotated; } }
        public bool HFlipped { get { return m_HFlipped; } }
        public bool VFlipped { get { return m_VFlipped; } }

        MeshRenderer m_MeshRenderer;
        MeshFilter m_MeshFilter;
        Transform m_Transform;

        Vector2 m_MinCoords;
        Vector2 m_MaxCoords;
        Vector2 m_Centroid;
        Vector2 m_UVOffset;
        Bounds m_SpriteBounds;
        int m_ParentInstanceID;
        int m_CutsSinceParentObject;
        bool m_Rotated;
        bool m_VFlipped;
        bool m_HFlipped;

        /// <summary>
        /// Called when the object is created
        /// </summary>
        void Awake()
        {
            m_Transform = transform;
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_ParentInstanceID = gameObject.GetInstanceID();
            m_MinCoords = new Vector2(0.0f, 0.0f);
            m_MaxCoords = new Vector2(1.0f, 1.0f);

            if (m_MeshFilter.mesh)
            {
                m_SpriteBounds = m_MeshFilter.mesh.bounds;
            }
        }
        /// <summary>
        /// Initialise this sliced sprite using an existing SlicedSprite
        /// </summary>
        public void InitFromSlicedSprite(SlicedSprite slicedSprite, ref PolygonCollider2D polygon, ref Vector2[] polygonPoints, bool isConcave)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            slicedSprite.m_MeshRenderer.GetPropertyBlock(block);
            m_MeshRenderer.SetPropertyBlock(block);

            InitSprite(slicedSprite.gameObject, slicedSprite.MeshRenderer, ref polygon, ref polygonPoints, slicedSprite.MinCoords, slicedSprite.MaxCoords, slicedSprite.SpriteBounds, slicedSprite.m_MeshRenderer.sharedMaterial, slicedSprite.Rotated, slicedSprite.HFlipped, slicedSprite.VFlipped, slicedSprite.m_Centroid, slicedSprite.m_UVOffset, isConcave);
            m_ParentInstanceID = slicedSprite.GetInstanceID();
            m_CutsSinceParentObject = slicedSprite.CutsSinceParentObject + 1;
        }

        /// <summary>
        /// Initialise using a unity sprite
        /// </summary>
        public void InitFromUnitySprite(SpriteRenderer unitySprite, ref PolygonCollider2D polygon, ref Vector2[] polygonPoints, bool isConcave)
        {
            Sprite sprite = unitySprite.sprite;

            Bounds bounds = unitySprite.bounds;
            Vector2 position = unitySprite.transform.position;
            Vector2 min = bounds.min;
            Vector2 max = bounds.max;
            Vector2 size = bounds.size;
            Vector2 offsetOfAbsolutePositionRelativelyToMinOfBounds = Vector2.zero;
            Vector3 lossyScale = unitySprite.transform.lossyScale;

            // Adjust pivot position calculation if sprite has been flipped on the x axis
            if (Mathf.Sign(lossyScale.x) < 0.0f)
            {
                offsetOfAbsolutePositionRelativelyToMinOfBounds.x = max.x - position.x;
            }
            else
            {
                offsetOfAbsolutePositionRelativelyToMinOfBounds.x = position.x - min.x;
            }

            // Adjust pivot position calculation if sprite has been flipped on the y axis
            if (Mathf.Sign(lossyScale.y) < 0.0f)
            {
                offsetOfAbsolutePositionRelativelyToMinOfBounds.y = max.y - position.y;
            }
            else
            {
                offsetOfAbsolutePositionRelativelyToMinOfBounds.y = position.y - min.y;
            }

            Vector2 pivotVector = new Vector2(
                    offsetOfAbsolutePositionRelativelyToMinOfBounds.x / size.x,
                    offsetOfAbsolutePositionRelativelyToMinOfBounds.y / size.y
                    );

            pivotVector -= new Vector2(0.5f, 0.5f);

            Texture2D spriteTexture = sprite.texture;
            Vector2 textureSize = new Vector2(spriteTexture.width, spriteTexture.height);

            Material material = unitySprite.sharedMaterial;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture("_MainTex", spriteTexture);
            m_MeshRenderer.SetPropertyBlock(block);

            Rect textureRect = sprite.textureRect;
            Vector2 minTextureCoords = new Vector2(textureRect.xMin / (float)textureSize.x, textureRect.yMin / (float)textureSize.y);
            Vector2 maxTextureCoords = new Vector2(textureRect.xMax / (float)textureSize.x, textureRect.yMax / (float)textureSize.y);

            InitSprite(unitySprite.gameObject, unitySprite.GetComponent<Renderer>(), ref polygon, ref polygonPoints, minTextureCoords, maxTextureCoords, unitySprite.sprite.bounds, material, false, false, false, Vector2.zero, pivotVector, isConcave);
            m_ParentInstanceID = unitySprite.gameObject.GetInstanceID();
        }

        /// <summary>
        /// Initialise this sprite using the given polygon definition
        /// </summary>
        void InitSprite(GameObject parentObject, Renderer parentRenderer, ref PolygonCollider2D polygon, ref Vector2[] polygonPoints, Vector3 minCoords, Vector3 maxCoords, Bounds spriteBounds, Material material, bool rotated, bool hFlipped, bool vFlipped, Vector2 parentCentroid, Vector2 uvOffset, bool isConcave)
        {
            m_MinCoords = minCoords;
            m_MaxCoords = maxCoords;
            m_SpriteBounds = spriteBounds;
            m_VFlipped = vFlipped;
            m_HFlipped = hFlipped;
            m_Rotated = rotated;
            m_SpriteBounds = spriteBounds;
            m_UVOffset = uvOffset;

            gameObject.tag = parentObject.tag;
            gameObject.layer = parentObject.layer;

            Mesh spriteMesh = new Mesh();
            spriteMesh.name = "SlicedSpriteMesh";
            m_MeshFilter.mesh = spriteMesh;

            int numVertices = polygonPoints.Length;
            Vector3[] vertices = new Vector3[numVertices];
            Color[] colors = new Color[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] triangles;

            // Convert vector2 -> vector3
            for (int loop = 0; loop < vertices.Length; loop++)
            {
                vertices[loop] = polygonPoints[loop];
                colors[loop] = Color.white;
            }

            Vector2 uvWidth = maxCoords - minCoords;
            Vector3 boundsSize = spriteBounds.size;
            Vector2 invBoundsSize = new Vector2(1.0f / boundsSize.x, 1.0f / boundsSize.y);

            for (int vertexIndex = 0; vertexIndex < numVertices; vertexIndex++)
            {
                Vector2 vertex = polygonPoints[vertexIndex] + parentCentroid;
                float widthFraction = 0.5f + ((vertex.x * invBoundsSize.x) + (uvOffset.x));
                float heightFraction = 0.5f + ((vertex.y * invBoundsSize.y) + (uvOffset.y));

                if (hFlipped)
                {
                    widthFraction = 1.0f - widthFraction;
                }

                if (vFlipped)
                {
                    heightFraction = 1.0f - heightFraction;
                }

                Vector2 texCoords = new Vector2();

                if (rotated)
                {
                    texCoords.y = maxCoords.y - (uvWidth.y * (1.0f - widthFraction));
                    texCoords.x = minCoords.x + (uvWidth.x * heightFraction);
                }
                else
                {
                    texCoords.x = minCoords.x + (uvWidth.x * widthFraction);
                    texCoords.y = minCoords.y + (uvWidth.y * heightFraction);
                }

                uvs[vertexIndex] = texCoords;
            }

            if (isConcave)
            {
                List<Vector2> polyPointList = new List<Vector2>(polygonPoints);
                triangles = SpriteSlicer2D.Triangulate(ref polyPointList);
            }
            else
            {
                int triangleIndex = 0;
                triangles = new int[numVertices * 3];

                for (int vertexIndex = 1; vertexIndex < numVertices - 1; vertexIndex++)
                {
                    triangles[triangleIndex++] = 0;
                    triangles[triangleIndex++] = vertexIndex + 1;
                    triangles[triangleIndex++] = vertexIndex;
                }
            }

            spriteMesh.Clear();
            spriteMesh.vertices = vertices;
            spriteMesh.uv = uvs;
            spriteMesh.triangles = triangles;
            spriteMesh.colors = colors;
            spriteMesh.RecalculateBounds();
            spriteMesh.RecalculateNormals();
            ;

            Vector2 localCentroid = Vector3.zero;

            if (SpriteSlicer2D.s_CentreChildSprites)
            {
                localCentroid = spriteMesh.bounds.center;

                // Finally, fix up our mesh, collider, and object position to at the same position as the pivot point
                for (int vertexIndex = 0; vertexIndex < numVertices; vertexIndex++)
                {
                    vertices[vertexIndex] -= (Vector3)localCentroid;
                }

                for (int vertexIndex = 0; vertexIndex < numVertices; vertexIndex++)
                {
                    polygonPoints[vertexIndex] -= localCentroid;
                }

                m_Centroid = localCentroid + parentCentroid;
                polygon.points = polygonPoints;
                spriteMesh.vertices = vertices;
                spriteMesh.RecalculateBounds();
            }

            Transform parentTransform = parentObject.transform;
            m_Transform.parent = parentTransform.parent;
            m_Transform.position = parentTransform.position + (parentTransform.rotation * (Vector3)localCentroid);
            m_Transform.rotation = parentTransform.rotation;
            m_Transform.localScale = parentTransform.localScale;
            m_MeshRenderer.material = material;

            m_MeshRenderer.sortingLayerID = parentRenderer.sortingLayerID;
            m_MeshRenderer.sortingOrder = parentRenderer.sortingOrder;
        }
    }
    [System.Serializable]
    public class SpriteSlicer2DSliceInfo
    {
        public GameObject SlicedObject { get; set; }
        public Vector2 SliceEnterWorldPosition { get; set; }
        public Vector2 SliceExitWorldPosition { get; set; }
        public List<GameObject> ChildObjects { get { return m_ChildObjects; } set { m_ChildObjects = value; } }

        List<GameObject> m_ChildObjects = new List<GameObject>();
    }
}