using UnityEngine;
using TMPro;
using Unity.Android.Gradle;

public class BlockInfoManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI blockInfoText;


    private void OnEnable()
    {
        BlockHighlighter.OnBlockHovered += UpdateBlockInfoText;
    }

    private void OnDisable()
    {
        BlockHighlighter.OnBlockHovered -= UpdateBlockInfoText;
    }

    private void UpdateBlockInfoText(BlockData blockData)
    {
        BlockDefinition bd = blockData.definition;
        if (bd != null)
        {
            blockInfoText.text = $"Block: {bd.displayName}\n" +
                                 $"ID: {bd.id}\n" +
                                 $"Description: {bd.description}\n" +
                                 $"Is Mineable: {bd.isMineable}\n" +
                                 $"Mining Time: {bd.miningTime}\n" +
                                 $"Hardness: {bd.hardness}\n" +
                                 $"Movement Cost: {bd.movementCost}\n" +
                                 $"Is Flammable: {bd.isFlammable}\n" +
                                 $"Ignition Temp: {bd.ignitionTemperature}\n" +
                                 $"Thermal Conductivity: {bd.thermalConductivity}\n" +
                                 $"Position: {blockData.X}, {blockData.Y}, {blockData.Z}\n";
        }
        else
        {
            blockInfoText.text = "No block data available.";
        }   
    }
}