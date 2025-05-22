using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class SoundZoneManager : MonoBehaviour
{
    //script where audio clip is assigned
    [SerializeField] private MonoBehaviour targetScript;
    
    //within this zone audio clip is audible
    private float soundZoneRadius;

    private Camera mainCamera;
    private FieldInfo playSoundField;

    void Start()
    {
        //circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;

        if (targetScript != null)
        {
            //pull variable soundZoneRadius value from target script
            var field = targetScript.GetType().GetField("soundZoneRadius");

            if (field != null)
            {
                soundZoneRadius = (float)field.GetValue(targetScript);
            }

            //assign variable playSound value from target script to playSoundField
            playSoundField = targetScript.GetType().GetField("playSound");
        }
    }

    void Update()
    {
        if (targetScript == null || mainCamera == null || playSoundField == null) return;

        float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);

        playSoundField.SetValue(targetScript, distanceToCamera <= soundZoneRadius);
    }
}
