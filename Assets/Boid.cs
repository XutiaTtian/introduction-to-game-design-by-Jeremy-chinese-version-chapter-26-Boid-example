using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    static public List<Boid> boids; //声明静态list存放游戏实体
    public Vector3 velocity;    //声明方向速度
    public Vector3 newVelocity; //声明下一帧的方向速度
    public Vector3 newPosition; //声明下一帧的位置

    public List<Boid> neighbors;        //存放相邻实体的list
    public List<Boid> collisionRisks;   //存放有碰撞风险的实体的list
    public Boid closest;                //声明距离最近的boid实体

    void Awake()            // 1.把新增的实体加入list  2.指定新增实体的起始位置、起始方向，并赋上设定的速度 
                            // 3.指定父实体  4.上色
    {
        if (boids == null)
            boids = new List<Boid>();
        boids.Add(this);            //向boids添加当前对象

        Vector3 randPos = Random.insideUnitSphere * BoidSpawner.S.spawnRadius;// 生成设定范围内的随机vector3
        randPos.y = 0;          // y值归零
        this.transform.position = randPos;  //赋值给本实体
        velocity = Random.onUnitSphere;
        velocity *= BoidSpawner.S.spawnVelocity;    //生成随机方向并乘以设定的初始速度

        neighbors = new List<Boid>();
        collisionRisks = new List<Boid>();

        this.transform.parent = GameObject.Find("Boids").transform; //指定父实体

        Color randColor = Color.black;          //声明并初始化颜色变量
        while (randColor.r + randColor.g + randColor.b < 1f)
            randColor = new Color(Random.value, Random.value, Random.value);    // 生成一个明亮的初始颜色
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();      // 利用数组给fuselage和wing赋上一个颜色
        foreach (Renderer r in rends) 
            r.material.color = randColor;
    }

    void Update()
    {
        //利用相邻集合对实体的向量和位置进行控制

        List<Boid> neighbors = GetNeighbors(this); // 获取当前实体相邻的对象，加入相邻对象的list

        newVelocity = velocity;     // 初始化下一帧的速度为当前速度
        newPosition = this.transform.position;  // 初始化下一帧的位置为当前对象的位置

        Vector3 neighborVel = GetAverageVelocity(neighbors);    // 声明并定义相邻对象的平均向量
        newVelocity += neighborVel * BoidSpawner.S.velocityMatchingAmt; // 让下一帧的向量趋向于相邻对象的平均向量

        Vector3 neighborCenterOffset = GetAveragePosition(neighbors) - this.transform.position; // 获得对象相对于相邻实体的平均位置的向量
        newVelocity += neighborCenterOffset * BoidSpawner.S.flockCenteringAmt;      // 让下一帧的向量趋向于相邻对象的平均位置点



        //利用碰撞集合对实体的向量和位置进行控制

        Vector3 dist;   // 声明向量，用于计算下一帧的方向速度
        if (collisionRisks.Count > 0)       // 碰撞集合不为空则执行以下代码
        {
            Vector3 collisionAveragePos = GetAveragePosition(collisionRisks);   // 获取碰撞集合中实体的平均向量
            dist = collisionAveragePos - this.transform.position;           // 获得实体相对于这个平均向量的朝向
            newVelocity += dist * BoidSpawner.S.collisionAvoidanceAmt;      // 赋下一帧的向量速度值
        }

        dist = BoidSpawner.S.mousePos - this.transform.position;    // 把实体相对鼠标位置的向量赋给dist
        if (dist.magnitude > BoidSpawner.S.mouseAttractionDist)      // 如果向量距离远于设定值则趋向鼠标
            newVelocity += dist * BoidSpawner.S.mouseAttractionAmt;
        else
        {
            if(dist.magnitude < BoidSpawner.S.mouseAvoidanceDist)
                newVelocity -= dist.normalized * BoidSpawner.S.mouseAvoidanceDist * BoidSpawner.S.mouseAvoidanceAmt;
        }                                                           // 在设定值之内则远离鼠标
    }

    // 运行时每一帧执行的函数，作用：1.控制速度 2.计算对象朝向 3.移动对象
    private void LateUpdate()
    {
        velocity = (1 - BoidSpawner.S.velocityLerpAmt) * velocity + BoidSpawner.S.velocityLerpAmt * newVelocity;
        // 当前方向速度取 当前方向速度与下一帧方向速度0.25的插值
        if (velocity.magnitude > BoidSpawner.S.maxVelocity)
            velocity = velocity.normalized * BoidSpawner.S.minVelocity;
        if (velocity.magnitude < BoidSpawner.S.minVelocity)
            velocity = velocity.normalized * BoidSpawner.S.maxVelocity;
        // ^ 控制速度在预设的最小值与最大值之间

        newPosition = this.transform.position + velocity * Time.deltaTime;  // 计算下一帧的位置

        this.transform.LookAt(newPosition);     // 指定当前对象的朝向
        this.transform.position = newPosition;  // 将当前对象移动到下一帧的位置
    }


    // 生成相邻对象的集合的 GetNeighbors 函数, 返回相邻对象的list，同时算出了碰撞对象的list，形参是Boid对象，在46行调用，实参是当前对象
    // boi = boid of interest 不用太在意名称，指当前对象
    public List<Boid> GetNeighbors(Boid boi)
    {
        float closestDist = float.MaxValue;     // 给最近距离赋上极大值，初始化
        Vector3 delta;
        float dist;
        neighbors.Clear();  // 对neighbors(list)初始化
        collisionRisks.Clear(); // 对collisionRisks(list)初始化

        foreach( Boid b in boids )      // 遍历所有Boid对象
        {
            if (b == boi) continue;
            delta = b.transform.position - boi.transform.position;  // 取得b位置 相对于当前对象位置的 向量
            dist = delta.magnitude;         // 取得相对距离
            if(dist < closestDist)
            {
                closestDist = dist;
                closest = b;            // closest取遍历对象相对当前位置距离的最小值
            }
            if (dist < BoidSpawner.S.nearDist) neighbors.Add(b);                // 把符合条件的对象添加到相邻对象list
            if (dist < BoidSpawner.S.collisionDist) collisionRisks.Add(b);      // 把符合条件的对象添加到碰撞对象list
        }
        if (neighbors.Count == 0) neighbors.Add(closest);           // 如果没有符合条件的相邻对象，就取最近对象为相邻对象，作为唯一的list成员
        return neighbors;                               // 返回相邻对象的list
    }


    // 生成list成员的平均位置的函数，返回一个三维向量，用于生成成员的平均位置
    public Vector3 GetAveragePosition(List<Boid> someBoids)
    {
        Vector3 sum = Vector3.zero;
        foreach (Boid b in someBoids)
            sum += b.transform.position;
        Vector3 center = sum / someBoids.Count;
        return (center);
    }

    // 生成list成员的平均方向速度的函数，返回一个三维向量，用于生成成员的平均方向速度
    public Vector3 GetAverageVelocity(List<Boid> someBoids)
    {
        Vector3 sum = Vector3.zero;
        foreach (Boid b in someBoids) sum += b.velocity;
        Vector3 avg = sum / someBoids.Count;
        return avg;
    }
}
