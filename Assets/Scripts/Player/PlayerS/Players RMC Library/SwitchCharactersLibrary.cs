using UnityEngine;
using RMC.Player_Interaction.ThePlayerGeneralManager;

public class SwitchCharactersLibrary : MonoBehaviour
{
    #region Dependency

    //implementacion estandarizada, este script no es mio, la ifnrastructura es enorme debido a que reducirlo implica cambiar cosas en especifico con el control manager
    //debido a esto este scritp es a si de complejo por naturaleza y gran aprte de hacerlo es trabajo hormiga
    [Header ("dependencias")]
    [SerializeField] private  Arkhalis_Movement_Manager BloodManager_M; // Arkhalis
    [SerializeField] private  Ariamis_Movement_Manager MagicManager_M; // Ariamis
    [SerializeField] PlayerStats Pstats;
    [SerializeField] private Input_Manager InputsActions; 
    [SerializeField] private PlayerManager Pmanager; 
    [SerializeField] private GameObject Blood_Character;
    [SerializeField] private GameObject Magic_Character; 
    [Range (1f,25f), SerializeField] private float DistanceBetweenCharactersXorControled = 5f; // es que tan lejos queda el personaje

    [Header ("Teletransporte")]
    [SerializeField] private float TeleportDistance = 20f; // si el seguidor queda mas lejos que esto, reaparece detras del activo

    [Header ("Movimiento del seguidor")]
    [SerializeField] private float FollowerAcceleration = 40f;     // que tan rapido acelera y frena en X
    [SerializeField] private float FollowerArriveSmoothing = 4f;   // frena al acercarse al objetivo (mas alto = frena mas tarde)

    [Header ("Gravedad del seguidor")]
    [SerializeField] private float FollowerGroundCheckDistance = 0.05f; // que tan abajo busca piso cuando esta parado
    [SerializeField] private float FollowerFeetWidthMult = 0.95f;       // ancho de los rayos de piso respecto al FeetCoil
    [SerializeField] private float FollowerStepHeight = 0.4f;           // escalon o pendiente que sube caminando sin saltar

    [Header ("Saltos del seguidor")]
    [Range (1f, 100f), SerializeField] private float FollowerJumpHeightMult = 1.3f; // altura de salto del seguidor respecto a jumpHitgh (subelo si aun no llega)
    [SerializeField] private float NoFloorCheckDistance = 3f;      // si no hay piso a esta distancia hacia abajo, busca plataforma
    [SerializeField] private float PlatformSearchInterval = 0.15f; // cada cuanto repite la busqueda mientras cae
    [SerializeField] private float PlatformSearchStep = 0.5f;      // separacion entre columnas que revisa
    [SerializeField] private float PlatformEdgeMargin = 0.3f;      // la plataforma debe seguir a esta distancia a cada lado (evita bordes justos)
    [SerializeField] private float JumpApexMargin = 0.4f;          // cuanto sube por encima de la plataforma antes de bajar
    [Range (0.5f, 1f), SerializeField] private float JumpReachSafety = 0.9f; // usa solo este porcentaje del alcance horizontal teorico
    [SerializeField] private float PlannedJumpMinSpeed = 4f;       // velocidad horizontal minima en un salto hacia plataforma
    [SerializeField] private float MinProgressToJump = 0.5f;       // cuanto debe acercarlo al objetivo una plataforma para valer la pena

    [Header ("Debug")]
    [SerializeField] private bool DebugFollower = false;           // imprime por que el seguidor esta bloqueado y el estado fisico al cambiar de personaje
    #endregion

    #region Variables
    private struct JumpPlan
    {
        public float vy, vx, x, y;
        public bool needsAirJump;
    }

    // -1 = el seguidor esta a la izquierda del activo, +1 = a la derecha
    private int _followSide = -1;
    private bool _lastAriamisActive;
    private bool _sideInitialized = false;

    // rigidbodies: el seguidor va Kinematic y el activo Dynamic
    private Rigidbody2D _bloodRb;
    private Rigidbody2D _magicRb;
    private RigidbodyInterpolation2D _bloodInterp;
    private RigidbodyInterpolation2D _magicInterp;

