using LLama.Common;
using LLama.Transformers;

namespace LLama.Unittest.Transformers;

public class PromptTemplateTransformerTests
    : IDisposable
{
    private readonly LLamaWeights _model;
    private readonly PromptTemplateTransformer TestableTransformer;

    public PromptTemplateTransformerTests()
    {
        var @params = new ModelParams(Constants.GenerativeModelPath)
        {
            ContextSize = 1,
            GpuLayerCount = Constants.CIGpuLayerCount
        };
        _model = LLamaWeights.LoadFromFile(@params);

        TestableTransformer = new PromptTemplateTransformer(_model, true);
    }

    public void Dispose()
    {
        _model.Dispose();
    }

    [Fact]
    public void HistoryToText_EncodesCorrectly()
    {
        const string userData = nameof(userData);
        var template = TestableTransformer.HistoryToText(new ChatHistory(){
            Messages = [new ChatHistory.Message(AuthorRole.User, userData)]
        });

        const string expected = $"<|start_header_id|>user<|end_header_id|>\n\n{userData}<|eot_id|><|start_header_id|>assistant<|end_header_id|>\n\n";
        Assert.Equal(expected, template);
    }

    [Fact]
    public void ToModelPrompt_FormatsCorrectly()
    {
        var templater = new LLamaTemplate(_model)
        {
            AddAssistant = true,
        };

        Assert.Equal(0, templater.Count);
        templater.Add("assistant", "hello");
        Assert.Equal(1, templater.Count);
        templater.Add("user", "world");
        Assert.Equal(2, templater.Count);
        templater.Add("assistant", "111");
        Assert.Equal(3, templater.Count);
        templater.Add("user", "aaa");
        Assert.Equal(4, templater.Count);
        templater.Add("assistant", "222");
        Assert.Equal(5, templater.Count);
        templater.Add("user", "bbb");
        Assert.Equal(6, templater.Count);
        templater.Add("assistant", "333");
        Assert.Equal(7, templater.Count);
        templater.Add("user", "ccc");
        Assert.Equal(8, templater.Count);

        // Call once with empty array to discover length
        var templateResult = PromptTemplateTransformer.ToModelPrompt(templater);
        const string expected = "<|start_header_id|>assistant<|end_header_id|>\n\nhello<|eot_id|>"
                                    + "<|start_header_id|>user<|end_header_id|>\n\nworld<|eot_id|>"
                                    + "<|start_header_id|>assistant<|end_header_id|>\n\n111<|eot_id|>"
                                    + "<|start_header_id|>user<|end_header_id|>\n\naaa<|eot_id|>"
                                    + "<|start_header_id|>assistant<|end_header_id|>\n\n222<|eot_id|>"
                                    + "<|start_header_id|>user<|end_header_id|>\n\nbbb<|eot_id|>"
                                    + "<|start_header_id|>assistant<|end_header_id|>\n\n333<|eot_id|>"
                                    + "<|start_header_id|>user<|end_header_id|>\n\nccc<|eot_id|>"
                                    + "<|start_header_id|>assistant<|end_header_id|>\n\n";

        Assert.Equal(expected, templateResult);
    }
}
