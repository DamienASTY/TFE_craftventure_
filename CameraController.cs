using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset;
    public float distance = 100.0f;
    public float maxDistance = 200;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public int zoomRate = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;
    private Vector3 position;

    void Start() { Init(); }

    // Initialisation de la caméra
    public void Init()
    {
        GameObject go = new GameObject("Fake Cam Target");
        go.transform.position = transform.position + (transform.forward * distance);
        target = go.transform;

        distance = Vector3.Distance(transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;

        // Assurer de récupérer les rotations actuelles comme points de départ.
        position = transform.position;
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
    }

    /*
     * Logique de la caméra dans LateUpdate pour mettre à jour uniquement après la gestion de tous les mouvements des personnages.
     */
    void LateUpdate()
    {
        // Si Control et Alt et le bouton du milieu sont enfoncés, ZOOM !
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
        {
            desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
        }
        // Si le bouton du milieu et Alt gauche sont sélectionnés, ORBITER
        else if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt))
        {
            xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            ////////Angle d'orbite

            // Limiter l'axe vertical de l'orbite
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            // Définir la rotation de la caméra
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            transform.rotation = rotation;
        }
        // Bouton gauche de la souris et touche Q, nous effectuons un panoramique en transformant la cible dans l'espace écran
        else if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.Q))
        {
            // Récupérer la rotation de la caméra pour pouvoir se déplacer dans un espace XY local pseudo
            target.rotation = transform.rotation;
            target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
            target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
        }

        ////////Position de l'orbite

        // Affecter la distance de zoom désirée si nous faisons défiler la molette de défilement
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        // Limiter le zoom min/max
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        // Pour un lissage du zoom, effectuer une interpolation de distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

        // Calculer la position en fonction de la nouvelle currentDistance
        position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}