    // fisica manual del seguidor
    private float _followerVelX;
    private float _followerVelY;
    private bool _followerGrounded;
    private Vector2 _followerGoal; // punto al que quiere llegar: detras del activo, a la altura de sus pies

    // saltos
    private bool _followerPlannedJump;
    private float _followerPlannedVelX;
    private float _plannedTargetX;
    private float _plannedTargetY;
    private bool _pendingAirJump;     // salto planeado de dos tiempos: el segundo se hace en el apice
    private int _followerJumpsUsed;
    private float _platformSearchT;
    #endregion

    #region Unity Update API

    void Awake()
    {
        _bloodRb = Blood_Character.GetComponent<Rigidbody2D>();
        _magicRb = Magic_Character.GetComponent<Rigidbody2D>();

        if (_bloodRb == null || _magicRb == null)
        {
            Debug.LogError("SwitchCharactersLibrary: Blood_Character y Magic_Character necesitan un Rigidbody2D en el mismo objeto.");
            return;
        }

        // guarda la interpolacion original para devolverla cuando vuelvan a ser el activo
        _bloodInterp = _bloodRb.interpolation;
        _magicInterp = _magicRb.interpolation;
    }

    void Update()
    {
        _SwitchControlCharacter();
    }

    void FixedUpdate()
    {
        if (!(Pmanager._IsArkhalisActive ^ Pmanager._IsAriamisActive))
        {
            Debug.Log("Non Character is active");
            return;
        }

        followPlayer();
    }

    #endregion

    #region Characterfollow
    void followPlayer()
    {
        bool ariamisActive = Pmanager._IsAriamisActive;

        GameObject active   = ariamisActive ? Magic_Character : Blood_Character;
        GameObject follower = ariamisActive ? Blood_Character : Magic_Character;

        Movement_Manager_Abstract activeM   = ariamisActive ? (Movement_Manager_Abstract)MagicManager_M : BloodManager_M;
        Movement_Manager_Abstract followerM = ariamisActive ? (Movement_Manager_Abstract)BloodManager_M : MagicManager_M;

        float speed;
        if (ariamisActive)
        {
            speed = Input_Manager.OnRun ? Pstats._Arkhais_RunSpeed : Pstats._Arkhalis_Speed;
        }
        else
        {
            speed = Input_Manager.OnRun ? Pstats._Ariamis_RunSpeed : Pstats.Ariamis_Speed;
        }

        // al iniciar o al cambiar de personaje, el lado se toma de donde esten parados ahora
        if (!_sideInitialized || ariamisActive != _lastAriamisActive)
        {
            bool firstTime = !_sideInitialized;

            _followSide = follower.transform.position.x >= active.transform.position.x ? 1 : -1;
            _lastAriamisActive = ariamisActive;
            _sideInitialized = true;
            ResetFollowerState();

            // los cambios posteriores ya aplican la fisica en _SwitchControlCharacter, antes del siguiente FixedUpdate
            if (firstTime)
            {
                SetFollowerPhysics(ariamisActive);
            }
        }

        // punto al que quiere llegar el seguidor
        int desiredSide = activeM._IsFacingR ? -1 : 1;
        _followerGoal = new Vector2(
            active.transform.position.x + desiredSide * DistanceBetweenCharactersXorControled,
            activeM._FeetCoil.bounds.min.y);

        // demasiado lejos: reaparece detras del activo (solo cuando el activo esta en el suelo)
        float distance = Vector2.Distance(active.transform.position, follower.transform.position);
        if (distance > TeleportDistance && activeM._Ghit.collider != null)
        {
            TeleportFollowerBehindActive(active, follower, activeM, followerM, desiredSide);
            return;
        }

        // gravedad, piso y busqueda de plataforma: corre siempre
        ApplyFollowerVertical(follower, followerM);

        // segundo salto de un salto doble planeado, en el apice
        if (_pendingAirJump && _followerVelY <= 0f)
        {
            _pendingAirJump = false;
            TryAirJumpToTarget(followerM);
        }

        // salto hacia plataforma: avanza hacia el punto de aterrizaje
        if (_followerPlannedJump)
        {
            float speedX = Mathf.Max(Mathf.Abs(_followerPlannedVelX), PlannedJumpMinSpeed);
            float currentX = follower.transform.position.x;
            float newX = Mathf.MoveTowards(currentX, _plannedTargetX, speedX * Time.fixedDeltaTime);
            MoveFollowerX(follower, followerM, newX - currentX);
            _followerVelX = 0f;
            return;
        }

        FollowTargetX(active, follower, followerM, activeM._IsFacingR, speed);
    }

