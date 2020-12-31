using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform m_PlayerTransform;
    private Vector3 m_OriginalOffset;

    void Start()
    {
        PlayerCharacterController playerCharacterController = GameObject.FindObjectOfType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, FollowPlayer>(playerCharacterController, this);

        m_PlayerTransform = playerCharacterController.transform;

        m_OriginalOffset = transform.position - m_PlayerTransform.position;
    }

    void LateUpdate()
    {
        if (GameFlowManager.IsGameActive) return;
        if (m_PlayerTransform) transform.position = m_PlayerTransform.position + m_OriginalOffset;
    }
}
