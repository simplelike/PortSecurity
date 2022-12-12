using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
using UnityEditor;

public class QuarterContent : MonoBehaviour
{
    public bool isCriticZone = false;
    public bool isSelectable = true;

    private HashSet<GameObject> innerlements = new HashSet<GameObject>();
    private CriticZonesController cr_zone_controller;
    void Awake()
    {
        cr_zone_controller = GameObject.FindGameObjectWithTag("EventSystem").GetComponentInChildren<CriticZonesController>();
    }

    public void makeChildObjectsSelectable()
    {
        if (isSelectable)
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                if (child.GetComponent<MeshFilter>())
                {
                    BoxCollider collider;
                    if (child.GetComponent<BoxCollider>() == null)
                    {
                        child.gameObject.AddComponent<BoxCollider>();
                    }
                    collider = child.GetComponent<BoxCollider>();
                    collider.size = new Vector3(collider.size.x * 0.9f, collider.size.y * 0.9f, collider.size.z * 0.9f);
                    child.gameObject.layer = 3;
                    var outline_c = child.gameObject.AddComponent<Outline>();
                    outline_c.enabled = false;

                    var rg = child.gameObject.AddComponent<Rigidbody>();
                    rg.angularDrag = 100;
                    rg.mass = 100;
                    rg.drag = 100;

                    innerlements.Add(child.gameObject);
                }
            }
            if (isCriticZone)
            {
                cr_zone_controller.addCriticElements(innerlements);
            }
        }
    }
}