    void ResetFollowerState()
    {
        _followerVelX = 0f;
        _followerVelY = 0f;
        _followerPlannedJump = false;
        _pendingAirJump = false;
        _followerJumpsUsed = 0;
    }

    // el seguidor se mueve por transform, asi que va Kinematic y sin interpolacion; el activo vuelve a Dynamic con estado limpio
    void SetFollowerPhysics(bool ariamisActive)
    {
        Rigidbody2D activeRb   = ariamisActive ? _magicRb : _bloodRb;
        Rigidbody2D followerRb = ariamisActive ? _bloodRb : _magicRb;
        Movement_Manager_Abstract activeM = ariamisActive ? (Movement_Manager_Abstract)MagicManager_M : BloodManager_M;

        if (activeRb == null || followerRb == null) return;

        followerRb.linearVelocity = Vector2.zero;
        followerRb.angularVelocity = 0f;
        followerRb.bodyType = RigidbodyType2D.Kinematic;
        followerRb.interpolation = RigidbodyInterpolation2D.None;

        activeRb.bodyType = RigidbodyType2D.Dynamic;
        activeRb.interpolation = ariamisActive ? _magicInterp : _bloodInterp;
        activeRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        activeRb.angularVelocity = 0f;

        // limpia saltos, dash, borde agarrado y temporizadores que haya dejado la ultima vez
        activeM.KillMovement();
        Physics2D.SyncTransforms();

        if (DebugFollower)
        {
            Debug.Log($"Cambio de personaje | activo={(ariamisActive ? "Ariamis" : "Arkhalis")} bodyType={activeRb.bodyType} | seguidor bodyType={followerRb.bodyType}");
        }
    }

    void FollowTargetX(GameObject active, GameObject follower, Movement_Manager_Abstract followerM, bool activeFacingRight, float speed)
    {
        float activeX   = active.transform.position.x;
        float followerX = follower.transform.position.x;

        // el seguidor debe ir detras del activo: si mira a la derecha, detras es la izquierda (-1)
        int desiredSide = activeFacingRight ? -1 : 1;

        float targetX;

        if (desiredSide == _followSide)
        {
            // sigue normal, detras del activo
            targetX = activeX + _followSide * DistanceBetweenCharactersXorControled;
        }
        else
        {
            // el activo cambio de direccion: el seguidor se queda quieto
            targetX = followerX;

            // cuando el activo ya lo paso, recien ahi cambia de lado
            bool activePassedFollower = (_followSide == -1) ? activeX <= followerX : activeX >= followerX;

            if (activePassedFollower)
            {
                _followSide = desiredSide;
            }
        }

        // acelera y frena en vez de saltar a la velocidad maxima de golpe
        float toTarget = targetX - followerX;
        float desiredVel = Mathf.Clamp(toTarget * FollowerArriveSmoothing, -speed, speed);
        _followerVelX = Mathf.MoveTowards(_followerVelX, desiredVel, FollowerAcceleration * Time.fixedDeltaTime);

        float step = _followerVelX * Time.fixedDeltaTime;
        bool blocked = MoveFollowerX(follower, followerM, step);

        if (!blocked) return;

        _followerVelX = 0f;

        // choco con una pared en el suelo: busca la plataforma que mas lo acerque al activo; si no hay, salta recto
        if (_followerGrounded)
        {
            int dir = toTarget >= 0f ? 1 : -1;

            if (!TryPlanJumpToPlatform(followerM, dir, FollowerStepHeight, true))
            {
                if (DebugFollower)
                {
                    Debug.Log("Seguidor: pared sin plataforma alcanzable, salto recto");
                }

                StartFollowerJump(MaxJumpSpeed(followerM.Mstas), 0f, followerX, 0f, false, false);
            }
        }
    }

