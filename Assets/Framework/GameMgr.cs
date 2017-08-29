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

    private List<NodeData> m_Nodes;
    private LineData m_CurLine;
    private int m_CurIndex;     // drag点的序号
    private Vector3 m_CurPos;   // drag点的坐标
    private List<LineData> m_Lines;

    public override void Init()
    {
        base.Init();
        m_Nodes = new List<NodeData>();
        m_Lines = new List<LineData>();
        m_CurLine = null;
        m_CurIndex = -1;
        m_CurPos = Vector3.zero;
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
        m_CurIndex = -1;
        m_CurLine = null;
        UIMgr.It.OpenUI("UIGame");
        ClearMap();
        LoadMap(1);
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

    void LoadMap(int mapIdx)
    {
        MapConfig map = Config.MAP_CFG[mapIdx - 1];
        NodeData node;
        GameObject obj;
        m_Nodes.Clear();
        foreach (var cfg in map.nodes)
        {
            node = new NodeData() { cfg = cfg, };
            obj = ResMgr.It.CreatePrefab(Config.NODE_PREFAB[(int)cfg.type - 1]);
            node.tf = obj.transform;
            node.tf.parent = GameRoot;
            node.tf.localPosition = new Vector3(cfg.x, cfg.y);
            node.tf.localScale = new Vector3(2, 2);
            node.sr = obj.GetComponent<SpriteRenderer>();
            obj.SetActive(true);
            m_Nodes.Add(node);
        }
        m_Lines.Clear();
        LineData line;
        LineRenderer lr;
        int count = map.lines.GetLength(0);
        int item = map.lines.GetLength(1);
        int nodeidx;
        for (int i = 0; i < count; i++)
        {
            obj = ResMgr.It.CreatePrefab(Config.LINE_PREFAB);
            obj.transform.parent = GameRoot;
            lr = obj.GetComponent<LineRenderer>();
            line = new LineData() { id = i, obj = obj, lr = lr };
            lr.startColor = Config.LINE_COLOR[i];
            lr.endColor = Config.LINE_COLOR[i];
            lr.startWidth = Config.LINE_WIDTH;
            lr.endWidth = Config.LINE_WIDTH;
            lr.positionCount = 0;
            for (int j = 0; j < item; j++)
            {
                nodeidx = map.lines[i, j];
                if (nodeidx >= 0)
                {
                    node = m_Nodes[nodeidx];
                    line.lr.positionCount++;
                    line.lr.SetPosition(lr.positionCount-1, node.tf.position);
                    node.sr.color = lr.startColor;
                    node.LineCross(i);
                }
            }
            m_Lines.Add(line);
        }
    }

    void ClearMap()
    {
        foreach (var line in m_Lines)
        {
            ResMgr.It.ReleasePrefab(Config.LINE_PREFAB, line.obj);
        }
        m_Lines.Clear();
        foreach (var node in m_Nodes)
        {
            ResMgr.It.ReleasePrefab(Config.NODE_PREFAB[(int)node.cfg.type - 1], node.tf.gameObject);
        }
        m_Nodes.Clear();
    }

    #region 线条操作


    private void Update()
    {
        if (State != EnGameState.Start) return;
        if (Input.GetMouseButton(0))
        {
            m_CurPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_CurPos.z = 0;
            if (m_CurLine == null)
            {
                // 开始drag，计算当前点在哪个线段中，找出插入点的位置
                Vector3 cur, next;
                LineRenderer lr;
                foreach (var line in m_Lines)
                {
                    lr = line.lr;
                    for (int j = 0; j < lr.positionCount - 1; j++)
                    {
                        cur = lr.GetPosition(j);
                        next = lr.GetPosition(j + 1);
                        if (PointInLine(m_CurPos, cur, next))
                        {
                            m_CurLine = line;
                            m_CurIndex = j + 1;
                            break;
                        }
                    }
                }
                if (m_CurLine == null) return;

                // 移动idx后的点
                lr = m_CurLine.lr;
                lr.positionCount++;
                for (int i = lr.positionCount-1; i > m_CurIndex; i--)
                {
                    lr.SetPosition(i, lr.GetPosition(i-1));
                }
                lr.SetPosition(m_CurIndex, m_CurPos);
            }
            else
            {
                m_CurLine.lr.SetPosition(m_CurIndex, m_CurPos);
            }
        }
        else if (m_CurLine != null)
        {
            // 判断当前的临时点是否在某个节点上
            LineRenderer lr = m_CurLine.lr;
            if (PointInNode())
            {
                lr.SetPosition(m_CurIndex, m_CurPos);
            }
            else
            {
                for (int i = m_CurIndex; i < lr.positionCount - 1; i++)
                {
                    lr.SetPosition(i, lr.GetPosition(i + 1));
                }
                lr.positionCount--;
            }
            m_CurIndex = -1;
            m_CurLine = null;
        }
    }

    // 判定点是否在两个端点内
    bool PointInLine(Vector2 point, Vector2 begin, Vector2 end)
    {
        float offset = Config.LINE_OFFSET;

        float dx = begin.x > end.x ? begin.x : end.x;
        if ((dx > 0 && point.x > dx + offset) || (dx < 0 && point.x > dx + offset)) return false;

        dx = begin.x < end.x ? begin.x : end.x;
        if ((dx > 0 && point.x < dx - offset) || (dx < 0 && point.x < dx - offset)) return false;

        dx = begin.y > end.y ? begin.y : end.y;
        if ((dx > 0 && point.y > dx + offset) || (dx < 0 && point.y > dx + offset)) return false;

        dx = begin.y < end.y ? begin.y : end.y;
        if ((dx > 0 && point.y < dx - offset) || (dx < 0 && point.y < dx - offset)) return false;

        float a = end.y - begin.y;
        float b = begin.x - end.x;
        float c = end.x * begin.y - begin.x * end.y;
        float denominator = Mathf.Sqrt(a * a + c * c);
        float dis = Mathf.Abs((a * point.x + b * point.y + c) / denominator);
        return dis <= offset;
    }

    bool PointInNode()
    {
        float dist;
        foreach (var node in m_Nodes)
        {
            if (!node.IsCross(m_CurLine.id))
            {
                dist = Vector3.Distance(m_CurPos, node.tf.position);
                if (dist <= Config.POINT_OFFSET)
                {
                    m_CurPos = node.tf.position;
                    // 节点被线条联通
                    node.sr.color = m_CurLine.lr.startColor;
                    node.LineCross(m_CurLine.id);
                    return true;
                }
            }
        }
        return false;
    }

    #endregion
}
