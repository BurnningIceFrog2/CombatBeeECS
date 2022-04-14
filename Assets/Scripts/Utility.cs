using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class Utility
{
    public static int2 GetGridIndex(float3 pos,int2 gridCounts,float2 minGridPos,float2 gridSize)
    {
        int gridX = Mathf.FloorToInt((pos.x - minGridPos.x + gridSize.x * .5f) / gridSize.x);
        int gridY = Mathf.FloorToInt((pos.z - minGridPos.y + gridSize.y * .5f) / gridSize.y);

        gridX = Mathf.Clamp(gridX, 0, gridCounts.x - 1);
        gridY = Mathf.Clamp(gridY, 0, gridCounts.y - 1);
        return new int2(gridX, gridY);
    }

    public static float3 NearestSnappedPos(float3 pos, int2 gridCounts, float2 minGridPos, float2 gridSize)
    {
        int2 gridIndex=GetGridIndex(pos, gridCounts,minGridPos,gridSize);
        return new float3(minGridPos.x + gridIndex.x * gridSize.x, pos.y, minGridPos.y + gridIndex.y * gridSize.y);
    }

    public static float3 GetStackPos(int2 index, int height,float3 fieldSize,float resourceSize, int2 gridCounts, float2 minGridPos, float2 gridSize)
    {
        return new float3(minGridPos.x + index.x * gridSize.x, -fieldSize.y * .5f + (height + .5f) * resourceSize, minGridPos.y + index.y * gridSize.y);
    }

    /*public static bool IsTopOfStack(NativeArray<GridComp> gridArray,GridIndexComp gridIndex) 
    {
        bool isTop = false;
        for (int i = 0; i < gridArray.Length; i++)
        {
            if (gridIndex.Value.Equals(gridArray[i].Index)) 
            {
                Debug.Log("IsTopOfStack=="+ gridIndex.Value+","+gridIndex.StackHeight+","+ gridArray[i].StackHeight);
                isTop=(gridIndex.StackHeight==gridArray[i].StackHeight);
                break;
            }
        }
        return isTop;
    }*/

    public static int GetStackHeight(NativeArray<GridComp> gridArray, int2 gridIndex) 
    {
        int height = -1;
        for (int i = 0; i < gridArray.Length; i++)
        {
            if (gridIndex.Equals(gridArray[i].Index))
            {
                height = gridArray[i].StackHeight;
                break;
            }
        }
        return height;
    }
}
