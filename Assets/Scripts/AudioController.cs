using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] internal AudioListener m_Player_Listener;
    [SerializeField] internal AudioSource m_BG_Audio;
    [SerializeField] internal AudioSource m_Click_Audio;
    [SerializeField] internal AudioSource m_Win_Audio;
    [SerializeField] internal AudioSource m_Bonus_Audio;
    [SerializeField] internal AudioSource m_FreeSpin_Audio;
    [SerializeField] internal AudioSource m_Spin_Audio;

    private event Action m_On_Application_Focus;
    private event Action m_On_Application_Out_Of_Focus;

    private void OnDisable()
    {
        m_On_Application_Focus -= delegate
        {
            m_Player_Listener.enabled = true;
            if (m_BG_Audio) m_BG_Audio.UnPause();
            if (m_Click_Audio) m_Click_Audio.UnPause();
            if (m_Win_Audio) m_Win_Audio.UnPause();
            if (m_Bonus_Audio) m_Bonus_Audio.UnPause();
            if (m_FreeSpin_Audio) m_FreeSpin_Audio.UnPause();
            if (m_Spin_Audio) m_Spin_Audio.UnPause();
        };

        m_On_Application_Out_Of_Focus -= delegate
        {
            m_Player_Listener.enabled = false;
            if (m_BG_Audio) m_BG_Audio.Pause();
            if (m_Click_Audio) m_Click_Audio.Pause();
            if (m_Win_Audio) m_Win_Audio.Pause();
            if (m_Bonus_Audio) m_Bonus_Audio.Pause();
            if (m_FreeSpin_Audio) m_FreeSpin_Audio.Pause();
            if (m_Spin_Audio) m_Spin_Audio.Pause();
        };
    }

    internal void InitialAudioSetup()
    {
        if (m_BG_Audio) m_BG_Audio.Play();

        m_On_Application_Focus += delegate
        {
            m_Player_Listener.enabled = true;
            if (m_BG_Audio) m_BG_Audio.UnPause();
            if (m_Click_Audio) m_Click_Audio.UnPause();
            if (m_Win_Audio) m_Win_Audio.UnPause();
            if (m_Bonus_Audio) m_Bonus_Audio.UnPause();
            if (m_FreeSpin_Audio) m_FreeSpin_Audio.UnPause();
            if (m_Spin_Audio) m_Spin_Audio.UnPause();
        };

        m_On_Application_Out_Of_Focus += delegate
        {
            m_Player_Listener.enabled = false;
            if (m_BG_Audio) m_BG_Audio.Pause();
            if (m_Click_Audio) m_Click_Audio.Pause();
            if (m_Win_Audio) m_Win_Audio.Pause();
            if (m_Bonus_Audio) m_Bonus_Audio.Pause();
            if (m_FreeSpin_Audio) m_FreeSpin_Audio.Pause();
            if (m_Spin_Audio) m_Spin_Audio.Pause();
        };
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            m_On_Application_Out_Of_Focus?.Invoke();
        }
        else
        {
            m_On_Application_Focus?.Invoke();

        }
    }

    internal void ToggleMute(bool toggle, string type = "all")
    {
        switch (type)
        {
            // case "bg":
            //     m_BG_Audio.mute = toggleBG;
            //     break;
            case "button":
                m_Click_Audio.mute = toggle;
                m_Spin_Audio.mute = toggle;
                break;
            case "wl":
                m_Win_Audio.mute = toggle;
                m_Bonus_Audio.mute = toggle;
                m_FreeSpin_Audio.mute = toggle;
                break;
            case "all":
               // m_BG_Audio.mute = toggle;
                m_Click_Audio.mute = toggle;
                m_Win_Audio.mute = toggle;
                m_Bonus_Audio.mute = toggle;
                m_FreeSpin_Audio.mute = toggle;
                m_Spin_Audio.mute = toggle;
                break;
        }
    }
    
    internal void ToggleBG_Mute(bool toggle)
    {
        m_BG_Audio.mute = toggle;
    }

}
