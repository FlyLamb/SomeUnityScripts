/*
    A basic, probably glitchy, not really optymised Physics Player Movement script.
    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)
    This script may not be the best, I might improve it in the future, but I just needed to get it working and I've decided to share it in case someone needs a quck solution.
    If for some weird reason you decide to use this script feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.
    How to setup:
    Create an object (preferably an empty), move it into a separate layer.
    Drag script onto an object
    Right click the script name in the inspector
    Select [Setup Prefab] from the menu.
    Play around with the settings, make sure to disable the player layer from collision check.
    WARNING:
    If you change the player's height, you'll have to manually configure the `normalCheckerHeigh`, as it's not adjusted yet. To get the value, place the player somewhere and run the game. Disable the stair functionality, and
    when the playerfalls and touches ground, right click the script and select [Get Normal Checker Height]. Paste the value you got in the console to the `normalCheckerHeigh`.
    You have to handle the rotation yourself. 
    There are a few bugs that i know of and i'll list them here:
      BUG                 | DESCRIPTION                                                                          | VERDICT
    - acceleration issues | after landing, switching the directions quickly may result in getting a decent boost | Not gonna fix, seems cool for speedrunning like some of the bugs in HL2
    - edge jump issues    |  sometimes on edges the player might jump                                            | not fixed yet
    - issues with stairs  | it's hard to get on the first step on stairs                                         | fixed
    - stair bobbing       | the player jumps hard on stairs                                                      | probably its config dependent, dunno how to fix
*/


