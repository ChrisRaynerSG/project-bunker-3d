using System;
[Serializable]
public class DropDefinition
{
    /// <summary>
    /// The ID of the block that drops when this block is mined.
    /// </summary>
    public string id;

    /// <summary>
    /// The display name for this drop, shown in UI.
    /// </summary>
    public string displayName;

    /// <summary>
    /// A description of the drop, for tooltips or documentation.
    /// </summary>
    public string description;

    /// <summary>
    /// The model reference for this drop.
    /// </summary>
    public string model;
}