    // mueve en X sin atravesar paredes; devuelve true si hay una pared justo adelante
    bool MoveFollowerX(GameObject follower, Movement_Manager_Abstract followerM, float deltaX)
    {
        if (Mathf.Abs(deltaX) < 0.0001f) return false;

        Collider2D body = followerM._BodyCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        float dir = Mathf.Sign(deltaX);
        float dist = Mathf.Abs(deltaX);

        // en el suelo la caja sube para no chocar con escalones ni pendientes; en el aire solo deja libres los pies
        float clearance = _followerGrounded ? FollowerStepHeight + 0.05f : 0.1f;
        float height = Mathf.Max(body.bounds.size.y - clearance, 0.1f);

        Vector2 size   = new Vector2(body.bounds.size.x, height);
        Vector2 origin = new Vector2(body.bounds.center.x, body.bounds.max.y - height * 0.5f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, new Vector2(dir, 0f), dist + 0.02f, stats.Glayer);

        Vector3 pos = follower.transform.position;

        // distancia 0 = la caja ya empezaba dentro de un collider: no es una pared de frente, se ignora
        if (hit.collider != null && hit.distance > 0.0001f && Vector2.Angle(hit.normal, Vector2.up) > stats.MaxSlopeAngle)
        {
            if (DebugFollower)
            {
                Debug.Log($"Seguidor bloqueado por {hit.collider.name} | dist={hit.distance} | normal={hit.normal} | grounded={_followerGrounded}");
            }

            pos.x += dir * Mathf.Max(hit.distance - 0.02f, 0f);
            follower.transform.position = pos;
            return true;
        }

        pos.x += deltaX;
        follower.transform.position = pos;
        return false;
    }

    void StartFollowerJump(float velY, float velX, float targetX, float targetY, bool toPlatform, bool needsAirJump)
    {
        _followerVelY = velY;
        _followerGrounded = false;
        _followerPlannedJump = toPlatform;
        _followerPlannedVelX = velX;
        _plannedTargetX = targetX;
        _plannedTargetY = targetY;
        _pendingAirJump = needsAirJump;
        _followerJumpsUsed++;
    }

    // altura de salto del seguidor: la del jugador por el multiplicador
    float JumpHeight(C_ReveriaMovementStats stats)
    {
        return stats.jumpHitgh * FollowerJumpHeightMult;
    }

    // velocidad de un salto completo del seguidor
    float MaxJumpSpeed(C_ReveriaMovementStats stats)
    {
        return Mathf.Sqrt(2f * Mathf.Abs(stats.Gravity) * JumpHeight(stats));
    }

    // mismos limites que el jugador: el doble salto solo si esta desbloqueado y los stats lo permiten (maximo 2 saltos)
    int MaxFollowerJumps(C_ReveriaMovementStats stats)
    {
        bool doubleJump = RMC_ThePlayerGeneralManager.Game_Data_FromSave._DoubleJumpisAllow;
        return doubleJump ? Mathf.Clamp(stats.NumberOfJumpsAllowed, 1, 2) : 1;
    }
    #endregion

    #region Teletransporte
    void TeleportFollowerBehindActive(GameObject active, GameObject follower, Movement_Manager_Abstract activeM, Movement_Manager_Abstract followerM, int side)
    {
        Collider2D body = followerM._BodyCoil;
        Collider2D feet = followerM._FeetCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        Vector3 followerPos = follower.transform.position;

        // desfases del seguidor respecto a su transform
        float feetToPivotY     = followerPos.y - feet.bounds.min.y;
        float bodyCenterOffX   = body.bounds.center.x - followerPos.x;
        float bodyCenterOffY   = body.bounds.center.y - followerPos.y;

        float activeX = active.transform.position.x;
        float activeFeetY = activeM._FeetCoil.bounds.min.y;
        float pivotY = activeFeetY + feetToPivotY + 0.02f;

        // plan B: justo sobre el activo
        Vector3 chosen = new Vector3(activeX - bodyCenterOffX, pivotY, followerPos.z);

        // del lugar normal hacia el activo: el primero con espacio libre y suelo debajo
        for (float d = DistanceBetweenCharactersXorControled; d >= 0f; d -= 0.5f)
        {
            float x = activeX + side * d;
            Vector3 candidate = new Vector3(x - bodyCenterOffX, pivotY, followerPos.z);
            Vector2 bodyCenter = new Vector2(x, pivotY + bodyCenterOffY);

            if (Physics2D.OverlapBox(bodyCenter, body.bounds.size * 0.9f, 0f, stats.Glayer) != null) continue;
            if (Physics2D.Raycast(new Vector2(x, activeFeetY + 0.2f), Vector2.down, 1f, stats.Glayer).collider == null) continue;

            chosen = candidate;
            break;
        }

        SetFollowerPosition(follower, chosen);
        _followSide = side;
        ResetFollowerState();
    }
    #endregion

