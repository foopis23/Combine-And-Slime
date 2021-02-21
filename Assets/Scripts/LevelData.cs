using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelData
{
    public static int[] PAR_MOVES = new int[]{
        0,
        3,
        4,
        6,
        9,
        5,
        27
    };

    public static int[,] STAR_SCORES = {
        {0, 0, 0},
        {0, 18500, 33000},
        {0, 10000, 18000},
        {0, 5000, 8000},
        {0, 3000, 4000},
        {0, 5000, 8000},
        {0, 1000, 5000}
    };
}
