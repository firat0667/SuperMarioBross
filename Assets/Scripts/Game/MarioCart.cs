using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class MarioCart : MonoBehaviour
    {
        [SerializeField]
        public Sprite[]         m_mario;

        [SerializeField, Range(0.0f, 1.0f)]
        public float            m_fDrag = 0.0f;

        private float           m_fVelocity = 0.0f;
        private float           m_fPosition = 0.0f;

        private RectTransform   m_simulation;

        private Text            m_txtPosition;
        private Text            m_txtVelocity;
        private Text            m_txtAcceleration;

        private Image           m_imgMario;
        private RawImage        m_ground;

        private void Start()
        {
            m_simulation = transform.Find("Simulation").GetComponent<RectTransform>();
            m_txtAcceleration = transform.Find("HUD/Acceleration").GetComponent<Text>();
            m_txtVelocity = transform.Find("HUD/Velocity").GetComponent<Text>();
            m_txtPosition = transform.Find("HUD/Position").GetComponent<Text>();
            m_imgMario = transform.Find("Simulation/Mario").GetComponent<Image>();
            m_ground = transform.Find("Simulation/Ground").GetComponent<RawImage>();
        }

        private void FixedUpdate()
        {
            // calculate the acceleration
            Vector2 vGravity = Vector2.down * 9.82f;
            float fAcceleration = Vector2.Dot(vGravity, m_simulation.right);

            // update velocity 
            m_fVelocity += fAcceleration * Time.fixedDeltaTime;

            // apply drag
            m_fVelocity -= m_fVelocity * Time.fixedDeltaTime * m_fDrag;

            // update position
            m_fPosition += m_fVelocity * Time.fixedDeltaTime;


            // update the HUD
            m_txtAcceleration.text = fAcceleration.ToString("0.00");
            m_txtVelocity.text = m_fVelocity.ToString("0.00");
            m_txtPosition.text = m_fPosition.ToString("0.00");

            // make mario look behind
            m_imgMario.sprite = m_mario[m_fVelocity >= -0.001f ? 0 : 1];

            // animate the ground
            Rect uvRect = m_ground.uvRect;
            uvRect.x = m_fPosition;
            m_ground.uvRect = uvRect;
        }
    }
}