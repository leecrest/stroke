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
    public int idx;
    public EnNodeType type;
    public float x;
    public float y;
}

public class MapConfig
{
    public int id;
    public NodeConfig[] nodes;
}

public class NodeData
{
    public NodeConfig cfg;
    public GameObject obj;
}


public class Config {
    public static readonly string[] NodePrefab = {
        "Prefab/node_begin",
        "Prefab/node_mid",
        "Prefab/node_end",
    };

    public static readonly MapConfig[] MapCfg = {
        new MapConfig {id = 1, nodes = new NodeConfig[] {
            new NodeConfig {idx = 0, type = EnNodeType.Begin, x = -1, y = -1 },
            new NodeConfig {idx = 1, type = EnNodeType.Begin, x = -1, y = 1 },
            new NodeConfig {idx = 2, type = EnNodeType.Begin, x = 1, y = -1 },
            new NodeConfig {idx = 3, type = EnNodeType.Begin, x = 1, y = 1},
        } },
    };
	
}
