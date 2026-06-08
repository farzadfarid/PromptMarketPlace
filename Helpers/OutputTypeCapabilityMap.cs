using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Helpers;

public static class OutputTypeCapabilityMap
{
    private static readonly Dictionary<OutputType, AiCapability> Map = new()
    {
        { OutputType.Text,  AiCapability.TextGeneration  },
        { OutputType.Code,  AiCapability.CodeGeneration  },
        { OutputType.Form,  AiCapability.TextGeneration  },
        { OutputType.Image, AiCapability.ImageGeneration },
        { OutputType.Video, AiCapability.VideoGeneration },
        { OutputType.Audio, AiCapability.AudioGeneration },
    };

    public static AiCapability ToCapability(OutputType outputType) => Map[outputType];
}
