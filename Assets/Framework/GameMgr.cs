using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum EnGameState
{
    Ready = 1,
    Start,
    Pause,
    Over,
}

public class GameMgr : MgrBase
{
    public static GameMgr It;
    void Awake() { It = this; }

    public EnGameState State;
    public Transform GameRoot;
    public LineRenderer Line;

    private List<NodeData> m_Nodes;
    private int m_TmpIndex;     // drag点的序号
    private Vector3 m_TmpPos;   // drag点的坐标

    public override void Init()
    {
        base.Init();
        m_Nodes = new List<NodeData>();
        m_TmpIndex = -1;
        m_TmpPos = Vector3.zero;
        GameReady();
    }

    public override void UnInit()
    {
        base.UnInit();
    }

    #region 游戏流程控制
    public void GameReady()
    {
        State = EnGameState.Ready;
        UIMgr.It.OpenUI("UIHome");
    }

    public void GameStart()
    {
        State = EnGameState.Start;
        m_TmpIndex = -1;
        UIMgr.It.OpenUI("UIGame");
        LoadMap(1);
        InitLine();
    }

    public void GamePause()
    {
        State = EnGameState.Pause;
    }

    public void GameResume()
    {
        State = EnGameState.Start;
    }

    public void GameOver()
    {
        State = EnGameState.Over;
    }


    #endregion

    void LoadMap(int idx)
    {
        MapConfig map = Config.MapCfg[idx - 1];
        NodeData node;
        Transform tf;
        for (int i = 0; i < map.nodes.Length; i++)
        {
            node = new NodeData();
            node.cfg = map.nodes[i];
            node.obj = ResMgr.It.CreatePrefab(Config.NodePrefab[(int)node.cfg.type - 1]);
            tf = node.obj.transform;
            tf.parent = GameRoot;
            tf.localPosition = new Vector3(node.cfg.x, node.cfg.y);
            tf.localScale = new Vector3(2, 2);
            m_Nodes.Add(node);
        }
    }

    void InitLine()
    {
        NodeData node = m_Nodes[0];
        Line.positionCount = 2;
        Line.SetPosition(0, node.obj.transform.localPosition);
        node = m_Nodes[m_Nodes.Count - 1];
        Line.SetPosition(1, node.obj.transform.localPosition);
    }

    #region 线条操作


    private void Update()
    {
        if (State != EnGameState.Start) return;
        if (Input.GetMouseButton(0))
        {
            m_TmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_TmpPos.z = 0;
            if (m_TmpIndex < 0)
            {
                // 开始drag，计算当前点在哪个线段中，找出插入点的位置
                Vector3 cur, next;
                m_TmpIndex = -1;
                for (int i = 0; i < Line.positionCount-1; i++)
                {
                    cur = Line.GetPosition(i);
                    next = Line.GetPosition(i + 1);
                    if (PointInLine(m_TmpPos, cur, next))
                    {
                        m_TmpIndex = i+1;
                        break;
                    }
                }
                if (m_TmpIndex < 0) return;

                // 移动idx后的点
                Line.positionCount++;
                for (int i = Line.positionCount-1; i > m_TmpIndex; i--)
                {
                    Line.SetPosition(i, Line.GetPosition(i-1));
                }
                Line.SetPosition(m_TmpIndex, m_TmpPos);
            }
            else
            {
                Line.SetPosition(m_TmpIndex, m_TmpPos);
            }
        }
        else if (m_TmpIndex > 0)
        {
            for (int i = m_TmpIndex; i < Line.positionCount-1; i++)
            {
                Line.SetPosition(i, Line.GetPosition(i + 1));
            }
            Line.positionCount--;
            m_TmpIndex = -1;
        }
    }

    // 判定点是否在两个端点内
    bool PointInLine(Vector3 point, Vector3 begin, Vector3 end)
    {
        return true;
    }

   

    #endregion
}
