using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region EVENTS_AND_DELEGATES
    internal event Action OnGameStarted;
    internal event Action OnSpinClicked;
    internal event Action OnAutoSpinClicked;
    internal event Action OnAutoSpinStopClicked;
    internal event Action OnBetButtonClicked;
    internal event Action OnSpinSuccess;
    internal event Action OnBonusReceived;
    internal event Action OnFreeSpinReceived;
    internal event Action OnFreeSpinEnded;
    #endregion

    #region MANAGER_REFERENCES
    [SerializeField]
    private UIManager m_UIManager;
    [SerializeField]
    private SlotBehaviour m_SlotBehaviour;
    [SerializeField]
    private SocketIOManager m_SocketManager;
    [SerializeField]
    private AudioController m_AudioController;
    #endregion

    #region SERIALIZED_BOOLEANS
    [SerializeField]
    protected internal bool TurboSpin = false;
    [SerializeField]
    protected internal bool DemoFreeSpin = false;
    [SerializeField]
    protected internal bool m_IsExit = false;
    #endregion

    #region SERIALIZED_INTEGERS
    [SerializeField] internal int StopPos_Plus = 50;
    [SerializeField] internal int AutoSpin_Count = 10;
    #endregion

    #region SERIALIZED_ARRAYS_AND_LISTS
    [SerializeField]
    private List<GameObject> m_Initial_Animation = new List<GameObject>();
    [SerializeField]
    private List<int> m_Multiplier_Bet = new List<int>();
    #endregion

    [SerializeField] private Button[] m_Bet_Buttons;
    [SerializeField] Transform m_InfoContentReference;

    private Coroutine M_Initial_Animation = null;
    private bool m_SettingsClicked = false;
    [SerializeField]
    private SocketIOManager _socketIoManager;

    private KeyStruct m_Key;
  
    private void OnEnable()
    {
        OnFreeSpinReceived += delegate
        {
            FreeSpinAction();
           // m_UIManager.GetText(m_Key.m_text_free_spin_count).text = m_SocketManager.resultdata.payload.respinCount.ToString();
            m_UIManager.GetGameObject(m_Key.m_object_free_spin_panel).SetActive(true);
        };
        OnFreeSpinEnded += delegate
        {
            FreeSpinStopAction();
            m_UIManager.GetGameObject(m_Key.m_object_free_spin_panel).SetActive(false);
        };
        OnGameStarted += delegate
        {
            InitialSetup();
            InitiateButtons();
            BetButtonAssignClick();
            SetBetMultiplier();
            m_Bet_Buttons[0].Select();
            m_AudioController.InitialAudioSetup();
        };
    }

    private void Awake()
    {
        m_Key = new KeyStruct();
        M_Initial_Animation = StartCoroutine(InitialAnimation());
    }

    //private void Start()
    //{
    //    InitialSetup();

    //    InitiateButtons();
    //}

    private void InitialSetup()
    {
        m_UIManager.GetText(m_Key.m_text_total_auto_spin).text = AutoSpin_Count.ToString();
    }

    private void InitiateButtons()
    {
        m_UIManager.GetButton(m_Key.m_button_spin).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_spin).onClick.AddListener(delegate { OnSpinClicked?.Invoke(); m_AudioController.m_Spin_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_bet_button).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_bet_button).onClick.AddListener(delegate
        {
            m_AudioController.m_Click_Audio.Play();
            if (m_UIManager.GetGameObject(m_Key.m_object_bet_panel).activeSelf)
            {
                m_UIManager.GetGameObject(m_Key.m_object_bet_panel).SetActive(false);
            }
            else
            {
                m_UIManager.GetGameObject(m_Key.m_object_bet_panel).SetActive(true);
            }
        });

        m_UIManager.GetButton(m_Key.m_button_auto_spin).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_auto_spin).onClick.AddListener(delegate { OnAutoSpinClicked?.Invoke(); m_AudioController.m_Spin_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_auto_spin_stop).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_auto_spin_stop).onClick.AddListener(delegate { OnAutoSpinStopClicked?.Invoke(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_turbo_spin).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_turbo_spin).onClick.AddListener(delegate { TurboSpinClickedAction(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_turbo_spin_stop).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_turbo_spin_stop).onClick.AddListener(delegate { TurboSpinClickedAction(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_settings).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_settings).onClick.AddListener(delegate { SettingsButtonClickedAction(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_music).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_music).onClick.AddListener(delegate { OnSettingButtonClicked(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_info).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_info).onClick.AddListener(delegate { OnInfoButtonClicked(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_music_exit).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_music_exit).onClick.AddListener(delegate { ClosePopup("music"); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_info_exit).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_info_exit).onClick.AddListener(delegate { ClosePopup("info"); m_AudioController.m_Click_Audio.Play(); m_InfoContentReference.localPosition = new Vector3(0, 0, 0); });

        m_UIManager.GetButton(m_Key.m_auto_spin_setting_done).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_auto_spin_setting_done).onClick.AddListener(delegate { AutoSpinSettingsConfig(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_auto_spin_plus).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_auto_spin_plus).onClick.AddListener(delegate { AutoSpinPlusMinus(true); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_auto_spin_minus).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_auto_spin_minus).onClick.AddListener(delegate { AutoSpinPlusMinus(false); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_max_auto_spin).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_max_auto_spin).onClick.AddListener(delegate { AutoSpinMax(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_music_on).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_music_on).onClick.AddListener(delegate { OnMusicButtonClicked(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_music_off).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_music_off).onClick.AddListener(delegate { OnMusicButtonClicked(); m_AudioController.m_Click_Audio.Play(); });
        
         m_UIManager.GetButton(m_Key.m_button_sound_on).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_sound_on).onClick.AddListener(delegate { OnSoundButtonClicked(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_sound_off).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_sound_off).onClick.AddListener(delegate { OnSoundButtonClicked(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_game_exit).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_game_exit).onClick.AddListener(delegate { OpenPopup("quit"); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_quit_yes).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_quit_yes).onClick.AddListener(delegate { CallOnExitFunction(); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_quit_no).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_quit_no).onClick.AddListener(delegate { ClosePopup("quit"); m_AudioController.m_Click_Audio.Play(); });

        m_UIManager.GetButton(m_Key.m_button_low_balance_quit).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_low_balance_quit).onClick.AddListener(delegate { OpenCloseLowBalancePopup(false); });

        m_UIManager.GetButton(m_Key.m_button_stop_spin_button).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_stop_spin_button).onClick.AddListener(delegate
        {
            m_SlotBehaviour.StopSpinToggle = true;
            m_UIManager.GetButton(m_Key.m_button_stop_spin_button).gameObject.SetActive(false);
            m_AudioController.m_Click_Audio.Play();
        });

        Debug.Log(m_UIManager.GetButton(m_Key.m_button_close_Disconnect));
        m_UIManager.GetButton(m_Key.m_button_close_Disconnect).onClick.RemoveAllListeners();
        m_UIManager.GetButton(m_Key.m_button_close_Disconnect).onClick.AddListener(delegate { CallOnExitFunction(); m_AudioController.m_Click_Audio.Play(); _socketIoManager.ReactNativeCallOnFailedToConnect(); });
    }

    private IEnumerator InitialAnimation()
    {
        //m_UIManager.GetGameObject(m_Key.m_object_start_animation_panel).SetActive(true);
      //  m_UIManager.GetGameObject(m_Key.m_object_game_buttons_panel).SetActive(false);

        //m_Initial_Animation[0].SetActive(true);
        //m_Initial_Animation[0].GetComponent<ImageAnimation>().StartAnimation();
        //m_Initial_Animation[0].transform.DOLocalMoveX(550, 0.8f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(.2f);

        //for(int i = 1; i < m_Initial_Animation.Count; i++)
        //{
        //    GameObject _m = m_Initial_Animation[i];
        //    _m.SetActive(true);
        //    _m.GetComponent<ImageAnimation>().StartAnimation();
        //    _m.transform.DOLocalMoveY(-180, 0.8f).SetEase(Ease.Linear);
        //    yield return new WaitForSeconds(.2f);
        //}

        yield return new WaitForSeconds(1f);

        //foreach(var i in m_Initial_Animation)
        //{
        //    i.SetActive(false);
        //    i.GetComponent<ImageAnimation>().StopAnimation();
        //}

        yield return new WaitForSeconds(.2f);
        yield return new WaitUntil(() => m_SocketManager.isLoaded);

        OnGameStarted?.Invoke();

        //m_UIManager.GetGameObject(m_Key.m_object_start_animation_panel).SetActive(false);
       // m_UIManager.GetGameObject(m_Key.m_object_game_buttons_panel).SetActive(true);

        StopCoroutine(M_Initial_Animation);
    }

    internal void SetBetMultiplier()
    {

        for (int i = 1; i < m_SocketManager.initUIData.paylines.symbols.Count; i++)
        {
            Paylines m_Cur_Payline = m_SocketManager.initUIData.paylines;
            Symbol m_Cur_Symbol = m_Cur_Payline.symbols[i];

            switch (m_Cur_Symbol.name.ToUpper())
            {
                case ("777"):
                    //HACK: Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_tripple_7_combo).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();

                    m_UIManager.GetText(m_Key.m_text_any_7).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.mixedPayout.ToString())).ToString();

                    //Data Update In Paytable
                    m_UIManager.GetText(m_Key.m_text_pay_tripple_7).text = "<color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    m_UIManager.GetText(m_Key.m_text_pay_any_7).text = "Any <color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    m_UIManager.GetText(m_Key.m_text_pay_any_symbols).text = "Any <color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    break;
                case ("77"):
                    //HACK: Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_double_7_combo).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();

                    //Data Update In Paytable
                    m_UIManager.GetText(m_Key.m_text_pay_double_7).text = "<color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    break;
                case ("7"):
                    //HACK: Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_single_7_combo).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();

                    //Data Update In Paytable
                    m_UIManager.GetText(m_Key.m_text_pay_single_7).text = "<color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    break;
                case ("BAR/BAR"):
                    //HACK: Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_double_bar_combo).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();
                    m_UIManager.GetText(m_Key.m_text_any_bar).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.mixedPayout.ToString())).ToString();
                    m_UIManager.GetText(m_Key.m_text_pay_any_bar).text = "<color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();

                    //Data Update In Paytable
                    m_UIManager.GetText(m_Key.m_text_pay_double_bar).text = "<color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    break;
                case ("BAR"):
                    //HACK: Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_single_bar_combo).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();

                    //Data Update In Paytable
                    m_UIManager.GetText(m_Key.m_text_pay_single_bar).text = "<color=yellow>3X </color>" + m_Cur_Symbol.payout.ToString();
                    break;
                case ("10X"):
                    m_UIManager.GetText(m_Key.m_text_pay_x_description).text = m_Cur_Symbol.description.ToString();
                    break;
                case ("5X"):
                    break;
                case ("2X"):
                    break;
                case ("DOUBLE+"):
                    //Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_double_dollar).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();

                    //Data Update In Paytable
                    m_UIManager.GetText(m_Key.m_text_pay_dollar_description).text = m_Cur_Symbol.description.ToString();
                    break;
                case ("ADD"):
                    //Data Update In Upper Section
                    m_UIManager.GetText(m_Key.m_text_single_dollar).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();
                    break;
                case ("RESPIN"):
                    m_UIManager.GetText(m_Key.m_text_pay_respin_description).text = m_Cur_Symbol.description.ToString();
                    break;
                default:
                    m_UIManager.GetText(m_Key.m_text_any).text = (m_SocketManager.initialData.bets[m_SlotBehaviour.BetCounter] * double.Parse(m_Cur_Symbol.payout.ToString())).ToString();
                    break;
            }
        }
    }

    private void BetButtonAssignClick()
    {
        int m_data_count = m_SocketManager.initialData.bets.Count;
        for (int i = 0; i < m_Bet_Buttons.Length; i++)
        {
            Button m_Bet_Button = m_Bet_Buttons[i];
            if (i < m_data_count)
            {
                m_Bet_Button.transform.GetChild(2).GetComponent<TMP_Text>().text = (m_SocketManager.initialData.bets[i] ).ToString();
                m_Bet_Button.onClick.RemoveAllListeners();
                m_Bet_Button.onClick.AddListener(() =>
                {
                    m_SlotBehaviour.BetCounter = GetBetCounter(m_Bet_Button);
                    //Debug.Log("<color=red>" + m_SlotBehaviour.BetCounter + "</color>");
                    OnBetButtonClicked?.Invoke();
                    SetBetMultiplier();
                    m_AudioController.m_Click_Audio.Play();
                });
            }
            else
            {
                m_Bet_Button.gameObject.SetActive(false);
            }
        }
    }

    private int GetBetCounter(Button m_Click_Button)
    {
        for (int _ = 0; _ < m_Bet_Buttons.Length; _++)
        {
            if (m_Bet_Buttons[_] == m_Click_Button)
            {
                return _;
            }
        }
        return 0;
    }

    // This is the method use to trigger and off the turbo spin
    internal void TurboSpinClickedAction()
    {
        if (TurboSpin)
        {
            TurboSpin = !TurboSpin;
            m_UIManager.GetButton(m_Key.m_button_turbo_spin).gameObject.SetActive(true);
            m_UIManager.GetButton(m_Key.m_button_turbo_spin_stop).gameObject.SetActive(false);
        }
        else
        {
            TurboSpin = !TurboSpin;
            m_UIManager.GetButton(m_Key.m_button_turbo_spin).gameObject.SetActive(false);
            m_UIManager.GetButton(m_Key.m_button_turbo_spin_stop).gameObject.SetActive(true);
        }
    }

    private void SettingsButtonClickedAction()
    {
        if (m_SettingsClicked)
        {
            //DeanimateInfoMusicButton();
        }
        else
        {
            //AnimateInfoMusicButton();
        }
    }

    private void FreeSpinAction()
    {
        m_UIManager.GetGameObject(m_Key.m_object_reel_ref).GetComponent<Image>().sprite = m_UIManager.GetSprite(m_Key.m_sprite_free_spin_reel);
        m_UIManager.GetGameObject(m_Key.m_object_reel_ref).transform.GetChild(1).GetComponent<Image>().color = new Color(255, 255, 0, 255);
    }

    private void FreeSpinStopAction()
    {
        m_UIManager.GetGameObject(m_Key.m_object_reel_ref).GetComponent<Image>().sprite = m_UIManager.GetSprite(m_Key.m_sprite_normal_reel);
        m_UIManager.GetGameObject(m_Key.m_object_reel_ref).transform.GetChild(1).GetComponent<Image>().color = new Color(255, 255, 255, 255);
    }

    internal void InvokeFreeSpin()
    {
        //m_SlotBehaviour.FreeSpin(UnityEngine.Random.Range(1, 6));
        m_SlotBehaviour.FreeSpin(m_SocketManager.resultdata.payload.respinCount);
        OnFreeSpinReceived?.Invoke();
    }

    internal void InvokeFreeSpinEnd()
    {
        OnFreeSpinEnded?.Invoke();
    }

    private void OnInfoButtonClicked()
    {
        //DeanimateInfoMusicButton();
        OpenPopup("info");
    }

    internal void ReconnectionPopup()
    {
        if (m_UIManager.GetGameObject(m_Key.m_object_reconnect_popup) != null)
        {
            m_UIManager.GetGameObject(m_Key.m_object_reconnect_popup).SetActive(true);
            m_UIManager.GetGameObject(m_Key.m_object_popup_panel).SetActive(true);
        }
    }

    internal void DisconnectionPopup()
    {
        if (m_UIManager.GetGameObject(m_Key.m_object_disconnect_popup) != null && !m_IsExit)
        {
            m_UIManager.GetGameObject(m_Key.m_object_disconnect_popup).SetActive(true);
            m_UIManager.GetGameObject(m_Key.m_object_popup_panel).SetActive(true);
        }
    }

    internal void CheckAndClosePopups()
    {
        if (m_UIManager.GetGameObject(m_Key.m_object_reconnect_popup).activeInHierarchy)
        {
            ClosePopup("reconnect");
        }
        if (m_UIManager.GetGameObject(m_Key.m_object_disconnect_popup).activeInHierarchy)
        {
            ClosePopup("disconnect");
        }
    }

    private void OnSettingButtonClicked()
    {
        //DeanimateInfoMusicButton();
        OpenPopup("music");
    }

    internal void OpenCloseLowBalancePopup(bool m_config)
    {
        if (m_config)
        {
            OpenPopup("low_balance");
        }
        else
        {
            ClosePopup("low_balance");
        }
    }

    //private void AnimateInfoMusicButton()
    //{
    //    m_UIManager.GetButton(m_Key.m_button_info).GetComponent<RectTransform>().DOLocalMoveY(100f, 0.2f);
    //    //m_UIManager.GetButton(m_Key.m_button_music).GetComponent<RectTransform>().DOLocalMoveY(100f, 0.2f);
    //    m_UIManager.GetGameObject(m_Key.m_object_music_button_holder).transform.DOLocalMoveY(50f, 0.2f);

    //    m_SettingsClicked = !m_SettingsClicked;
    //}

    //private void DeanimateInfoMusicButton()
    //{
    //    m_UIManager.GetButton(m_Key.m_button_info).GetComponent<RectTransform>().DOLocalMoveY(-15f, 0.2f);
    //    //m_UIManager.GetButton(m_Key.m_button_music).GetComponent<RectTransform>().DOLocalMoveY(-15f, 0.2f);
    //    m_UIManager.GetGameObject(m_Key.m_object_music_button_holder).transform.DOLocalMoveY(-15f, 0.2f);

    //    m_SettingsClicked = !m_SettingsClicked;
    //}

    private void ClosePopup(string m_Config_String)
    {
        switch (m_Config_String)
        {
            case "info":
                m_UIManager.GetGameObject(m_Key.m_object_paytable_popup).SetActive(false);
                break;
            case "music":
                m_UIManager.GetGameObject(m_Key.m_object_settings_popup).SetActive(false);
                break;
            case "low_balance":
                m_UIManager.GetGameObject(m_Key.m_object_low_balance_popup).SetActive(false);
                break;
            case "quit":
                m_UIManager.GetGameObject(m_Key.m_object_quit_popup).SetActive(false);
                break;
             case "reconnect":
                m_UIManager.GetGameObject(m_Key.m_object_reconnect_popup).SetActive(false);
                break;
            case "disconnect":
                m_UIManager.GetGameObject(m_Key.m_object_disconnect_popup).SetActive(false);
                break;
        }

        m_UIManager.GetGameObject(m_Key.m_object_popup_panel).SetActive(false);
    }

    private void OpenPopup(string m_Config_String)
    {
        switch (m_Config_String)
        {
            case "info":
                m_UIManager.GetGameObject(m_Key.m_object_paytable_popup).SetActive(true);
                break;
            case "music":
                m_UIManager.GetGameObject(m_Key.m_object_settings_popup).SetActive(true);
                break;
            case "low_balance":
                m_UIManager.GetGameObject(m_Key.m_object_low_balance_popup).SetActive(true);
                break;
            case "quit":
                m_UIManager.GetGameObject(m_Key.m_object_quit_popup).SetActive(true);
                break;
            case "reconnect":
                m_UIManager.GetGameObject(m_Key.m_object_reconnect_popup).SetActive(true);
                break;
            case "disconnect":
                m_UIManager.GetGameObject(m_Key.m_object_disconnect_popup).SetActive(true);
                break;
        }

        m_UIManager.GetGameObject(m_Key.m_object_popup_panel).SetActive(true);
    }

    private void AutoSpinSettingsConfig()
    {
        ClosePopup("music");
    }

    private void AutoSpinPlusMinus(bool m_config)
    {
        AutoSpin_Count = int.Parse(m_UIManager.GetText(m_Key.m_text_total_auto_spin).text);
        if (m_config)
        {
            if (AutoSpin_Count < 10)
            {
                AutoSpin_Count++;
                m_UIManager.GetText(m_Key.m_text_total_auto_spin).text = AutoSpin_Count.ToString();
            }
        }
        else
        {
            if (AutoSpin_Count > 5)
            {
                AutoSpin_Count--;
                m_UIManager.GetText(m_Key.m_text_total_auto_spin).text = AutoSpin_Count.ToString();
            }
        }
    }

    private void OnMusicButtonClicked()
    {
        //DeanimateInfoMusicButton();

        GameObject m_music_obj = m_UIManager.GetButton(m_Key.m_button_music_on).gameObject;
        if (m_music_obj.activeSelf)
        {
            m_AudioController.ToggleMute(true);
            m_music_obj.SetActive(false);
            m_UIManager.GetButton(m_Key.m_button_music_off).gameObject.SetActive(true);
        }
        else
        {
            m_AudioController.ToggleMute(false);
            m_music_obj.SetActive(true);
            m_UIManager.GetButton(m_Key.m_button_music_off).gameObject.SetActive(false);
        }
    }


    private void OnSoundButtonClicked()
    {
        //DeanimateInfoMusicButton();

        GameObject m_music_obj = m_UIManager.GetButton(m_Key.m_button_sound_on).gameObject;
        if (m_music_obj.activeSelf)
        {
            m_AudioController.ToggleBG_Mute(true);
            m_music_obj.SetActive(false);
            m_UIManager.GetButton(m_Key.m_button_sound_off).gameObject.SetActive(true);
        }
        else
        {
            m_AudioController.ToggleBG_Mute(false);
            m_music_obj.SetActive(true);
            m_UIManager.GetButton(m_Key.m_button_sound_off).gameObject.SetActive(false);
        }
    }

    private void AutoSpinMax()
    {
        AutoSpin_Count = 10;
        m_UIManager.GetText(m_Key.m_text_total_auto_spin).text = AutoSpin_Count.ToString();
    }

    private void CallOnExitFunction()
    {
        m_IsExit = true;
        Debug.Log(string.Concat("<color=yellow><b>", "Exited Broo See You Next Time...", "</b></color>"));
        m_SlotBehaviour.CallCloseSocket();
        Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }

    private void OnDisable()
    {
        OnFreeSpinReceived -= FreeSpinAction;
        OnFreeSpinEnded -= FreeSpinStopAction;
        OnGameStarted -= delegate
        {
            InitialSetup();
            InitiateButtons();
            BetButtonAssignClick();
            SetBetMultiplier();
            m_Bet_Buttons[0].Select();
            m_AudioController.InitialAudioSetup();
        };
    }
}

[System.Serializable]
public struct SlotImage
{
    public List<Image> slotImages;
}