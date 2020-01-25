using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    static public BoidSpawner S;    // 声明单例

    public int numBoids = 100;      // 生成boid的数量
    public GameObject boidPrefab;   // 公共游戏实体，在面板中指定
    public float spawnRadius = 100f;    // 生成范围
    public float spawnVelocity = 10f;   // 初始速度
    public float minVelocity = 0f;      //最小速度
    public float maxVelocity = 30f;     //最大速度
    public float nearDist = 30f;        //相邻的判定范围
    public float collisionDist = 5f;    //碰撞的判定范围
    public float velocityMatchingAmt = 0.01f;       // 适配相邻对象速度的加速度
    public float flockCenteringAmt = 0.15f;         // 趋向相邻对象平均中心点的加速度
    public float collisionAvoidanceAmt = -0.5f;     // 避开碰撞的加速度
    public float mouseAttractionAmt = 0.01f;        // 过远对象趋向鼠标位置的加速度
    public float mouseAvoidanceAmt = 0.75f;         // 过近对象远离鼠标位置的加速度
    public float mouseAttractionDist = 15f;          // 预设与鼠标指定位置的距离
    public float mouseAvoidanceDist = 3f;
    public float velocityLerpAmt = 0.25f;           // 插值常量，使方向速度趋向于当前方向速度

    public bool ____________;

    public Vector3 mousePos;

    // 初始帧：实例化指定数量的对象
    void Start()
    {
        S = this;

        for(int i = 0; i < numBoids; i++)
        {
            Instantiate(boidPrefab);
        }
    }

    // 运行后每一帧执行的函数，作用：获取鼠标位置，投射到世界坐标中，计算结果是名为mousePos的三维坐标
    void LateUpdate()
    {
        Vector3 mousePos2d = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.GetComponent<Transform>().position.y);
        mousePos = this.GetComponent<Camera>().ScreenToWorldPoint(mousePos2d);
        //mousePos = Input.mousePosition;
    }
}
