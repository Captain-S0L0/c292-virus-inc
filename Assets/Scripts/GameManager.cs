using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.UI;

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
    GameObject panelHighScore;
    [SerializeField]
    TextMeshProUGUI textUsername;

    [SerializeField]
    GameObject panelUpgrades;
    [SerializeField]
    GameObject panelScores;

    [SerializeField]
    GameObject panelAbout;
    [SerializeField]
    GameObject panelDebug;

    // resources
    int turn = 1;

    double money = 100;
    double maxMoney = 100;
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

    double[] upgradePrices = {7777, 100, 666, 500};
    int[] upgradeCounts = { 1, 1, 1, 1 };

    double popDefense = 0;
    double popInfection = 0;
    double popMining = 0;

    bool x10 = false;
    bool x100 = false;
    bool escape = false;

    bool gameOver = false;

    Event[] goodEventsWeighted = new Event[0];
    Event[] badEventsWeighted = new Event[0];

    Event[] activeEvents = new Event[0];

    Scores scores = new Scores();
    [SerializeField]
    TextMeshProUGUI[] textScores;
    private const string SCORE_FILE = "scores.json";
    private const int SCORES_LENGTH = 10;

    [SerializeField]
    GameObject[] upgradeButtons = new GameObject[0];

    void Start()
    {
        this.CalculateRates();

        this.ConstructEvents();
        this.LoadScores();
    }

    void Update()
    {
        this.UpdateText();
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
                this.infectivityMultiplier *= 10;
                this.Popup("A crazy zero day dropped! You can infect a lot of machines today before it gets patched!");
            },
            1,
            () =>
            {
                this.infectivityMultiplier /= 10;
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
                this.Popup(string.Format(CultureInfo.InvariantCulture, "One of your cripto wallets got compromised! You lost ${0:g5}.", lost));
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

    void LoadScores()
    {
        if (File.Exists(SCORE_FILE))
        {
            this.scores = JsonUtility.FromJson<Scores>(File.ReadAllText(SCORE_FILE));
        }
        else
        {
            this.scores = new Scores();
        }
        UpdateScores();
    }

    void SaveScores()
    {
        File.WriteAllText(SCORE_FILE, JsonUtility.ToJson(this.scores));
    }

    void UpdateScores()
    {
        for (int i = 0; i < SCORES_LENGTH; i++)
        {
            string name = this.scores.GetName(i);
            if (name == null || name.Length == 0)
            {
                this.textScores[i].text = string.Format(CultureInfo.InvariantCulture, "#{0:d}", i + 1);
            }
            else
            {
                this.textScores[i].text = string.Format(CultureInfo.InvariantCulture, "#{0:d} {1} - ${2:g5}", i + 1, name, this.scores.GetValue(i));
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

        Event e = weightedList[Mathf.FloorToInt(Mathf.Min(Mathf.Max(UnityEngine.Random.Range(0.0f, 1.0f) * weightedList.Length, 0.0f), weightedList.Length - 1))];

        e.applyAction?.Invoke();

        Event[] oldEvents = this.activeEvents;
        this.activeEvents = new Event[oldEvents.Length + 1];
        for (int i = 0; i < oldEvents.Length; i++)
        {
            this.activeEvents[i] = oldEvents[i];
        }
        this.activeEvents[oldEvents.Length] = e.Copy();
    }

    public void AdvanceTurn()
    {
        if (this.gameOver)
        {
            return;
        }

        this.turn++;

        this.money += this.changeMoney;
        this.machines += this.changeMachines;

        if (this.money > this.maxMoney)
        {
            this.maxMoney = this.money;
        }

        TickEvents();

        if (this.threat >= 1.0f)
        {
            GameOver("You got caught!");
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
            GameOver("You ran out of money!");
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

        this.textStatMoney.text = string.Format(CultureInfo.InvariantCulture, "Money: ${0:g5} (+{1:g5})", this.money, this.changeMoney);
        this.textStatMachines.text = string.Format(CultureInfo.InvariantCulture, "Machines: {0:g5} (+{1:g5})", Math.Floor(this.machines), this.changeMachines);
        this.textStatThreat.text = string.Format(CultureInfo.InvariantCulture, "Threat: {0:f1}%", this.threat * 100);
        this.textStatInfectivity.text = string.Format(CultureInfo.InvariantCulture, "Infectivity: {0:g5}", this.infectivity);

        this.textPopDefense.text = string.Format(CultureInfo.InvariantCulture, "Defense {0:g5}/{1:g5}", this.popDefense, Math.Floor(this.machines));
        this.textPopInfection.text = string.Format(CultureInfo.InvariantCulture, "Infection {0:g5}/{1:g5}", this.popInfection, Math.Floor(this.machines));
        this.textPopMining.text = string.Format(CultureInfo.InvariantCulture, "Mining {0:g5}/{1:g5}", this.popMining, Math.Floor(this.machines));

        this.upgradeButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Lucky {0:d} - ${1:g5}\n1% more lucky", this.upgradeCounts[0], this.upgradePrices[0]);
        this.upgradeButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Zero Day Hunt {0:d} - ${1:g5}\n200% infection rate", this.upgradeCounts[1], this.upgradePrices[1]);
        this.upgradeButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Market Stalker {0:d} - ${1:g5}\n200% crypto profits", this.upgradeCounts[2], this.upgradePrices[2]);
        this.upgradeButtons[3].GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Stealthy {0:d} - ${1:g5}\n50% as detectable", this.upgradeCounts[3], this.upgradePrices[3]);
    }

    void CalculateRates()
    {
        // as some events remove machines, make sure too many aren't assigned
        if ((this.popDefense + this.popInfection + this.popMining) > (int)this.machines)
        {
            this.popDefense = Math.Max((int)this.machines - (this.popInfection + this.popMining), 0);
            if ((this.popInfection + this.popMining) > (int)this.machines)
            {
                this.popInfection = Math.Max((int)this.machines - this.popMining, 0);
                if (this.popMining > (int)this.machines)
                {
                    this.popMining = Math.Max((int)this.machines, 0);
                }
            }
        }

        this.infectivity = ((float)30f / (float)(turn + 50f)) * this.infectivityMultiplier;

        this.changeMoney = ((double)this.popMining * this.moneyMultiplier) - (this.upkeepMultiplier * (int)this.machines);
        this.changeMachines = this.popInfection * this.infectivity;

        this.threat = Mathf.Max(Mathf.Min(1f, (float)(((((machines - 1) * 0.0005f) + (this.changeMachines * 0.002f)) * (1 + (this.turn * 0.002f))) - (this.popDefense * 0.002f)) * this.threatMultiplier), 0f);
    }

    void GameOver(string message)
    {
        message = "GAME OVER!\n" + message;
        this.gameOver = true;

        if (this.scores.IsHighScore(this.maxMoney)) {
            this.textPopup.text = message;
            this.panelHighScore.SetActive(true);
        }
        else {
            Popup(message);
        }
    }

    void CloseWindow()
    {
        if (this.panelPopup.activeSelf)
        {
            if (this.gameOver)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                this.panelPopup.SetActive(false);
            }
            return;
        }

        if (this.panelHighScore.activeSelf)
        {
            this.panelHighScore.SetActive(false);
            string name = this.textUsername.text;
            if (name.Length <= 1)
            {
                name = "Anonymous";
            }
            this.scores.AddScore(this.maxMoney, name);
            this.SaveScores();
            this.panelPopup.SetActive(true);
        }

        if (this.panelUpgrades.activeSelf)
        {
            this.panelUpgrades.SetActive(false);
        }

        if (this.panelScores.activeSelf)
        {
            this.panelScores.SetActive(false);
        }

        if (this.panelDebug.activeSelf)
        {
            this.panelDebug.SetActive(false);
        }

        if (this.panelAbout.activeSelf)
        {
            this.panelAbout.SetActive(false);
        }
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
                // defense buttons
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }
                if (chars[1] == '+')
                {
                    this.popDefense = Math.Min(this.popDefense + mod, Math.Floor(this.machines) - (this.popInfection + this.popMining));
                }
                if (chars[1] == '-')
                {
                    this.popDefense = Math.Max(this.popDefense - mod, 0);
                }
                break;
            case 'i':
                // infection buttons
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }
                if (chars[1] == '+')
                {
                    this.popInfection = Math.Min(this.popInfection + mod, Math.Floor(this.machines) - (this.popDefense + this.popMining));
                }
                if (chars[1] == '-')
                {
                    this.popInfection = Math.Max(this.popInfection - mod, 0);
                }
                break;
            case 'm':
                // mining buttons
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }
                if (chars[1] == '+')
                {
                    this.popMining = Math.Min(this.popMining + mod, Math.Floor(this.machines) - (this.popInfection + this.popDefense));
                }
                if (chars[1] == '-')
                {
                    this.popMining = Math.Max(this.popMining - mod, 0);
                }
                break;
            case 'o':
                // open window
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }
                if (chars[1] == 'u')
                {
                    this.panelUpgrades.SetActive(true);
                }
                if (chars[1] == 's')
                {
                    this.panelScores.SetActive(true);
                }
                if (chars[1] == 'd')
                {
                    this.panelDebug.SetActive(true);
                }
                if (chars[1] == 'a')
                {
                    this.panelAbout.SetActive(true);
                }
                break;
            case 'c':
                // close window
                this.CloseWindow();

                break;
            case 'z':
                // debug
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }

                if (chars[1] == 'l')
                {
                    this.CloseWindow();
                    GameOver("debug");
                }
                if (chars[1] == 'm')
                {
                    this.money = Double.PositiveInfinity;
                }
                if (chars[1] == 'g')
                {
                    this.luck = 1.0f;
                }

                break;
            case 'u':
                // upgrade
                if (chars.Length < 2)
                {
                    Debug.LogWarning("Button had empty secondary id!");
                    return;
                }
                if (chars[1] == '0')
                {
                    if (this.money < this.upgradePrices[0])
                    {
                        this.Popup("You cannot afford that!");
                        return;
                    }
                    this.money -= this.upgradePrices[0];
                    this.luck += 0.01f;
                    this.upgradePrices[0] = this.upgradePrices[0] * 1000 + 777;
                    this.upgradeCounts[0]++;
                }
                if (chars[1] == '1')
                {
                    if (this.money < this.upgradePrices[1])
                    {
                        this.Popup("You cannot afford that!");
                        return;
                    }
                    this.money -= this.upgradePrices[1];
                    this.infectivityMultiplier *= 2f;
                    this.upgradePrices[1] = this.upgradePrices[1] * 100;
                    this.upgradeCounts[1]++;
                }
                if (chars[1] == '2')
                {
                    if (this.money < this.upgradePrices[2])
                    {
                        this.Popup("You cannot afford that!");
                        return;
                    }
                    this.money -= this.upgradePrices[2];
                    this.moneyMultiplier *= 2f;
                    this.upgradePrices[2] = this.upgradePrices[2] * 1000 + 666;
                    this.upgradeCounts[2]++;
                }
                if (chars[1] == '3')
                {
                    if (this.money < this.upgradePrices[3])
                    {
                        this.Popup("You cannot afford that!");
                        return;
                    }
                    this.money -= this.upgradePrices[3];
                    this.threatMultiplier /= 2f;
                    this.upgradePrices[3] = this.upgradePrices[3] * 100;
                    this.upgradeCounts[3]++;
                }
                break;
            case 's':
                this.scores = new Scores();
                this.UpdateScores();
                break;
            default:
                Debug.LogWarning("Button had unhandled id \"" + id + "\"!");
                break;
        }

        CalculateRates();
    }

    public void OnX10(InputAction.CallbackContext context)
    {
        this.x10 = context.ReadValue<float>() == 1.0f;
    }

    public void OnX100(InputAction.CallbackContext context)
    {
        this.x100 = context.ReadValue<float>() == 1.0f;
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() == 1.0f)
        {
            if (!this.escape)
            {
                this.CloseWindow();
                this.escape = true;
            }
        }
        else
        {
            this.escape = false;
        }
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

    [Serializable]
    public class Scores
    {
        public double[] values = new double[SCORES_LENGTH];
        public string[] names = new string[SCORES_LENGTH];

        public void AddScore(double value, string name)
        {
            for (int i = 0; i < SCORES_LENGTH; i++)
            {
                if (value > this.values[i])
                {
                    for (int j = SCORES_LENGTH - 1; j > i; j--)
                    {
                        this.values[j] = this.values[j - 1];
                        this.names[j] = this.names[j - 1];
                    }
                    this.values[i] = value;
                    this.names[i] = name;
                    return;
                }
            }
        }

        public bool IsHighScore(double value)
        {
            for (int i = 0; i < SCORES_LENGTH; i++)
            {
                if (value > this.values[i])
                {
                    return true;
                }
            }
            return false;
        }

        public double GetValue(int index)
        {
            return this.values[index];
        }

        public string GetName(int index)
        {
            return this.names[index];
        }
    }
}
