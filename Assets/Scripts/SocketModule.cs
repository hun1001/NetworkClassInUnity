using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class SocketModule : MonoBehaviour
{
    #region Singleton
    static SocketModule instance = null;
    public static SocketModule Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(SocketModule)) as SocketModule;
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

    private TcpClient clientSocket;
    private NetworkStream serverStream = default(NetworkStream);
    private string nickName;

    bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void LogIn(string id)
    {
        if (isRunning == false)
        {
            clientSocket = new TcpClient();
            IPAddress addess = IPAddress.Parse("127.0.0.1");
            clientSocket.Connect(addess, 8888);
            serverStream = clientSocket.GetStream();

            byte[] outStream = Encoding.UTF8.GetBytes(id + '$');
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            Thread ctThread = new Thread(GetMessage);
            ctThread.Start();
            isRunning = true;
            nickName = id;
        }
    }

    public void SendData(string str)
    {
        if (isRunning && serverStream != null)
        {
            byte[] outStream = Encoding.UTF8.GetBytes('$' + str);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }
    }

    private void StopThread()
    {
        isRunning = false;
    }

    public void LogOut()
    {
        if (isRunning)
        {
            StopThread();
            nickName = "";
        }

        if (serverStream != null)
        {
            serverStream.Close();
            serverStream = null;
        }

        clientSocket.Close();
    }

    private void GetMessage()
    {
        byte[] inStream = new byte[1024];
        string returnData = "";
        try
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = clientSocket.ReceiveBufferSize;
                int numBytesRead;

                if (serverStream.DataAvailable)
                {
                    numBytesRead = serverStream.Read(inStream, 0, inStream.Length);
                    returnData += Encoding.UTF8.GetString(inStream, 0, numBytesRead);
                }
                GameManager.Instance.QueueCommand(returnData);
                Debug.Log(returnData);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            StopThread();
        }
    }
}

/*
모험심(직무개발과 연관이 되어야한다) <- 내가 직무를 잘할 수 있는것과 연결되어야 됨
모험심만 있으면 추상적이다. 이럴경우 그 단어에 대한 정의를 추가하기
팀이 있다면 팀의 인원까지 + 팀의 목표도 기입
자신이 경험한 일에서 자신의 강점과 연결되어야 된다
자신이 했던 일을 구체적인 숫자와 수치로 연결하는게 필요하다
마지막에 강점과 연결되는 프로젝트를 진행하며 느낀점 적기

에피소드 정리할때 2가지 카테고리로 정리하기
1. 직무와 관련된 카데고리 (강점과 연결)
2. 팀원들과 적극적으로 소통하는 카데고리 : 팀워크
소통 강점/성격
이게 팀프로젝트에 어떤 영향을 주었는지

책임감은 직무도 되지만 소통도 가능하다

1. 자신의 강점 PR
2. 상황설명
3. 강점에 대한 재정의
4. 자신이 맡은 역할에서 어려웠던 점
5. 그에따른 강점과 연결된 행동
6. 행동에 따른 결과
7. 결과
8. 마무리는 웬만하면 느낀점
*/