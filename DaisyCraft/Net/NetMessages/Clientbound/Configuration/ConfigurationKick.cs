using Nbt.Components;
using System.Drawing;
using System.Text;

namespace Net.NetMessages.Clientbound.Configuration;

[PacketMetaData(GameState.Configuration, 0x02)]
public class ConfigurationKick
{
    [NetVarType(NetVarTypeEnum.TextComponent, 0)]
    public TextComponent textComponent { get; init; }

    public ConfigurationKick(string reason)
    {
        textComponent = new TextComponent();
        textComponent.SetText(reason);
        textComponent.SetColor(Color.AliceBlue);
        textComponent.SetShadow(Color.Red);
        textComponent.SetBold(true);
        textComponent.SetItalic(true);
        textComponent.SetUnderlined(true);
        textComponent.SetStrikeThrough(true);
        textComponent.SetObfuscated(true);
    }
}