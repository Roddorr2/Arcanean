using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTAI;

namespace Gamekit2D
{
    public class KaitoBT : MonoBehaviour
    {
        Animator m_Animator;
        Damageable m_Damageable;
        Root m_Ai = BT.Root();
        EnemyBehaviour m_EnemyBehaviour;
        KaitoSpecial m_KaitoSpecial;

        private void OnEnable()
        {
            m_EnemyBehaviour = GetComponent<EnemyBehaviour>();
            m_Animator = GetComponent<Animator>();
            m_KaitoSpecial = GetComponent<KaitoSpecial>();

            m_Ai.OpenBranch(

                BT.If(() => { return m_EnemyBehaviour.Target != null; }).OpenBranch(
                    BT.Call(m_EnemyBehaviour.CheckTargetStillVisible),
                    BT.Call(m_EnemyBehaviour.OrientToTarget),

                    // Comportamiento específico de Kaito: correr hacia el objetivo y disparar
                    BT.If(() => {
                        if (m_EnemyBehaviour.Target == null) return false;
                        float distance = Vector2.Distance(transform.position, m_EnemyBehaviour.Target.position);
                        return distance > m_KaitoSpecial.stoppingDistance;
                    }).OpenBranch(
                        BT.Trigger(m_Animator, "Running")
                    ),

                    // Disparar cuando está en rango
                    BT.If(() => {
                        if (m_EnemyBehaviour.Target == null) return false;
                        float distance = Vector2.Distance(transform.position, m_EnemyBehaviour.Target.position);
                        return distance <= m_KaitoSpecial.detectionRange;
                    }).OpenBranch(
                        BT.Call(() => m_KaitoSpecial.ForceShoot())
                    ),

                    BT.Call(m_EnemyBehaviour.RememberTargetPos)
                ),

                BT.If(() => { return m_EnemyBehaviour.Target == null; }).OpenBranch(
                    BT.Call(m_EnemyBehaviour.ScanForPlayer),
                    BT.Trigger(m_Animator, "Idle")
                )
            );
        }

        private void Update()
        {
            m_Ai.Tick();
        }
    }
}