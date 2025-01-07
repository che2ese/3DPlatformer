using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private readonly string version = "1.0"; // 게임 버전
    private string userId; // 유저 ID

    //룸 ID를 입력할 인풋 필드
    public TMP_InputField roomInputField;

    public TMP_Text NickName; // 닉네임 UI 텍스트
    const string URL = "https://script.google.com/macros/s/AKfycbyTfXkFJW5FQh_lDEg0R6T5YTef4z7yCbmXVKe7SNdpAaKncquSuyYYwBcFlPIZgZs/exec";

    //룸 목록에 대한 데이터 저장
    Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    //룸 목록을 표시할 프리팹
    GameObject roomItemPrefab;
    //룸 목록이 표시될 scroll content
    public Transform scrollContent;

    //네트워크 접속은 Start()보다 먼저 실행되어야한다. Awake() 함수 사용
    private void Awake()
    {
        //씬 동기화. 맨 처음 접속한 사람이 방장이 된다.
        PhotonNetwork.AutomaticallySyncScene = true;
        //버전 할당. 위에 string으로 만들었던 version을 쓴다.
        PhotonNetwork.GameVersion = version;
        //포톤 서버와의 통신 횟수를 로그로 찍기. 기본값 : 30
        Debug.Log(PhotonNetwork.SendRate); //제대로 통신이 되었다면 30이 출력된다.

        roomItemPrefab = Resources.Load<GameObject>("RoomItem");

        //포톤 서버에 접속
        if (PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    //CallBack 함수
    public override void OnConnectedToMaster() //정상적으로 마스터 서버에 접속이 되면 호출된다.
    {
        //마스터 서버에 접속이 되었는지 디버깅 한다.
        Debug.Log("Connected to Master");
        Debug.Log($"In Lobby = {PhotonNetwork.InLobby}"); //로비에 들어와 있으면 True, 아니면 False 반환. Master 서버에는 접속했지만 로비에는 아니므로 False 반환된다.
        //로비 접속
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() //로비에 접속이 제대로 되었다면 해당 콜백함수 호출
    {
        Debug.Log($"In Lobby = {PhotonNetwork.InLobby}"); //로비에 접속이 되었다면 True가 반환 될 것이다.
        //방 접속 방법은 두 가지. 1.랜덤 매치메이킹, 2.선택된 방 접속
        //PhotonNetwork.JoinRandomRoom();
    }
    //방 생성이 되지 않았으면 오류 콜백 함수 실행
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRandom Failed {returnCode}: {message}");

        OnMakeRoomClick(); //오류 나는 것을 방지하기 위해서.

        //룸 속성 설정
        //RoomOptions roomOptions = new RoomOptions();
        //룸의 접속할 수 있는 최대 접속자 수 최대 제한을 해놔야 CCU를 제한할 수 있다.
        //roomOptions.MaxPlayers = 20;
        //룸 오픈 여부
        //roomOptions.IsOpen = true;
        //로비에서 룸의 목록에 노출시킬지 여부. 공개방 생성
        //roomOptions.IsVisible = true;
        //룸 생성
        //PhotonNetwork.CreateRoom("Room1", roomOptions); //룸 이름과 룸 설정. 우리는 roomOptions에 설정을 이미 해놓았다.
    }

    //제대로 룸이 있다면 다음의 콜백 함수를 호출한다.
    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room");
        Debug.Log($"Room Name: {PhotonNetwork.CurrentRoom.Name}");
    }

    //룸에 들어왔을 때 콜백 함수
    public override void OnJoinedRoom()
    {
        Debug.Log($"In Room = {PhotonNetwork.InRoom}");
        Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");
        //접속한 사용자 닉네임 확인
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            //플레이어 닉네임, 유저의 고유값 가져오기
            Debug.Log($"플레이어 닉네임: {player.Value.NickName}, 유저 고유값: {player.Value.ActorNumber}");
        }

        //플레이어 생성 포인트 그룹 배열을 받아오기. 포인트 그룹의 자식 오브젝트의 Transform 받아오기.
        //Transform[] points = GameObject.Find("PointGroup").GetComponentsInChildren<Transform>();
        //1부터 배열의 길이까지의 숫자 중 Random한 값을 추출
        //int idx = Random.Range(1, points.Length);
        //플레이어 프리팹을 추출한 idx 위치와 회전 값에 생성. 네트워크를 통해서.
        //PhotonNetwork.Instantiate("Player", points[idx].position, points[idx].rotation, 0);
        
        //마스터 클라이언트인 경우 게임 씬 로딩
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("RoomScene"); //씬 이름으로 불러오기
        }
    }
    void Start()
    {
        StartCoroutine(GetNicknameFromGoogleSheet());
    }
    string SetRoomName()
    {
        //비어있으면 랜덤한 룸 이름. 그렇지 않으면 가져오도록.
        if (string.IsNullOrEmpty(roomInputField.text))
        {
            roomInputField.text = $"ROOM_{Random.Range(1, 101):000}";
        }
        return roomInputField.text;
    }
    public void OnMakeRoomClick() //방 생성 버튼 매핑 함수
    {
        //룸 속성 정의
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 2;
        ro.IsOpen = true;
        //공개방 설정
        ro.IsVisible = true;
        //룸 생성
        PhotonNetwork.CreateRoom(SetRoomName(), ro); //고정된 값이 아니라 유저가 타이핑한 값을 받아온다.
    }
    //방 리스트를 수신하는 콜백 함수 생성
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 삭제된 RoomItem 프리팹을 저장할 임시변수
        GameObject tempRoom = null;
        foreach (var roomInfo in roomList)
        {
            // 룸이 삭제된 경우
            if (roomInfo.RemovedFromList == true)
            {
                // 딕셔너리에서 룸 이름으로 검색해 저장된 RoomItem 프리팹을 추출
                rooms.TryGetValue(roomInfo.Name, out tempRoom);
                // RoomItem 프리팹 삭제
                Destroy(tempRoom);
                // 딕셔너리에서 해당 룸 이름의 데이터를 삭제
                rooms.Remove(roomInfo.Name);

            }
            else // 룸 정보가 변경된 경우
            {
                // 룸 이름이 딕셔너리에 없는 경우 새로 추가
                if (rooms.ContainsKey(roomInfo.Name) == false)
                {
                    // RoomInfo 프리팹을 scrollContent 하위에 생성
                    GameObject roomPrefab = Instantiate(roomItemPrefab, scrollContent);
                    // 룸 정보를 표시하기 위해 RoomInfo 정보 전달
                    roomPrefab.GetComponent<RoomData>().RoomInfo = roomInfo;
                    // 딕셔너리 자료형에 데이터 추가
                    rooms.Add(roomInfo.Name, roomPrefab);
                }
                else  // 룸 이름이 딕셔너리에 없는 경우에 룸 정보를 갱신
                {
                    rooms.TryGetValue(roomInfo.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = roomInfo;
                }
            }
            Debug.Log($"Room={roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})");
        }
    }
    IEnumerator GetNicknameFromGoogleSheet()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "getNickname");
        form.AddField("id", PlayerPrefs.GetString("userId")); // 로그인 시 저장된 ID 사용

        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text;
                Debug.Log("응답: " + response);

                var serverResponse = JsonUtility.FromJson<ServerResponse>(response);
                if (serverResponse.result == "OK")
                {
                    userId = serverResponse.msg; // 닉네임을 userId로 설정
                    PhotonNetwork.NickName = userId; // Photon의 닉네임 설정
                    NickName.text = userId; // UI에 닉네임 표시
                }
                else
                {
                    Debug.LogError("닉네임을 가져올 수 없습니다: " + serverResponse.msg);
                }
            }
            else
            {
                Debug.LogError("서버 요청 실패: " + www.error);
            }
        }
        NickName.text = userId;
    }
}
