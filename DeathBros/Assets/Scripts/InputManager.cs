﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static Vector2 Direction;
    public static Vector2 CStick;

    public static DualInput Up, Down, Left, Right;
    public static DualInput CUp, CDown, CLeft, CRight;

    public static DualInput Shield;
    public static DualInput Jump;
    public static DualInput Attack;
    public static DualInput Special;
    public static DualInput Grab;

    public static DualInput Pause;

    public List<DualInput> Inputs;

    public bool settingInput = false;
    public static bool disableNavigation { get; private set; }
    public float waitForInputTimer;

    #region Singelton
    private static InputManager instance;
    public static InputManager Instance { get { return instance; } }
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    void Start()
    {
        waitForInputTimer = 0;

        Up = new DualInput("Up");
        Down = new DualInput("Down");
        Left = new DualInput("Left");
        Right = new DualInput("Right");

        CUp = new DualInput("CUp");
        CDown = new DualInput("CDown");
        CLeft = new DualInput("CLeft");
        CRight = new DualInput("CRight");

        Shield = new DualInput("Shield");
        Jump = new DualInput("Jump");
        Attack = new DualInput("Attack");
        Special = new DualInput("Special");
        Grab = new DualInput("Grab");

        Pause = new DualInput("Pause");

        Inputs = new List<DualInput>();
        Inputs.Add(Left);
        Inputs.Add(Right);
        Inputs.Add(Up);
        Inputs.Add(Down);

        Inputs.Add(CLeft);
        Inputs.Add(CRight);
        Inputs.Add(CUp);
        Inputs.Add(CDown);

        Inputs.Add(Shield);
        Inputs.Add(Jump);
        Inputs.Add(Attack);
        Inputs.Add(Special);
        Inputs.Add(Grab);

        Inputs.Add(Pause);
    }

    void Update()
    {
        if (settingInput)
        {
            disableNavigation = false;

            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i].SettingButton)
                {
                    disableNavigation = true;

                    Inputs[i].IsSet = false;
                    WaitForSetup(Inputs[i]);
                    {
                        if (Inputs[i].IsSet)
                        {
                            Inputs[i].SettingButton = false;
                        }
                    }
                }
            }
        }

        Direction = new Vector2(-Left.GetAxis() + Right.GetAxis(), Up.GetAxis() - Down.GetAxis());
        CStick = new Vector2(-CLeft.GetAxis() + CRight.GetAxis(), CUp.GetAxis() - CDown.GetAxis());

        //Checking Inputs
        /*
        foreach (DualInput input in Inputs)
        {
            if (input.GetButtonDown())
            {
                Debug.Log(input.InputName);
            }
        }
        */
    }

    public static bool GetButton(string inputName)
    {
        return Instance.Inputs.Find(input => input.InputName == inputName).GetButton();
    }

    public static bool GetButtonDown(string inputName)
    {
        return Instance.Inputs.Find(input => input.InputName == inputName).GetButtonDown();
    }

    public void SetInput(string inputName)
    {
        GetInput(inputName).SettingButton = true;
    }

    private DualInput GetInput(string inputName)
    {
        return Inputs.Find(x => x.InputName == inputName);
    }

    private void WaitForSetup(DualInput input)
    {
        waitForInputTimer += Time.unscaledDeltaTime;

        if (waitForInputTimer > 0.1f)
        {
            for (int i = 1; i <= 2; i++)
            {
                for (int j = 0; j < 19; j++)
                {
                    string buttonName = ("Joystick" + i + "Button" + j);
                    KeyCode currentButton = (KeyCode)Enum.Parse(typeof(KeyCode), buttonName);

                    if (Input.GetKeyDown(currentButton))
                    {
                        input.Init(currentButton);
                        waitForInputTimer = 0;
                    }
                }
            }

            for (int i = 1; i < 9; i++)
            {
                string axisName = "axis" + i;
                if (Mathf.Abs(Input.GetAxisRaw(axisName)) > 0.75f)
                {
                    input.Init(axisName, Input.GetAxisRaw(axisName));
                    waitForInputTimer = 0;
                }
            }
        }
    }
}

[System.Serializable]
public class DualInput
{
    public string InputName;// { get; set; }
    public bool SettingButton;// { get; set; }
    public bool IsSet;

    public KeyCode buttonName;
    public string axisName;
    public bool isButton;
    public float axisDirection;

    private bool axisButtonDown;

    public DualInput(string inputName)
    {
        this.InputName = inputName;
        buttonName = KeyCode.None;
        axisName = "";
        isButton = true;
        axisDirection = 0;
        SettingButton = false;

        Load();
    }

    public void Init(KeyCode buttonName)
    {
        this.buttonName = buttonName;
        this.axisName = "button";
        this.isButton = true;
        this.axisDirection = 0;
        this.SettingButton = false;
        this.IsSet = true;

        Save();
    }

    public void Init(string axisName, float axisDirection)
    {
        this.buttonName = KeyCode.None;
        this.axisName = axisName;
        this.isButton = false;
        this.axisDirection = Mathf.Sign(axisDirection);
        this.SettingButton = false;
        this.IsSet = true;

        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetString(InputName + " name", InputName);
        PlayerPrefs.SetString(InputName + " buttonName", buttonName.ToString());
        PlayerPrefs.SetString(InputName + " axisName", axisName);
        PlayerPrefs.SetInt(InputName + " isButton", isButton ? 1 : 0);
        PlayerPrefs.SetFloat(InputName + " axisDrirection", axisDirection);

        Debug.Log(InputName + " saved.");
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(InputName + " name"))
        {
            InputName = PlayerPrefs.GetString(InputName + " name");
            buttonName = (KeyCode)KeyCode.Parse(typeof(KeyCode), PlayerPrefs.GetString(InputName + " buttonName"));
            axisName = PlayerPrefs.GetString(InputName + " axisName");
            isButton = PlayerPrefs.GetInt(InputName + " isButton") == 1 ? true : false;
            axisDirection = PlayerPrefs.GetFloat(InputName + " axisDrirection");
            IsSet = true;
        }
    }

    public bool GetButton()
    {
        bool returnValue = false;

        if (axisName != "")
        {
            if (isButton)
            {
                if (Input.GetKey(buttonName))
                {
                    returnValue = true;
                }
            }
            else
            {
                if (Mathf.Abs(Input.GetAxisRaw(axisName) - axisDirection) < 0.5f)
                {
                    returnValue = true;
                }
            }
        }

        return returnValue;
    }

    public bool GetButtonDown()
    {
        bool returnValue = false;

        if (axisName != "")
        {
            if (isButton)
            {
                if (Input.GetKeyDown(buttonName))
                {
                    returnValue = true;
                }
            }
            else
            {
                if (Input.GetAxisRaw(axisName) == 0)
                    axisButtonDown = false;

                if (Mathf.Abs(Input.GetAxisRaw(axisName) - axisDirection) < 0.2f && !axisButtonDown)
                {
                    axisButtonDown = true;
                    returnValue = true;
                }
            }
        }

        return returnValue;
    }

    public float GetAxis()
    {
        float returnValue = 0;

        if (IsSet)
        {
            if (isButton)
            {
                if (Input.GetKey(buttonName))
                {
                    returnValue = 1;
                }
            }
            else
            {
                if (Mathf.Sign(Input.GetAxisRaw(axisName)) == Mathf.Sign(axisDirection)) //only count if the direction that is set up is pushed
                    returnValue = Mathf.Abs(Input.GetAxisRaw(axisName));
            }
        }

        return returnValue;
    }
}