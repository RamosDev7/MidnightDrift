using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorCarro : MonoBehaviour
{
    [Header("Colisores das Rodas")]
    [SerializeField] private WheelCollider rodaFrenteEsquerdaCollider;
    [SerializeField] private WheelCollider rodaFrenteDireitaCollider;
    [SerializeField] private WheelCollider rodaTraseiraEsquerdaCollider;
    [SerializeField] private WheelCollider rodaTraseiraDireitaCollider;

    [Header("Visuais das Rodas")]
    [SerializeField] private Transform rodaFrenteEsquerdaVisual;
    [SerializeField] private Transform rodaFrenteDireitaVisual;
    [SerializeField] private Transform rodaTraseiraEsquerdaVisual;
    [SerializeField] private Transform rodaTraseiraDireitaVisual;

    [Header("Configurações do Veículo (Drift)")]
    [SerializeField] private float forcaDoMotor = 500f;
    [SerializeField] private float forcaDoFreio = 3000f;
    [SerializeField] private float forcaFreioDeMao = 10000f;
    [SerializeField] private float resistenciaDeRolagem = 1000f;
    [SerializeField] private float anguloMaximoDirecao = 45f;

    [Header("Ajustes de Aderência (Drift)")]
    [SerializeField] private float aderenciaNormal = 1.0f; // Pneu grudado no chão
    [SerializeField] private float aderenciaDrift = 0.4f;  // Pneu escorregando de lado

    [Header("Controles")]
    [SerializeField] private InputAction acaoMover;
    [SerializeField] private InputAction acaoFrear;
    [SerializeField] private InputAction acaoFreioMao;

    private float entradaVertical;
    private float entradaHorizontal;
    private bool estaFreando;
    private bool estaFazendoDrift;

    void Start()
    {
        acaoMover.Enable();
        acaoFrear.Enable();
        acaoFreioMao.Enable();
    }

    void FixedUpdate()
    {
        LerEntradas();
        ControlarMotorEFreio();
        ControlarDirecao();
        AtualizarRodaVisual(rodaFrenteDireitaCollider, rodaFrenteDireitaVisual);
        AtualizarRodaVisual(rodaFrenteEsquerdaCollider, rodaFrenteEsquerdaVisual);
        AtualizarRodaVisual(rodaTraseiraDireitaCollider, rodaTraseiraDireitaVisual);
        AtualizarRodaVisual(rodaTraseiraEsquerdaCollider, rodaTraseiraEsquerdaVisual);
    }

    private void LerEntradas()
    {
        Vector2 mover = acaoMover.ReadValue<Vector2>();
        entradaHorizontal = mover.x;
        entradaVertical = mover.y;

        estaFreando = acaoFrear.IsPressed();
        estaFazendoDrift = acaoFreioMao.IsPressed();
    }

    private void ControlarMotorEFreio()
    {
        // 1. Aplica a força de aceleração nas rodas traseiras
        float forcaAtualDoMotor = entradaVertical * forcaDoMotor;
        rodaTraseiraDireitaCollider.motorTorque = forcaAtualDoMotor;
        rodaTraseiraEsquerdaCollider.motorTorque = forcaAtualDoMotor;

        float freioFrente = 0f;
        float freioTraseira = 0f;

        // 2. Lógica de Frenagem e Drift
        if (estaFazendoDrift)
        {
            // Não trava a roda com o freio! Deixa ela girar livre e dá mais força pro motor
            freioTraseira = 0f;
            freioFrente = 0f;

            rodaTraseiraDireitaCollider.motorTorque = forcaAtualDoMotor * 2.5f;
            rodaTraseiraEsquerdaCollider.motorTorque = forcaAtualDoMotor * 2.5f;

            AjustarAtritoTraseiro(aderenciaDrift); // Solta a traseira para escorregar
        }
        else
        {
            AjustarAtritoTraseiro(aderenciaNormal); // Recupera a aderência normal

            if (estaFreando)
            {
                freioFrente = forcaDoFreio;
                freioTraseira = forcaDoFreio;
            }
            else if (Mathf.Abs(entradaVertical) < 0.1f)
            {
                freioFrente = resistenciaDeRolagem;
                freioTraseira = resistenciaDeRolagem;
            }
            else if (entradaVertical * rodaTraseiraEsquerdaCollider.rpm < 0f)
            {
                freioFrente = resistenciaDeRolagem;
                freioTraseira = resistenciaDeRolagem;
            }
        }

        // 3. Aplica o torque de frenagem de forma independente
        rodaFrenteDireitaCollider.brakeTorque = freioFrente;
        rodaFrenteEsquerdaCollider.brakeTorque = freioFrente;
        rodaTraseiraDireitaCollider.brakeTorque = freioTraseira;
        rodaTraseiraEsquerdaCollider.brakeTorque = freioTraseira;
    }

    private void AjustarAtritoTraseiro(float novaAderencia)
    {
        WheelFrictionCurve atritoEsquerda = rodaTraseiraEsquerdaCollider.sidewaysFriction;
        atritoEsquerda.stiffness = novaAderencia;
        rodaTraseiraEsquerdaCollider.sidewaysFriction = atritoEsquerda;

        WheelFrictionCurve atritoDireita = rodaTraseiraDireitaCollider.sidewaysFriction;
        atritoDireita.stiffness = novaAderencia;
        rodaTraseiraDireitaCollider.sidewaysFriction = atritoDireita;
    }

    private void ControlarDirecao()
    {
        float anguloAtualDirecao = entradaHorizontal * anguloMaximoDirecao;
        rodaFrenteDireitaCollider.steerAngle = anguloAtualDirecao;
        rodaFrenteEsquerdaCollider.steerAngle = anguloAtualDirecao;
    }

    private void AtualizarRodaVisual(WheelCollider rodaCollider, Transform rodaVisual)
    {
        rodaCollider.GetWorldPose(out Vector3 posicao, out Quaternion rotacao);
        rodaVisual.position = posicao;
        rodaVisual.rotation = rotacao;
    }
}