using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    #region Singleton
    static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;
                if (instance == null)
                {
                    Debug.LogError("There's no active SocketModule object");
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    const char CHAR_TERMINATOR = ';';
    const char CHAR_COMMA = ',';

    [SerializeField]
    private UserControl userControl;

    [SerializeField]
    private InputField nickName;
    [SerializeField]
    private InputField chat;
    string myID;

    public GameObject prefabUser;
    // public GameObject user;

    Dictionary<string, UserControl> remoteUsers;
    Queue<string> commandQueue;

    private void Start()
    {
        remoteUsers = new();
        commandQueue = new();
    }

    private void Update()
    {
        ProcessQueue();
    }

    public void SendCommand(string cmd)
    {
        SocketModule.Instance.SendData(cmd);
        Debug.Log("cmd send: " + cmd);
    }

    public void QueueCommand(string cmd)
    {
        commandQueue.Enqueue(cmd);
    }

    public void ProcessQueue()
    {
        while (commandQueue.Count > 0)
        {
            string nextCommand = commandQueue.Dequeue();
            ProcessCommand(nextCommand);
        }
    }

    public void ProcessCommand(string cmd)
    {
        bool isMore = true;
        while (isMore)
        {
            Debug.Log("Process cmd: " + cmd);

            int nameIdx = cmd.IndexOf("$");
            string id = "";
            if (nameIdx > 0)
            {
                id = cmd.Substring(0, nameIdx);
            }

            int cmdIdx1 = cmd.IndexOf("#");
            if (cmdIdx1 > nameIdx)
            {
                int cmdIdx2 = cmd.IndexOf("#", cmdIdx1 + 1);
                if (cmdIdx2 > cmdIdx1)
                {
                    string command = cmd.Substring(cmdIdx1 + 1, cmdIdx2 - cmdIdx1 - 1);

                    string remain = "";
                    string nextCommand;
                    int endIdx = cmd.IndexOf(CHAR_TERMINATOR, cmdIdx2 + 1);
                    if (endIdx > cmdIdx2)
                    {
                        remain = cmd.Substring(cmdIdx2 + 1, endIdx - cmdIdx2 - 1);
                        nextCommand = cmd.Substring(endIdx + 1);
                    }
                    else
                    {
                        nextCommand = cmd.Substring(cmdIdx2 + 1);
                    }
                    Debug.Log($"command = {command}, id = {id}, remain = {remain}, nextCommand = {nextCommand}");

                    if (myID.CompareTo(id) != 0)
                    {
                        Debug.Log(command + "이거 이거");
                        switch (command)
                        {
                            case "Enter":
                                AddUser(id);
                                break;
                            case "Move":
                                SetMove(id, remain);
                                break;
                            case "Left":
                                UserLeft(id);
                                break;
                            case "Heal":
                                UserHeal(id);
                                break;
                            case "Attack":
                                Debug.Log($"Attack {id}");
                                break;
                            case "Damage":
                                TakeDamage(id);
                                break;
                        }
                    }
                    else
                    {
                        Debug.Log("Skip");
                    }
                }
                else
                {
                    isMore = false;
                }
            }
            else
            {
                isMore = false;
            }
        }
    }

    private void AddUser(string id)
    {
        if (!remoteUsers.ContainsKey(id))
        {
            Debug.Log("AddUser: " + id);
            remoteUsers.Add(id, Instantiate(prefabUser).GetComponent<UserControl>());
        }
    }

    private void UserLeft(string id)
    {
        if (remoteUsers.ContainsKey(id))
        {
            Destroy(remoteUsers[id].gameObject);
            remoteUsers.Remove(id);
        }
    }

    private void UserHeal(string id)
    {
        if (remoteUsers.ContainsKey(id))
        {
            remoteUsers[id].Revive();
        }
    }

    private void TakeDamage(string remain)
    {
        var str = remain.Split(CHAR_COMMA);
        for (int i = 0; i < str.Length; i++)
        {
            if (remoteUsers.ContainsKey(str[i]))
            {
                var uc = remoteUsers[str[i]];
                if (uc != null)
                {
                    uc.HP = -10;
                }
            }
            else
            {
                if (myID.CompareTo(str[i]) == 0)
                {
                    userControl.HP = -10;
                }
            }
        }
    }

    private void SetMove(string id, string cmdMove)
    {
        if (remoteUsers.ContainsKey(id))
        {
            string[] posStr = cmdMove.Split(CHAR_COMMA);
            remoteUsers[id].Move(new Vector3(float.Parse(posStr[0]), 0, float.Parse(posStr[1])));
        }
    }

    #region UI
    public void OnLogin()
    {
        myID = nickName.text;
        if (myID.Length > 0)
        {
            SocketModule.Instance.LogIn(nickName.text);
        }
    }

    public void OnLogOut()
    {
        SocketModule.Instance.LogOut();
        foreach (var user in remoteUsers)
        {
            Destroy(user.Value.gameObject);
        }
        remoteUsers.Clear();
    }

    public void OnRevive()
    {
        userControl.Revive();
        SendCommand("#Heal#");
    }

    public void OnMessage()
    {
        if (myID != null)
        {
            SocketModule.Instance.SendData(chat.text);
            chat.text = "";
        }
    }
    #endregion
}