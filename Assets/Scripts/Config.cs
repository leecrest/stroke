using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnNodeType
{
    Begin = 1,      // 开始节点
    Mid,            // 中间节点
    End,            // 结束节点
}

public class NodeConfig
{
    public int id;
    public EnNodeType type;
    public float x;
    public float y;
}

public class MapConfig
{
    public int id;
    public NodeConfig[] nodes;
    public int[,] lines;
}

public class NodeData
{
    public NodeConfig cfg;
    public Transform tf;
    public SpriteRenderer sr;
    private int flag = 0;

    public void LineCross(int line)
    {
        flag |= 0x01 << (line+1);
    }

    public bool IsCross(int line)
    {
        return (flag & (0x01 << (line+1))) != 0;
    }
}

public class LineData
{
    public int id;
    public GameObject obj;
    public LineRenderer lr;
}


public class Config {
    public static readonly float LINE_WIDTH = 0.2f;
    public static readonly float LINE_OFFSET = 0.3f;
    public static readonly float POINT_OFFSET = 0.2f;

    public static readonly string[] NODE_PREFAB = {
        "Prefab/node_begin",
        "Prefab/node_mid",
        "Prefab/node_end",
    };

    public static readonly string LINE_PREFAB = "Prefab/line";

    public static readonly MapConfig[] MAP_CFG = {
        new MapConfig {
            id = 1,
            nodes = new NodeConfig[] {
                new NodeConfig {id = 0, type = EnNodeType.Begin, x = -1, y = -1 },
                new NodeConfig {id = 1, type = EnNodeType.Mid, x = -1, y = 1 },
                new NodeConfig {id = 2, type = EnNodeType.Mid, x = 1, y = -1 },
                new NodeConfig {id = 3, type = EnNodeType.Mid, x = 1, y = 1},
                new NodeConfig {id = 4, type = EnNodeType.Mid, x = 2, y = 0},
                new NodeConfig {id = 5, type = EnNodeType.Mid, x = 0, y = 3},
                new NodeConfig {id = 6, type = EnNodeType.Mid, x = -2, y = 0},
                new NodeConfig {id = 7, type = EnNodeType.End, x = 0, y = -3},
            },
            lines = new int[,] {
                { 5, 7},
            }
        },
    };

    public static readonly Color[] LINE_COLOR = {
        Color.blue,
        Color.white,
        Color.green,
    };
}