    #region Gravedad y piso del seguidor
    void ApplyFollowerVertical(GameObject follower, Movement_Manager_Abstract followerM)
    {
        Collider2D feet = followerM._FeetCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        // velocidad media del paso: asi la parabola coincide con la formula y no pierde altura
        float prevVel = _followerVelY;
        _followerVelY = Mathf.Max(prevVel + stats.Gravity * Time.fixedDeltaTime, -stats.MaxFallSpeed);
        float avgVel = (prevVel + _followerVelY) * 0.5f;
        float deltaY = avgVel * Time.fixedDeltaTime;

        Vector3 pos = follower.transform.position;

        // subiendo: no se busca piso, solo se mueve
        if (avgVel > 0f)
        {
            pos.y += deltaY;
            _followerGrounded = false;
            SetFollowerPosition(follower, pos);
            return;
        }

        float fallDistance = Mathf.Max(-deltaY, FollowerGroundCheckDistance);

        if (SampleGround(feet, stats, fallDistance, out float groundY))
        {
            // piso: acomoda los pies justo sobre la superficie, corta la velocidad y reinicia el estado de salto
            pos.y += groundY - feet.bounds.min.y;
            _followerVelY = 0f;
            _followerGrounded = true;
            _followerPlannedJump = false;
            _pendingAirJump = false;
            _followerJumpsUsed = 0;
            _platformSearchT = 0f;
            SetFollowerPosition(follower, pos);
            return;
        }

        pos.y += deltaY;
        _followerGrounded = false;
        SetFollowerPosition(follower, pos);

        TryPlanFallingJump(followerM);
    }

    // tres rayos hacia abajo desde un escalon sobre los pies; se queda con la superficie mas alta.
    // Los hits con distancia 0 (rayo que empieza dentro de un collider) se ignoran: eso causaba los saltos hacia arriba junto a paredes.
    bool SampleGround(Collider2D feet, C_ReveriaMovementStats stats, float fallDistance, out float groundY)
    {
        groundY = float.MinValue;
        bool found = false;

        float halfW   = feet.bounds.extents.x * FollowerFeetWidthMult;
        float centerX = feet.bounds.center.x;
        float originY = feet.bounds.min.y + FollowerStepHeight;
        float length  = FollowerStepHeight + fallDistance;

        for (int i = -1; i <= 1; i++)
        {
            Vector2 origin = new Vector2(centerX + i * halfW, originY);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, length, stats.Glayer);

            if (hit.collider == null || hit.distance < 0.0001f) continue;
            if (Vector2.Angle(hit.normal, Vector2.up) > stats.MaxSlopeAngle) continue;

            if (hit.point.y > groundY)
            {
                groundY = hit.point.y;
                found = true;
            }
        }

