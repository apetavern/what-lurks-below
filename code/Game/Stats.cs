using System;
using Sandbox;

namespace BrickJam.Game;

public static class Stats
{
    public static int EnemiesKilled { get; set; } = 0;
    public static int FloorsCompleted { get; set; } = 0;

    public static int TotalEnemiesKilled { get; private set; } = 0;
    public static int TotalFloorsCompleted { get; private set; } = 0;

    public static void Reset()
    {
        EnemiesKilled = 0;
        FloorsCompleted = 0;
    }

    public static void Save()
    {
        TotalEnemiesKilled += EnemiesKilled;
        TotalFloorsCompleted += FloorsCompleted;

        Sandbox.Services.Stats.SetValue( "enemies_killed", EnemiesKilled );
        Sandbox.Services.Stats.SetValue( "floors_completed", FloorsCompleted );

        Cookie.Set( "total_enemies_killed", TotalEnemiesKilled );
        Cookie.Set( "total_floors_completed", TotalFloorsCompleted );
    }
}