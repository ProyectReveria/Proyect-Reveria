using UnityEngine;

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
    [SerializeField] private float TeleportDistance = 20f;         // si el seguidor queda mas lejos que esto, reaparece detras del activo
    [SerializeField] private float TalismanDelay = 0.4f;           // cuanto espera atascado antes de reaparecer (el "talisman")
    [SerializeField] private float ActiveRiseToTeleport = 1.5f;    // cuanto mas alto debe estar el activo (ya debajo de el) para que cuente como inalcanzable

    [Header ("Movimiento del seguidor")]
    [SerializeField] private float FollowerAcceleration = 40f;     // que tan rapido acelera y frena en X
    [SerializeField] private float FollowerArriveSmoothing = 4f;   // frena al acercarse al objetivo (mas alto = frena mas tarde)

    [Header ("Gravedad del seguidor")]
    [SerializeField] private float FollowerGroundCheckDistance = 0.05f; // que tan abajo busca piso cuando esta parado
    [SerializeField] private float FollowerFeetWidthMult = 0.95f;       // ancho de los rayos de piso respecto al FeetCoil
    [SerializeField] private float FollowerStepHeight = 0.4f;           // escalon o pendiente que sube caminando

    [Header ("Zonas de daño y huecos")]
    [SerializeField] private LayerMask FollowerHazardLayer;        // vacio = usa automaticamente la capa "DamageEnviorment"
    [SerializeField] private float HazardLookAhead = 1.5f;         // margen de vision hacia adelante, se suma a la distancia de frenado
    [SerializeField] private float HazardCheckDepth = 6f;          // que tan abajo busca una zona de daño en cada columna
    [SerializeField] private float HazardCheckStep = 0.5f;         // separacion entre columnas al mirar hacia adelante (que sea menor al ancho de la trampa mas angosta)
    [SerializeField] private float NoFloorCheckDistance = 3f;      // un hueco es una caida de mas de esto; si el activo esta mas abajo que esto, lo sigue y cae

    [Header ("Debug")]
    [SerializeField] private bool DebugFollower = false;           // imprime por que el seguidor se detiene o reaparece
    #endregion

    #region Variables
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

    // tiempo que lleva sin poder llegar caminando
    private float _stuckT;
    private bool _followerWaitingHazard;
    #endregion

    #region Unity Update API

    void Awake()
    {
        // si no se asigno una capa de daño a mano, usa la capa "DamageEnviorment"
        if (FollowerHazardLayer.value == 0)
        {
            int hazardLayer = LayerMask.NameToLayer("DamageEnviorment");

            if (hazardLayer >= 0)
            {
                FollowerHazardLayer = 1 << hazardLayer;
            }
            else
            {
                Debug.LogWarning("SwitchCharactersLibrary: no existe la capa 'DamageEnviorment'. Asigna Follower Hazard Layer a mano o el seguidor no vera las trampas.");
            }
        }

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

        bool activeGrounded = activeM._Ghit.collider != null;

        // demasiado lejos: reaparece detras del activo (solo cuando el activo esta en el suelo)
        float distance = Vector2.Distance(active.transform.position, follower.transform.position);
        if (distance > TeleportDistance && activeGrounded)
        {
            TeleportFollowerBehindActive(active, follower, activeM, followerM, desiredSide);
            return;
        }

        // gravedad y piso: corre siempre
        ApplyFollowerVertical(follower, followerM);

        // camina hacia el activo; devuelve true si no puede llegar caminando
        bool stuck = FollowTargetX(active, follower, activeM, followerM, speed);

        UpdateTalisman(active, follower, activeM, followerM, desiredSide, stuck, activeGrounded);
    }

    void ResetFollowerState()
    {
        _followerVelX = 0f;
        _followerVelY = 0f;
        _followerWaitingHazard = false;
        _stuckT = 0f;
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

    // camina hacia su lugar detras del activo. Devuelve true si estando en el suelo no puede llegar caminando (pared, trampa, hueco o activo mucho mas arriba)
    bool FollowTargetX(GameObject active, GameObject follower, Movement_Manager_Abstract activeM, Movement_Manager_Abstract followerM, float speed)
    {
        bool activeFacingRight = activeM._IsFacingR;

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

        float toTarget = targetX - followerX;

        // mira hacia adelante: trampa o hueco en su camino
        if (Mathf.Abs(toTarget) > 0.05f)
        {
            int moveDir = toTarget >= 0f ? 1 : -1;
            float brakeDistance = (_followerVelX * _followerVelX) / (2f * Mathf.Max(FollowerAcceleration, 0.01f));

            // solo mira hasta donde va a llegar: una trampa mas alla de su objetivo no lo detiene
            float lookDistance = Mathf.Min(HazardLookAhead + brakeDistance, Mathf.Abs(toTarget) + HazardCheckStep);

            bool hazardAhead = HazardAhead(followerM, moveDir, lookDistance);

            // hueco: solo cuenta si el activo no esta muy por debajo (si bajo a un foso, lo sigue y cae)
            bool gapAhead = _followerGrounded
                && !hazardAhead
                && _followerGoal.y >= followerM._FeetCoil.bounds.min.y - NoFloorCheckDistance
                && GapAhead(followerM, moveDir, lookDistance);

            if (hazardAhead || gapAhead)
            {
                if (DebugFollower && !_followerWaitingHazard)
                {
                    Debug.Log(hazardAhead ? "Seguidor: zona de daño adelante, se detiene" : "Seguidor: hueco adelante, se detiene");
                }

                _followerWaitingHazard = true;
                _followerVelX = Mathf.MoveTowards(_followerVelX, 0f, FollowerAcceleration * Time.fixedDeltaTime);
                MoveFollowerX(follower, followerM, _followerVelX * Time.fixedDeltaTime);

                return _followerGrounded;
            }
        }

        _followerWaitingHazard = false;

        // acelera y frena en vez de saltar a la velocidad maxima de golpe
        float desiredVel = Mathf.Clamp(toTarget * FollowerArriveSmoothing, -speed, speed);
        _followerVelX = Mathf.MoveTowards(_followerVelX, desiredVel, FollowerAcceleration * Time.fixedDeltaTime);

        bool blocked = MoveFollowerX(follower, followerM, _followerVelX * Time.fixedDeltaTime);

        if (blocked)
        {
            _followerVelX = 0f;

            if (DebugFollower && _followerGrounded)
            {
                Debug.Log("Seguidor: pared adelante");
            }

            return _followerGrounded;
        }

        // ya esta en su sitio pero el activo esta en una plataforma mas alta de lo que sube caminando
        if (_followerGrounded && Mathf.Abs(toTarget) < 0.5f)
        {
            float rise = activeM._FeetCoil.bounds.min.y - followerM._FeetCoil.bounds.min.y;

            if (rise > ActiveRiseToTeleport)
            {
                return true;
            }
        }

        return false;
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
    #endregion

    #region Zonas de daño y huecos
    // un collider es zona de daño si el o alguno de sus padres tiene EnviormentDamage
    bool IsHazard(Collider2D col)
    {
        return col != null && col.GetComponentInParent<EnviormentDamage>() != null;
    }

    // suelo normal + capas de daño: asi las trampas se detectan aunque no esten en Glayer
    int HazardMask(C_ReveriaMovementStats stats)
    {
        return stats.Glayer | FollowerHazardLayer;
    }

    // true si lo primero que hay bajo esa columna es una zona de daño
    bool HazardBelow(float x, float fromY, C_ReveriaMovementStats stats)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, fromY), Vector2.down, HazardCheckDepth, HazardMask(stats));
        return hit.collider != null && hit.distance > 0.0001f && IsHazard(hit.collider);
    }

    // mira columna por columna hacia adelante (hasta 'distance') si hay una zona de daño debajo
    bool HazardAhead(Movement_Manager_Abstract followerM, int dir, float distance)
    {
        Collider2D feet = followerM._FeetCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        float startX = feet.bounds.center.x;
        float fromY  = feet.bounds.min.y + FollowerStepHeight;

        for (float d = HazardCheckStep; d <= distance + HazardCheckStep; d += HazardCheckStep)
        {
            float x = startX + dir * Mathf.Min(d, distance);

            if (HazardBelow(x, fromY, stats)) return true;
        }

        return false;
    }

    // mira columna por columna hacia adelante si no hay suelo debajo (hueco)
    bool GapAhead(Movement_Manager_Abstract followerM, int dir, float distance)
    {
        Collider2D feet = followerM._FeetCoil;
        C_ReveriaMovementStats stats = followerM.Mstas;

        float startX = feet.bounds.center.x;
        float fromY  = feet.bounds.min.y + FollowerStepHeight;

        for (float d = HazardCheckStep; d <= distance + HazardCheckStep; d += HazardCheckStep)
        {
            float x = startX + dir * Mathf.Min(d, distance);

            RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, fromY), Vector2.down, FollowerStepHeight + NoFloorCheckDistance, HazardMask(stats));

            if (hit.collider == null) return true;
        }

        return false;
    }
    #endregion

    #region Teletransporte
    // si lleva atascado un rato y el activo ya toco piso, reaparece detras de el
    void UpdateTalisman(GameObject active, GameObject follower, Movement_Manager_Abstract activeM, Movement_Manager_Abstract followerM, int side, bool stuck, bool activeGrounded)
    {
        if (!stuck)
        {
            _stuckT = 0f;
            return;
        }

        _stuckT += Time.fixedDeltaTime;

        if (_stuckT < TalismanDelay) return;

        // espera a que el activo aterrice: asi nunca aparece junto a alguien en el aire
        if (!activeGrounded) return;

        if (DebugFollower)
        {
            Debug.Log("Seguidor: no puede llegar caminando, reaparece detras del activo");
        }

        ///<Aqui va el efecto del talisman (destello, sonido o animacion) antes de mover al seguidor

        TeleportFollowerBehindActive(active, follower, activeM, followerM, side);
    }

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

        // del lugar normal hacia el activo: el primero con espacio libre, suelo debajo y sin zona de daño
        for (float d = DistanceBetweenCharactersXorControled; d >= 0f; d -= 0.5f)
        {
            float x = activeX + side * d;
            Vector3 candidate = new Vector3(x - bodyCenterOffX, pivotY, followerPos.z);
            Vector2 bodyCenter = new Vector2(x, pivotY + bodyCenterOffY);

            if (Physics2D.OverlapBox(bodyCenter, body.bounds.size * 0.9f, 0f, stats.Glayer) != null) continue;
            if (Physics2D.Raycast(new Vector2(x, activeFeetY + 0.2f), Vector2.down, 1f, stats.Glayer).collider == null) continue;
            if (HazardBelow(x, activeFeetY + 0.2f, stats)) continue;

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

        // velocidad media del paso, igual que el resto del movimiento
        float prevVel = _followerVelY;
        _followerVelY = Mathf.Max(prevVel + stats.Gravity * Time.fixedDeltaTime, -stats.MaxFallSpeed);
        float deltaY = (prevVel + _followerVelY) * 0.5f * Time.fixedDeltaTime;

        Vector3 pos = follower.transform.position;

        float fallDistance = Mathf.Max(-deltaY, FollowerGroundCheckDistance);

        if (SampleGround(feet, stats, fallDistance, out float groundY))
        {
            // piso: acomoda los pies justo sobre la superficie y corta la velocidad
            pos.y += groundY - feet.bounds.min.y;
            _followerVelY = 0f;
            _followerGrounded = true;
            SetFollowerPosition(follower, pos);
            return;
        }

        pos.y += deltaY;
        _followerGrounded = false;
        SetFollowerPosition(follower, pos);
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