        return found;
    }

    // mueve el seguidor y sincroniza para que los bounds de los colliders no queden desactualizados
    void SetFollowerPosition(GameObject follower, Vector3 pos)
    {
        follower.transform.position = pos;
        Physics2D.SyncTransforms();
    }
    #endregion

    #region Saltar a una plataforma
    void TryPlanFallingJump(Movement_Manager_Abstract followerM)
    {
        if (_followerPlannedJump || _pendingAirJump || _followerVelY >= 0f) return;
        if (_followerJumpsUsed >= MaxFollowerJumps(followerM.Mstas)) return;

        _platformSearchT -= Time.fixedDeltaTime;
        if (_platformSearchT > 0f) return;
        _platformSearchT = PlatformSearchInterval;

        Collider2D feet = followerM._FeetCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        // si hay piso mas abajo, solo cae, no necesita saltar
        Vector2 origin = new Vector2(feet.bounds.center.x, feet.bounds.min.y + 0.01f);
        Vector2 size   = new Vector2(feet.bounds.size.x * FollowerFeetWidthMult, 0.02f);

        if (Physics2D.BoxCast(origin, size, 0f, Vector2.down, NoFloorCheckDistance, stats.Glayer).collider != null) return;

        TryPlanJumpToPlatform(followerM, 0, float.NegativeInfinity, false);
    }

    // onlySide: 0 = ambos lados, 1 = derecha, -1 = izquierda | minDy: altura minima respecto a los pies
    // requireProgress: la plataforma debe acercarlo al objetivo (para paredes); al caer se acepta cualquiera
    bool TryPlanJumpToPlatform(Movement_Manager_Abstract followerM, int onlySide, float minDy, bool requireProgress)
    {
        if (!FindReachablePlatform(followerM, onlySide, minDy, requireProgress, out JumpPlan plan))
        {
            return false;
        }

        StartFollowerJump(plan.vy, plan.vx, plan.x, plan.y, true, plan.needsAirJump);
        return true;
    }

    // segundo salto en el apice: recalcula hacia la plataforma elegida desde donde esta ahora
    void TryAirJumpToTarget(Movement_Manager_Abstract followerM)
    {
        C_ReveriaMovementStats stats = followerM.Mstas;
        if (_followerJumpsUsed >= MaxFollowerJumps(stats)) return;

        Collider2D feet = followerM._FeetCoil;
        float dx = _plannedTargetX - feet.bounds.center.x;
        float dy = _plannedTargetY - feet.bounds.min.y;

        if (!ComputeJump(dx, dy, stats, out float vy, out float vx))
        {
            // mejor intento: salto completo hacia el objetivo
            vy = MaxJumpSpeed(stats);
            vx = Mathf.Sign(dx) * PlannedJumpMinSpeed;
        }

        StartFollowerJump(vy, vx, _plannedTargetX, _plannedTargetY, true, false);
    }

    // barre columnas y elige la plataforma alcanzable que deja al seguidor mas cerca de su objetivo
    bool FindReachablePlatform(Movement_Manager_Abstract followerM, int onlySide, float minDy, bool requireProgress, out JumpPlan best)
    {
        best = default;

        Collider2D feet = followerM._FeetCoil;
        Collider2D body = followerM._BodyCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        int jumpsLeft = Mathf.Clamp(MaxFollowerJumps(stats) - _followerJumpsUsed, 1, 2);

        float feetX = feet.bounds.center.x;
        float feetY = feet.bounds.min.y;

        float singleMax = JumpHeight(stats) - 0.1f;
        float maxHeight = jumpsLeft >= 2 ? JumpHeight(stats) * 2f - 0.3f : singleMax;
        float maxReachX = stats.MaxRunSpeed * (2f * stats.TimeTillJumpApex * jumpsLeft) * JumpReachSafety;

        float currentScore = Vector2.Distance(new Vector2(feetX, feetY), _followerGoal);
        float bestScore = float.MaxValue;
        bool found = false;

        for (float offset = 0f; offset <= maxReachX; offset += PlatformSearchStep)
        {
            for (int side = 0; side < 2; side++)
            {
                int sideDir = side == 0 ? 1 : -1;

                if (offset == 0f && side == 1) continue;
                if (onlySide != 0 && sideDir != onlySide) continue;

                float x = feetX + sideDir * offset;

                if (!SampleTopSurface(x, feetY, maxHeight, body, stats, out float topY)) continue;

                float dx = x - feetX;
                float dy = topY - feetY;

                if (dy < minDy) continue;

                JumpPlan plan = default;
                bool ok = false;

                if (dy <= singleMax)
                {
                    // un solo salto alcanza
                    ok = ComputeJump(dx, dy, stats, out float vy, out float vx);
                    plan = new JumpPlan { vy = vy, vx = vx, x = x, y = topY, needsAirJump = false };
                }
                else if (jumpsLeft >= 2 && dy <= maxHeight)
                {
                    // hace falta doble salto: salto completo ahora y el segundo en el apice
                    float vx = Mathf.Sign(dx) * Mathf.Clamp(Mathf.Abs(dx) / (stats.TimeTillJumpApex * 3f), PlannedJumpMinSpeed, stats.MaxRunSpeed);
                    plan = new JumpPlan { vy = MaxJumpSpeed(stats), vx = vx, x = x, y = topY, needsAirJump = true };
                    ok = true;
                }

                if (!ok) continue;

                float score = Vector2.Distance(new Vector2(x, topY), _followerGoal);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = plan;
                    found = true;
                }
            }
        }

        if (found && requireProgress && bestScore > currentScore - MinProgressToJump)
        {
            return false;
        }

        return found;
    }

    // devuelve la altura de la superficie caminable en esa columna, si es valida para aterrizar
    bool SampleTopSurface(float x, float feetY, float maxHeight, Collider2D body, C_ReveriaMovementStats stats, out float topY)
    {
        topY = 0f;

        float startY = feetY + maxHeight;
        float rayLength = maxHeight + 0.2f;

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, startY), Vector2.down, rayLength, stats.Glayer);

        // distancia 0 = el rayo empezo dentro de un collider
        if (hit.collider == null || hit.distance < 0.001f) return false;
        if (Vector2.Angle(hit.normal, Vector2.up) > stats.MaxSlopeAngle) return false;

        // la plataforma debe seguir a ambos lados, para no apuntar justo al borde
        RaycastHit2D left  = Physics2D.Raycast(new Vector2(x - PlatformEdgeMargin, startY), Vector2.down, rayLength, stats.Glayer);
        RaycastHit2D right = Physics2D.Raycast(new Vector2(x + PlatformEdgeMargin, startY), Vector2.down, rayLength, stats.Glayer);

        if (left.collider != hit.collider || right.collider != hit.collider) return false;
        if (Mathf.Abs(left.point.y - hit.point.y) > 0.1f || Mathf.Abs(right.point.y - hit.point.y) > 0.1f) return false;

        // tiene que caber el cuerpo encima
        Vector2 bodyCenter = new Vector2(x, hit.point.y + body.bounds.extents.y + 0.02f);
        if (Physics2D.OverlapBox(bodyCenter, body.bounds.size * 0.95f, 0f, stats.Glayer) != null) return false;

        topY = hit.point.y;
        return true;
    }

    // calcula la velocidad vertical y horizontal para llegar a (dx, dy) cayendo sobre el punto; false si el salto no alcanza
    bool ComputeJump(float dx, float dy, C_ReveriaMovementStats stats, out float vy, out float vx)
    {
        vx = 0f;
        vy = 0f;
        float Gravity_OnAbs = Mathf.Abs(stats.Gravity);

        // el punto mas alto del salto queda JumpApexMargin sobre la plataforma, sin pasar de la altura de salto del seguidor
        float apex = Mathf.Clamp(dy + JumpApexMargin, 0.2f, JumpHeight(stats));
        if (apex < dy + 0.1f) return false; // no sobra altura para librar el borde

        vy = Mathf.Sqrt(2f * Gravity_OnAbs * apex);

        // tiempo hasta cruzar la altura de la plataforma ya bajando
        float t = (vy + Mathf.Sqrt(Mathf.Max(vy * vy - 2f * Gravity_OnAbs * dy, 0f))) / Gravity_OnAbs;

        vx = dx / t;
        return Mathf.Abs(vx) <= stats.MaxRunSpeed;
    }
    #endregion

    #region SwitchCharacters
    void _SwitchControlCharacter()
    {
        if (Input_Manager._OnChangeCharacterWasPressThisFrame)
        {
            if (Pmanager._IsArkhalisActive)
            {
                Pmanager._IsArkhalisActive = false;
                Pmanager._IsAriamisActive = true;
            }else
            {
                Pmanager._IsAriamisActive = false;
                Pmanager._IsArkhalisActive = true;
            }

            // sin esto IActive_HasPlayer nunca corre y el manager viejo sigue activo aplicando gravedad
            BloodManager_M.AlrearyRender = false;
            MagicManager_M.AlrearyRender = false;

            // el nuevo activo debe ser Dynamic ya, antes del siguiente FixedUpdate
            SetFollowerPhysics(Pmanager._IsAriamisActive);
        }
    }

    #endregion
}