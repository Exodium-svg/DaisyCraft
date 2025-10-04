using Nbt.Components;
using System.Drawing;
using System.Text;

namespace Net.NetMessages.Clientbound.Configuration;

[PacketMetaData(GameState.Configuration, 0x02)]
public class ConfigurationKick
{
    [NetVarType(NetVarTypeEnum.NbtComponent, 0)]
    public TextComponent textComponent { get; init; }

    public ConfigurationKick(string reason)
    {
        textComponent = new TextComponent() { 
        Bold = true,
        TextColor = Color.Red,
        ShadowColor = Color.Black,
        Italic = true,
        Underlined = true,
        StrikeThrough = true,
        Obfuscated = true,
        Type = "text",
        Text = reason
        };

        //textComponent.SetText(reason);

    }
}