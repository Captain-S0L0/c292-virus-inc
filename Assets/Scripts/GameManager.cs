using System;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // GUI text fields
    [SerializeField]
    TextMeshProUGUI textTurn;

    [SerializeField]
    TextMeshProUGUI textStatMoney;
    [SerializeField]
    TextMeshProUGUI textStatMachines;
    [SerializeField]
    TextMeshProUGUI textStatThreat;
    [SerializeField]
    TextMeshProUGUI textStatInfectivity;

    [SerializeField]
    TextMeshProUGUI textPopDefense;
    [SerializeField]
    TextMeshProUGUI textPopInfection;
    [SerializeField]
    TextMeshProUGUI textPopMining;

    [SerializeField]
    GameObject panelPopup;
    [SerializeField]
    TextMeshProUGUI textPopup;

    // resources
    int turn = 1;

    double money = 100;
    double machines = 1;
    float threat = 0;
    float infectivity = 1;

    double changeMoney;
    double changeMachines;

    int popDefense = 0;
    int popInfection = 0;
    int popMining = 0;

    bool gameOver = false;

    void Start()
    {
        CalculateRates();
    }

    void Update()
    {
        this.UpdateText();
    }

    public void AdvanceTurn()
    {
        this.turn++;

        this.infectivity = (float)30f / (float)(turn + 50f);

        this.money += this.changeMoney;
        this.machines += this.changeMachines;

        if (UnityEngine.Random.Range(0.0f, 1.0f) < this.threat)
        {
            Popup("Something bad happened!");
        }

        if (this.money < 0)
        {
            this.gameOver = true;
            Popup("GAME OVER!\nYou ran out of money!");
        }

        CalculateRates();
    }

    public void HandlePopButton(string id)
    {
        char[] chars = id.ToCharArray();

        if (chars.Length < 2)
        {
            Debug.LogWarning("HandlePopButton ID was not 2 characters!");
            return;
        }

        switch (chars[0])
        {
            case 'd':
                if (chars[1] == '+' && (this.popDefense + this.popInfection + this.popMining + 1) <= this.machines)
                {
                    this.popDefense++;
                }
                if (chars[1] == '-' && this.popDefense > 0)
                {
                    this.popDefense--;
                }
                break;
            case 'i':
                if (chars[1] == '+' && (this.popDefense + this.popInfection + this.popMining + 1) <= this.machines)
                {
                    this.popInfection++;
                }
                if (chars[1] == '-' && this.popInfection > 0)
                {
                    this.popInfection--;
                }
                break;
            case 'm':
                if (chars[1] == '+' && (this.popDefense + this.popInfection + this.popMining + 1) <= this.machines)
                {
                    this.popMining++;
                }
                if (chars[1] == '-' && this.popMining > 0)
                {
                    this.popMining--;
                }
                break;
            default:
                Debug.LogWarning("HandlePopButton had invalid id \"" + chars[0] +"\"!");
                break;
        }

        CalculateRates();
    }

    public void ClosePopup()
    {
        if (gameOver)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            this.panelPopup.SetActive(false);
        }
    }

    void Popup(string message)
    {
        this.textPopup.text = message;
        this.panelPopup.SetActive(true);
    }

    void UpdateText()
    {
        this.textTurn.text = string.Format(CultureInfo.InvariantCulture, "Turn: {0:d}", this.turn);

        this.textStatMoney.text = string.Format(CultureInfo.InvariantCulture, "Money: ${0:f2} (+{1:f2})", this.money, this.changeMoney);
        this.textStatMachines.text = string.Format(CultureInfo.InvariantCulture, "Machines: {0:f0} (+{1:f1})", Math.Floor(this.machines), this.changeMachines);
        this.textStatThreat.text = string.Format(CultureInfo.InvariantCulture, "Threat: {0:f1}%", this.threat * 100);
        this.textStatInfectivity.text = string.Format(CultureInfo.InvariantCulture, "Infectivity: {0:f2}", this.infectivity);

        this.textPopDefense.text = string.Format(CultureInfo.InvariantCulture, "Defense {0:d}/{1:f0}", this.popDefense, Math.Floor(this.machines));
        this.textPopInfection.text = string.Format(CultureInfo.InvariantCulture, "Infection {0:d}/{1:f0}", this.popInfection, Math.Floor(this.machines));
        this.textPopMining.text = string.Format(CultureInfo.InvariantCulture, "Mining {0:d}/{1:f0}", this.popMining, Math.Floor(this.machines));
    }

    void CalculateRates()
    {
        this.threat = Mathf.Max(Mathf.Min(1f, ((float)turn * 0.0005f) + ((float)machines * 0.001f) + ((float)this.popInfection * 0.001f) - ((float)this.popDefense * 0.002f)), 0f);

        this.changeMoney = (double)this.popMining - (0.01 * (int)this.machines);
        this.changeMachines = this.popInfection * this.infectivity;
    }
}
