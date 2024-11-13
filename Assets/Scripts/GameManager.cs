using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [SerializeField]
    GameObject panelUpgrades;

    // resources
    int turn = 1;

    double money = 100;
    double machines = 1;
    float threat = 0;
    float infectivity = 1;
    
    double changeMoney;
    double changeMachines;

    float threatMultiplier = 1;
    float moneyMultiplier = 1;
    float infectivityMultiplier = 1;
    float upkeepMultiplier = 0.01f;
    float luck = 0.03f;

    int popDefense = 0;
    int popInfection = 0;
    int popMining = 0;

    bool x10 = false;
    bool x100 = false;

    bool gameOver = false;

    Event[] goodEventsWeighted = new Event[0];
    Event[] badEventsWeighted = new Event[0];

    Event[] activeEvents = new Event[0];

    void Start()
    {
        this.CalculateRates();

        this.ConstructEvents();
    }

    void ConstructEvents()
    {
        Event[] goodEvents = new Event[] {
            new Event(1, () =>
            {
                float percentGained = UnityEngine.Random.Range(0.01f, 0.5f);

                int gained = (int)Mathf.Min((float)this.machines * percentGained, 1000f);

                this.machines += gained;
                this.Popup(string.Format(CultureInfo.InvariantCulture, "You found {0:d} perfectly good computers in the local dump!", gained));
            }),
            new Event(1, () =>
            {
                this.infectivity *= 10;
                this.Popup("A crazy zero day dropped! You can infect a lot of machines today before it gets patched!");
            }),
            new Event(1, () =>
            {
                this.money += 100;
                this.Popup("You got $100 from your grandmother!");
            }),
            new Event(1, () =>
            {
                this.threatMultiplier *= 0.01f;
                this.Popup("Cybersecurity employees are on strike for a week! Very few are still working to find hackers!");
            },
            7,
            () =>
            {
                this.threatMultiplier *= 100f;
            })
        };
        Event[] badEvents = new Event[] {
            new Event(1, () =>
            {
                float percentLost = UnityEngine.Random.Range(0.01f, 0.5f);

                int lost = (int)Mathf.Min((float)this.machines * percentLost, 1000f);

                this.machines -= lost;
                this.Popup(string.Format(CultureInfo.InvariantCulture, "Unfortunately, {0:d} machines had your virus removed from them.", lost));
            }),
            new Event(1, () =>
            {
                this.threatMultiplier *= 5f;

                this.Popup("Cybersecurity employees are on high alert this week!");
            },
            7,
            () =>
            {
                this.threatMultiplier *= 0.2f;
            }),
            new Event(1, () =>
            {
                this.infectivityMultiplier *= 0.1f;
                this.Popup("One of your main infection vectors got patched! It will take you a bit to find another...");
            },
            7,
            () =>
            {
                this.infectivityMultiplier *= 10f;
            }),
            new Event(1, () =>
            {
                float percentLost = UnityEngine.Random.Range(0.01f, 0.5f);

                double lost = Mathf.Min((float)this.money * percentLost, 1000f);

                this.money -= lost;
                this.Popup(string.Format(CultureInfo.InvariantCulture, "One of your cripto wallets got compromised! You lost ${0:f2}.", lost));
            })
        };

        int totalWeightGood = 0;
        int totalWeightBad = 0;

        foreach (Event e in goodEvents)
        {
            totalWeightGood += e.weight;
        }
        foreach (Event e in badEvents)
        {
            totalWeightBad += e.weight;
        }

        this.goodEventsWeighted = new Event[totalWeightGood];
        this.badEventsWeighted = new Event[totalWeightBad];

        int indexAll = 0;

        foreach (Event e in goodEvents)
        {
            for (int i = 0; i < e.weight; i++)
            {
                this.goodEventsWeighted[indexAll++] = e;
            }
        }
        indexAll = 0;
        foreach (Event e in badEvents)
        {
            for (int i = 0;i < e.weight; i++)
            {
                this.badEventsWeighted[indexAll++] = e;
            }
        }
    }

    void TickEvents()
    {
        int activeCount = 0;

        Event[] oldEvents = this.activeEvents;

        foreach (Event e in oldEvents)
        {
            e.duration--;
            if (e.duration > 0)
            {
                activeCount++;
            }
            else
            {
                e.removeAction?.Invoke();
            }
        }

        this.activeEvents = new Event[activeCount];
        int i = 0;

        foreach (Event e in oldEvents)
        {
            if (e.duration > 0)
            {
                this.activeEvents[i++] = e;
            }
        }
    }

    void ApplyRandomEvent(bool good)
    {
        Event[] weightedList;
        if (good)
        {
            weightedList = this.goodEventsWeighted;
        }
        else
        {
            weightedList = this.badEventsWeighted;
        }

        if (weightedList.Length == 0)
        {
            return;
        }

        Event e = weightedList[Mathf.FloorToInt(Mathf.Max(Mathf.Min(UnityEngine.Random.Range(0.0f, 1.0f) * weightedList.Length, 0.0f), weightedList.Length - 1))];

        e.applyAction?.Invoke();

        Event[] oldEvents = this.activeEvents;
        this.activeEvents = new Event[oldEvents.Length + 1];
        for (int i = 0; i < oldEvents.Length; i++)
        {
            this.activeEvents[i] = oldEvents[i];
        }
        this.activeEvents[oldEvents.Length] = e.Copy();
    }

    void Update()
    {
        this.UpdateText();
    }

    public void AdvanceTurn()
    {
        this.turn++;

        this.money += this.changeMoney;
        this.machines += this.changeMachines;

        TickEvents();

        if (this.threat >= 1.0f)
        {
            this.gameOver = true;
            Popup("GAME OVER!\nYou got caught!");
            return;
        }

        if (UnityEngine.Random.Range(0.0f, 1.0f) < this.threat)
        {
            ApplyRandomEvent(false);
        }
        else if (UnityEngine.Random.Range(0.0f, 1.0f) < this.luck)
        {
            ApplyRandomEvent(true);
        }

        if (this.money < 0)
        {
            this.gameOver = true;
            Popup("GAME OVER!\nYou ran out of money!");
            return;
        }

        CalculateRates();
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
        this.infectivity = (float)30f / (float)(turn + 50f);

        this.changeMoney = ((double)this.popMining * this.moneyMultiplier) - (this.upkeepMultiplier * (int)this.machines);
        this.changeMachines = this.popInfection * this.infectivity;

        this.threat = Mathf.Max(Mathf.Min(1f, ((((float)(machines - 1) * 0.0005f) + ((float)this.changeMachines * 0.002f)) * (1 + (this.turn * 0.002f))) - (this.popDefense * 0.002f)), 0f);
        this.threat *= this.threatMultiplier;
    }

    double GetMoney()
    {
        return this.money;
    }

    public void HandleButton(string id)
    {
        char[] chars = id.ToCharArray();

        if (chars.Length < 1)
        {
            Debug.LogWarning("Button id was empty!");
            return;
        }

        int mod = 1;

        if (this.x10)
        {
            mod *= 10;
        }
        if (this.x100)
        {
            mod *= 100;
        }

        switch (chars[0])
        {
            case 'd':
                if (chars[1] == '+')
                {
                    this.popDefense = Math.Min(this.popDefense + this.popInfection + this.popMining + mod, (int)this.machines - (this.popInfection + this.popMining));
                }
                if (chars[1] == '-')
                {
                    this.popDefense = Math.Max(this.popDefense - mod, 0);
                }
                break;
            case 'i':
                if (chars[1] == '+')
                {
                    this.popInfection = Math.Min(this.popDefense + this.popInfection + this.popMining + mod, (int)this.machines - (this.popDefense + this.popMining));
                }
                if (chars[1] == '-')
                {
                    this.popInfection = Math.Max(this.popInfection - mod, 0);
                }
                break;
            case 'm':
                if (chars[1] == '+')
                {
                    this.popMining = Math.Min(this.popDefense + this.popInfection + this.popMining + mod, (int)this.machines - (this.popInfection + this.popDefense));
                }
                if (chars[1] == '-')
                {
                    this.popMining = Math.Max(this.popMining - mod, 0);
                }
                break;
            case 'w':
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }
                if (chars[1] == 'o')
                {
                    this.panelUpgrades.SetActive(true);
                }
                if (chars[1] == 'c')
                {
                    this.panelUpgrades.SetActive(false);
                }
                break;
            default:
                Debug.LogWarning("Button had unhandled id \"" + id + "\"!");
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

    public void OnX10(InputAction.CallbackContext context)
    {
        this.x10 = context.ReadValue<float>() == 1.0f;
    }

    public void OnX100(InputAction.CallbackContext context)
    {
        this.x100 = context.ReadValue<float>() == 1.0f;
    }

    private class Event {
        public delegate void Action();

        public int weight;
        public Action applyAction;
        public int duration;
        public Action removeAction;

        public Event(int weight, Action apply)
        {
            this.weight = weight;
            this.applyAction = apply;
            this.duration = 1;
            this.removeAction = null;
        }

        public Event(int weight, Action apply, int duration, Action remove)
        {
            this.weight = weight;
            this.applyAction = apply;
            this.duration = duration;
            this.removeAction = remove;
        }

        public Event Copy()
        {
            return new Event(this.weight, this.applyAction, this.duration, this.removeAction);
        }
    }
}
