using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish {
    static List<Fish> fishies = new List<Fish>();
    static float wCentering;
    static float wMatching;
    static float wAvoidance;
    static float wWandering;
    static float worldBoxSize;

    static float rCentering = 10;
    static float rAvoidance = 4;

    bool ignoreme;
    float ignoreStart;

    List<GameObject> trail;
    GameObject obj;
    Vector3 velocity;

    public Fish(GameObject obj) {
        this.obj = obj;
        this.velocity = Vector3.zero;
        this.trail = new List<GameObject>();
        fishies.Add(this);
    }

    public static void initConsts(float centering, float matching, float avoidance, float wandering, float worldBoxSize) {
        Fish.wCentering = centering;
        Fish.wMatching = matching;
        Fish.wAvoidance = avoidance;
        Fish.wWandering = wandering;
        Fish.worldBoxSize = worldBoxSize;
    }

    public void Scatter() {
        obj.transform.position = new Vector3(-1 * worldBoxSize + 2 * worldBoxSize * UnityEngine.Random.value, -1 * worldBoxSize + 2 * worldBoxSize * UnityEngine.Random.value, -1 * worldBoxSize + 2 * worldBoxSize * UnityEngine.Random.value);
        velocity = Vector3.zero;
    }

    public void Update(float dt, bool trails, int trailLength, bool useCentering, bool useMatching, bool useAvoidance, bool useWandering) {
        float centerWeightSum = -1;
        float collisionWeightSum = -1;
        Vector3 flockCentering = Vector3.zero;
        Vector3 velocityMatching = Vector3.zero;
        Vector3 collisionAvoidance = Vector3.zero;
        Vector3 wandering = new Vector3(-1 + 2 * UnityEngine.Random.value, -1 + 2 * UnityEngine.Random.value, -1 + 2 * UnityEngine.Random.value);

        // social patterns
        foreach (Fish fish in fishies) {
            if (fish.getPosition() == getPosition() || fish.ignored() || ignored()) continue;
            float dist = Vector3.Distance(getPosition(), fish.getPosition());
            float fishWeight = 1f / (dist * dist + 0.1f);
            if (dist < rCentering) {
                if (centerWeightSum == -1) centerWeightSum = 0;
                centerWeightSum += fishWeight;
                flockCentering += fishWeight * (fish.getPosition() - getPosition());
                velocityMatching += fishWeight * (fish.getVelocity() - getVelocity());
                if (dist < rAvoidance) {
                    if (collisionWeightSum == -1) collisionWeightSum = 0;
                    collisionWeightSum += fishWeight;
                    collisionAvoidance += fishWeight * (getPosition() - fish.getPosition());
                }
            }
        }
        Vector3 totalForces = (useCentering ? wCentering : 0) * flockCentering / centerWeightSum + (useMatching ? wMatching : 0) * velocityMatching / centerWeightSum + (useAvoidance ? wAvoidance : 0) * collisionAvoidance / collisionWeightSum + (useWandering || velocity.magnitude == 0 ? wWandering : 0) * wandering;
        velocity = Vector3.ClampMagnitude(velocity + dt * totalForces, 14);
        if (velocity.magnitude < 6) velocity = velocity * 6 / velocity.magnitude;
        
        // boundary behavior
        if (ignoreme && Time.time >= ignoreStart + 1) ignoreme = false;
        if (obj.transform.position.x > worldBoxSize && velocity.x > 0 || obj.transform.position.x < -1 * worldBoxSize && velocity.x < 0) {
            velocity.x *= -1;
            ignoreme = true;
            ignoreStart = Time.time;
        } else if (obj.transform.position.y > worldBoxSize && velocity.y > 0 || obj.transform.position.y < -1 * worldBoxSize && velocity.y < 0) {
            velocity.y *= -1;
            ignoreme = true;
            ignoreStart = Time.time;
        } else if (obj.transform.position.z > worldBoxSize && velocity.z > 0 || obj.transform.position.z < -1 * worldBoxSize && velocity.z < 0) {
            velocity.z *= -1;
            ignoreme = true;
            ignoreStart = Time.time;
        }

        // obstacle avoidance
        if (Physics.Raycast(obj.transform.position, velocity, 14f)) {
            Vector3 tan = velocity.normalized;
            Vector3 norm = Vector3.up;
            Vector3 binorm = Vector3.right;
            Vector3.OrthoNormalize(ref tan, ref norm, ref binorm);
            for (float n = 0.1f; n < 1; n += 0.1f) {
                if (!Physics.Raycast(obj.transform.position, tan + n * binorm, 14f)) {
                    velocity = (tan + (n + 0.1f) * binorm) * velocity.magnitude;
                    break;
                } 
                if (!Physics.Raycast(obj.transform.position, tan + -1 * n * norm, 14f)) {
                    velocity = (tan + -1 * (n + 0.1f) * norm) * velocity.magnitude;
                    break;
                } 
                if (!Physics.Raycast(obj.transform.position, tan + -1 * n * binorm, 14f)) {
                    velocity = (tan + -1 * (n + 0.1f) * binorm) * velocity.magnitude;
                    break;
                } 
                if (!Physics.Raycast(obj.transform.position, tan + n * norm, 14f)) {
                    velocity = (tan + (n + 0.1f) * norm) * velocity.magnitude;
                    break;
                } 
            }
        }

        // trail creation
        if (trails) {
            trail.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            trail[trail.Count - 1].transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
            if (trail.Count > trailLength) {
                UnityEngine.Object.Destroy(trail[0]);
                trail.RemoveAt(0);
            }
        } if (!trails) {
            while (trail.Count > 0) {
                UnityEngine.Object.Destroy(trail[0]);
                trail.RemoveAt(0);
            }
        }

        obj.transform.position += dt * velocity;
        obj.transform.rotation = Quaternion.LookRotation(-1 * velocity);
    }

    public void Unalive() {
        fishies.Remove(this);
        while (trail.Count > 0) {
            UnityEngine.Object.Destroy(trail[0]);
            trail.RemoveAt(0);
        }
    }

    public bool ignored() {
        return ignoreme;
    }

    public Vector3 getPosition() {
        return obj.transform.position;
    }

    public Vector3 getVelocity() {
        return velocity;
    }

    public GameObject getObject() {
        return obj;
    }
}