using BiblioLinx.Models;
using BiblioLinx.ViewModels;

namespace BiblioLinx.Converters;

public class ChatBubbleSelector : DataTemplateSelector
{
    public DataTemplate UserMessageTemplate { get; set; }
    public DataTemplate AiMessageTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is ChatMessage message)
        {
            return message.Role.Equals("User", StringComparison.OrdinalIgnoreCase) 
                ? UserMessageTemplate 
                : AiMessageTemplate;
        }

        return AiMessageTemplate; // Fallback
    }
}