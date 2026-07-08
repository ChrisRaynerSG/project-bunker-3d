using UnityEngine;

namespace _Scripts.Simulation.Jobs
{
    public class Job
    {
        /// <summary>The world position of the Job to be performed.</summary>
        public Vector3Int Position { get; }
        
        /// <summary>How long a dweller must "work" before the Job is completed (seconds).</summary>
        public float WorkDuration { get; }

        /// <summary>True once a dweller has taken responsibility for this job.</summary>
        public bool IsClaimed { get; set; }
        
        /// <summary>Optional world-space marker shown while the job is pending.</summary>
        public GameObject Marker { get; set; }
        
        public Job(Vector3Int position, float workDuration)
        {
            Position = position;
            WorkDuration = Mathf.Max(0.1f, workDuration);
            IsClaimed = false;
        }
    }
}