using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BajtixPlayerController : MonoBehaviour {

    [Header("Basic Settings")]
    public float force = 300;
    public float speed = 4;
    public float jumpForce = 700;
    [Tooltip("What is treated as ground")]
    public LayerMask groundMask;
    [Tooltip("List of objects, which's positions will be used for ground checking.")]
    public Transform[] groundCheckers;
    public Transform stairChecker;
    public bool enableStairs = true;

    [Header("Advanced Fancy Stuff")]
    public float normalSteepPoint = 0.4f;
    public float rampAdjustForce = 500f;
    public float airControl = 0.1f;

    public float stepHeight = 0.8f;
    public float downPushForce = 15;
    public float normalCheckerHeight = 0.9603f;
    public float stairLookahead = 16;

    [Tooltip("This curve determines the material friction and the ramp slide force that the object will encounter. It should decrease after a certain value.")]
    public AnimationCurve angleFriction = new AnimationCurve(new Keyframe[] {
        new Keyframe(0f, 0.7f),
        new Keyframe(0.4f, 0.7f),
        new Keyframe(0.5f, 0f)}); // the default curve



    private PhysicMaterial frictionMaterial;
    private Collider feet;
    private Rigidbody rb;
    private Vector3 feetPosition;
    private float stepCheckY;
    private Vector3 smoothedVelocity;

    [Space]
    [Header("Display-only")]
    public bool isGrounded = true;
    public float avgNormal;

    private float jumpCooldown = 0.2f;

    [ContextMenu("Setup prefab")]
    void CreateBaseSetup() {
        SphereCollider sc = GetComponent<SphereCollider>();
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        Rigidbody bd = GetComponent<Rigidbody>();

        cc.radius = 0.4f;
        cc.height = 2;
        cc.isTrigger = false;

        sc.center = Vector3.down * 0.62f;
        sc.radius = 0.42f;

        bd.constraints = RigidbodyConstraints.FreezeRotation;
        bd.mass = 2;
        bd.drag = 0.01f;
        bd.interpolation = RigidbodyInterpolation.Interpolate;
        bd.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Transform po = new GameObject("Checkers").transform;
        po.SetParent(transform);
        po.localPosition = new Vector3(0, -0.783f, 0);

        // STUPID ASS WAY OF DOING IT! THERE SOULD BE A LOOP HERE OR SOMETHIN... 
        groundCheckers = new Transform[9];

        var g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0, -0.017f, 0);
        groundCheckers[0] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(-0.4f, -0.017f, 0);
        groundCheckers[1] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0.4f, -0.017f, 0);
        groundCheckers[2] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0f, -0.017f, -0.4f);
        groundCheckers[3] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0f, -0.017f, 0.4f);
        groundCheckers[4] = g.transform;


        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(-0.32f, -0.017f, -0.32f);
        groundCheckers[5] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0.32f, -0.017f, -0.32f);
        groundCheckers[6] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(-0.32f, -0.017f, 0.32f);
        groundCheckers[7] = g.transform;

        g = new GameObject("Ground Checker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0.32f, -0.017f, 0.32f);
        groundCheckers[8] = g.transform;

        g = new GameObject("StairChecker");
        g.transform.SetParent(po);
        g.transform.localPosition = new Vector3(0, 0.9f, 0);
        stairChecker = g.transform;
    }

    [ContextMenu("Get Normal Checker Height ")]
    void GetNormalCheckerHeight() {
        if (!isGrounded) { Debug.LogError("Not grounded!"); return; }
        stairChecker.localPosition = transform.InverseTransformDirection(SkipY(smoothedVelocity * Time.fixedDeltaTime * stairLookahead) + stepCheckY * Vector3.up);
        RaycastHit hit;
        if (Physics.Raycast(stairChecker.position, Vector3.down, out hit, normalCheckerHeight + 0.5f, groundMask, QueryTriggerInteraction.Ignore)) {
            var w = hit.distance - normalCheckerHeight;
            Debug.Log("Hit distance: " + hit.distance);

        } else {
            Debug.LogError("The ground checkers did not hit! Do you have the layer mask set up correctly?");
        }
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        feet = GetComponent<SphereCollider>();
        feetPosition = GetComponent<CapsuleCollider>().height / 2 * Vector3.down;
        stepCheckY = stairChecker.localPosition.y;

        //Create and setup the material
        frictionMaterial = new PhysicMaterial("Player Instance " + Random.Range(0, 10000)) { frictionCombine = PhysicMaterialCombine.Minimum };
        feet.material = frictionMaterial;
    }



    // TODO: Maybe some cleanup?

    void FixedUpdate() {
        isGrounded = CheckGroundClearence();
        // THE INPUT IS DONE HERE
        Motors(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        var jump = Input.GetButton("Jump");



        // Friction and ramps

        avgNormal = GetAverageFeetNormal();
        float estimatedFriction = EstimateFrictionForNormal(avgNormal);
        frictionMaterial.staticFriction = frictionMaterial.dynamicFriction = isGrounded ? estimatedFriction : 0f;

        if (avgNormal > normalSteepPoint) {
            DoRampForces(estimatedFriction);
        }

        if (enableStairs) StairWalking();

        // Jumping

        if (jumpCooldown > 0) jumpCooldown -= Time.fixedDeltaTime;

        if (jump && jumpCooldown <= 0) Jump(); else if (isGrounded) rb.AddForce(Vector3.down * downPushForce);
    }

    ///<summary>Manages the downwards force that causes the player to slide down</summary>
    void DoRampForces(float frictionEst) {
        var nrml = GetAverageFeetNormalVector3();
        nrml.y = -nrml.y;
        rb.AddForce(nrml * (1 - frictionEst) * rampAdjustForce);
    }

    ///<summary>Step height</summary>
    void StairWalking() {
        if (!isGrounded) return;
        stairChecker.localPosition = transform.InverseTransformDirection(SkipY(smoothedVelocity * Time.fixedDeltaTime * stairLookahead) + stepCheckY * Vector3.up);
        RaycastHit hit;
        if (Physics.Raycast(stairChecker.position, Vector3.down, out hit, normalCheckerHeight + 0.5f, groundMask, QueryTriggerInteraction.Ignore)) {
            var w = hit.distance - normalCheckerHeight;
            //Debug.Log("Hit distance: " + hit.distance + "; Diff: " + (hit.distance - normalCheckerHeight));
            if (w < -0.01 && w > -stepHeight && Vector3.Angle(Vector3.up, hit.normal) < normalSteepPoint * 45) {
                rb.MovePosition(transform.position - (hit.distance - normalCheckerHeight) * Vector3.up * 0.4f);
                // Debug.Log("Step!");
            }
        }

    }

    ///<summary>Tries to jump if possible</summary>
    void Jump() {
        if (isGrounded) JustJump();
        jumpCooldown = 0.2f;
    }

    ///<summary>Jumps. Self explainatory</summary>
    void JustJump() {
        rb.AddForce(Vector3.up * jumpForce);
    }

    bool CheckGroundClearence() {
        bool g = false;
        foreach (var w in groundCheckers)
            g |= Physics.Raycast(w.position, Vector3.down, 0.3f, groundMask, QueryTriggerInteraction.Ignore);

        return g;
    }

    ///<summary>Does the movement stand</summary>
    void Motors(float x, float y) {
        Vector3 input = new Vector3(x, 0, y);
        input = Vector3.ClampMagnitude(input, 1);

        Vector3 forceDirection = transform.TransformDirection(input);
        float velocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z); // pythagoras theorem
        float forceCoefficient = Mathf.Clamp01(speed - velocity); // we calculate it so it is gradual, used for the player not to exceed the speed too much
        Vector3 velocityDirection = transform.InverseTransformDirection(rb.velocity);

        if (velocityDirection.magnitude > 0.2f && isGrounded) {

            velocityDirection = Vector3.ClampMagnitude(velocityDirection, 1);

            // ignore the y components in further calculations
            velocityDirection = SkipY(velocityDirection);
            input = SkipY(input);

            Vector3 correctionForce = transform.TransformDirection(input - velocityDirection);
            rb.AddForce(correctionForce * force / 5); // this force is there to handle qucik direction changes

            // DEBUG ONLY VECTOR DRAWING
            // Debug.DrawRay(transform.position, input, Color.black, Time.fixedDeltaTime);
            // Debug.DrawRay(transform.position, velocityDirection, Color.blue, Time.fixedDeltaTime);
            // Debug.DrawRay(transform.position, correctionForce, Color.red, Time.fixedDeltaTime);     
        }
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, rb.velocity * 0.5f + forceDirection * 2, Time.fixedDeltaTime * 20);
        if (!isGrounded || avgNormal > normalSteepPoint) {
            if (IsRoughlyOpposite(forceDirection.normalized, rb.velocity.normalized) || SkipY(rb.velocity).magnitude < 0.5f)
                rb.AddForce(forceDirection * force * airControl); // this makes it so it is easier to change direction midair
            else
                rb.AddForce(forceDirection * force * airControl * 0.05f); // air control
        } else {
            rb.AddForce(forceDirection * force * forceCoefficient);
        }
    }

    ///<summary>Checks if the vectors are roughly opposite</summary>
    bool IsRoughlyOpposite(Vector3 a, Vector3 b) {
        return Vector3.Angle(a, b) > 90;
    }

    ///<summary>Ignores the Y component of vec3</summary>
    Vector3 SkipY(Vector3 vector) {
        vector.y = 0;
        return vector;
    }

    float EstimateFrictionForNormal(float normal) {
        return angleFriction.Evaluate(normal);
    }

    float GetAverageFeetNormal() {
        return Mathf.Clamp01(Vector3.Angle(Vector3.up, GetAverageFeetNormalVector3()) / 90f);
    }

    Vector3 GetAverageFeetNormalVector3() {
        Vector3 normal = Vector3.zero;
        Vector3 feetPosition = this.feetPosition + transform.position;
        foreach (var w in groundCheckers) {
            RaycastHit hit;
            if (Physics.Raycast(w.position, Vector3.down, out hit, 0.3f, groundMask, QueryTriggerInteraction.Ignore)) {
                normal += hit.normal;
            }
        }

        return normal / groundCheckers.Length;
    }
}
