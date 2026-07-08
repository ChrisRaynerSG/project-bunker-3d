using System.Collections.Generic;
using _Scripts.Simulation.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Central registry for mining jobs. The mining selection enqueues jobs here; idle
/// dwellers claim the closest reachable one, walk to it and, once finished, ask the
/// manager to complete it (which actually removes the block, routing tree logs
/// through the <see cref="TreeFellingService"/>).
///
/// Implemented as a lazily created singleton (like <see cref="BlockDatabase"/>) so it
/// needs no scene wiring. It is configured with the shared block/tree services by the
/// <see cref="MiningManager"/> when the world is ready.
/// </summary>
public class JobManager
{
    private static JobManager _instance;
    public static JobManager Instance => _instance ??= new JobManager();

    private readonly List<Job> _jobs = new List<Job>();
    private readonly HashSet<Vector3Int> _jobPositions = new HashSet<Vector3Int>();

    private BlockAccessor _blockAccessor;
    private BlockDatabase _blockDatabase;
    private TreeFellingService _treeFellingService;
    private Material _markerMaterial;

    /// <summary>Whether pending jobs show a translucent marker cube in the world.</summary>
    public bool ShowMarkers { get; set; } = true;

    /// <summary>Number of jobs currently pending or in progress.</summary>
    public int JobCount => _jobs.Count;

    /// <summary>
    /// Supplies the shared services used to modify the world when a job completes.
    /// Called by <see cref="MiningManager"/>.
    /// </summary>
    public void Configure(BlockAccessor blockAccessor, BlockDatabase blockDatabase, TreeFellingService treeFellingService)
    {
        _blockAccessor = blockAccessor;
        _blockDatabase = blockDatabase;
        _treeFellingService = treeFellingService;
    }

    /// <summary>
    /// Queues a mining job for the block at the given position, if it is a solid,
    /// mineable block that is not already queued.
    /// </summary>
    public void EnqueueMiningJob(Vector3Int position)
    {
        if (_blockAccessor == null) return;
        if (_jobPositions.Contains(position)) return;

        BlockData block = _blockAccessor.GetBlockDataFromPosition(position);
        if (block?.definition == null) return;
        if (!block.definition.isSolid || !block.definition.isMineable) return;

        Job job = JobFactory.CreateJob(JobType.Mining, position, block.definition.miningTime);
        if (ShowMarkers)
        {
            job.Marker = CreateMarker(position);
        }

        _jobs.Add(job);
        _jobPositions.Add(position);
        
    }

    /// <summary>
    /// Claims the closest pending job to <paramref name="from"/> that is not in the
    /// <paramref name="exclude"/> set. Returns null when nothing is available.
    /// </summary>
    public Job TryClaimClosestJob(Vector3Int from, HashSet<Vector3Int> exclude)
    {
        Job best = null;
        float bestDistance = float.PositiveInfinity;

        foreach (Job job in _jobs)
        {
            if (job.IsClaimed) continue;
            if (exclude != null && exclude.Contains(job.Position)) continue;

            float distance = (job.Position - from).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = job;
            }
        }

        if (best != null)
        {
            best.IsClaimed = true;
        }

        return best;
    }

    /// <summary>Returns a claimed job to the pending pool so another dweller can take it.</summary>
    public void AbandonJob(Job job)
    {
        if (job == null) return;
        job.IsClaimed = false;
    }

    /// <summary>
    /// Completes a job: removes the block (felling the whole tree for logs) and clears
    /// the job from the registry. Silently drops the job if the block is no longer a
    /// valid mining target (e.g. it was already removed as part of a felled tree).
    /// </summary>
    public void CompleteJob(Job job)
    {
        if (job == null) return;

        RemoveJob(job);

        if (_blockAccessor == null) return;

        BlockData block = _blockAccessor.GetBlockDataFromPosition(job.Position);
        if (block?.definition == null) return;
        if (!block.definition.isSolid || !block.definition.isMineable) return;

        if (block.definition.id == "bunker:oak_tree_log_block" && _treeFellingService != null)
        {
            _treeFellingService.FellTreeAt(job.Position);
        }
        else
        {
            _blockAccessor.SetBlock(job.Position, _blockAccessor.GetBlockDef("bunker:air_block"));
        }
    }

    private void RemoveJob(Job job)
    {
        _jobs.Remove(job);
        _jobPositions.Remove(job.Position);
        if (job.Marker != null)
        {
            Object.Destroy(job.Marker);
            job.Marker = null;
        }
    }

    private GameObject CreateMarker(Vector3Int position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = $"MiningJobMarker_{position.x}_{position.y}_{position.z}";
        marker.layer = LayerMask.NameToLayer("Ignore Raycast");

        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            Object.Destroy(markerCollider);
        }

        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 1.02f;

        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        Material material = GetMarkerMaterial();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        return marker;
    }

    private Material GetMarkerMaterial()
    {
        if (_markerMaterial != null) return _markerMaterial;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Unlit/Color") ??
            Shader.Find("Sprites/Default");

        if (shader == null) return null;

        Material material = new Material(shader);
        Color color = new Color(1f, 0.2f, 0.2f, 0.25f);

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        _markerMaterial = material;
        return _markerMaterial;
    }
}
