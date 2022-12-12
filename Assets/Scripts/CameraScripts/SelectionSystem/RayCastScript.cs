using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RayCastScript : MonoBehaviour
{
    public GameObject menuObj;

    private MenuController menu;
    private GameObject currentSelectedObject;


    private SelectionController dataDictionary;
    LayerMask mask;
    LayerMask mask_ui;

    Vector3 p1;
    Vector3 p2;
    bool dragSelect;
    RaycastHit hit;

    //the vertices of our meshcollider
    Vector3[] verts;
    Vector3[] vecs;
    Vector2[] corners;
    Mesh selectionMesh;
    MeshCollider selectionBox;

    public float timeAllowedToChain = 10000;
    float lastPressCtrl = 0.0f;
    private void Start()
    {
        //menu = menuObj.GetComponent<MenuController>();

        dataDictionary = gameObject.GetComponent<SelectionController>();
        //menuObj.SetActive(false);
    }
    void Update()
    {
        //1. when left mouse button clicked (but not released)
        if (Input.GetMouseButtonDown(0))
        {
            p1 = Input.mousePosition;
        }

        //2. while left mouse button held
        if (Input.GetMouseButton(0))
        {
            if ((p1 - Input.mousePosition).magnitude > 40)
            {
                dragSelect = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (dragSelect == false) //single select
            {
                mask = LayerMask.GetMask("Buildings");
                mask_ui = LayerMask.GetMask("UI");

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

                foreach (RaycastResult r in results)
                {
                    if (r.gameObject.layer != mask_ui)
                    {
                        return;
                    }

                }
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask | mask_ui))
                {
                    if (Input.GetKey(KeyCode.LeftShift)) //inclusive select
                    {
                        dataDictionary.addSelected(hit.transform.gameObject);
                    }
                    else
                    {
                        dataDictionary.deselectAll();
                        dataDictionary.addSelected(hit.transform.gameObject);
                    }
                }
                else
                {
                    dataDictionary.deselectAll();
                }
            }
            else
            {
                verts = new Vector3[4];
                vecs = new Vector3[4];
                int i = 0;
                p2 = Input.mousePosition;
                corners = getBoundingBox(p1, p2);
                foreach (Vector2 corner in corners)
                {
                    Ray ray = Camera.main.ScreenPointToRay(corner);
                    LayerMask groundMask = LayerMask.GetMask("Ground");
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                    {
                        verts[i] = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                        vecs[i] = ray.origin - hit.point;
                        Debug.DrawLine(Camera.main.ScreenToWorldPoint(corner), hit.point, Color.red, 1.0f);
                    }
                    i++;
                }

                //generate the mesh
                selectionMesh = generateSelectionMesh(verts, vecs);

                selectionBox = gameObject.AddComponent<MeshCollider>();
                selectionBox.sharedMesh = selectionMesh;
                selectionBox.convex = true;
                selectionBox.isTrigger = true;

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    dataDictionary.deselectAll();
                }

                Destroy(selectionBox, 0.02f);

            }//end marquee select

            dragSelect = false;
            
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            lastPressCtrl = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.Z) && (Time.time <= (lastPressCtrl + timeAllowedToChain)))
        {
            //dataDictionary.Undo();
        }
    }
    private void OnGUI()
    {
        if (dragSelect == true)
        {
            var rect = Utils.GetScreenRect(p1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    //create a bounding box (4 corners in order) from the start and end mouse position
    Vector2[] getBoundingBox(Vector2 p1, Vector2 p2)
    {
        // Min and Max to get 2 corners of rectangle regardless of drag direction.
        var bottomLeft = Vector3.Min(p1, p2);
        var topRight = Vector3.Max(p1, p2);

        // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
        Vector2[] corners =
        {
            new Vector2(bottomLeft.x, topRight.y),
            new Vector2(topRight.x, topRight.y),
            new Vector2(bottomLeft.x, bottomLeft.y),
            new Vector2(topRight.x, bottomLeft.y)
        };
        return corners;

    }

    //generate a mesh from the 4 bottom points
    Mesh generateSelectionMesh(Vector3[] corners, Vector3[] vecs)
    {
        Vector3[] verts = new Vector3[8];
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 }; //map the tris of our cube

        for (int i = 0; i < 4; i++)
        {
            verts[i] = corners[i];
        }

        for (int j = 4; j < 8; j++)
        {
            verts[j] = corners[j - 4] + vecs[j - 4];
        }

        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = verts;
        selectionMesh.triangles = tris;

        return selectionMesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        dataDictionary.addSelected(other.gameObject);
    }

}
