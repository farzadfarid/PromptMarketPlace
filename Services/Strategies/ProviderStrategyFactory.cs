using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Strategies;

public class ProviderStrategyFactory
{
    private readonly OpenAiCompatibleStrategy _openAi;
    private readonly AvalAiStrategy _avalAi;
    private readonly AnthropicStrategy _anthropic;
    private readonly ChatQtStrategy _chatQt;
    private readonly GoogleGeminiStrategy _google;
    private readonly VertexAiStrategy _vertex;

    public ProviderStrategyFactory(
        OpenAiCompatibleStrategy openAi,
        AvalAiStrategy avalAi,
        AnthropicStrategy anthropic,
        ChatQtStrategy chatQt,
        GoogleGeminiStrategy google,
        VertexAiStrategy vertex)
    {
        _openAi = openAi;
        _avalAi = avalAi;
        _anthropic = anthropic;
        _chatQt = chatQt;
        _google = google;
        _vertex = vertex;
    }

    public IProviderStrategy Resolve(ProviderType type) => type switch
    {
        ProviderType.AvalAi       => _avalAi,
        ProviderType.Anthropic    => _anthropic,
        ProviderType.ChatQt       => _chatQt,
        ProviderType.GoogleGemini => _google,
        ProviderType.VertexAi     => _vertex,
        _                         => _openAi
    };
}
