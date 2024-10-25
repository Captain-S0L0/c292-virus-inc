using System.Globalization;
using TMPro;
using UnityEngine;

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

    // resources
    int turn = 1;

    double money = 100;
    int machines = 1;
    float threat = 0;
    float infectivity = 1;

    int popDefense = 0;
    int popInfection = 0;
    int popMining = 0;





    void Start()
    {
        
    }

    void Update()
    {
        this.UpdateText();
    }

    public void AdvanceTurn()
    {
        this.turn++;

        this.machines++;
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
                if (chars[1] == '+' && (this.popDefense + this.popInfection + this.popMining) < this.machines)
                {
                    this.popDefense++;
                }
                if (chars[1] == '-' && this.popDefense > 0)
                {
                    this.popDefense--;
                }
                break;
            case 'i':
                if (chars[1] == '+' && (this.popDefense + this.popInfection + this.popMining) < this.machines)
                {
                    this.popInfection++;
                }
                if (chars[1] == '-' && this.popInfection > 0)
                {
                    this.popInfection--;
                }
                break;
            case 'm':
                if (chars[1] == '+' && (this.popDefense + this.popInfection + this.popMining) < this.machines)
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
    }

    void UpdateText()
    {
        this.textTurn.text = string.Format(CultureInfo.InvariantCulture, "Turn: {0:d}", this.turn);

        this.textStatMoney.text = string.Format(CultureInfo.InvariantCulture, "Money: ${0:f2}", this.money);
        this.textStatMachines.text = string.Format(CultureInfo.InvariantCulture, "Machines: {0:f0}", this.machines);
        this.textStatThreat.text = string.Format(CultureInfo.InvariantCulture, "Threat: {0:f1}%", this.threat);
        this.textStatInfectivity.text = string.Format(CultureInfo.InvariantCulture, "Infectivity: {0:f2}", this.infectivity);

        this.textPopDefense.text = string.Format(CultureInfo.InvariantCulture, "Defense {0:d}/{1:d}", this.popDefense, this.machines);
        this.textPopInfection.text = string.Format(CultureInfo.InvariantCulture, "Infection {0:d}/{1:d}", this.popInfection, this.machines);
        this.textPopMining.text = string.Format(CultureInfo.InvariantCulture, "Mining {0:d}/{1:d}", this.popMining, this.machines);
    }
}
