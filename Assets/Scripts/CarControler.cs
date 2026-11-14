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
    [SerializeField] private float forcaDoMotor = 500f; // Reduzido para maior controle
    [SerializeField] private float forcaDoFreio = 3000f; // Freio normal (todas as rodas)
    [SerializeField] private float forcaFreioDeMao = 10000f; // Força massiva para travar apenas a traseira
    [SerializeField] private float resistenciaDeRolagem = 1000f;
    [SerializeField] private float anguloMaximoDirecao = 45f; // Aumentado para 45º para permitir contratesterço

    [Header("Controles")]
    [SerializeField] private InputAction acaoMover;
    [SerializeField] private InputAction acaoFrear; // Freio normal
    [SerializeField] private InputAction acaoFreioMao; // Nova ação para iniciar o Drift

    private float entradaVertical;
    private float entradaHorizontal;
    private bool estaFreando;
    private bool estaFazendoDrift;

    void Start()
    {
        acaoMover.Enable();
        acaoFrear.Enable();
        acaoFreioMao.Enable(); // Habilitando a nova ação
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
        estaFazendoDrift = acaoFreioMao.IsPressed(); // Lê se o freio de mão está pressionado
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
            // Freio de Mão: Trava apenas as rodas traseiras com força máxima para derrapar
            freioTraseira = forcaFreioDeMao;
            freioFrente = 0f;
        }
        else if (estaFreando)
        {
            // Freio Normal: Aplica força em todas as rodas
            freioFrente = forcaDoFreio;
            freioTraseira = forcaDoFreio;
        }
        else if (Mathf.Abs(entradaVertical) < 0.1f)
        {
            // Desaceleração natural ao soltar o acelerador
            freioFrente = resistenciaDeRolagem;
            freioTraseira = resistenciaDeRolagem;
        }
        else if (entradaVertical * rodaTraseiraEsquerdaCollider.rpm < 0f)
        {
            // Resistência ao tentar dar ré enquanto o carro ainda vai pra frente
            freioFrente = resistenciaDeRolagem;
            freioTraseira = resistenciaDeRolagem;
        }

        // 3. Aplica o torque de frenagem de forma independente
        rodaFrenteDireitaCollider.brakeTorque = freioFrente;
        rodaFrenteEsquerdaCollider.brakeTorque = freioFrente;
        rodaTraseiraDireitaCollider.brakeTorque = freioTraseira;
        rodaTraseiraEsquerdaCollider.brakeTorque = freioTraseira;
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