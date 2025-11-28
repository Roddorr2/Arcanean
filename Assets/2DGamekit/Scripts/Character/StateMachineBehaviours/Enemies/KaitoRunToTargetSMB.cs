using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class KaitoRunToTargetSMB : SceneLinkedSMB<EnemyBehaviour>
    {
        private KaitoSpecial kaitoSpecial;
        private Transform player;
        private SpriteRenderer spriteRenderer;

        [Header("Configuración Kaito")]
        public float runSpeed = 4f;
        public float detectionRange = 8f;
        public float stoppingDistance = 3f;

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateEnter(animator, stateInfo, layerIndex);

            // Obtener referencia a KaitoSpecial
            kaitoSpecial = m_MonoBehaviour.GetComponent<KaitoSpecial>();
            spriteRenderer = m_MonoBehaviour.GetComponent<SpriteRenderer>();

            // Buscar jugador
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            OrientToTarget();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

            if (player == null) return;

            m_MonoBehaviour.CheckTargetStillVisible();
            m_MonoBehaviour.CheckMeleeAttack();

            float distanceToPlayer = Vector2.Distance(m_MonoBehaviour.transform.position, player.position);

            // Solo moverse si el jugador está en rango y no demasiado cerca
            if (distanceToPlayer <= detectionRange && distanceToPlayer > stoppingDistance)
            {
                if (!CheckForObstacle(runSpeed))
                {
                    MoveTowardsPlayer();

                    // Disparar mientras se mueve
                    if (kaitoSpecial != null && distanceToPlayer > stoppingDistance + 1f)
                    {
                        kaitoSpecial.ForceShoot();
                    }
                }
                else
                {
                    m_MonoBehaviour.SetHorizontalSpeed(0);
                }
            }
            else
            {
                m_MonoBehaviour.SetHorizontalSpeed(0);

                // Disparar si está en distancia óptima
                if (kaitoSpecial != null && distanceToPlayer <= detectionRange && distanceToPlayer > stoppingDistance - 0.5f)
                {
                    kaitoSpecial.ForceShoot();
                }
            }

            // Actualizar orientación
            OrientToTarget();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateExit(animator, stateInfo, layerIndex);
            m_MonoBehaviour.SetHorizontalSpeed(0);
        }

        private void OrientToTarget()
        {
            if (player == null || spriteRenderer == null) return;

            // Orientar sprite hacia el jugador
            bool playerIsRight = player.position.x > m_MonoBehaviour.transform.position.x;
            spriteRenderer.flipX = playerIsRight;
        }

        private void MoveTowardsPlayer()
        {
            if (player == null) return;

            Vector2 direction = (player.position - m_MonoBehaviour.transform.position).normalized;
            float horizontalSpeed = direction.x > 0 ? runSpeed : -runSpeed;

            m_MonoBehaviour.SetHorizontalSpeed(horizontalSpeed);
        }

        private bool CheckForObstacle(float speed)
        {
            if (spriteRenderer == null) return false;

            // Dirección basada en la orientación del sprite
            Vector2 direction = spriteRenderer.flipX ? Vector2.right : Vector2.left;
            float checkDistance = 1.2f;

            // Raycast para detectar obstáculos
            RaycastHit2D hit = Physics2D.Raycast(
                m_MonoBehaviour.transform.position,
                direction,
                checkDistance,
                LayerMask.GetMask("Default", "Platform", "Ground")
            );

            // Debug visual
            Debug.DrawRay(m_MonoBehaviour.transform.position, direction * checkDistance,
                         hit.collider != null ? Color.red : Color.green);

            return hit.collider != null && !hit.collider.CompareTag("Player");
        }
    }
}