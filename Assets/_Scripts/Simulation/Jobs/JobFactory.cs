using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Simulation.Jobs
{
    public static class JobFactory
    {
        private static readonly Dictionary<JobType, Func<Vector3Int, float, Job>> Jobs = new()
        {
            {JobType.Mining, (position, workDuration) => new MiningJob(position, workDuration)}
            // Add new Jobs when created here
        };
            
        public static Job CreateJob(JobType type, Vector3Int position, float workDuration)
        {
            return Jobs[type](position, workDuration);
        }
    }
    // Add new Jobs when created here, we may need to flesh out the inheritance a bit more and make some changes here
    // but this should hopefully do for now.
    public enum JobType
    {
        Mining
    }
} 