using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishGenerator : MonoBehaviour
{
    int steps = 10;
    [SerializeField]
    int seed = 2793;
    [SerializeField]
    Texture2D scaleTexture;

    [SerializeField]
    int numFish = 100;
    [SerializeField]
    bool useTrails = false;
    [SerializeField]
    int trailLength = 100;
    [SerializeField]
    bool useCentering = true;
    [SerializeField]
    float centeringWeight = 2f;
    [SerializeField]
    bool useMatching = true;
    [SerializeField]
    float matchingWeight = 1.9f;
    [SerializeField]
    bool useAvoidance = true;
    [SerializeField]
    float avoidanceWeight = 5f;
    [SerializeField]
    bool useWandering = true;
    [SerializeField]
    float wanderingWeight = 40.67f;
    
    [SerializeField]
    float worldBoxSize = 35;

    float[][] blendConsts;

    Vector3 eyePos;
    Vector3 sideFinPos;
    float tailPos;

    List<GameObject> rightFins;
    List<GameObject> leftFins;
    List<GameObject> tails;
    List<Fish> fish;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState(seed);
        initBlendConsts();
        Fish.initConsts(centeringWeight, matchingWeight, avoidanceWeight, wanderingWeight, worldBoxSize);

        rightFins = new List<GameObject>();
        leftFins = new List<GameObject>();
        tails = new List<GameObject>();
        fish = new List<Fish>();

        for (int i = 0; i < numFish; i++) {
            fish.Add(new Fish(GenerateFish(new Vector3(0, 0, 0))));
        }
    }

    // Update is called once per frame
    void Update()
    {
        float space = Input.GetAxis("Jump");

        if (fish.Count < numFish) {
            for (int i = fish.Count; i < numFish; i++) {
                print("making feesh");
                fish.Add(new Fish(GenerateFish(new Vector3(0, 0, 0))));
            }
        } else if (fish.Count > numFish) {
            Debug.Log(fish.Count);
            for (int i = numFish; i < fish.Count; i++) {
                GameObject f = fish[numFish].getObject();
                rightFins.Remove(f.transform.GetChild(4).gameObject);
                leftFins.Remove(f.transform.GetChild(5).gameObject);
                tails.Remove(f.transform.GetChild(6).gameObject);
                Destroy(f);
                fish[numFish].Unalive();
                fish.RemoveAt(numFish);
            }
        }
        // animations
        foreach (GameObject fin in rightFins) fin.transform.RotateAround(fin.transform.position, Vector3.up, Time.deltaTime * ((int) Mathf.Floor(Time.time) % 2 == 0 ? -40 : 40));
        foreach (GameObject fin in leftFins) fin.transform.RotateAround(fin.transform.position, Vector3.up, Time.deltaTime * ((int) Mathf.Floor(Time.time) % 2 == 0 ? 40 : -40));
        foreach (GameObject tail in tails) tail.transform.RotateAround(tail.transform.position, Vector3.up, Time.deltaTime * (((int) Mathf.Floor(Time.time) - 1) % 4 > 1 || Time.time < 1 ? -20 : 20));
        
        // fish movement
        foreach (Fish f in fish) {
            if (space == 1) f.Scatter();
            f.Update(Time.deltaTime, useTrails, trailLength, useCentering, useMatching, useAvoidance, useWandering);
        }
    }

    void initBlendConsts() {
        blendConsts = new float[][]{new float[steps + 1], new float[steps + 1], new float[steps + 1], new float[steps + 1]};
        for (int i = 0; i <= steps; i++) {
            float u = ((float) i)/steps;
            float temp = 1.0f - u;

            blendConsts[0][i] = temp * temp * temp;
            blendConsts[1][i] = 3 * u * temp * temp;
            blendConsts[2][i] = 3 * u * u * temp;
            blendConsts[3][i] = u * u * u;
        }
    }

    GameObject GenerateFish(Vector3 position) {
        GameObject fish = new GameObject("Fish");
        Color fishColor = new Color(0.2f + 0.5f * UnityEngine.Random.value, 0.2f + 0.5f * UnityEngine.Random.value, 0.2f + 0.5f * UnityEngine.Random.value);
        fish.AddComponent<MeshFilter>();
        fish.AddComponent<MeshRenderer>();
        fish.GetComponent<MeshFilter>().mesh = GenerateFishBody();
        fish.GetComponent<Renderer>().material.color = fishColor;
        fish.GetComponent<Renderer>().material.EnableKeyword("_NORMALMAP");
        fish.GetComponent<Renderer>().material.SetTexture("_BumpMap", scaleTexture);
        fish.transform.position = position;
        
        Color eyeColor = new Color(0.8f + 0.2f * UnityEngine.Random.value, 0.8f + 0.2f * UnityEngine.Random.value, 0.8f + 0.2f * UnityEngine.Random.value);
        GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.transform.SetParent(fish.transform);
        rightEye.transform.localScale = new Vector3(0.1f, 0.2f, 0.2f);
        rightEye.transform.localPosition = new Vector3(eyePos.x, eyePos.y, eyePos.z);
        rightEye.GetComponent<Renderer>().material.color = eyeColor;
        GameObject rightIris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightIris.transform.SetParent(fish.transform);
        rightIris.transform.localScale = new Vector3(0.12f, 0.18f, 0.18f);
        rightIris.transform.localPosition = new Vector3(eyePos.x, eyePos.y, eyePos.z);
        rightIris.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.transform.SetParent(fish.transform);
        leftEye.transform.localScale = new Vector3(0.1f, 0.2f, 0.2f);
        leftEye.transform.localPosition = new Vector3(-1 * eyePos.x, eyePos.y, eyePos.z);
        rightEye.GetComponent<Renderer>().material.color = eyeColor;
        GameObject leftIris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftIris.transform.SetParent(fish.transform);
        leftIris.transform.localScale = new Vector3(0.12f, 0.18f, 0.18f);
        leftIris.transform.localPosition = new Vector3(-1 * eyePos.x, eyePos.y, eyePos.z);
        leftIris.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        
        int finType = (int) Mathf.Floor(UnityEngine.Random.value * 3);
        float finScale = 0.8f + 0.6f * UnityEngine.Random.value;
        Color finColor = new Color(fishColor.r - 0.1f + 0.2f * UnityEngine.Random.value, fishColor.g - 0.1f + 0.2f * UnityEngine.Random.value, fishColor.b - 0.1f + 0.2f * UnityEngine.Random.value, 0.3f + 0.5f * UnityEngine.Random.value);
        GameObject rightFin = new GameObject("Right Fin");
        rightFin.AddComponent<MeshFilter>();
        rightFin.AddComponent<MeshRenderer>();
        rightFin.GetComponent<MeshFilter>().mesh = GenerateSideFin(1, finType);
        rightFin.transform.SetParent(fish.transform);
        rightFin.transform.localScale = new Vector3(finScale, finScale, finScale);
        rightFin.transform.localPosition = sideFinPos;
        rightFin.GetComponent<Renderer>().material.color = finColor;
        rightFin.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0);
        rightFins.Add(rightFin);
        GameObject leftFin = new GameObject("Left Fin");
        leftFin.AddComponent<MeshFilter>();
        leftFin.AddComponent<MeshRenderer>();
        leftFin.GetComponent<MeshFilter>().mesh = GenerateSideFin(-1, finType);
        leftFin.transform.SetParent(fish.transform);
        leftFin.transform.localScale = new Vector3(finScale, finScale, finScale);
        leftFin.transform.localPosition = new Vector3(-1 * sideFinPos.x, sideFinPos.y, sideFinPos.z);
        leftFin.GetComponent<Renderer>().material.color = finColor;
        leftFin.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0);
        leftFins.Add(leftFin);

        float tailScale = 0.7f + 0.4f * UnityEngine.Random.value;
        GameObject tail = new GameObject("Tail");
        tail.AddComponent<MeshFilter>();
        tail.AddComponent<MeshRenderer>();
        tail.GetComponent<MeshFilter>().mesh = GenerateTail((int) Mathf.Floor(UnityEngine.Random.value * 3));
        tail.transform.SetParent(fish.transform);
        tail.transform.localScale = new Vector3(tailScale, tailScale, tailScale);
        tail.transform.localPosition = new Vector3(0, 0, tailPos);
        tail.GetComponent<Renderer>().material.color = finColor;
        tail.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0);
        tails.Add(tail);

        return fish;
    }

    Mesh GenerateTail(int type) {
        Vector3[][] crescentTailControl = new Vector3[][]{
            new Vector3[]{new Vector3(0f, 0f, -0.3f), new Vector3(0f, 0f, -0.3f), new Vector3(0f, 0f, -0.3f), new Vector3(0f, 0f, -0.3f)},
            new Vector3[]{new Vector3(0f, -0.5f, -0.1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0.5f, 0f)},
            new Vector3[]{new Vector3(0f, -0.9f, 0.2f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0.9f, 0.3f)},
            new Vector3[]{new Vector3(0f, -1.5f, 1f), new Vector3(0f, -0.3f, 0.3f), new Vector3(0f, 0.3f, 0.3f), new Vector3(0f, 1.5f, 1f)}};
        Vector3[][] longTailControl = new Vector3[][]{
            new Vector3[]{new Vector3(0f, -0.3f, -0.6f), new Vector3(0f, 0f, -0.6f), new Vector3(0f, 0f, -0.6f), new Vector3(0f, 0.3f, -0.6f)},
            new Vector3[]{new Vector3(0f, -0.55f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0.55f, 0f)},
            new Vector3[]{new Vector3(0f, -1f, 1.2f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 1.2f)},
            new Vector3[]{new Vector3(0f, -0.8f, 1.05f), new Vector3(0f, 0f, 0.7f), new Vector3(0f, 0.1f, 0.7f), new Vector3(0f, 0.8f, 1.05f)}};
        Vector3[][] shortTailControl = new Vector3[][]{
            new Vector3[]{new Vector3(0f, -0.3f, -0.6f), new Vector3(0f, 0f, -0.6f), new Vector3(0f, 0f, -0.6f), new Vector3(0f, 0.3f, -0.6f)},
            new Vector3[]{new Vector3(0f, -0.45f, 0.15f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0.45f, 0.05f)},
            new Vector3[]{new Vector3(0f, -1.1f, 0.85f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1.3f, 0.85f)},
            new Vector3[]{new Vector3(0f, -0.8f, 0.73f), new Vector3(0f, -0.2f, 0.97f), new Vector3(0f, 0.1f, 1.03f), new Vector3(0f, 0.8f, 0.67f)}};
        
        return GenerateBezierPatch(type == 0 ? crescentTailControl : (type == 1 ? longTailControl : shortTailControl));
    }

    // pass right = 1 for right fin, right = -1 for left fin
    // pass type = 0 for long fin, type = 1 for short fin, type = 2 for pointed fin
    Mesh GenerateSideFin(int right, int type) {
        Vector3[][] longFinControl = new Vector3[][]{
            new Vector3[]{new Vector3(right * 0.1f, -0.15f, 0), new Vector3(right * 0.1f, -0.1f, 0), new Vector3(right * 0.1f, 0.1f, 0), new Vector3(right * 0.1f, 0.15f, 0)},
            new Vector3[]{new Vector3(right * -0.2f, -0.25f, 0.3f), new Vector3(right * -0.2f, -0.1f, 0.3f), new Vector3(right * -0.2f, 0.1f, 0.3f), new Vector3(right * -0.2f, 0.25f, 0.3f)},
            new Vector3[]{new Vector3(right * -0.35f, -0.4f, 1), new Vector3(right * -0.45f, -0.1f, 1), new Vector3(right * -0.4f, 0.1f, 1), new Vector3(right * -0.4f, 0.35f, 1)},
            new Vector3[]{new Vector3(right * -0.45f, -0.3f, 1.2f), new Vector3(right * -0.5f, -0.1f, 1.5f), new Vector3(right * -0.45f, 0.2f, 1.35f), new Vector3(right * -0.5f, 0.28f, 1.45f)}};
        Vector3[][] shortFinControl = new Vector3[][]{
            new Vector3[]{new Vector3(right * 0.1f, -0.1f, 0), new Vector3(right * 0.1f, -0.1f, 0), new Vector3(right * 0.1f, 0.1f, 0), new Vector3(right * 0.1f, 0.1f, 0)},
            new Vector3[]{new Vector3(right * -0.1f, -0.3f, 0.2f), new Vector3(right * -0.1f, -0.1f, 0.2f), new Vector3(right * -0.1f, 0.1f, 0.2f), new Vector3(right * -0.1f, 0.25f, 0.2f)},
            new Vector3[]{new Vector3(right * -0.25f, -0.55f, 0.55f), new Vector3(right * -0.35f, -0.1f, 0.54f), new Vector3(right * -0.33f, 0.1f, 0.6f), new Vector3(right * -0.4f, 0.55f, 0.7f)},
            new Vector3[]{new Vector3(right * -0.35f, -0.45f, 0.65f), new Vector3(right * -0.38f, -0.15f, 0.7f), new Vector3(right * -0.38f, 0.1f, 0.7f), new Vector3(right * -0.35f, 0.33f, 0.65f)}};
        Vector3[][] pointedFinControl = new Vector3[][]{
            new Vector3[]{new Vector3(right * 0.1f, 0f, 0.2f), new Vector3(right * 0.1f, 0.1f, 0.19f), new Vector3(right * 0.1f, 0.19f, 0.1f), new Vector3(right * 0.1f, 0.2f, 0f)},
            new Vector3[]{new Vector3(right * -0.1f, 0.03f, 0.4f), new Vector3(right * -0.13f, 0.1f, 0.4f), new Vector3(right * -0.15f, 0.1f, 0.4f), new Vector3(right * -0.1f, 0.4f, 0.35f)},
            new Vector3[]{new Vector3(right * -0.15f, 0.14f, 0.65f), new Vector3(right * -0.19f, 0.1f, 0.65f), new Vector3(right * -0.19f, 0.1f, 0.65f), new Vector3(right * -0.16f, 0.31f, 0.65f)},
            new Vector3[]{new Vector3(right * -0.25f, 0.25f, 0.85f), new Vector3(right * -0.25f, 0.25f, 0.85f), new Vector3(right * -0.25f, 0.25f, 0.85f), new Vector3(right * -0.25f, 0.25f, 0.85f)}};

        return GenerateBezierPatch(type == 0 ? longFinControl : (type == 1 ? shortFinControl : pointedFinControl));
    }

    Mesh GenerateFishBody() {
        float widthScale = UnityEngine.Random.value * 0.8f + 0.4f;
        float lengthScale = UnityEngine.Random.value * 0.7f + 0.7f;
        float upperScale = UnityEngine.Random.value * 1.1f + 0.4f;
        float lowerScale = UnityEngine.Random.value * 0.5f + 0.8f;

        Vector3[][] upperBodyControl = new Vector3[][]{
            new Vector3[]{new Vector3(widthScale * -0.25f, 0, 0), new Vector3(widthScale * -0.2f, upperScale * 0.1f, -0.1f), new Vector3(widthScale * 0.2f, upperScale * 0.1f, -0.1f), new Vector3(widthScale * 0.25f, 0, 0)}, 
            new Vector3[]{new Vector3(widthScale * -0.8f, 0, lengthScale * 0.9f), new Vector3(widthScale * -0.2f, upperScale * 2.1f, lengthScale * 0.9f), new Vector3(widthScale * 0.2f, upperScale * 2.1f, lengthScale * 0.9f), new Vector3(widthScale * 0.8f, 0, lengthScale * 0.9f)}, 
            new Vector3[]{new Vector3(widthScale * -0.7f, 0, lengthScale * 2.4f), new Vector3(widthScale * -0.2f, upperScale * 1.9f, lengthScale * 2.8f), new Vector3(widthScale * 0.2f, upperScale * 1.9f, lengthScale * 2.8f), new Vector3(widthScale * 0.7f, 0, lengthScale * 2.4f)}, 
            new Vector3[]{new Vector3(0, 0, lengthScale * 4.1f), new Vector3(0, 0, lengthScale * 4.1f), new Vector3(0, 0, lengthScale * 4.1f), new Vector3(0, 0, lengthScale * 4.1f)}};
        Vector3[][] lowerBodyControl = new Vector3[][]{
            new Vector3[]{new Vector3(widthScale * -0.25f, 0, 0), new Vector3(widthScale * -0.2f, lowerScale * -0.1f, -0.1f), new Vector3(widthScale * 0.2f, lowerScale * -0.1f, -0.1f), new Vector3(widthScale * 0.25f, 0, 0)}, 
            new Vector3[]{new Vector3(widthScale * -0.8f, 0, lengthScale * 0.9f), new Vector3(widthScale * -0.3f, lowerScale * -1.8f, lengthScale * 0.8f), new Vector3(widthScale * 0.3f, lowerScale * -1.8f, lengthScale * 0.8f), new Vector3(widthScale * 0.8f, 0, lengthScale * 0.9f)}, 
            new Vector3[]{new Vector3(widthScale * -0.7f, 0, lengthScale * 2.4f), new Vector3(widthScale * -0.2f, lowerScale * -1.6f, lengthScale * 2.6f), new Vector3(widthScale * 0.2f, lowerScale * -1.6f, lengthScale * 2.6f), new Vector3(widthScale * 0.7f, 0, lengthScale * 2.4f)}, 
            new Vector3[]{new Vector3(0, 0, lengthScale * 4.1f), new Vector3(0, 0, lengthScale * 4.1f), new Vector3(0, 0, lengthScale * 4.1f), new Vector3(0, 0, lengthScale * 4.1f)}};
        tailPos = lengthScale * 4.1f;

        CombineInstance[] body = new CombineInstance[2];
        body[0].mesh = GenerateBezierPatch(lowerBodyControl);
        body[1].mesh = GenerateBezierPatch(upperBodyControl, true);
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(body, true, false);

        return mesh;
    }

    // 4x4 array of control points as input
    Mesh GenerateBezierPatch(Vector3[][] controlPoints) {
        return GenerateBezierPatch(controlPoints, false);
    }

    Mesh GenerateBezierPatch(Vector3[][] controlPoints, bool saveEyePos) {
        Vector3[] verts = new Vector3[(steps + 1) * (steps + 1)];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[steps * steps * 6 * 2];
        int finPos = (int) Mathf.Floor(3 + UnityEngine.Random.value * 2);

        for (int u = 0; u <= steps; u++) {
            Vector3[] tempControlPoints = new Vector3[4];
            tempControlPoints[0] = blendPoints(controlPoints[0], u);
            tempControlPoints[1] = blendPoints(controlPoints[1], u);
            tempControlPoints[2] = blendPoints(controlPoints[2], u);
            tempControlPoints[3] = blendPoints(controlPoints[3], u);

            for (int v = 0; v <= steps; v++) {
                verts[u * (steps + 1) + v] = blendPoints(tempControlPoints, v);
                uvs[u * (steps + 1) + v] = new Vector2(verts[u * (steps + 1) + v].x, verts[u * (steps + 1) + v].z);
                if (saveEyePos && v == 2 && u == 1) eyePos = verts[u * (steps + 1) + v];
                if (saveEyePos && u == 0 && v == finPos) sideFinPos = verts[u * (steps + 1) + v];
                if (u > 0 && v > 0) {
                    tris[((u - 1) * (steps) + v - 1) * 12] = (u - 1) * (steps + 1) + v - 1;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 1] = u * (steps + 1) + v;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 2] = u * (steps + 1) + v - 1;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 3] = (u - 1) * (steps + 1) + v - 1;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 4] = (u - 1) * (steps + 1) + v;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 5] = u * (steps + 1) + v;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 6] = (u - 1) * (steps + 1) + v - 1;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 7] = u * (steps + 1) + v - 1;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 8] = u * (steps + 1) + v;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 9] = (u - 1) * (steps + 1) + v - 1;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 10] = u * (steps + 1) + v;
                    tris[((u - 1) * (steps) + v - 1) * 12 + 11] = (u - 1) * (steps + 1) + v;
                }
            }
        }

        Mesh patchMesh = new Mesh();
        patchMesh.vertices = verts;
        patchMesh.triangles = tris;
        patchMesh.uv = uvs;
        return patchMesh;
    }

    Vector3 blendPoints(Vector3[] points, int u) {
        return blendConsts[0][u] * points[0] + blendConsts[1][u] * points[1] + blendConsts[2][u] * points[2] + blendConsts[3][u] * points[3];
    }
}
